﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Tzkt.Data;
using Tzkt.Data.Models;
using Tzkt.Sync.Protocols;
using Tzkt.Sync.Services;

namespace Tzkt.Sync
{
    public abstract class ProtocolHandler
    {
        public abstract string Protocol { get; }
        public abstract IDiagnostics Diagnostics { get; }
        public abstract ISerializer Serializer { get; }
        public abstract IValidator Validator { get; }
        public abstract IRpc Rpc { get; }

        public readonly TezosNode Node;
        public readonly TzktContext Db;
        public readonly CacheService Cache;
        public readonly QuotesService Quotes;
        public readonly TezosProtocolsConfig Config;
        public readonly ILogger Logger;
        
        public ProtocolHandler(TezosNode node, TzktContext db, CacheService cache, QuotesService quotes, IConfiguration config, ILogger logger)
        {
            Node = node;
            Db = db;
            Cache = cache;
            Quotes = quotes;
            Config = config.GetTezosProtocolsConfig();
            Logger = logger;
        }

        public virtual async Task<AppState> CommitBlock(int head, DateTime sync)
        {
            var state = Cache.AppState.Get();

            Logger.LogDebug($"Loading block {state.Level + 1}...");
            var block = await Rpc.GetBlockAsync(state.Level + 1);

            using var tx = await Db.Database.BeginTransactionAsync();
            try
            {
                Logger.LogDebug("Loading entities...");
                await Precache(block);

                Logger.LogDebug("Loading constants...");
                await InitProtocol(block);

                if (Config.Validation)
                {
                    Logger.LogDebug("Validating block...");
                    await Validator.ValidateBlock(block);
                }

                Logger.LogDebug("Committing block...");
                await Commit(block);

                var protocolEnd = false;
                if (state.Protocol != state.NextProtocol)
                {
                    protocolEnd = true;
                    Logger.LogDebug("Migrating context...");
                    await Migration();
                }

                Logger.LogDebug("Touch accounts...");
                TouchAccounts();

                if (Config.Diagnostics)
                {
                    Logger.LogDebug("Diagnostics...");
                    if (!protocolEnd)
                        await Diagnostics.Run(block);
                    else
                        await FindDiagnostics(state.NextProtocol).Run(block);
                }

                state.KnownHead = head;
                state.LastSync = sync;
 
                Logger.LogDebug("Saving...");
                await Db.SaveChangesAsync();

                await AfterCommit(block);

                await Quotes.Commit();

                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }

            ClearCachedRelations();

            return Cache.AppState.Get();
        }
        
        public virtual async Task<AppState> RevertLastBlock(string predecessor)
        {
            using var tx = await Db.Database.BeginTransactionAsync();
            try
            {
                await Quotes.Revert();

                await BeforeRevert();

                var state = Cache.AppState.Get();
                if (state.Protocol != state.NextProtocol)
                {
                    Logger.LogDebug("Migrating context...");
                    await CancelMigration();
                }

                Logger.LogDebug("Loading protocol...");
                await InitProtocol();

                Logger.LogDebug("Reverting...");
                await Revert();

                Logger.LogDebug("Clear accounts...");
                ClearAccounts(state.Level + 1);

                if (Config.Diagnostics && state.Hash == predecessor)
                {
                    Logger.LogDebug("Diagnostics...");
                    await Diagnostics.Run(state.Level);
                }

                Logger.LogDebug("Saving...");
                await Db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }

            ClearCachedRelations();

            return Cache.AppState.Get();
        }

        public virtual void TouchAccounts()
        {
            var state = Cache.AppState.Get();
            var block = Db.ChangeTracker.Entries()
                .First(x => x.Entity is Block block && block.Level == state.Level).Entity as Block;

            foreach (var entry in Db.ChangeTracker.Entries().Where(x => x.Entity is Account))
            {
                var account = entry.Entity as Account;

                if (entry.State == EntityState.Modified)
                {
                    account.LastLevel = state.Level;
                }
                else if (entry.State == EntityState.Added)
                {
                    state.AccountsCount++;
                    account.FirstLevel = state.Level;
                    account.LastLevel = state.Level;
                    block.Events |= BlockEvents.NewAccounts;
                }
            }
        }

        public virtual void ClearAccounts(int level)
        {
            var state = Cache.AppState.Get();

            foreach (var entry in Db.ChangeTracker.Entries().Where(x => x.Entity is Account))
            {
                var account = entry.Entity as Account;

                if (entry.State == EntityState.Modified)
                    account.LastLevel = level;
                
                if (account.FirstLevel == level)
                {
                    Db.Remove(account);
                    Cache.Accounts.Remove(account);
                    state.AccountsCount--;
                }
            }
        }

        public virtual Task Precache(JsonElement block)
        {
            var accounts = new HashSet<string>(64);
            var operations = block.RequiredArray("operations", 4);

            foreach (var op in operations[2].RequiredArray().EnumerateArray())
            {
                var content = op.RequiredArray("contents", 1)[0];
                if (content.RequiredString("kind")[0] == 'a') // activate_account
                    accounts.Add(content.RequiredString("pkh"));
            }

            foreach (var op in operations[3].RequiredArray().EnumerateArray())
            {
                foreach (var content in op.RequiredArray("contents").EnumerateArray())
                {
                    accounts.Add(content.RequiredString("source"));
                    if (content.RequiredString("kind")[0] == 't') // transaction
                    {
                        if (content.TryGetProperty("destination", out var dest))
                            accounts.Add(dest.GetString());

                        if (content.Required("metadata").TryGetProperty("internal_operation_results", out var internalResults))
                            foreach (var internalContent in internalResults.RequiredArray().EnumerateArray())
                            {
                                accounts.Add(internalContent.RequiredString("source"));
                                if (internalContent.RequiredString("kind")[0] == 't') // transaction
                                {
                                    if (internalContent.TryGetProperty("destination", out var internalDest))
                                        accounts.Add(internalDest.GetString());
                                }
                            }
                    }
                }
            }

            return Cache.Accounts.LoadAsync(accounts);
        }

        public virtual Task Migration() => Task.CompletedTask;

        public virtual Task CancelMigration() => Task.CompletedTask;

        public abstract Task InitProtocol();

        public abstract Task InitProtocol(JsonElement block);

        public abstract Task Commit(JsonElement block);

        public virtual Task AfterCommit(JsonElement block) => Task.CompletedTask;

        public virtual Task BeforeRevert() => Task.CompletedTask;

        public abstract Task Revert();

        IDiagnostics FindDiagnostics(string hash)
        {
            return hash switch
            {
                "PrihK96nBAFSxVL1GLJTVhu9YnzkMFiBeuJRPA8NwuZVZCE1L6i" => new Protocols.Genesis.Diagnostics(),
                "Ps9mPmXaRzmzk35gbAYNCAw6UXdE2qoABTHbN2oEEc1qM7CwT9P" => new Protocols.Initiator.Diagnostics(),
                "PtBMwNZT94N7gXKw4i273CKcSaBrrBnqnt3RATExNKr9KNX2USV" => new Protocols.Initiator.Diagnostics(),
                "PtYuensgYBb3G3x1hLLbCmcav8ue8Kyd2khADcL5LsT5R1hcXex" => new Protocols.Initiator.Diagnostics(),
                "PtCJ7pwoxe8JasnHY8YonnLYjcVHmhiARPJvqcC6VfHT5s8k8sY" => new Protocols.Proto1.Diagnostics(Db, Rpc),
                "PsYLVpVvgbLhAhoqAkMFUo6gudkJ9weNXhUYCiLDzcUpFpkk8Wt" => new Protocols.Proto2.Diagnostics(Db, Rpc),
                "PsddFKi32cMJ2qPjf43Qv5GDWLDPZb3T3bF6fLKiF5HtvHNU7aP" => new Protocols.Proto3.Diagnostics(Db, Rpc),
                "Pt24m4xiPbLDhVgVfABUjirbmda3yohdN82Sp9FeuAXJ4eV9otd" => new Protocols.Proto4.Diagnostics(Db, Rpc),
                "PsBabyM1eUXZseaJdmXFApDSBqj8YBfwELoxZHHW77EMcAbbwAS" => new Protocols.Proto5.Diagnostics(Db, Rpc),
                "PsBABY5HQTSkA4297zNHfsZNKtxULfL18y95qb3m53QJiXGmrbU" => new Protocols.Proto5.Diagnostics(Db, Rpc),
                "PsCARTHAGazKbHtnKfLzQg3kms52kSRpgnDY982a9oYsSXRLQEb" => new Protocols.Proto6.Diagnostics(Db, Rpc),
                "PsDELPH1Kxsxt8f9eWbxQeRxkjfbxoqM52jvs5Y5fBxWWh4ifpo" => new Protocols.Proto7.Diagnostics(Db, Rpc),
                _ => throw new NotImplementedException($"Diagnostics for the protocol {hash} hasn't been implemented yet")
            };
        }

        void ClearCachedRelations()
        {
            foreach (var entry in Db.ChangeTracker.Entries())
            {
                switch(entry.Entity)
                {
                    case Data.Models.Delegate delegat:
                        delegat.Delegate = null;
                        delegat.DelegatedAccounts = null;
                        delegat.FirstBlock = null;
                        delegat.Software = null;
                        break;
                    case User user:
                        user.Delegate = null;
                        user.FirstBlock = null;
                        break;
                    case Contract contract:
                        contract.Delegate = null;
                        contract.WeirdDelegate = null;
                        contract.Manager = null;
                        contract.Creator = null;
                        contract.FirstBlock = null;
                        break;
                    case Block b:
                        b.Activations = null;
                        b.Baker = null;
                        b.Ballots = null;
                        b.CreatedAccounts = null;
                        b.Delegations = null;
                        b.DoubleBakings = null;
                        b.DoubleEndorsings = null;
                        b.Endorsements = null;
                        b.Originations = null;
                        b.Proposals = null;
                        b.Protocol = null;
                        b.Reveals = null;
                        b.Revelation = null;
                        b.Revelations = null;
                        b.Transactions = null;
                        b.Migrations = null;
                        b.RevelationPenalties = null;
                        b.Software = null;
                        break;
                    case VotingPeriod period:
                        period.Epoch = null;
                        if (period is ExplorationPeriod exploration)
                            exploration.Proposal = null;
                        else if (period is PromotionPeriod promotion)
                            promotion.Proposal = null;
                        else if (period is TestingPeriod testing)
                            testing.Proposal = null;
                        break;
                    case Proposal proposal:
                        proposal.ExplorationPeriod = null;
                        proposal.Initiator = null;
                        proposal.PromotionPeriod = null;
                        proposal.ProposalPeriod = null;
                        proposal.TestingPeriod = null;
                        break;
                }
            }
        }
    }
}

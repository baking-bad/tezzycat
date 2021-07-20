﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Netezos.Contracts;
using Netezos.Encoding;
using Netezos.Keys;
using Newtonsoft.Json.Linq;
using Tzkt.Data.Models;

namespace Tzkt.Sync.Protocols.Proto1
{
    partial class ProtoActivator : ProtocolCommit
    {
        async Task<List<Account>> BootstrapAccounts(Protocol protocol, JToken parameters)
        {
            var bootstrapAccounts = parameters["bootstrap_accounts"]?
                .Select(x => (x[0].Value<string>(), x[1].Value<long>()))
                .ToList() ?? new(0);

            var bootstrapContracts = parameters["bootstrap_contracts"]?
                .Select(x =>
                (
                    x["amount"].Value<long>(),
                    x["delegate"]?.Value<string>() ?? null,
                    x["script"]["code"].ToString(),
                    x["script"]["storage"].ToString())
                )
                .ToList() ?? new(0);

            var accounts = new List<Account>(bootstrapAccounts.Count + bootstrapContracts.Count);

            #region allocate null-address
            var nullAddress = (User)await Cache.Accounts.GetAsync(NullAddress.Address);
            if (nullAddress.Id != NullAddress.Id)
                throw new Exception("Failed to allocate null-address");
            #endregion

            #region bootstrap delegates
            foreach (var (pubKey, balance) in bootstrapAccounts.Where(x => x.Item1[0] != 't'))
            {
                var baker = new Data.Models.Delegate
                {
                    Id = Cache.AppState.NextAccountId(),
                    Address = PubKey.FromBase58(pubKey).Address,
                    Balance = balance,
                    StakingBalance = balance,
                    Counter = 0,
                    PublicKey = pubKey,
                    FirstLevel = 1,
                    LastLevel = 1,
                    ActivationLevel = 1,
                    DeactivationLevel = GracePeriod.Init(2, protocol),
                    Staked = true,
                    Revealed = true,
                    Type = AccountType.Delegate
                };
                Cache.Accounts.Add(baker);
                accounts.Add(baker);
            }
            #endregion

            #region bootstrap users
            foreach (var (pkh, balance) in bootstrapAccounts.Where(x => x.Item1[0] == 't'))
            {
                var user = new User
                {
                    Id = Cache.AppState.NextAccountId(),
                    Address = pkh,
                    Balance = balance,
                    Counter = 0,
                    FirstLevel = 1,
                    LastLevel = 1,
                    Type = AccountType.User
                };
                Cache.Accounts.Add(user);
                accounts.Add(user);
            }
            #endregion

            #region bootstrap contracts
            var index = 0;
            foreach (var (balance, delegatePkh, codeStr, storageStr) in bootstrapContracts)
            {
                #region contract
                var delegat = Cache.Accounts.GetDelegate(delegatePkh);
                var manager = nullAddress;

                var contract = new Contract
                {
                    Id = Cache.AppState.NextAccountId(),
                    Address = OriginationNonce.GetContractAddress(index++),
                    Balance = balance,
                    Counter = 0,
                    FirstLevel = 1,
                    LastLevel = 1,
                    Spendable = false,
                    DelegationLevel = delegat == null ? null : 1,
                    Delegate = delegat,
                    Manager = manager,
                    Staked = delegat != null,
                    Type = AccountType.Contract,
                    Kind = ContractKind.SmartContract,
                };

                manager.ContractsCount++;
                if (delegat != null)
                {
                    delegat.DelegatorsCount++;
                    delegat.StakingBalance += contract.Balance;
                }

                Cache.Accounts.Add(contract);
                accounts.Add(contract);
                #endregion

                #region script
                var code = Micheline.FromJson(codeStr) as MichelineArray;
                var micheParameter = code.First(x => x is MichelinePrim p && p.Prim == PrimType.parameter);
                var micheStorage = code.First(x => x is MichelinePrim p && p.Prim == PrimType.storage);
                var micheCode = code.First(x => x is MichelinePrim p && p.Prim == PrimType.code);
                var script = new Script
                {
                    Id = Cache.AppState.NextScriptId(),
                    Level = 1,
                    ContractId = contract.Id,
                    ParameterSchema = micheParameter.ToBytes(),
                    StorageSchema = micheStorage.ToBytes(),
                    CodeSchema = micheCode.ToBytes(),
                    Current = true
                };

                var typeSchema = script.ParameterSchema.Concat(script.StorageSchema);
                var fullSchema = typeSchema.Concat(script.CodeSchema);
                contract.TypeHash = script.TypeHash = Script.GetHash(typeSchema);
                contract.CodeHash = script.CodeHash = Script.GetHash(fullSchema);

                contract.Tzips = Tzip.None;
                if (script.Schema.IsFA1())
                {
                    if (script.Schema.IsFA12())
                        contract.Tzips |= Tzip.FA12;

                    contract.Tzips |= Tzip.FA1;
                    contract.Kind = ContractKind.Asset;
                }
                if (script.Schema.IsFA2())
                {
                    contract.Tzips |= Tzip.FA2;
                    contract.Kind = ContractKind.Asset;
                }

                Db.Scripts.Add(script);
                Cache.Schemas.Add(contract, script.Schema);
                #endregion

                #region storage
                var storageValue = Micheline.FromJson(storageStr);
                var storage = new Storage
                {
                    Id = Cache.AppState.NextStorageId(),
                    Level = 1,
                    ContractId = contract.Id,
                    RawValue = script.Schema.OptimizeStorage(storageValue, false).ToBytes(),
                    JsonValue = script.Schema.HumanizeStorage(storageValue),
                    Current = true
                };

                Db.Storages.Add(storage);
                Cache.Storages.Add(contract, storage);
                #endregion

            }
            #endregion

            Db.Accounts.AddRange(accounts);

            #region migration ops
            var block = Cache.Blocks.Current();

            block.Operations |= Operations.Migrations;
            if (accounts.Any(x => x.Type == AccountType.Contract))
                block.Events |= BlockEvents.SmartContracts;

            foreach (var account in accounts)
            {
                var migration = new MigrationOperation
                {
                    Id = Cache.AppState.NextOperationId(),
                    Block = block,
                    Level = block.Level,
                    Timestamp = block.Timestamp,
                    Account = account,
                    Kind = MigrationKind.Bootstrap,
                    BalanceChange = account.Balance,
                };

                if (account is Contract contract)
                {
                    var script = Db.ChangeTracker.Entries()
                        .First(x => x.Entity is Script s && s.ContractId == contract.Id).Entity as Script;
                    var storage = await Cache.Storages.GetAsync(contract);
                    
                    script.MigrationId = migration.Id;
                    storage.MigrationId = migration.Id;

                    migration.Script = script;
                    migration.Storage = storage;
                }

                Db.MigrationOps.Add(migration);
                account.MigrationsCount++;
            }

            var state = Cache.AppState.Get();
            state.MigrationOpsCount += accounts.Count;
            #endregion

            #region statistics
            var stats = await Cache.Statistics.GetAsync(1);
            stats.TotalBootstrapped = accounts.Sum(x => x.Balance);
            stats.TotalVested = accounts.Where(x => x.Type == AccountType.Contract).Sum(x => x.Balance);
            #endregion

            return accounts;
        }

        async Task ClearAccounts()
        { 
            await Db.Database.ExecuteSqlRawAsync(@"
                DELETE FROM ""Accounts"";
                DELETE FROM ""MigrationOps"";
                DELETE FROM ""Scripts"";
                DELETE FROM ""Storages"";");

            await Cache.Accounts.ResetAsync();
            Cache.Schemas.Reset();
            Cache.Storages.Reset();

            var state = Cache.AppState.Get();
            state.AccountsCount = 0;
            state.MigrationOpsCount = 0;
        }
    }
}

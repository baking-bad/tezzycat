﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Tzkt.Data.Models;
using Tzkt.Data.Models.Base;

namespace Tzkt.Sync.Protocols.Proto1
{
    class DelegationsCommit : ProtocolCommit
    {
        public List<DelegationOperation> Delegations { get; private set; }
        public HashSet<string> Activations { get; private set; }
        public Protocol Protocol { get; private set; }

        public DelegationsCommit(ProtocolHandler protocol, List<ICommit> commits) : base(protocol, commits) { }

        public override async Task Init()
        {
            var block = await Cache.GetCurrentBlockAsync();
            block.Baker ??= (Data.Models.Delegate)await Cache.GetAccountAsync(block.BakerId);

            Activations = new HashSet<string>();
            Protocol = await Cache.GetCurrentProtocolAsync();
            Delegations = await Db.DelegationOps.Include(x => x.Parent).Where(x => x.Level == block.Level).ToListAsync();
            foreach (var op in Delegations)
            {
                op.Block = block;
                op.Sender = await Cache.GetAccountAsync(op.SenderId);
                op.Sender.Delegate ??= op.Sender.DelegateId != null
                    ? (Data.Models.Delegate)await Cache.GetAccountAsync(op.Sender.DelegateId)
                    : null;

                op.Delegate = (Data.Models.Delegate)await Cache.GetAccountAsync(op.DelegateId);

                //op.Parent ??= op.ParentId != null
                //    ? Transactions.FirstOrDefault(x => x.Id == op.ParentId)
                //    : null;
            }
        }

        public override async Task Init(IBlock block)
        {
            var rawBlock = block as RawBlock;
            var parsedBlock = FindCommit<BlockCommit>().Block;
            parsedBlock.Baker ??= (Data.Models.Delegate)await Cache.GetAccountAsync(parsedBlock.BakerId);

            Activations = new HashSet<string>();
            Protocol = await Cache.GetProtocolAsync(block.Protocol);
            Delegations = new List<DelegationOperation>();
            foreach (var op in rawBlock.Operations[3])
            {
                foreach (var content in op.Contents.Where(x => x is RawDelegationContent))
                {
                    var delegation = content as RawDelegationContent;

                    var sender = await Cache.GetAccountAsync(delegation.Source);
                    sender.Delegate ??= (Data.Models.Delegate)await Cache.GetAccountAsync(sender.DelegateId);

                    var delegat = !String.IsNullOrEmpty(delegation.Delegate) && delegation.Source != delegation.Delegate
                        ? (Data.Models.Delegate)await Cache.GetAccountAsync(delegation.Delegate)
                        : null;

                    if (delegation.Source == delegation.Delegate)
                        Activations.Add(delegation.Source);

                    Delegations.Add(new DelegationOperation
                    {
                        Block = parsedBlock,
                        Timestamp = parsedBlock.Timestamp,

                        OpHash = op.Hash,

                        BakerFee = delegation.Fee,
                        Counter = delegation.Counter,
                        GasLimit = delegation.GasLimit,
                        StorageLimit = delegation.StorageLimit,
                        Sender = sender,
                        Delegate = delegat,
                        Status = delegation.Metadata.Result.Status switch
                        {
                            "applied" => OperationStatus.Applied,
                            _ => throw new NotImplementedException()
                        }
                    });
                }
            }
        }

        public override async Task Apply()
        {
            if (Delegations == null)
                throw new Exception("Commit is not initialized");

            foreach (var delegation in Delegations)
            {
                #region entities
                var block = delegation.Block;
                var blockBaker = block.Baker;

                var sender = delegation.Sender;
                var senderDelegate = sender.Delegate ?? sender as Data.Models.Delegate;

                var newDelegate = delegation.Delegate;

                //Db.TryAttach(block);
                Db.TryAttach(blockBaker);

                Db.TryAttach(sender);
                Db.TryAttach(senderDelegate);

                Db.TryAttach(newDelegate);
                #endregion

                #region apply operation
                sender.Balance -= delegation.BakerFee;
                if (senderDelegate != null) senderDelegate.StakingBalance -= delegation.BakerFee;
                blockBaker.FrozenFees += delegation.BakerFee;

                sender.Operations |= Operations.Delegations;
                block.Operations |= Operations.Delegations;

                sender.Counter = Math.Max(sender.Counter, delegation.Counter);
                #endregion

                #region apply result
                if (delegation.Status == OperationStatus.Applied)
                {
                    await ResetDelegate(sender, senderDelegate);

                    if (Activations.Contains(sender.Address))
                    {
                        if (sender is User user)
                        {
                            await UpgradeUser(delegation);
                            sender = delegation.Sender;
                            newDelegate = delegation.Delegate;
                        }
                        else if (sender is Data.Models.Delegate)
                        {
                            await ReactivateDelegate(delegation);
                        }
                    }
                    else if (newDelegate != null)
                    {
                        await SetDelegate(sender, newDelegate, delegation.Level);
                    }
                }
                #endregion

                Db.DelegationOps.Add(delegation);
            }
        }

        public override async Task Revert()
        {
            if (Delegations == null)
                throw new Exception("Commit is not initialized");

            foreach (var delegation in Delegations)
            {
                #region entities
                var block = delegation.Block;
                var blockBaker = block.Baker;

                var sender = delegation.Sender;
                var senderDelegate = sender.Delegate ?? sender as Data.Models.Delegate;

                var newDelegate = delegation.Delegate;

                var prevDelegation = await GetPrevDelegationAsync(sender, delegation.Counter);
                var prevDelegate = prevDelegation?.Delegate;

                //Db.TryAttach(block);
                Db.TryAttach(blockBaker);

                Db.TryAttach(sender);
                Db.TryAttach(senderDelegate);

                Db.TryAttach(newDelegate);
                Db.TryAttach(prevDelegate);
                #endregion

                #region revert result
                if (delegation.Status == OperationStatus.Applied)
                {
                    delegation.Sender = sender;
                    delegation.Delegate = newDelegate;

                    await ResetDelegate(sender, senderDelegate);

                    if (sender.Address == newDelegate.Address)
                    {
                        var prevActivation = await GetPrevActivationAsync(sender, delegation.Counter);
                        if (prevActivation == null)
                        {
                            await DowngradeDelegate(delegation);
                            sender = delegation.Sender;
                            newDelegate = delegation.Delegate;

                            if (prevDelegate != null)
                                await SetDelegate(sender, newDelegate, delegation.Level);
                        }
                        else
                        {
                            var prevState = await GetDelegateAsync(delegation.Level - 1, sender.Address);
                            if (prevState.Deactivated)
                                await DeactivateDelegate(delegation, (prevState.GracePeriod + 1) * Protocol.BlocksPerCycle);
                        }
                    }
                    else
                    {
                        if (prevDelegate != null)
                            await SetDelegate(sender, newDelegate, delegation.Level);
                    }
                }
                #endregion

                #region revert operation
                sender.Balance += delegation.BakerFee;
                if (prevDelegate != null) prevDelegate.StakingBalance += delegation.BakerFee;
                blockBaker.FrozenFees -= delegation.BakerFee;

                if (!await Db.DelegationOps.AnyAsync(x => x.SenderId == delegation.SenderId && x.Counter < delegation.Counter))
                    delegation.Sender.Operations &= ~Operations.Delegations;

                sender.Counter = Math.Min(sender.Counter, delegation.Counter - 1);
                #endregion

                Db.DelegationOps.Remove(delegation);
            }
        }

        Task UpgradeUser(DelegationOperation delegation)
        {
            var user = delegation.Sender as User;

            var delegat = new Data.Models.Delegate
            {
                ActivationBlock = delegation.Block,
                ActivationLevel = delegation.Level,
                Activation = user.Activation,
                Address = user.Address,
                Balance = user.Balance,
                Counter = user.Counter,
                DeactivationBlock = null,
                DeactivationLevel = null,
                Delegate = null,
                DelegateId = null,
                DelegationLevel = null,
                Id = user.Id,
                ManagedContracts = user.ManagedContracts,
                ManagedOriginations = user.ManagedOriginations,
                Operations = user.Operations,
                OriginatedContracts = user.OriginatedContracts,
                PublicKey = user.PublicKey,
                ReceivedTransactions = user.ReceivedTransactions,
                Reveal = user.Reveal,
                SentDelegations = user.SentDelegations,
                SentOriginations = user.SentOriginations,
                SentTransactions = user.SentTransactions,
                Staked = true,
                StakingBalance = user.Balance,
                Type = AccountType.Delegate
            };

            #region update relations
            foreach (var op in FindCommit<RevealsCommit>().Reveals)
            {
                if (op.SenderId == user.Id)
                    op.Sender = delegat;
            }

            foreach (var op in Delegations)
            {
                if (op.SenderId == user.Id)
                    op.Sender = delegat;

                if (op.DelegateId == user.Id)
                    op.Delegate = delegat;
            }
            #endregion

            Db.Entry(user).State = EntityState.Detached;
            Db.Entry(delegat).State = EntityState.Modified;
            Cache.AddAccount(delegat);

            #region update graph
            foreach (var op in FindCommit<RevealsCommit>().Reveals
                .Where(x => x.SenderId == user.Id))
                Db.RevealOps.Update(op);

            foreach (var op in FindCommit<DelegationsCommit>().Delegations
                .Where(x => x.SenderId == user.Id || x.DelegateId == user.Id))
                Db.DelegationOps.Update(op);
            #endregion

            delegation.Sender = delegation.Delegate = delegat;
            return Task.CompletedTask;
        }

        Task DowngradeDelegate(DelegationOperation delegation)
        {
            var delegat = delegation.Delegate as Data.Models.Delegate;

            var user = new User
            {
                Activation = delegat.Activation,
                Address = delegat.Address,
                Balance = delegat.Balance,
                Counter = delegat.Counter,
                Delegate = null,
                DelegateId = null,
                DelegationLevel = null,
                Id = delegat.Id,
                ManagedContracts = delegat.ManagedContracts,
                ManagedOriginations = delegat.ManagedOriginations,
                Operations = delegat.Operations,
                OriginatedContracts = delegat.OriginatedContracts,
                PublicKey = delegat.PublicKey,
                ReceivedTransactions = delegat.ReceivedTransactions,
                Reveal = delegat.Reveal,
                SentDelegations = delegat.SentDelegations,
                SentOriginations = delegat.SentOriginations,
                SentTransactions = delegat.SentTransactions,
                Staked = false,
                Type = AccountType.User
            };

            #region update relations
            foreach (var op in FindCommit<RevealsCommit>().Reveals)
            {
                if (op.SenderId == delegat.Id)
                    op.Sender = user;
            }

            foreach (var op in FindCommit<DelegationsCommit>().Delegations)
            {
                if (op.SenderId == delegat.Id)
                    op.Sender = user;

                if (op.DelegateId == delegat.Id)
                    op.Delegate = null;
            }

            foreach (var op in FindCommit<TransactionsCommit>().Transactions)
            {
                if (op.SenderId == delegat.Id)
                    op.Sender = user;

                if (op.TargetId == delegat.Id)
                    op.Target = user;
            }

            foreach (var op in FindCommit<OriginationsCommit>().Originations)
            {
                if (op.SenderId == delegat.Id)
                    op.Sender = user;

                if (op.DelegateId == delegat.Id)
                    op.Delegate = null;

                if (op.ManagerId == delegat.Id)
                    op.Manager = user;
            }
            #endregion

            Db.Entry(delegat).State = EntityState.Detached;
            Db.Entry(user).State = EntityState.Modified;
            Cache.AddAccount(user);

            #region update graph
            foreach (var op in FindCommit<RevealsCommit>().Reveals
                .Where(x => x.SenderId == user.Id))
                Db.RevealOps.Update(op);

            foreach (var op in FindCommit<DelegationsCommit>().Delegations
                .Where(x => x.SenderId == user.Id || x.DelegateId == null && x.Sender is User))
                Db.DelegationOps.Update(op);

            foreach (var op in FindCommit<TransactionsCommit>().Transactions
                .Where(x => x.SenderId == user.Id || x.TargetId == user.Id))
                Db.TransactionOps.Update(op);

            foreach (var op in FindCommit<OriginationsCommit>().Originations
                .Where(x => x.SenderId == user.Id || x.DelegateId == null && x.Sender is User || x.ManagerId == user.Id))
                Db.OriginationOps.Update(op);
            #endregion

            delegation.Sender = user;
            delegation.Delegate = null;
            return Task.CompletedTask;
        }

        async Task ReactivateDelegate(DelegationOperation delegation)
        {
            var delegat = delegation.Sender as Data.Models.Delegate;

            delegat.DeactivationBlock = null;
            delegat.DeactivationLevel = null;
            delegat.Staked = true;

            foreach (var delegator in await Db.Accounts.Where(x => x.DelegateId == delegat.Id).ToListAsync())
            {
                var account = await Cache.GetAccountAsync(delegator);
                account.Staked = true;
            }

            delegation.Delegate = delegat;
        }

        async Task DeactivateDelegate(DelegationOperation delegation, int deactivationLevel)
        {
            var delegat = delegation.Sender as Data.Models.Delegate;

            var deactivationBlock = await Db.Blocks.FirstOrDefaultAsync(x => x.Level == deactivationLevel);
            delegat.DeactivationLevel = deactivationBlock.Level;
            delegat.Staked = false;

            foreach (var delegator in await Db.Accounts.Where(x => x.DelegateId == delegat.Id).ToListAsync())
            {
                var account = await Cache.GetAccountAsync(delegator);
                account.Staked = false;
            }
        }

        Task SetDelegate(Account sender, Data.Models.Delegate newDelegate, int level)
        {
            sender.Delegate = newDelegate;
            sender.DelegateId = newDelegate.Id;
            sender.DelegationLevel = level;
            sender.Staked = true;

            newDelegate.Delegators++;
            newDelegate.StakingBalance += sender.Balance;

            return Task.CompletedTask;
        }

        Task ResetDelegate(Account sender, Data.Models.Delegate currentDelegate)
        {
            if (currentDelegate != null)
            {
                currentDelegate.Delegators--;
                currentDelegate.StakingBalance -= sender.Balance;
            }

            sender.Delegate = null;
            sender.DelegateId = null;
            sender.DelegationLevel = null;
            sender.Staked = false;

            return Task.CompletedTask;
        }

        async Task<DelegationOperation> GetPrevDelegationAsync(Account sender, int counter)
        {
            var result = await Db.DelegationOps
                .Where(x => x.Status == OperationStatus.Applied &&
                    x.SenderId == sender.Id &&
                    x.Counter < counter)
                .OrderByDescending(x => x.Counter)
                .FirstOrDefaultAsync();

            if (result != null)
            {
                result.Sender = sender;
                result.Delegate = (Data.Models.Delegate)await Cache.GetAccountAsync(result.DelegateId);
            }

            return result;
        }

        async Task<DelegationOperation> GetPrevActivationAsync(Account sender, int counter)
        {
            var result = await Db.DelegationOps
                .Where(x => x.Status == OperationStatus.Applied &&
                    x.SenderId == sender.Id &&
                    x.DelegateId == sender.Id &&
                    x.Counter < counter)
                .OrderByDescending(x => x.Counter)
                .FirstOrDefaultAsync();

            if (result != null)
                result.Sender = result.Delegate = (Data.Models.Delegate)sender;

            return result;
        }

        async Task<RawDelegate> GetDelegateAsync(int level, string address)
        {
            var stream = await Proto.Node.GetDelegateAsync(level, address);
            return await (Proto.Serializer as Serializer).DeserializeDelegate(stream);
        }

        #region static
        public static async Task<DelegationsCommit> Create(ProtocolHandler protocol, List<ICommit> commits, RawBlock rawBlock)
        {
            var commit = new DelegationsCommit(protocol, commits);
            await commit.Init(rawBlock);
            return commit;
        }

        public static async Task<DelegationsCommit> Create(ProtocolHandler protocol, List<ICommit> commits)
        {
            var commit = new DelegationsCommit(protocol, commits);
            await commit.Init();
            return commit;
        }
        #endregion
    }
}

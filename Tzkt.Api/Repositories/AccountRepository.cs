﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dapper;

using Tzkt.Api.Models;
using Tzkt.Api.Services.Cache;
using Tzkt.Api.Services.Metadata;

namespace Tzkt.Api.Repositories
{
    public class AccountRepository : DbConnection
    {
        readonly AccountsCache Accounts;
        readonly StateCache State;
        readonly TimeCache Time;
        readonly OperationRepository Operations;
        readonly SoftwareMetadataService Software;

        public AccountRepository(AccountsCache accounts, StateCache state, TimeCache time, OperationRepository operations, SoftwareMetadataService software, IConfiguration config) : base(config)
        {
            Accounts = accounts;
            State = state;
            Time = time;
            Operations = operations;
            Software = software;
        }

        public async Task<Account> Get(string address, bool metadata)
        {
            var rawAccount = await Accounts.GetAsync(address);

            if (rawAccount == null)
                return address[0] == 't'
                    ? new EmptyAccount
                    {
                        Address = address,
                        Counter = State.GetCounter(),
                    }
                    : null;

            var accMetadata = Accounts.GetMetadata(rawAccount.Id);

            switch (rawAccount)
            {
                case RawDelegate delegat:
                    #region build delegate
                    return new Models.Delegate
                    {
                        Alias = accMetadata?.Alias,
                        Active = delegat.Staked,
                        Address = delegat.Address,
                        PublicKey = delegat.PublicKey,
                        Revealed = delegat.Revealed,
                        Balance = delegat.Balance,
                        FrozenDeposits = delegat.FrozenDeposits,
                        FrozenRewards = delegat.FrozenRewards,
                        FrozenFees = delegat.FrozenFees,
                        Counter = delegat.Counter,
                        ActivationLevel = delegat.ActivationLevel,
                        ActivationTime = Time[delegat.ActivationLevel],
                        DeactivationLevel = delegat.Staked ? null : (int?)delegat.DeactivationLevel,
                        DeactivationTime = delegat.Staked ? null : (DateTime?)Time[delegat.DeactivationLevel],
                        StakingBalance = delegat.StakingBalance,
                        FirstActivity = delegat.FirstLevel,
                        FirstActivityTime = Time[delegat.FirstLevel],
                        LastActivity = delegat.LastLevel,
                        LastActivityTime = Time[delegat.LastLevel],
                        NumActivations = delegat.Activated == true ? 1 : 0,
                        NumBallots = delegat.BallotsCount,
                        NumContracts = delegat.ContractsCount,
                        NumDelegators = delegat.DelegatorsCount,
                        NumBlocks = delegat.BlocksCount,
                        NumDelegations = delegat.DelegationsCount,
                        NumDoubleBaking = delegat.DoubleBakingCount,
                        NumDoubleEndorsing = delegat.DoubleEndorsingCount,
                        NumEndorsements = delegat.EndorsementsCount,
                        NumNonceRevelations = delegat.NonceRevelationsCount,
                        NumRevelationPenalties = delegat.RevelationPenaltiesCount,
                        NumOriginations = delegat.OriginationsCount,
                        NumProposals = delegat.ProposalsCount,
                        NumReveals = delegat.RevealsCount,
                        NumMigrations = delegat.MigrationsCount,
                        NumTransactions = delegat.TransactionsCount,
                        Metadata = metadata ? accMetadata : null,
                        Software = delegat.SoftwareId == null ? null : Software[(int)delegat.SoftwareId]
                    };
                    #endregion
                case RawUser user:
                    #region build user
                    var userDelegate = user.DelegateId == null ? null
                        : await Accounts.GetAsync((int)user.DelegateId);

                    var userDelegateMetadata = userDelegate == null ? null
                        : Accounts.GetMetadata(userDelegate.Id);

                    return new User
                    {
                        Alias = accMetadata?.Alias,
                        Address = user.Address,
                        Balance = user.Balance,
                        Counter = user.Balance > 0 ? user.Counter : State.GetCounter(),
                        FirstActivity = user.FirstLevel,
                        FirstActivityTime = Time[user.FirstLevel],
                        LastActivity = user.LastLevel,
                        LastActivityTime = Time[user.LastLevel],
                        PublicKey = user.PublicKey,
                        Revealed = user.Revealed,
                        Delegate = userDelegate == null ? null
                            : new DelegateInfo
                            {
                                Alias = userDelegateMetadata?.Alias,
                                Address = userDelegate.Address,
                                Active = userDelegate.Staked
                            },
                        DelegationLevel = userDelegate == null ? null
                            : user.DelegationLevel,
                        DelegationTime = userDelegate == null ? null
                            : (DateTime?)Time[(int)user.DelegationLevel],
                        NumActivations = user.Activated == true ? 1 : 0,
                        NumContracts = user.ContractsCount,
                        NumDelegations = user.DelegationsCount,
                        NumOriginations = user.OriginationsCount,
                        NumReveals = user.RevealsCount,
                        NumMigrations = user.MigrationsCount,
                        NumTransactions = user.TransactionsCount,
                        Metadata = metadata ? accMetadata : null
                    };
                    #endregion
                case RawContract contract:
                    #region build contract
                    var creator = contract.CreatorId == null ? null
                        : await Accounts.GetAsync((int)contract.CreatorId);

                    var creatorMetadata = creator == null ? null
                        : Accounts.GetMetadata(creator.Id);

                    var manager = contract.ManagerId == null ? null
                        : (RawUser)await Accounts.GetAsync((int)contract.ManagerId);

                    var managerMetadata = manager == null ? null
                        : Accounts.GetMetadata(manager.Id);

                    var contractDelegate = contract.DelegateId == null ? null
                        : await Accounts.GetAsync((int)contract.DelegateId);

                    var contractDelegateMetadata = contractDelegate == null ? null
                        : Accounts.GetMetadata(contractDelegate.Id);

                    return new Contract
                    {
                        Alias = accMetadata?.Alias,
                        Address = contract.Address,
                        Kind = KindToString(contract.Kind),
                        Balance = contract.Balance,
                        Creator = creator == null ? null
                            : new CreatorInfo
                            {
                                Alias = creatorMetadata?.Alias,
                                Address = creator.Address
                            },
                        Manager = manager == null ? null
                            : new ManagerInfo
                            {
                                Alias = managerMetadata?.Alias,
                                Address = manager.Address,
                                PublicKey = manager.PublicKey,
                            },
                        Delegate = contractDelegate == null ? null
                            : new DelegateInfo
                            {
                                Alias = contractDelegateMetadata?.Alias,
                                Address = contractDelegate.Address,
                                Active = contractDelegate.Staked
                            },
                        DelegationLevel = contractDelegate == null ? null
                            : contract.DelegationLevel,
                        DelegationTime = contractDelegate == null ? null
                            : (DateTime?)Time[(int)contract.DelegationLevel],
                        FirstActivity = contract.FirstLevel,
                        FirstActivityTime = Time[contract.FirstLevel],
                        LastActivity = contract.LastLevel,
                        LastActivityTime = Time[contract.LastLevel],
                        NumContracts = contract.ContractsCount,
                        NumDelegations = contract.DelegationsCount,
                        NumOriginations = contract.OriginationsCount,
                        NumReveals = contract.RevealsCount,
                        NumMigrations = contract.MigrationsCount,
                        NumTransactions = contract.TransactionsCount,
                        Metadata = metadata ? accMetadata : null
                    };
                    #endregion
                default:
                    throw new Exception($"Invalid raw account type");
            }
        }

        public async Task<Models.Delegate> GetDelegate(string address)
        {
            var rawAccount = await Accounts.GetAsync(address);

            if (!(rawAccount is RawDelegate delegat))
                return null;

            var metadata = Accounts.GetMetadata(delegat.Id);

            return new Models.Delegate
            {
                Alias = metadata?.Alias,
                Active = delegat.Staked,
                Address = delegat.Address,
                PublicKey = delegat.PublicKey,
                Revealed = delegat.Revealed,
                Balance = delegat.Balance,
                FrozenDeposits = delegat.FrozenDeposits,
                FrozenRewards = delegat.FrozenRewards,
                FrozenFees = delegat.FrozenFees,
                Counter = delegat.Counter,
                ActivationLevel = delegat.ActivationLevel,
                ActivationTime = Time[delegat.ActivationLevel],
                DeactivationLevel = delegat.Staked ? null : (int?)delegat.DeactivationLevel,
                DeactivationTime = delegat.Staked ? null : (DateTime?)Time[delegat.DeactivationLevel],
                StakingBalance = delegat.StakingBalance,
                FirstActivity = delegat.FirstLevel,
                FirstActivityTime = Time[delegat.FirstLevel],
                LastActivity = delegat.LastLevel,
                LastActivityTime = Time[delegat.LastLevel],
                NumActivations = delegat.Activated == true ? 1 : 0,
                NumBallots = delegat.BallotsCount,
                NumContracts = delegat.ContractsCount,
                NumDelegators = delegat.DelegatorsCount,
                NumBlocks = delegat.BlocksCount,
                NumDelegations = delegat.DelegationsCount,
                NumDoubleBaking = delegat.DoubleBakingCount,
                NumDoubleEndorsing = delegat.DoubleEndorsingCount,
                NumEndorsements = delegat.EndorsementsCount,
                NumNonceRevelations = delegat.NonceRevelationsCount,
                NumRevelationPenalties = delegat.RevelationPenaltiesCount,
                NumOriginations = delegat.OriginationsCount,
                NumProposals = delegat.ProposalsCount,
                NumReveals = delegat.RevealsCount,
                NumMigrations = delegat.MigrationsCount,
                NumTransactions = delegat.TransactionsCount,
                Software = delegat.SoftwareId == null ? null : Software[(int)delegat.SoftwareId]
            };
        }

        public async Task<Contract> GetContract(string address)
        {
            var rawAccount = await Accounts.GetAsync(address);

            if (!(rawAccount is RawContract contract))
                return null;

            var metadata = Accounts.GetMetadata(contract.Id);

            var creator = contract.CreatorId == null ? null
                        : await Accounts.GetAsync((int)contract.CreatorId);

            var creatorMetadata = creator == null ? null
                : Accounts.GetMetadata(creator.Id);

            var manager = contract.ManagerId == null ? null
                : (RawUser)await Accounts.GetAsync((int)contract.ManagerId);

            var managerMetadata = manager == null ? null
                : Accounts.GetMetadata(manager.Id);

            var contractDelegate = contract.DelegateId == null ? null
                : await Accounts.GetAsync((int)contract.DelegateId);

            var contractDelegateMetadata = contractDelegate == null ? null
                : Accounts.GetMetadata(contractDelegate.Id);

            return new Contract
            {
                Alias = metadata?.Alias,
                Address = contract.Address,
                Kind = KindToString(contract.Kind),
                Balance = contract.Balance,
                Creator = creator == null ? null
                    : new CreatorInfo
                    {
                        Alias = creatorMetadata?.Alias,
                        Address = creator.Address
                    },
                Manager = manager == null ? null
                    : new ManagerInfo
                    {
                        Alias = managerMetadata?.Alias,
                        Address = manager.Address,
                        PublicKey = manager.PublicKey,
                    },
                Delegate = contractDelegate == null ? null
                    : new DelegateInfo
                    {
                        Alias = contractDelegateMetadata?.Alias,
                        Address = contractDelegate.Address,
                        Active = contractDelegate.Staked
                    },
                DelegationLevel = contractDelegate == null ? null
                    : contract.DelegationLevel,
                DelegationTime = contractDelegate == null ? null
                    : (DateTime?)Time[(int)contract.DelegationLevel],
                FirstActivity = contract.FirstLevel,
                FirstActivityTime = Time[contract.FirstLevel],
                LastActivity = contract.LastLevel,
                LastActivityTime = Time[contract.LastLevel],
                NumContracts = contract.ContractsCount,
                NumDelegations = contract.DelegationsCount,
                NumOriginations = contract.OriginationsCount,
                NumReveals = contract.RevealsCount,
                NumMigrations = contract.MigrationsCount,
                NumTransactions = contract.TransactionsCount
            };
        }

        #region accounts
        public async Task<int> GetCount(AccountTypeParameter type, ContractKindParameter kind, Int64Parameter balance, BoolParameter staked)
        {
            var sql = new SqlBuilder(@"SELECT COUNT(*) FROM ""Accounts""")
                .Filter("Type", type)
                .Filter("Kind", kind)
                .Filter("Balance", balance)
                .Filter("Staked", staked);

            using var db = GetConnection();
            return await db.QueryFirstAsync<int>(sql.Query, sql.Params);
        }

        public async Task<IEnumerable<Account>> Get(
            AccountTypeParameter type,
            ContractKindParameter kind,
            Int64Parameter balance,
            BoolParameter staked,
            SortParameter sort,
            OffsetParameter offset,
            int limit)
        {
            var sql = new SqlBuilder(@"SELECT * FROM ""Accounts""")
                .Filter("Type", type)
                .Filter("Kind", kind)
                .Filter("Balance", balance)
                .Filter("Staked", staked)
                .Take(sort, offset, limit, x => x switch
                {
                    "balance" => ("Balance", "Balance"),
                    "firstActivity" => ("FirstLevel", "FirstLevel"),
                    "lastActivity" => ("LastLevel", "LastLevel"),
                    "numTransactions" => ("TransactionsCount", "TransactionsCount"),
                    "numContracts" => ("ContractsCount", "ContractsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            var accounts = new List<Account>(rows.Count());
            foreach (var row in rows)
            {
                var metadata = Accounts.GetMetadata((int)row.Id);

                switch ((int)row.Type)
                {
                    case 0:
                        #region build user
                        var userDelegate = row.DelegateId == null ? null
                            : await Accounts.GetAsync((int)row.DelegateId);

                        var userDelegateMetadata = userDelegate == null ? null
                            : Accounts.GetMetadata(userDelegate.Id);

                        accounts.Add(new User
                        {
                            Alias = metadata?.Alias,
                            Address = row.Address,
                            Balance = row.Balance,
                            Counter = row.Balance > 0 ? row.Counter : State.GetCounter(),
                            FirstActivity = row.FirstLevel,
                            FirstActivityTime = Time[row.FirstLevel],
                            LastActivity = row.LastLevel,
                            LastActivityTime = Time[row.LastLevel],
                            PublicKey = row.PublicKey,
                            Revealed = row.Revealed,
                            Delegate = userDelegate == null ? null
                            : new DelegateInfo
                            {
                                Alias = userDelegateMetadata?.Alias,
                                Address = userDelegate.Address,
                                Active = userDelegate.Staked
                            },
                            DelegationLevel = userDelegate == null ? null
                                : row.DelegationLevel,
                            DelegationTime = userDelegate == null ? null
                                : (DateTime?)Time[row.DelegationLevel],
                            NumActivations = row.Activated == true ? 1 : 0,
                            NumContracts = row.ContractsCount,
                            NumDelegations = row.DelegationsCount,
                            NumOriginations = row.OriginationsCount,
                            NumReveals = row.RevealsCount,
                            NumMigrations = row.MigrationsCount,
                            NumTransactions = row.TransactionsCount
                        });
                        #endregion
                        break;
                    case 1:
                        #region build delegate
                        accounts.Add(new Models.Delegate
                        {
                            Alias = metadata?.Alias,
                            Active = row.Staked,
                            Address = row.Address,
                            PublicKey = row.PublicKey,
                            Revealed = row.Revealed,
                            Balance = row.Balance,
                            FrozenDeposits = row.FrozenDeposits,
                            FrozenRewards = row.FrozenRewards,
                            FrozenFees = row.FrozenFees,
                            Counter = row.Counter,
                            ActivationLevel = row.ActivationLevel,
                            ActivationTime = Time[row.ActivationLevel],
                            DeactivationLevel = row.Staked ? null : (int?)row.DeactivationLevel,
                            DeactivationTime = row.Staked ? null : (DateTime?)Time[row.DeactivationLevel],
                            StakingBalance = row.StakingBalance,
                            FirstActivity = row.FirstLevel,
                            FirstActivityTime = Time[row.FirstLevel],
                            LastActivity = row.LastLevel,
                            LastActivityTime = Time[row.LastLevel],
                            NumActivations = row.Activated == true ? 1 : 0,
                            NumBallots = row.BallotsCount,
                            NumContracts = row.ContractsCount,
                            NumDelegators = row.DelegatorsCount,
                            NumBlocks = row.BlocksCount,
                            NumDelegations = row.DelegationsCount,
                            NumDoubleBaking = row.DoubleBakingCount,
                            NumDoubleEndorsing = row.DoubleEndorsingCount,
                            NumEndorsements = row.EndorsementsCount,
                            NumNonceRevelations = row.NonceRevelationsCount,
                            NumRevelationPenalties = row.RevelationPenaltiesCount,
                            NumOriginations = row.OriginationsCount,
                            NumProposals = row.ProposalsCount,
                            NumReveals = row.RevealsCount,
                            NumMigrations = row.MigrationsCount,
                            NumTransactions = row.TransactionsCount,
                            Software = row.SoftwareId == null ? null : Software[row.SoftwareId]
                        });
                        #endregion
                        break;
                    case 2:
                        #region build contract
                        var creator = row.CreatorId == null ? null
                            : await Accounts.GetAsync((int)row.CreatorId);

                        var creatorMetadata = creator == null ? null
                            : Accounts.GetMetadata(creator.Id);

                        var manager = row.ManagerId == null ? null
                            : (RawUser)await Accounts.GetAsync((int)row.ManagerId);

                        var managerMetadata = manager == null ? null
                            : Accounts.GetMetadata(manager.Id);

                        var contractDelegate = row.DelegateId == null ? null
                            : await Accounts.GetAsync((int)row.DelegateId);

                        var contractDelegateMetadata = contractDelegate == null ? null
                            : Accounts.GetMetadata(contractDelegate.Id);

                        accounts.Add(new Contract
                        {
                            Alias = metadata?.Alias,
                            Address = row.Address,
                            Kind = KindToString(row.Kind),
                            Balance = row.Balance,
                            Creator = creator == null ? null
                            : new CreatorInfo
                            {
                                Alias = creatorMetadata?.Alias,
                                Address = creator.Address
                            },
                            Manager = manager == null ? null
                            : new ManagerInfo
                            {
                                Alias = managerMetadata?.Alias,
                                Address = manager.Address,
                                PublicKey = manager.PublicKey,
                            },
                            Delegate = contractDelegate == null ? null
                            : new DelegateInfo
                            {
                                Alias = contractDelegateMetadata?.Alias,
                                Address = contractDelegate.Address,
                                Active = contractDelegate.Staked
                            },
                            DelegationLevel = contractDelegate == null ? null
                                : row.DelegationLevel,
                            DelegationTime = contractDelegate == null ? null
                                : (DateTime?)Time[row.DelegationLevel],
                            FirstActivity = row.FirstLevel,
                            FirstActivityTime = Time[row.FirstLevel],
                            LastActivity = row.LastLevel,
                            LastActivityTime = Time[row.LastLevel],
                            NumContracts = row.ContractsCount,
                            NumDelegations = row.DelegationsCount,
                            NumOriginations = row.OriginationsCount,
                            NumReveals = row.RevealsCount,
                            NumMigrations = row.MigrationsCount,
                            NumTransactions = row.TransactionsCount
                        });
                        #endregion
                        break;
                }
            }

            return accounts;
        }

        public async Task<object[][]> Get(
            AccountTypeParameter type,
            ContractKindParameter kind,
            Int64Parameter balance,
            BoolParameter staked,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            string[] fields)
        {
            var columns = new HashSet<string>(fields.Length + 2);
            foreach (var field in fields)
            {
                switch (field)
                {
                    case "alias": columns.Add(@"""Id"""); break;
                    case "type": columns.Add(@"""Type"""); break;
                    case "active": columns.Add(@"""Staked"""); break;
                    case "address": columns.Add(@"""Address"""); break;
                    case "publicKey": columns.Add(@"""PublicKey"""); break;
                    case "revealed": columns.Add(@"""Revealed"""); break;
                    case "balance": columns.Add(@"""Balance"""); break;
                    case "frozenDeposits": columns.Add(@"""FrozenDeposits"""); break;
                    case "frozenRewards": columns.Add(@"""FrozenRewards"""); break;
                    case "frozenFees": columns.Add(@"""FrozenFees"""); break;
                    case "counter": columns.Add(@"""Counter"""); break;
                    case "activationLevel": columns.Add(@"""ActivationLevel"""); break;
                    case "activationTime": columns.Add(@"""ActivationLevel"""); break;
                    case "deactivationLevel": columns.Add(@"""DeactivationLevel"""); columns.Add(@"""Staked"""); break;
                    case "deactivationTime": columns.Add(@"""DeactivationLevel"""); columns.Add(@"""Staked"""); break;
                    case "stakingBalance": columns.Add(@"""StakingBalance"""); break;
                    case "firstActivity": columns.Add(@"""FirstLevel"""); break;
                    case "firstActivityTime": columns.Add(@"""FirstLevel"""); break;
                    case "lastActivity": columns.Add(@"""LastLevel"""); break;
                    case "lastActivityTime": columns.Add(@"""LastLevel"""); break;
                    case "numActivations": columns.Add(@"""Activated"""); break;
                    case "numBallots": columns.Add(@"""BallotsCount"""); break;
                    case "numContracts": columns.Add(@"""ContractsCount"""); break;
                    case "numDelegators": columns.Add(@"""DelegatorsCount"""); break;
                    case "numBlocks": columns.Add(@"""BlocksCount"""); break;
                    case "numDelegations": columns.Add(@"""DelegationsCount"""); break;
                    case "numDoubleBaking": columns.Add(@"""DoubleBakingCount"""); break;
                    case "numDoubleEndorsing": columns.Add(@"""DoubleEndorsingCount"""); break;
                    case "numEndorsements": columns.Add(@"""EndorsementsCount"""); break;
                    case "numNonceRevelations": columns.Add(@"""NonceRevelationsCount"""); break;
                    case "numRevelationPenalties": columns.Add(@"""RevelationPenaltiesCount"""); break;
                    case "numOriginations": columns.Add(@"""OriginationsCount"""); break;
                    case "numProposals": columns.Add(@"""ProposalsCount"""); break;
                    case "numReveals": columns.Add(@"""RevealsCount"""); break;
                    case "numMigrations": columns.Add(@"""MigrationsCount"""); break;
                    case "numTransactions": columns.Add(@"""TransactionsCount"""); break;
                    case "software": columns.Add(@"""SoftwareId"""); break;

                    case "delegate": columns.Add(@"""DelegateId"""); break;
                    case "delegationLevel": columns.Add(@"""DelegationLevel"""); columns.Add(@"""DelegateId"""); break;
                    case "delegationTime": columns.Add(@"""DelegationLevel"""); columns.Add(@"""DelegateId"""); break;

                    case "kind": columns.Add(@"""Kind"""); break;
                    case "creator": columns.Add(@"""CreatorId"""); break;
                    case "manager": columns.Add(@"""ManagerId"""); break;
                }
            }

            if (columns.Count == 0)
                return Array.Empty<object[]>();

            var sql = new SqlBuilder($@"SELECT {string.Join(',', columns)} FROM ""Accounts""")
                .Filter("Type", type)
                .Filter("Kind", kind)
                .Filter("Balance", balance)
                .Filter("Staked", staked)
                .Take(sort, offset, limit, x => x switch
                {
                    "balance" => ("Balance", "Balance"),
                    "firstActivity" => ("FirstLevel", "FirstLevel"),
                    "lastActivity" => ("LastLevel", "LastLevel"),
                    "numTransactions" => ("TransactionsCount", "TransactionsCount"),
                    "numContracts" => ("ContractsCount", "ContractsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            var result = new object[rows.Count()][];
            for (int i = 0; i < result.Length; i++)
                result[i] = new object[fields.Length];

            for (int i = 0, j = 0; i < fields.Length; j = 0, i++)
            {
                switch (fields[i])
                {
                    case "alias":
                        foreach (var row in rows)
                        {
                            var metadata = Accounts.GetMetadata((int)row.Id);
                            result[j++][i] = metadata?.Alias;
                        }
                        break;
                    case "type":
                        foreach (var row in rows)
                            result[j++][i] = TypeToString(row.Type);
                        break;
                    case "active":
                        foreach (var row in rows)
                            result[j++][i] = row.Staked;
                        break;
                    case "address":
                        foreach (var row in rows)
                            result[j++][i] = row.Address;
                        break;
                    case "publicKey":
                        foreach (var row in rows)
                            result[j++][i] = row.PublicKey;
                        break;
                    case "revealed":
                        foreach (var row in rows)
                            result[j++][i] = row.Revealed;
                        break;
                    case "balance":
                        foreach (var row in rows)
                            result[j++][i] = row.Balance;
                        break;
                    case "frozenDeposits":
                        foreach (var row in rows)
                            result[j++][i] = row.FrozenDeposits;
                        break;
                    case "frozenRewards":
                        foreach (var row in rows)
                            result[j++][i] = row.FrozenRewards;
                        break;
                    case "frozenFees":
                        foreach (var row in rows)
                            result[j++][i] = row.FrozenFees;
                        break;
                    case "counter":
                        foreach (var row in rows)
                            result[j++][i] = row.Counter;
                        break;
                    case "activationLevel":
                        foreach (var row in rows)
                            result[j++][i] = row.ActivationLevel;
                        break;
                    case "activationTime":
                        foreach (var row in rows)
                            result[j++][i] = row.ActivationLevel == null ? null : Time[row.ActivationLevel];
                        break;
                    case "deactivationLevel":
                        foreach (var row in rows)
                            result[j++][i] = row.Staked ? null : (int?)row.DeactivationLevel;
                        break;
                    case "deactivationTime":
                        foreach (var row in rows)
                            result[j++][i] = row.Staked ? null : (DateTime?)Time[row.DeactivationLevel];
                        break;
                    case "stakingBalance":
                        foreach (var row in rows)
                            result[j++][i] = row.StakingBalance;
                        break;
                    case "firstActivity":
                        foreach (var row in rows)
                            result[j++][i] = row.FirstLevel;
                        break;
                    case "firstActivityTime":
                        foreach (var row in rows)
                            result[j++][i] = Time[row.FirstLevel];
                        break;
                    case "lastActivity":
                        foreach (var row in rows)
                            result[j++][i] = row.LastLevel;
                        break;
                    case "lastActivityTime":
                        foreach (var row in rows)
                            result[j++][i] = Time[row.LastLevel];
                        break;
                    case "numActivations":
                        foreach (var row in rows)
                            result[j++][i] = row.Activated == true ? 1 : 0;
                        break;
                    case "numBallots":
                        foreach (var row in rows)
                            result[j++][i] = row.BallotsCount;
                        break;
                    case "numContracts":
                        foreach (var row in rows)
                            result[j++][i] = row.ContractsCount;
                        break;
                    case "numDelegators":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegatorsCount;
                        break;
                    case "numBlocks":
                        foreach (var row in rows)
                            result[j++][i] = row.BlocksCount;
                        break;
                    case "numDelegations":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegationsCount;
                        break;
                    case "numDoubleBaking":
                        foreach (var row in rows)
                            result[j++][i] = row.DoubleBakingCount;
                        break;
                    case "numDoubleEndorsing":
                        foreach (var row in rows)
                            result[j++][i] = row.DoubleEndorsingCount;
                        break;
                    case "numEndorsements":
                        foreach (var row in rows)
                            result[j++][i] = row.EndorsementsCount;
                        break;
                    case "numNonceRevelations":
                        foreach (var row in rows)
                            result[j++][i] = row.NonceRevelationsCount;
                        break;
                    case "numRevelationPenalties":
                        foreach (var row in rows)
                            result[j++][i] = row.RevelationPenaltiesCount;
                        break;
                    case "numOriginations":
                        foreach (var row in rows)
                            result[j++][i] = row.OriginationsCount;
                        break;
                    case "numProposals":
                        foreach (var row in rows)
                            result[j++][i] = row.ProposalsCount;
                        break;
                    case "numReveals":
                        foreach (var row in rows)
                            result[j++][i] = row.RevealsCount;
                        break;
                    case "numMigrations":
                        foreach (var row in rows)
                            result[j++][i] = row.MigrationsCount;
                        break;
                    case "numTransactions":
                        foreach (var row in rows)
                            result[j++][i] = row.TransactionsCount;
                        break;
                    case "software":
                        foreach (var row in rows)
                            result[j++][i] = row.SoftwareId != null ? Software[row.SoftwareId] : null;
                        break;
                    case "delegate":
                        foreach (var row in rows)
                        {
                            var delegat = row.DelegateId == null ? null : Accounts.Get((int)row.DelegateId);
                            var delegatMetadata = delegat == null ? null : Accounts.GetMetadata(delegat.Id);
                            result[j++][i] = delegat == null ? null : new DelegateInfo
                            {
                                Alias = delegatMetadata?.Alias,
                                Address = delegat.Address,
                                Active = delegat.Staked
                            };
                        }
                        break;
                    case "delegationLevel":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegateId == null ? null : row.DelegationLevel;
                        break;
                    case "delegationTime":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegateId == null ? null : Time[row.DelegationLevel];
                        break;
                    case "kind":
                        foreach (var row in rows)
                            result[j++][i] = row.Kind == null ? null : KindToString(row.Kind);
                        break;
                    case "creator":
                        foreach (var row in rows)
                        {
                            var creator = row.CreatorId == null ? null : Accounts.Get((int)row.CreatorId);
                            var creatorMetadata = creator == null ? null : Accounts.GetMetadata(creator.Id);
                            result[j++][i] = creator == null ? null : new CreatorInfo
                            {
                                Alias = creatorMetadata?.Alias,
                                Address = creator.Address
                            };
                        }
                        break;
                    case "manager":
                        foreach (var row in rows)
                        {
                            var manager = row.ManagerId == null ? null : (RawUser)Accounts.Get((int)row.ManagerId);
                            var managerMetadata = manager == null ? null : Accounts.GetMetadata(manager.Id);
                            result[j++][i] = manager == null ? null : new ManagerInfo
                            {
                                Alias = managerMetadata?.Alias,
                                Address = manager.Address,
                                PublicKey = manager.PublicKey,
                            };
                        }
                        break;
                }
            }

            return result;
        }

        public async Task<object[]> Get(
            AccountTypeParameter type,
            ContractKindParameter kind,
            Int64Parameter balance,
            BoolParameter staked,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            string field)
        {
            var columns = new HashSet<string>(3);
            switch (field)
            {
                case "alias": columns.Add(@"""Id"""); break;
                case "type": columns.Add(@"""Type"""); break;
                case "active": columns.Add(@"""Staked"""); break;
                case "address": columns.Add(@"""Address"""); break;
                case "publicKey": columns.Add(@"""PublicKey"""); break;
                case "revealed": columns.Add(@"""Revealed"""); break;
                case "balance": columns.Add(@"""Balance"""); break;
                case "frozenDeposits": columns.Add(@"""FrozenDeposits"""); break;
                case "frozenRewards": columns.Add(@"""FrozenRewards"""); break;
                case "frozenFees": columns.Add(@"""FrozenFees"""); break;
                case "counter": columns.Add(@"""Counter"""); break;
                case "activationLevel": columns.Add(@"""ActivationLevel"""); break;
                case "activationTime": columns.Add(@"""ActivationLevel"""); break;
                case "deactivationLevel": columns.Add(@"""DeactivationLevel"""); columns.Add(@"""Staked"""); break;
                case "deactivationTime": columns.Add(@"""DeactivationLevel"""); columns.Add(@"""Staked"""); break;
                case "stakingBalance": columns.Add(@"""StakingBalance"""); break;
                case "firstActivity": columns.Add(@"""FirstLevel"""); break;
                case "firstActivityTime": columns.Add(@"""FirstLevel"""); break;
                case "lastActivity": columns.Add(@"""LastLevel"""); break;
                case "lastActivityTime": columns.Add(@"""LastLevel"""); break;
                case "numActivations": columns.Add(@"""Activated"""); break;
                case "numBallots": columns.Add(@"""BallotsCount"""); break;
                case "numContracts": columns.Add(@"""ContractsCount"""); break;
                case "numDelegators": columns.Add(@"""DelegatorsCount"""); break;
                case "numBlocks": columns.Add(@"""BlocksCount"""); break;
                case "numDelegations": columns.Add(@"""DelegationsCount"""); break;
                case "numDoubleBaking": columns.Add(@"""DoubleBakingCount"""); break;
                case "numDoubleEndorsing": columns.Add(@"""DoubleEndorsingCount"""); break;
                case "numEndorsements": columns.Add(@"""EndorsementsCount"""); break;
                case "numNonceRevelations": columns.Add(@"""NonceRevelationsCount"""); break;
                case "numRevelationPenalties": columns.Add(@"""RevelationPenaltiesCount"""); break;
                case "numOriginations": columns.Add(@"""OriginationsCount"""); break;
                case "numProposals": columns.Add(@"""ProposalsCount"""); break;
                case "numReveals": columns.Add(@"""RevealsCount"""); break;
                case "numMigrations": columns.Add(@"""MigrationsCount"""); break;
                case "numTransactions": columns.Add(@"""TransactionsCount"""); break;
                case "software": columns.Add(@"""SoftwareId"""); break;

                case "delegate": columns.Add(@"""DelegateId"""); break;
                case "delegationLevel": columns.Add(@"""DelegationLevel"""); columns.Add(@"""DelegateId"""); break;
                case "delegationTime": columns.Add(@"""DelegationLevel"""); columns.Add(@"""DelegateId"""); break;

                case "kind": columns.Add(@"""Kind"""); break;
                case "creator": columns.Add(@"""CreatorId"""); break;
                case "manager": columns.Add(@"""ManagerId"""); break;
            }

            if (columns.Count == 0)
                return Array.Empty<object>();

            var sql = new SqlBuilder($@"SELECT {string.Join(',', columns)} FROM ""Accounts""")
                .Filter("Type", type)
                .Filter("Kind", kind)
                .Filter("Balance", balance)
                .Filter("Staked", staked)
                .Take(sort, offset, limit, x => x switch
                {
                    "balance" => ("Balance", "Balance"),
                    "firstActivity" => ("FirstLevel", "FirstLevel"),
                    "lastActivity" => ("LastLevel", "LastLevel"),
                    "numTransactions" => ("TransactionsCount", "TransactionsCount"),
                    "numContracts" => ("ContractsCount", "ContractsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            var result = new object[rows.Count()];
            var j = 0;

            switch (field)
            {
                case "alias":
                    foreach (var row in rows)
                    {
                        var metadata = Accounts.GetMetadata((int)row.Id);
                        result[j++] = metadata?.Alias;
                    }
                    break;
                case "type":
                    foreach (var row in rows)
                        result[j++] = TypeToString(row.Type);
                    break;
                case "active":
                    foreach (var row in rows)
                        result[j++] = row.Staked;
                    break;
                case "address":
                    foreach (var row in rows)
                        result[j++] = row.Address;
                    break;
                case "publicKey":
                    foreach (var row in rows)
                        result[j++] = row.PublicKey;
                    break;
                case "revealed":
                    foreach (var row in rows)
                        result[j++] = row.Revealed;
                    break;
                case "balance":
                    foreach (var row in rows)
                        result[j++] = row.Balance;
                    break;
                case "frozenDeposits":
                    foreach (var row in rows)
                        result[j++] = row.FrozenDeposits;
                    break;
                case "frozenRewards":
                    foreach (var row in rows)
                        result[j++] = row.FrozenRewards;
                    break;
                case "frozenFees":
                    foreach (var row in rows)
                        result[j++] = row.FrozenFees;
                    break;
                case "counter":
                    foreach (var row in rows)
                        result[j++] = row.Counter;
                    break;
                case "activationLevel":
                    foreach (var row in rows)
                        result[j++] = row.ActivationLevel;
                    break;
                case "activationTime":
                    foreach (var row in rows)
                        result[j++] = row.ActivationLevel == null ? null : Time[row.ActivationLevel];
                    break;
                case "deactivationLevel":
                    foreach (var row in rows)
                        result[j++] = row.Staked ? null : (int?)row.DeactivationLevel;
                    break;
                case "deactivationTime":
                    foreach (var row in rows)
                        result[j++] = row.Staked ? null : (DateTime?)Time[row.DeactivationLevel];
                    break;
                case "stakingBalance":
                    foreach (var row in rows)
                        result[j++] = row.StakingBalance;
                    break;
                case "firstActivity":
                    foreach (var row in rows)
                        result[j++] = row.FirstLevel;
                    break;
                case "firstActivityTime":
                    foreach (var row in rows)
                        result[j++] = Time[row.FirstLevel];
                    break;
                case "lastActivity":
                    foreach (var row in rows)
                        result[j++] = row.LastLevel;
                    break;
                case "lastActivityTime":
                    foreach (var row in rows)
                        result[j++] = Time[row.LastLevel];
                    break;
                case "numActivations":
                    foreach (var row in rows)
                        result[j++] = row.Activated == true ? 1 : 0;
                    break;
                case "numBallots":
                    foreach (var row in rows)
                        result[j++] = row.BallotsCount;
                    break;
                case "numContracts":
                    foreach (var row in rows)
                        result[j++] = row.ContractsCount;
                    break;
                case "numDelegators":
                    foreach (var row in rows)
                        result[j++] = row.DelegatorsCount;
                    break;
                case "numBlocks":
                    foreach (var row in rows)
                        result[j++] = row.BlocksCount;
                    break;
                case "numDelegations":
                    foreach (var row in rows)
                        result[j++] = row.DelegationsCount;
                    break;
                case "numDoubleBaking":
                    foreach (var row in rows)
                        result[j++] = row.DoubleBakingCount;
                    break;
                case "numDoubleEndorsing":
                    foreach (var row in rows)
                        result[j++] = row.DoubleEndorsingCount;
                    break;
                case "numEndorsements":
                    foreach (var row in rows)
                        result[j++] = row.EndorsementsCount;
                    break;
                case "numNonceRevelations":
                    foreach (var row in rows)
                        result[j++] = row.NonceRevelationsCount;
                    break;
                case "numRevelationPenalties":
                    foreach (var row in rows)
                        result[j++] = row.RevelationPenaltiesCount;
                    break;
                case "numOriginations":
                    foreach (var row in rows)
                        result[j++] = row.OriginationsCount;
                    break;
                case "numProposals":
                    foreach (var row in rows)
                        result[j++] = row.ProposalsCount;
                    break;
                case "numReveals":
                    foreach (var row in rows)
                        result[j++] = row.RevealsCount;
                    break;
                case "numMigrations":
                    foreach (var row in rows)
                        result[j++] = row.MigrationsCount;
                    break;
                case "numTransactions":
                    foreach (var row in rows)
                        result[j++] = row.TransactionsCount;
                    break;
                case "software":
                    foreach (var row in rows)
                        result[j++] = row.SoftwareId != null ? Software[row.SoftwareId] : null;
                    break;
                case "delegate":
                    foreach (var row in rows)
                    {
                        var delegat = row.DelegateId == null ? null : Accounts.Get((int)row.DelegateId);
                        var delegatMetadata = delegat == null ? null : Accounts.GetMetadata(delegat.Id);
                        result[j++] = delegat == null ? null : new DelegateInfo
                        {
                            Alias = delegatMetadata?.Alias,
                            Address = delegat.Address,
                            Active = delegat.Staked
                        };
                    }
                    break;
                case "delegationLevel":
                    foreach (var row in rows)
                        result[j++] = row.DelegateId == null ? null : row.DelegationLevel;
                    break;
                case "delegationTime":
                    foreach (var row in rows)
                        result[j++] = row.DelegateId == null ? null : Time[row.DelegationLevel];
                    break;
                case "kind":
                    foreach (var row in rows)
                        result[j++] = row.Kind == null ? null : KindToString(row.Kind);
                    break;
                case "creator":
                    foreach (var row in rows)
                    {
                        var creator = row.CreatorId == null ? null : Accounts.Get((int)row.CreatorId);
                        var creatorMetadata = creator == null ? null : Accounts.GetMetadata(creator.Id);
                        result[j++] = creator == null ? null : new CreatorInfo
                        {
                            Alias = creatorMetadata?.Alias,
                            Address = creator.Address
                        };
                    }
                    break;
                case "manager":
                    foreach (var row in rows)
                    {
                        var manager = row.ManagerId == null ? null : (RawUser)Accounts.Get((int)row.ManagerId);
                        var managerMetadata = manager == null ? null : Accounts.GetMetadata(manager.Id);
                        result[j++] = manager == null ? null : new ManagerInfo
                        {
                            Alias = managerMetadata?.Alias,
                            Address = manager.Address,
                            PublicKey = manager.PublicKey,
                        };
                    }
                    break;
            }

            return result;
        }
        #endregion

        #region delegates
        public async Task<int> GetDelegatesCount(BoolParameter active)
        {
            var sql = new SqlBuilder(@"SELECT COUNT(*) FROM ""Accounts""")
                .Filter("Type", 1)
                .Filter("Staked", active);

            using var db = GetConnection();
            return await db.QueryFirstAsync<int>(sql.Query, sql.Params);
        }

        public async Task<IEnumerable<Models.Delegate>> GetDelegates(
            BoolParameter active,
            SortParameter sort,
            OffsetParameter offset,
            int limit)
        {
            var sql = new SqlBuilder(@"SELECT * FROM ""Accounts""")
                .Filter("Type", 1)
                .Filter("Staked", active)
                .Take(sort, offset, limit, x => x switch
                {
                    "activationLevel" => ("ActivationLevel", "ActivationLevel"),
                    "deactivationLevel" => ("DeactivationLevel", "DeactivationLevel"),
                    "stakingBalance" => ("StakingBalance", "StakingBalance"),
                    "balance" => ("Balance", "Balance"),
                    "numDelegators" => ("DelegatorsCount", "DelegatorsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            return rows.Select(row =>
            {
                var metadata = Accounts.GetMetadata((int)row.Id);
                return new Models.Delegate
                {
                    Alias = metadata?.Alias,
                    Active = row.Staked,
                    Address = row.Address,
                    PublicKey = row.PublicKey,
                    Revealed = row.Revealed,
                    Balance = row.Balance,
                    FrozenDeposits = row.FrozenDeposits,
                    FrozenRewards = row.FrozenRewards,
                    FrozenFees = row.FrozenFees,
                    Counter = row.Counter,
                    ActivationLevel = row.ActivationLevel,
                    ActivationTime = Time[row.ActivationLevel],
                    DeactivationLevel = row.Staked ? null : (int?)row.DeactivationLevel,
                    DeactivationTime = row.Staked ? null : (DateTime?)Time[row.DeactivationLevel],
                    StakingBalance = row.StakingBalance,
                    FirstActivity = row.FirstLevel,
                    FirstActivityTime = Time[row.FirstLevel],
                    LastActivity = row.LastLevel,
                    LastActivityTime = Time[row.LastLevel],
                    NumActivations = row.Activated == true ? 1 : 0,
                    NumBallots = row.BallotsCount,
                    NumContracts = row.ContractsCount,
                    NumDelegators = row.DelegatorsCount,
                    NumBlocks = row.BlocksCount,
                    NumDelegations = row.DelegationsCount,
                    NumDoubleBaking = row.DoubleBakingCount,
                    NumDoubleEndorsing = row.DoubleEndorsingCount,
                    NumEndorsements = row.EndorsementsCount,
                    NumNonceRevelations = row.NonceRevelationsCount,
                    NumRevelationPenalties = row.RevelationPenaltiesCount,
                    NumOriginations = row.OriginationsCount,
                    NumProposals = row.ProposalsCount,
                    NumReveals = row.RevealsCount,
                    NumMigrations = row.MigrationsCount,
                    NumTransactions = row.TransactionsCount,
                    Software = row.SoftwareId == null ? null : Software[row.SoftwareId]
                };
            });
        }

        public async Task<object[][]> GetDelegates(
            BoolParameter active,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            string[] fields)
        {
            var columns = new HashSet<string>(fields.Length + 2);
            foreach (var field in fields)
            {
                switch (field)
                {
                    case "alias": columns.Add(@"""Id"""); break;
                    case "type": columns.Add(@"""Type"""); break;
                    case "active": columns.Add(@"""Staked"""); break;
                    case "address": columns.Add(@"""Address"""); break;
                    case "publicKey": columns.Add(@"""PublicKey"""); break;
                    case "revealed": columns.Add(@"""Revealed"""); break;
                    case "balance": columns.Add(@"""Balance"""); break;
                    case "frozenDeposits": columns.Add(@"""FrozenDeposits"""); break;
                    case "frozenRewards": columns.Add(@"""FrozenRewards"""); break;
                    case "frozenFees": columns.Add(@"""FrozenFees"""); break;
                    case "counter": columns.Add(@"""Counter"""); break;
                    case "activationLevel": columns.Add(@"""ActivationLevel"""); break;
                    case "activationTime": columns.Add(@"""ActivationLevel"""); break;
                    case "deactivationLevel": columns.Add(@"""DeactivationLevel"""); columns.Add(@"""Staked"""); break;
                    case "deactivationTime": columns.Add(@"""DeactivationLevel"""); columns.Add(@"""Staked"""); break;
                    case "stakingBalance": columns.Add(@"""StakingBalance"""); break;
                    case "firstActivity": columns.Add(@"""FirstLevel"""); break;
                    case "firstActivityTime": columns.Add(@"""FirstLevel"""); break;
                    case "lastActivity": columns.Add(@"""LastLevel"""); break;
                    case "lastActivityTime": columns.Add(@"""LastLevel"""); break;
                    case "numActivations": columns.Add(@"""Activated"""); break;
                    case "numBallots": columns.Add(@"""BallotsCount"""); break;
                    case "numContracts": columns.Add(@"""ContractsCount"""); break;
                    case "numDelegators": columns.Add(@"""DelegatorsCount"""); break;
                    case "numBlocks": columns.Add(@"""BlocksCount"""); break;
                    case "numDelegations": columns.Add(@"""DelegationsCount"""); break;
                    case "numDoubleBaking": columns.Add(@"""DoubleBakingCount"""); break;
                    case "numDoubleEndorsing": columns.Add(@"""DoubleEndorsingCount"""); break;
                    case "numEndorsements": columns.Add(@"""EndorsementsCount"""); break;
                    case "numNonceRevelations": columns.Add(@"""NonceRevelationsCount"""); break;
                    case "numRevelationPenalties": columns.Add(@"""RevelationPenaltiesCount"""); break;
                    case "numOriginations": columns.Add(@"""OriginationsCount"""); break;
                    case "numProposals": columns.Add(@"""ProposalsCount"""); break;
                    case "numReveals": columns.Add(@"""RevealsCount"""); break;
                    case "numMigrations": columns.Add(@"""MigrationsCount"""); break;
                    case "numTransactions": columns.Add(@"""TransactionsCount"""); break;
                    case "software": columns.Add(@"""SoftwareId"""); break;
                }
            }

            if (columns.Count == 0)
                return Array.Empty<object[]>();

            var sql = new SqlBuilder($@"SELECT {string.Join(',', columns)} FROM ""Accounts""")
                .Filter("Type", 1)
                .Filter("Staked", active)
                .Take(sort, offset, limit, x => x switch
                {
                    "activationLevel" => ("ActivationLevel", "ActivationLevel"),
                    "deactivationLevel" => ("DeactivationLevel", "DeactivationLevel"),
                    "stakingBalance" => ("StakingBalance", "StakingBalance"),
                    "balance" => ("Balance", "Balance"),
                    "numDelegators" => ("DelegatorsCount", "DelegatorsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            var result = new object[rows.Count()][];
            for (int i = 0; i < result.Length; i++)
                result[i] = new object[fields.Length];

            for (int i = 0, j = 0; i < fields.Length; j = 0, i++)
            {
                switch (fields[i])
                {
                    case "alias":
                        foreach (var row in rows)
                        {
                            var metadata = Accounts.GetMetadata((int)row.Id);
                            result[j++][i] = metadata?.Alias;
                        }
                        break;
                    case "type":
                        foreach (var row in rows)
                            result[j++][i] = AccountTypes.Delegate;
                        break;
                    case "active":
                        foreach (var row in rows)
                            result[j++][i] = row.Staked;
                        break;
                    case "address":
                        foreach (var row in rows)
                            result[j++][i] = row.Address;
                        break;
                    case "publicKey":
                        foreach (var row in rows)
                            result[j++][i] = row.PublicKey;
                        break;
                    case "revealed":
                        foreach (var row in rows)
                            result[j++][i] = row.Revealed;
                        break;
                    case "balance":
                        foreach (var row in rows)
                            result[j++][i] = row.Balance;
                        break;
                    case "frozenDeposits":
                        foreach (var row in rows)
                            result[j++][i] = row.FrozenDeposits;
                        break;
                    case "frozenRewards":
                        foreach (var row in rows)
                            result[j++][i] = row.FrozenRewards;
                        break;
                    case "frozenFees":
                        foreach (var row in rows)
                            result[j++][i] = row.FrozenFees;
                        break;
                    case "counter":
                        foreach (var row in rows)
                            result[j++][i] = row.Counter;
                        break;
                    case "activationLevel":
                        foreach (var row in rows)
                            result[j++][i] = row.ActivationLevel;
                        break;
                    case "activationTime":
                        foreach (var row in rows)
                            result[j++][i] = Time[row.ActivationLevel];
                        break;
                    case "deactivationLevel":
                        foreach (var row in rows)
                            result[j++][i] = row.Staked ? null : (int?)row.DeactivationLevel;
                        break;
                    case "deactivationTime":
                        foreach (var row in rows)
                            result[j++][i] = row.Staked ? null : (DateTime?)Time[row.DeactivationLevel];
                        break;
                    case "stakingBalance":
                        foreach (var row in rows)
                            result[j++][i] = row.StakingBalance;
                        break;
                    case "firstActivity":
                        foreach (var row in rows)
                            result[j++][i] = row.FirstLevel;
                        break;
                    case "firstActivityTime":
                        foreach (var row in rows)
                            result[j++][i] = Time[row.FirstLevel];
                        break;
                    case "lastActivity":
                        foreach (var row in rows)
                            result[j++][i] = row.LastLevel;
                        break;
                    case "lastActivityTime":
                        foreach (var row in rows)
                            result[j++][i] = Time[row.LastLevel];
                        break;
                    case "numActivations":
                        foreach (var row in rows)
                            result[j++][i] = row.Activated == true ? 1 : 0;
                        break;
                    case "numBallots":
                        foreach (var row in rows)
                            result[j++][i] = row.BallotsCount;
                        break;
                    case "numContracts":
                        foreach (var row in rows)
                            result[j++][i] = row.ContractsCount;
                        break;
                    case "numDelegators":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegatorsCount;
                        break;
                    case "numBlocks":
                        foreach (var row in rows)
                            result[j++][i] = row.BlocksCount;
                        break;
                    case "numDelegations":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegationsCount;
                        break;
                    case "numDoubleBaking":
                        foreach (var row in rows)
                            result[j++][i] = row.DoubleBakingCount;
                        break;
                    case "numDoubleEndorsing":
                        foreach (var row in rows)
                            result[j++][i] = row.DoubleEndorsingCount;
                        break;
                    case "numEndorsements":
                        foreach (var row in rows)
                            result[j++][i] = row.EndorsementsCount;
                        break;
                    case "numNonceRevelations":
                        foreach (var row in rows)
                            result[j++][i] = row.NonceRevelationsCount;
                        break;
                    case "numRevelationPenalties":
                        foreach (var row in rows)
                            result[j++][i] = row.RevelationPenaltiesCount;
                        break;
                    case "numOriginations":
                        foreach (var row in rows)
                            result[j++][i] = row.OriginationsCount;
                        break;
                    case "numProposals":
                        foreach (var row in rows)
                            result[j++][i] = row.ProposalsCount;
                        break;
                    case "numReveals":
                        foreach (var row in rows)
                            result[j++][i] = row.RevealsCount;
                        break;
                    case "numMigrations":
                        foreach (var row in rows)
                            result[j++][i] = row.MigrationsCount;
                        break;
                    case "numTransactions":
                        foreach (var row in rows)
                            result[j++][i] = row.TransactionsCount;
                        break;
                    case "software":
                        foreach (var row in rows)
                            result[j++][i] = row.SoftwareId != null ? Software[row.SoftwareId] : null;
                        break;
                }
            }

            return result;
        }

        public async Task<object[]> GetDelegates(
            BoolParameter active,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            string field)
        {
            var columns = new HashSet<string>(3);
            switch (field)
            {
                case "alias": columns.Add(@"""Id"""); break;
                case "type": columns.Add(@"""Type"""); break;
                case "active": columns.Add(@"""Staked"""); break;
                case "address": columns.Add(@"""Address"""); break;
                case "publicKey": columns.Add(@"""PublicKey"""); break;
                case "revealed": columns.Add(@"""Revealed"""); break;
                case "balance": columns.Add(@"""Balance"""); break;
                case "frozenDeposits": columns.Add(@"""FrozenDeposits"""); break;
                case "frozenRewards": columns.Add(@"""FrozenRewards"""); break;
                case "frozenFees": columns.Add(@"""FrozenFees"""); break;
                case "counter": columns.Add(@"""Counter"""); break;
                case "activationLevel": columns.Add(@"""ActivationLevel"""); break;
                case "activationTime": columns.Add(@"""ActivationLevel"""); break;
                case "deactivationLevel": columns.Add(@"""DeactivationLevel"""); columns.Add(@"""Staked"""); break;
                case "deactivationTime": columns.Add(@"""DeactivationLevel"""); columns.Add(@"""Staked"""); break;
                case "stakingBalance": columns.Add(@"""StakingBalance"""); break;
                case "firstActivity": columns.Add(@"""FirstLevel"""); break;
                case "firstActivityTime": columns.Add(@"""FirstLevel"""); break;
                case "lastActivity": columns.Add(@"""LastLevel"""); break;
                case "lastActivityTime": columns.Add(@"""LastLevel"""); break;
                case "numActivations": columns.Add(@"""Activated"""); break;
                case "numBallots": columns.Add(@"""BallotsCount"""); break;
                case "numContracts": columns.Add(@"""ContractsCount"""); break;
                case "numDelegators": columns.Add(@"""DelegatorsCount"""); break;
                case "numBlocks": columns.Add(@"""BlocksCount"""); break;
                case "numDelegations": columns.Add(@"""DelegationsCount"""); break;
                case "numDoubleBaking": columns.Add(@"""DoubleBakingCount"""); break;
                case "numDoubleEndorsing": columns.Add(@"""DoubleEndorsingCount"""); break;
                case "numEndorsements": columns.Add(@"""EndorsementsCount"""); break;
                case "numNonceRevelations": columns.Add(@"""NonceRevelationsCount"""); break;
                case "numRevelationPenalties": columns.Add(@"""RevelationPenaltiesCount"""); break;
                case "numOriginations": columns.Add(@"""OriginationsCount"""); break;
                case "numProposals": columns.Add(@"""ProposalsCount"""); break;
                case "numReveals": columns.Add(@"""RevealsCount"""); break;
                case "numMigrations": columns.Add(@"""MigrationsCount"""); break;
                case "numTransactions": columns.Add(@"""TransactionsCount"""); break;
                case "software": columns.Add(@"""SoftwareId"""); break;
            }

            if (columns.Count == 0)
                return Array.Empty<object>();

            var sql = new SqlBuilder($@"SELECT {string.Join(',', columns)} FROM ""Accounts""")
                .Filter("Type", 1)
                .Filter("Staked", active)
                .Take(sort, offset, limit, x => x switch
                {
                    "activationLevel" => ("ActivationLevel", "ActivationLevel"),
                    "deactivationLevel" => ("DeactivationLevel", "DeactivationLevel"),
                    "stakingBalance" => ("StakingBalance", "StakingBalance"),
                    "balance" => ("Balance", "Balance"),
                    "numDelegators" => ("DelegatorsCount", "DelegatorsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            var result = new object[rows.Count()];
            var j = 0;

            switch (field)
            {
                case "alias":
                    foreach (var row in rows)
                    {
                        var metadata = Accounts.GetMetadata((int)row.Id);
                        result[j++] = metadata?.Alias;
                    }
                    break;
                case "type":
                    foreach (var row in rows)
                        result[j++] = AccountTypes.Delegate;
                    break;
                case "active":
                    foreach (var row in rows)
                        result[j++] = row.Staked;
                    break;
                case "address":
                    foreach (var row in rows)
                        result[j++] = row.Address;
                    break;
                case "publicKey":
                    foreach (var row in rows)
                        result[j++] = row.PublicKey;
                    break;
                case "revealed":
                    foreach (var row in rows)
                        result[j++] = row.Revealed;
                    break;
                case "balance":
                    foreach (var row in rows)
                        result[j++] = row.Balance;
                    break;
                case "frozenDeposits":
                    foreach (var row in rows)
                        result[j++] = row.FrozenDeposits;
                    break;
                case "frozenRewards":
                    foreach (var row in rows)
                        result[j++] = row.FrozenRewards;
                    break;
                case "frozenFees":
                    foreach (var row in rows)
                        result[j++] = row.FrozenFees;
                    break;
                case "counter":
                    foreach (var row in rows)
                        result[j++] = row.Counter;
                    break;
                case "activationLevel":
                    foreach (var row in rows)
                        result[j++] = row.ActivationLevel;
                    break;
                case "activationTime":
                    foreach (var row in rows)
                        result[j++] = Time[row.ActivationLevel];
                    break;
                case "deactivationLevel":
                    foreach (var row in rows)
                        result[j++] = row.Staked ? null : (int?)row.DeactivationLevel;
                    break;
                case "deactivationTime":
                    foreach (var row in rows)
                        result[j++] = row.Staked ? null : (DateTime?)Time[row.DeactivationLevel];
                    break;
                case "stakingBalance":
                    foreach (var row in rows)
                        result[j++] = row.StakingBalance;
                    break;
                case "firstActivity":
                    foreach (var row in rows)
                        result[j++] = row.FirstLevel;
                    break;
                case "firstActivityTime":
                    foreach (var row in rows)
                        result[j++] = Time[row.FirstLevel];
                    break;
                case "lastActivity":
                    foreach (var row in rows)
                        result[j++] = row.LastLevel;
                    break;
                case "lastActivityTime":
                    foreach (var row in rows)
                        result[j++] = Time[row.LastLevel];
                    break;
                case "numActivations":
                    foreach (var row in rows)
                        result[j++] = row.Activated == true ? 1 : 0;
                    break;
                case "numBallots":
                    foreach (var row in rows)
                        result[j++] = row.BallotsCount;
                    break;
                case "numContracts":
                    foreach (var row in rows)
                        result[j++] = row.ContractsCount;
                    break;
                case "numDelegators":
                    foreach (var row in rows)
                        result[j++] = row.DelegatorsCount;
                    break;
                case "numBlocks":
                    foreach (var row in rows)
                        result[j++] = row.BlocksCount;
                    break;
                case "numDelegations":
                    foreach (var row in rows)
                        result[j++] = row.DelegationsCount;
                    break;
                case "numDoubleBaking":
                    foreach (var row in rows)
                        result[j++] = row.DoubleBakingCount;
                    break;
                case "numDoubleEndorsing":
                    foreach (var row in rows)
                        result[j++] = row.DoubleEndorsingCount;
                    break;
                case "numEndorsements":
                    foreach (var row in rows)
                        result[j++] = row.EndorsementsCount;
                    break;
                case "numNonceRevelations":
                    foreach (var row in rows)
                        result[j++] = row.NonceRevelationsCount;
                    break;
                case "numRevelationPenalties":
                    foreach (var row in rows)
                        result[j++] = row.RevelationPenaltiesCount;
                    break;
                case "numOriginations":
                    foreach (var row in rows)
                        result[j++] = row.OriginationsCount;
                    break;
                case "numProposals":
                    foreach (var row in rows)
                        result[j++] = row.ProposalsCount;
                    break;
                case "numReveals":
                    foreach (var row in rows)
                        result[j++] = row.RevealsCount;
                    break;
                case "numMigrations":
                    foreach (var row in rows)
                        result[j++] = row.MigrationsCount;
                    break;
                case "numTransactions":
                    foreach (var row in rows)
                        result[j++] = row.TransactionsCount;
                    break;
                case "software":
                    foreach (var row in rows)
                        result[j++] = row.SoftwareId != null ? Software[row.SoftwareId] : null;
                    break;
            }

            return result;
        }
        #endregion

        #region contracts
        public async Task<int> GetContractsCount(ContractKindParameter kind)
        {
            var sql = new SqlBuilder(@"SELECT COUNT(*) FROM ""Accounts""")
                .Filter("Type", 2)
                .Filter("Kind", kind);

            using var db = GetConnection();
            return await db.QueryFirstAsync<int>(sql.Query, sql.Params);
        }

        public async Task<IEnumerable<Contract>> GetContracts(
            ContractKindParameter kind,
            SortParameter sort,
            OffsetParameter offset,
            int limit)
        {
            var sql = new SqlBuilder(@"SELECT * FROM ""Accounts""")
                .Filter("Type", 2)
                .Filter("Kind", kind)
                .Take(sort, offset, limit, x => x switch
                {
                    "balance" => ("Balance", "Balance"),
                    "firstActivity" => ("FirstLevel", "FirstLevel"),
                    "lastActivity" => ("LastLevel", "LastLevel"),
                    "numTransactions" => ("TransactionsCount", "TransactionsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            return rows.Select(row =>
            {
                var metadata = Accounts.GetMetadata((int)row.Id);

                var creator = row.CreatorId == null ? null
                            : Accounts.Get((int)row.CreatorId);

                var creatorMetadata = creator == null ? null
                    : Accounts.GetMetadata(creator.Id);

                var manager = row.ManagerId == null ? null
                    : (RawUser)Accounts.Get((int)row.ManagerId);

                var managerMetadata = manager == null ? null
                    : Accounts.GetMetadata(manager.Id);

                var contractDelegate = row.DelegateId == null ? null
                    : Accounts.Get((int)row.DelegateId);

                var contractDelegateMetadata = contractDelegate == null ? null
                    : Accounts.GetMetadata(contractDelegate.Id);

                return new Contract
                {
                    Alias = metadata?.Alias,
                    Address = row.Address,
                    Kind = KindToString(row.Kind),
                    Balance = row.Balance,
                    Creator = creator == null ? null
                        : new CreatorInfo
                        {
                            Alias = creatorMetadata?.Alias,
                            Address = creator.Address
                        },
                    Manager = manager == null ? null
                        : new ManagerInfo
                        {
                            Alias = managerMetadata?.Alias,
                            Address = manager.Address,
                            PublicKey = manager.PublicKey,
                        },
                    Delegate = contractDelegate == null ? null
                        : new DelegateInfo
                        {
                            Alias = contractDelegateMetadata?.Alias,
                            Address = contractDelegate.Address,
                            Active = contractDelegate.Staked
                        },
                    DelegationLevel = contractDelegate == null ? null
                        : row.DelegationLevel,
                    DelegationTime = contractDelegate == null ? null
                        : (DateTime?)Time[row.DelegationLevel],
                    FirstActivity = row.FirstLevel,
                    FirstActivityTime = Time[row.FirstLevel],
                    LastActivity = row.LastLevel,
                    LastActivityTime = Time[row.LastLevel],
                    NumContracts = row.ContractsCount,
                    NumDelegations = row.DelegationsCount,
                    NumOriginations = row.OriginationsCount,
                    NumReveals = row.RevealsCount,
                    NumMigrations = row.MigrationsCount,
                    NumTransactions = row.TransactionsCount
                };
            });
        }

        public async Task<object[][]> GetContracts(
            ContractKindParameter kind,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            string[] fields)
        {
            var columns = new HashSet<string>(fields.Length + 2);
            foreach (var field in fields)
            {
                switch (field)
                {
                    case "type": columns.Add(@"""Type"""); break;
                    case "kind": columns.Add(@"""Kind"""); break;
                    case "alias": columns.Add(@"""Id"""); break;
                    case "address": columns.Add(@"""Address"""); break;
                    case "balance": columns.Add(@"""Balance"""); break;
                    case "creator": columns.Add(@"""CreatorId"""); break;
                    case "manager": columns.Add(@"""ManagerId"""); break;
                    case "delegate": columns.Add(@"""DelegateId"""); break;
                    case "delegationLevel": columns.Add(@"""DelegationLevel"""); columns.Add(@"""DelegateId"""); break;
                    case "delegationTime": columns.Add(@"""DelegationLevel"""); columns.Add(@"""DelegateId"""); break;
                    case "numContracts": columns.Add(@"""ContractsCount"""); break;
                    case "numDelegations": columns.Add(@"""DelegationsCount"""); break;
                    case "numOriginations": columns.Add(@"""OriginationsCount"""); break;
                    case "numTransactions": columns.Add(@"""TransactionsCount"""); break;
                    case "numReveals": columns.Add(@"""RevealsCount"""); break;
                    case "numMigrations": columns.Add(@"""MigrationsCount"""); break;
                    case "firstActivity": columns.Add(@"""FirstLevel"""); break;
                    case "firstActivityTime": columns.Add(@"""FirstLevel"""); break;
                    case "lastActivity": columns.Add(@"""LastLevel"""); break;
                    case "lastActivityTime": columns.Add(@"""LastLevel"""); break;
                }
            }

            if (columns.Count == 0)
                return Array.Empty<object[]>();

            var sql = new SqlBuilder($@"SELECT {string.Join(',', columns)} FROM ""Accounts""")
                .Filter("Type", 2)
                .Filter("Kind", kind)
                .Take(sort, offset, limit, x => x switch
                {
                    "balance" => ("Balance", "Balance"),
                    "firstActivity" => ("FirstLevel", "FirstLevel"),
                    "lastActivity" => ("LastLevel", "LastLevel"),
                    "numTransactions" => ("TransactionsCount", "TransactionsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            var result = new object[rows.Count()][];
            for (int i = 0; i < result.Length; i++)
                result[i] = new object[fields.Length];

            for (int i = 0, j = 0; i < fields.Length; j = 0, i++)
            {
                switch (fields[i])
                {
                    case "type":
                        foreach (var row in rows)
                            result[j++][i] = AccountTypes.Contract;
                        break;
                    case "kind":
                        foreach (var row in rows)
                            result[j++][i] = KindToString(row.Kind);
                        break;
                    case "alias":
                        foreach (var row in rows)
                        {
                            var metadata = Accounts.GetMetadata((int)row.Id);
                            result[j++][i] = metadata?.Alias;
                        }
                        break;
                    case "address":
                        foreach (var row in rows)
                            result[j++][i] = row.Address;
                        break;
                    case "balance":
                        foreach (var row in rows)
                            result[j++][i] = row.Balance;
                        break;
                    case "creator":
                        foreach (var row in rows)
                        {
                            var creator = row.CreatorId == null ? null : Accounts.Get((int)row.CreatorId);
                            var creatorMetadata = creator == null ? null : Accounts.GetMetadata(creator.Id);
                            result[j++][i] = creator == null ? null : new CreatorInfo
                            {
                                Alias = creatorMetadata?.Alias,
                                Address = creator.Address
                            };
                        }
                        break;
                    case "manager":
                        foreach (var row in rows)
                        {
                            var manager = row.ManagerId == null ? null : (RawUser)Accounts.Get((int)row.ManagerId);
                            var managerMetadata = manager == null ? null : Accounts.GetMetadata(manager.Id);
                            result[j++][i] = manager == null ? null : new ManagerInfo
                            {
                                Alias = managerMetadata?.Alias,
                                Address = manager.Address,
                                PublicKey = manager.PublicKey,
                            };
                        }
                        break;
                    case "delegate":
                        foreach (var row in rows)
                        {
                            var delegat = row.DelegateId == null ? null : Accounts.Get((int)row.DelegateId);
                            var delegatMetadata = delegat == null ? null : Accounts.GetMetadata(delegat.Id);
                            result[j++][i] = delegat == null ? null : new DelegateInfo
                            {
                                Alias = delegatMetadata?.Alias,
                                Address = delegat.Address,
                                Active = delegat.Staked
                            };
                        }
                        break;
                    case "delegationLevel":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegateId == null ? null : row.DelegationLevel;
                        break;
                    case "delegationTime":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegateId == null ? null : Time[row.DelegationLevel];
                        break;
                    case "numContracts":
                        foreach (var row in rows)
                            result[j++][i] = row.ContractsCount;
                        break;
                    case "numDelegations":
                        foreach (var row in rows)
                            result[j++][i] = row.DelegationsCount;
                        break;
                    case "numOriginations":
                        foreach (var row in rows)
                            result[j++][i] = row.OriginationsCount;
                        break;
                    case "numTransactions":
                        foreach (var row in rows)
                            result[j++][i] = row.TransactionsCount;
                        break;
                    case "numReveals":
                        foreach (var row in rows)
                            result[j++][i] = row.RevealsCount;
                        break;
                    case "numMigrations":
                        foreach (var row in rows)
                            result[j++][i] = row.MigrationsCount;
                        break;
                    case "firstActivity":
                        foreach (var row in rows)
                            result[j++][i] = row.FirstLevel;
                        break;
                    case "firstActivityTime":
                        foreach (var row in rows)
                            result[j++][i] = Time[row.FirstLevel];
                        break;
                    case "lastActivity":
                        foreach (var row in rows)
                            result[j++][i] = row.LastLevel;
                        break;
                    case "lastActivityTime":
                        foreach (var row in rows)
                            result[j++][i] = Time[row.LastLevel];
                        break;
                }
            }

            return result;
        }


        public async Task<object[]> GetContracts(
            ContractKindParameter kind,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            string field)
        {
            var columns = new HashSet<string>(3);
            switch (field)
            {
                case "type": columns.Add(@"""Type"""); break;
                case "kind": columns.Add(@"""Kind"""); break;
                case "alias": columns.Add(@"""Id"""); break;
                case "address": columns.Add(@"""Address"""); break;
                case "balance": columns.Add(@"""Balance"""); break;
                case "creator": columns.Add(@"""CreatorId"""); break;
                case "manager": columns.Add(@"""ManagerId"""); break;
                case "delegate": columns.Add(@"""DelegateId"""); break;
                case "delegationLevel": columns.Add(@"""DelegationLevel"""); columns.Add(@"""DelegateId"""); break;
                case "delegationTime": columns.Add(@"""DelegationLevel"""); columns.Add(@"""DelegateId"""); break;
                case "numContracts": columns.Add(@"""ContractsCount"""); break;
                case "numDelegations": columns.Add(@"""DelegationsCount"""); break;
                case "numOriginations": columns.Add(@"""OriginationsCount"""); break;
                case "numTransactions": columns.Add(@"""TransactionsCount"""); break;
                case "numReveals": columns.Add(@"""RevealsCount"""); break;
                case "numMigrations": columns.Add(@"""MigrationsCount"""); break;
                case "firstActivity": columns.Add(@"""FirstLevel"""); break;
                case "firstActivityTime": columns.Add(@"""FirstLevel"""); break;
                case "lastActivity": columns.Add(@"""LastLevel"""); break;
                case "lastActivityTime": columns.Add(@"""LastLevel"""); break;
            }

            if (columns.Count == 0)
                return Array.Empty<object>();

            var sql = new SqlBuilder($@"SELECT {string.Join(',', columns)} FROM ""Accounts""")
                .Filter("Type", 2)
                .Filter("Kind", kind)
                .Take(sort, offset, limit, x => x switch
                {
                    "balance" => ("Balance", "Balance"),
                    "firstActivity" => ("FirstLevel", "FirstLevel"),
                    "lastActivity" => ("LastLevel", "LastLevel"),
                    "numTransactions" => ("TransactionsCount", "TransactionsCount"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            var result = new object[rows.Count()];
            var j = 0;

            switch (field)
            {
                case "type":
                    foreach (var row in rows)
                        result[j++] = AccountTypes.Contract;
                    break;
                case "kind":
                    foreach (var row in rows)
                        result[j++] = KindToString(row.Kind);
                    break;
                case "alias":
                    foreach (var row in rows)
                    {
                        var metadata = Accounts.GetMetadata((int)row.Id);
                        result[j++] = metadata?.Alias;
                    }
                    break;
                case "address":
                    foreach (var row in rows)
                        result[j++] = row.Address;
                    break;
                case "balance":
                    foreach (var row in rows)
                        result[j++] = row.Balance;
                    break;
                case "creator":
                    foreach (var row in rows)
                    {
                        var creator = row.CreatorId == null ? null : Accounts.Get((int)row.CreatorId);
                        var creatorMetadata = creator == null ? null : Accounts.GetMetadata(creator.Id);
                        result[j++] = creator == null ? null : new CreatorInfo
                        {
                            Alias = creatorMetadata?.Alias,
                            Address = creator.Address
                        };
                    }
                    break;
                case "manager":
                    foreach (var row in rows)
                    {
                        var manager = row.ManagerId == null ? null : (RawUser)Accounts.Get((int)row.ManagerId);
                        var managerMetadata = manager == null ? null : Accounts.GetMetadata(manager.Id);
                        result[j++] = manager == null ? null : new ManagerInfo
                        {
                            Alias = managerMetadata?.Alias,
                            Address = manager.Address,
                            PublicKey = manager.PublicKey,
                        };
                    }
                    break;
                case "delegate":
                    foreach (var row in rows)
                    {
                        var delegat = row.DelegateId == null ? null : Accounts.Get((int)row.DelegateId);
                        var delegatMetadata = delegat == null ? null : Accounts.GetMetadata(delegat.Id);
                        result[j++] = delegat == null ? null : new DelegateInfo
                        {
                            Alias = delegatMetadata?.Alias,
                            Address = delegat.Address,
                            Active = delegat.Staked
                        };
                    }
                    break;
                case "delegationLevel":
                    foreach (var row in rows)
                        result[j++] = row.DelegateId == null ? null : row.DelegationLevel;
                    break;
                case "delegationTime":
                    foreach (var row in rows)
                        result[j++] = row.DelegateId == null ? null : Time[row.DelegationLevel];
                    break;
                case "numContracts":
                    foreach (var row in rows)
                        result[j++] = row.ContractsCount;
                    break;
                case "numDelegations":
                    foreach (var row in rows)
                        result[j++] = row.DelegationsCount;
                    break;
                case "numOriginations":
                    foreach (var row in rows)
                        result[j++] = row.OriginationsCount;
                    break;
                case "numTransactions":
                    foreach (var row in rows)
                        result[j++] = row.TransactionsCount;
                    break;
                case "numReveals":
                    foreach (var row in rows)
                        result[j++] = row.RevealsCount;
                    break;
                case "numMigrations":
                    foreach (var row in rows)
                        result[j++] = row.MigrationsCount;
                    break;
                case "firstActivity":
                    foreach (var row in rows)
                        result[j++] = row.FirstLevel;
                    break;
                case "firstActivityTime":
                    foreach (var row in rows)
                        result[j++] = Time[row.FirstLevel];
                    break;
                case "lastActivity":
                    foreach (var row in rows)
                        result[j++] = row.LastLevel;
                    break;
                case "lastActivityTime":
                    foreach (var row in rows)
                        result[j++] = Time[row.LastLevel];
                    break;
            }

            return result;
        }
        #endregion

        public async Task<IEnumerable<RelatedContract>> GetRelatedContracts(
            string address,
            SortParameter sort,
            OffsetParameter offset,
            int limit)
        {
            var account = await Accounts.GetAsync(address);
            if (account == null || account.ContractsCount == 0)
                return Enumerable.Empty<RelatedContract>();

            var sql = new SqlBuilder(@"SELECT ""Id"", ""Kind"", ""Address"", ""Balance"", ""DelegateId"", ""FirstLevel"" FROM ""Accounts""")
                .Filter($@"(""CreatorId"" = {account.Id} OR ""ManagerId"" = {account.Id})")
                .Take(sort ?? new SortParameter { Desc = "id" }, offset, limit, x => x switch
                {
                    "balance" => ("Balance", "Balance"),
                    "creationLevel" => ("Id", "FirstLevel"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            return rows.Select(row =>
            {
                var metadata = Accounts.GetMetadata((int)row.Id);

                var delegat = row.DelegateId == null ? null
                    : Accounts.Get((int)row.DelegateId);

                var delegatMetadata = delegat == null ? null
                    : Accounts.GetMetadata(delegat.Id);

                return new RelatedContract
                {
                    Kind = KindToString(row.Kind),
                    Alias = metadata?.Alias,
                    Address = row.Address,
                    Balance = row.Balance,
                    Delegate = row.DelegateId == null ? null
                         : new DelegateInfo
                         {
                             Alias = delegatMetadata?.Alias,
                             Address = delegat.Address,
                             Active = delegat.Staked
                         },
                    CreationLevel = row.FirstLevel,
                    CreationTime = Time[row.FirstLevel]
                };
            });
        }
        
        public async Task<IEnumerable<Delegator>> GetDelegators(string address, int limit = 100, int offset = 0)
        {
            var delegat = await Accounts.GetAsync(address) as RawDelegate;
            if (delegat == null || delegat.DelegatorsCount == 0)
                return Enumerable.Empty<Delegator>();

            var sql = @"
                SELECT      ""Id"", ""Address"", ""Type"", ""Balance"", ""DelegationLevel""
                FROM        ""Accounts""
                WHERE       ""DelegateId"" = @delegateId
                ORDER BY    ""DelegationLevel"" DESC
                OFFSET      @offset
                LIMIT       @limit";

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql, new { delegateId = delegat.Id, limit, offset });

            return rows.Select(row =>
            {
                var metadata = Accounts.GetMetadata((int)row.Id);

                return new Delegator
                {
                    Type = TypeToString(row.Type),
                    Alias = metadata?.Alias,
                    Address = row.Address,
                    Balance = row.Balance,
                    DelegationLevel = row.DelegationLevel,
                    DelegationTime = Time[row.DelegationLevel]
                };
            });
        }

        public async Task<IEnumerable<Delegator>> GetDelegators(
            string address,
            AccountTypeParameter type,
            Int64Parameter balance,
            Int32Parameter delegationLevel, 
            SortParameter sort,
            OffsetParameter offset,
            int limit)
        {
            var delegat = await Accounts.GetAsync(address) as RawDelegate;
            if (delegat == null || delegat.DelegatorsCount == 0)
                return Enumerable.Empty<Delegator>();

            var sql = new SqlBuilder(@"SELECT ""Id"", ""Address"", ""Type"", ""Balance"", ""DelegationLevel"" FROM ""Accounts""")
                .Filter("DelegateId", delegat.Id)
                .Filter("Type", type)
                .Filter("Balance", balance)
                .Filter("DelegationLevel", delegationLevel)
                .Take(sort ?? new SortParameter { Desc = "delegationLevel" }, offset, limit, x => x == "balance" ? ("Balance", "Balance") : ("DelegationLevel", "DelegationLevel"));

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            return rows.Select(row =>
            {
                var metadata = Accounts.GetMetadata((int)row.Id);

                return new Delegator
                {
                    Type = TypeToString(row.Type),
                    Alias = metadata?.Alias,
                    Address = row.Address,
                    Balance = row.Balance,
                    DelegationLevel = row.DelegationLevel,
                    DelegationTime = Time[row.DelegationLevel]
                };
            });
        }

        public async Task<IEnumerable<Operation>> GetOperations(
            string address,
            HashSet<string> types,
            AccountParameter initiator,
            AccountParameter sender,
            AccountParameter target,
            AccountParameter prevDelegate,
            AccountParameter newDelegate,
            AccountParameter contractManager,
            AccountParameter contractDelegate,
            AccountParameter originatedContract,
            AccountParameter accuser,
            AccountParameter offender,
            AccountParameter baker,
            Int32Parameter level,
            DateTimeParameter timestamp,
            StringParameter entrypoint,
            JsonParameter parameters,
            BoolParameter hasInternals,
            OperationStatusParameter status,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            Symbols quote)
        {
            var account = await Accounts.GetAsync(address);
            if (account == null) return Enumerable.Empty<Operation>();
            var result = new List<Operation>(limit * 2);

            switch (account)
            {
                case RawDelegate delegat:
                    var _delegat = new AccountParameter { Eq = delegat.Id };

                    var endorsements = delegat.EndorsementsCount > 0 && types.Contains(OpTypes.Endorsement)
                        ? Operations.GetEndorsements(_delegat, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<EndorsementOperation>());

                    var ballots = delegat.BallotsCount > 0 && types.Contains(OpTypes.Ballot)
                        ? Operations.GetBallots(_delegat, level, timestamp, null, null, null, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<BallotOperation>());

                    var proposals = delegat.ProposalsCount > 0 && types.Contains(OpTypes.Proposal)
                        ? Operations.GetProposals(_delegat, level, timestamp, null, null, null, null, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<ProposalOperation>());

                    var activations = delegat.Activated == true && types.Contains(OpTypes.Activation)
                        ? Operations.GetActivations(_delegat, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<ActivationOperation>());

                    var doubleBaking = delegat.DoubleBakingCount > 0 && types.Contains(OpTypes.DoubleBaking)
                        ? Operations.GetDoubleBakings(new AnyOfParameter { Fields = new[] { "accuser", "offender" }, Value = delegat.Id }, accuser, offender, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<DoubleBakingOperation>());

                    var doubleEndorsing = delegat.DoubleEndorsingCount > 0 && types.Contains(OpTypes.DoubleEndorsing)
                        ? Operations.GetDoubleEndorsings(new AnyOfParameter { Fields = new[] { "accuser", "offender" }, Value = delegat.Id }, accuser, offender, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<DoubleEndorsingOperation>());

                    var nonceRevelations = delegat.NonceRevelationsCount > 0 && types.Contains(OpTypes.NonceRevelation)
                        ? Operations.GetNonceRevelations(new AnyOfParameter { Fields = new[] { "baker", "sender" }, Value = delegat.Id }, baker, sender, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<NonceRevelationOperation>());

                    var delegations = delegat.DelegationsCount > 0 && types.Contains(OpTypes.Delegation)
                        ? Operations.GetDelegations(new AnyOfParameter { Fields = new[] { "initiator", "sender", "prevDelegate", "newDelegate" }, Value = delegat.Id }, initiator, sender, prevDelegate, newDelegate, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<DelegationOperation>());

                    var originations = delegat.OriginationsCount > 0 && types.Contains(OpTypes.Origination)
                        ? Operations.GetOriginations(new AnyOfParameter { Fields = new[] { "initiator", "sender", "contractManager", "contractDelegate", "originatedContract" }, Value = delegat.Id }, initiator, sender, contractManager, contractDelegate, originatedContract, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<OriginationOperation>());

                    var transactions = delegat.TransactionsCount > 0 && types.Contains(OpTypes.Transaction)
                        ? Operations.GetTransactions(new AnyOfParameter { Fields = new[] { "initiator", "sender", "target" }, Value = delegat.Id }, initiator, sender, target, null, level, timestamp, entrypoint, parameters, hasInternals, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<TransactionOperation>());

                    var reveals = delegat.RevealsCount > 0 && types.Contains(OpTypes.Reveal)
                        ? Operations.GetReveals(_delegat, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<RevealOperation>());

                    var migrations = delegat.MigrationsCount > 0 && types.Contains(OpTypes.Migration)
                        ? Operations.GetMigrations(_delegat, null, null, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<MigrationOperation>());

                    var revelationPenalties = delegat.RevelationPenaltiesCount > 0 && types.Contains(OpTypes.RevelationPenalty)
                        ? Operations.GetRevelationPenalties(_delegat, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<RevelationPenaltyOperation>());

                    var blockOps = delegat.BlocksCount > 0 && types.Contains(OpTypes.Baking)
                        ? Operations.GetBakings(_delegat, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<BakingOperation>());

                    await Task.WhenAll(
                        endorsements,
                        proposals,
                        ballots,
                        activations,
                        doubleBaking,
                        doubleEndorsing,
                        nonceRevelations,
                        delegations,
                        originations,
                        transactions,
                        reveals,
                        migrations,
                        revelationPenalties,
                        blockOps);

                    result.AddRange(endorsements.Result);
                    result.AddRange(proposals.Result);
                    result.AddRange(ballots.Result);
                    result.AddRange(activations.Result);
                    result.AddRange(doubleBaking.Result);
                    result.AddRange(doubleEndorsing.Result);
                    result.AddRange(nonceRevelations.Result);
                    result.AddRange(delegations.Result);
                    result.AddRange(originations.Result);
                    result.AddRange(transactions.Result);
                    result.AddRange(reveals.Result);
                    result.AddRange(migrations.Result);
                    result.AddRange(revelationPenalties.Result);
                    result.AddRange(blockOps.Result);

                    break;
                case RawUser user:
                    var _user = new AccountParameter { Eq = user.Id };

                    var userActivations = user.Activated == true && types.Contains(OpTypes.Activation)
                        ? Operations.GetActivations(_user, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<ActivationOperation>());

                    var userDelegations = user.DelegationsCount > 0 && types.Contains(OpTypes.Delegation)
                        ? Operations.GetDelegations(new AnyOfParameter { Fields = new[] { "initiator", "sender", "prevDelegate", "newDelegate" }, Value = user.Id }, initiator, sender, prevDelegate, newDelegate, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<DelegationOperation>());

                    var userOriginations = user.OriginationsCount > 0 && types.Contains(OpTypes.Origination)
                        ? Operations.GetOriginations(new AnyOfParameter { Fields = new[] { "initiator", "sender", "contractManager", "contractDelegate", "originatedContract" }, Value = user.Id }, initiator, sender, contractManager, contractDelegate, originatedContract, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<OriginationOperation>());

                    var userTransactions = user.TransactionsCount > 0 && types.Contains(OpTypes.Transaction)
                        ? Operations.GetTransactions(new AnyOfParameter { Fields = new[] { "initiator", "sender", "target" }, Value = user.Id }, initiator, sender, target, null, level, timestamp, entrypoint, parameters, hasInternals, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<TransactionOperation>());

                    var userReveals = user.RevealsCount > 0 && types.Contains(OpTypes.Reveal)
                        ? Operations.GetReveals(_user, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<RevealOperation>());

                    var userMigrations = user.MigrationsCount > 0 && types.Contains(OpTypes.Migration)
                        ? Operations.GetMigrations(_user, null, null, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<MigrationOperation>());

                    await Task.WhenAll(
                        userActivations,
                        userDelegations,
                        userOriginations,
                        userTransactions,
                        userReveals,
                        userMigrations);

                    result.AddRange(userActivations.Result);
                    result.AddRange(userDelegations.Result);
                    result.AddRange(userOriginations.Result);
                    result.AddRange(userTransactions.Result);
                    result.AddRange(userReveals.Result);
                    result.AddRange(userMigrations.Result);

                    break;
                case RawContract contract:
                    var _contract = new AccountParameter { Eq = contract.Id };

                    var contractDelegations = contract.DelegationsCount > 0 && types.Contains(OpTypes.Delegation)
                        ? Operations.GetDelegations(new AnyOfParameter { Fields = new[] { "initiator", "sender", "prevDelegate", "newDelegate" }, Value = contract.Id }, initiator, sender, prevDelegate, newDelegate, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<DelegationOperation>());

                    var contractOriginations = contract.OriginationsCount > 0 && types.Contains(OpTypes.Origination)
                        ? Operations.GetOriginations(new AnyOfParameter { Fields = new[] { "initiator", "sender", "contractManager", "contractDelegate", "originatedContract" }, Value = contract.Id }, initiator, sender, contractManager, contractDelegate, originatedContract, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<OriginationOperation>());

                    var contractTransactions = contract.TransactionsCount > 0 && types.Contains(OpTypes.Transaction)
                        ? Operations.GetTransactions(new AnyOfParameter { Fields = new[] { "initiator", "sender", "target" }, Value = contract.Id }, initiator, sender, target, null, level, timestamp, entrypoint, parameters, hasInternals, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<TransactionOperation>());

                    var contractReveals = contract.RevealsCount > 0 && types.Contains(OpTypes.Reveal)
                        ? Operations.GetReveals(_contract, level, timestamp, status, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<RevealOperation>());

                    var contractMigrations = contract.MigrationsCount > 0 && types.Contains(OpTypes.Migration)
                        ? Operations.GetMigrations(_contract, null, null, level, timestamp, sort, offset, limit, quote)
                        : Task.FromResult(Enumerable.Empty<MigrationOperation>());

                    await Task.WhenAll(
                        contractDelegations,
                        contractOriginations,
                        contractTransactions,
                        contractReveals,
                        contractMigrations);

                    result.AddRange(contractDelegations.Result);
                    result.AddRange(contractOriginations.Result);
                    result.AddRange(contractTransactions.Result);
                    result.AddRange(contractReveals.Result);
                    result.AddRange(contractMigrations.Result);

                    break;
            }

            return sort?.Desc == null
                ? result.OrderBy(x => x.Id).Take(limit)
                : result.OrderByDescending(x => x.Id).Take(limit);
        }

        public async Task<AccountMetadata> GetMetadata(string address)
        {
            var account = await Accounts.GetAsync(address);
            return account == null ? null : Accounts.GetMetadata(account.Id);
        }

        string TypeToString(int type) => type switch
        {
            0 => AccountTypes.User,
            1 => AccountTypes.Delegate,
            2 => AccountTypes.Contract,
            _ => "unknown"
        };

        string KindToString(int kind) => kind switch
        {
            0 => ContractKinds.Delegator,
            1 => ContractKinds.SmartContract,
            2 => ContractKinds.Asset,
            _ => "unknown"
        };
    }
}

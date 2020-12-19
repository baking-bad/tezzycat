﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dapper;

using Tzkt.Api.Models;
using Tzkt.Api.Services.Metadata;

namespace Tzkt.Api.Repositories
{
    public class ProtocolRepository : DbConnection
    {
        readonly ProtocolMetadataService ProtocolMetadata;

        public ProtocolRepository(ProtocolMetadataService protocolMetadata, IConfiguration config) : base(config) 
        {
            ProtocolMetadata = protocolMetadata;
        }

        public async Task<int> GetCount()
        {
            var sql = @"
                SELECT   COUNT(*)
                FROM     ""Protocols""";

            using var db = GetConnection();
            return await db.QueryFirstAsync<int>(sql);
        }

        public async Task<Protocol> GetByCycle(int cycle)
        {
            var sql = $@"
                SELECT      *
                FROM        ""Protocols""
                WHERE       (""FirstLevel"" - 1) / GREATEST(""BlocksPerCycle"", 1) <= {cycle}
                ORDER BY    ""FirstLevel"" DESC
                LIMIT       1";

            using var db = GetConnection();
            var row = await db.QueryFirstOrDefaultAsync(sql);
            if (row == null) return null;

            return new Protocol
            {
                Code = row.Code,
                Hash = row.Hash,
                FirstLevel = row.FirstLevel,
                LastLevel = row.LastLevel == -1 ? null : row.LastLevel,
                Constants = new ProtocolConstants
                {
                    RampUpCycles = row.RampUpCycles,
                    NoRewardCycles = row.NoRewardCycles,
                    BlockDeposit = row.BlockDeposit,
                    BlockReward = row.BlockReward1 == 0
                        ? new List<long> { row.BlockReward0 }
                        : new List<long> { row.BlockReward0, row.BlockReward1 },
                    BlocksPerCommitment = row.BlocksPerCommitment,
                    BlocksPerCycle = row.BlocksPerCycle,
                    BlocksPerSnapshot = row.BlocksPerSnapshot,
                    BlocksPerVoting = row.BlocksPerVoting,
                    ByteCost = row.ByteCost,
                    EndorsementDeposit = row.EndorsementDeposit,
                    EndorsementReward = row.EndorsementReward1 == 0
                        ? new List<long> { row.EndorsementReward0 }
                        : new List<long> { row.EndorsementReward0, row.EndorsementReward1 },
                    EndorsersPerBlock = row.EndorsersPerBlock,
                    HardBlockGasLimit = row.HardBlockGasLimit,
                    HardOperationGasLimit = row.HardOperationGasLimit,
                    HardOperationStorageLimit = row.HardOperationStorageLimit,
                    OriginationSize = row.OriginationSize,
                    PreservedCycles = row.PreservedCycles,
                    RevelationReward = row.RevelationReward,
                    TimeBetweenBlocks = row.TimeBetweenBlocks,
                    TokensPerRoll = row.TokensPerRoll,
                    ProposalQuorum = row.ProposalQuorum / 100.0,
                    BallotQuorumMin = row.BallotQuorumMin / 100.0,
                    BallotQuorumMax = row.BallotQuorumMax / 100.0
                },
                Metadata = ProtocolMetadata[row.Hash]
            };
        }

        public async Task<Protocol> Get(int code)
        {
            var sql = @"
                SELECT  *
                FROM    ""Protocols""
                WHERE   ""Code"" = @code
                LIMIT   1";

            using var db = GetConnection();
            var row = await db.QueryFirstOrDefaultAsync(sql, new { code });
            if (row == null) return null;

            return new Protocol
            {
                Code = code,
                Hash = row.Hash,
                FirstLevel = row.FirstLevel,
                LastLevel = row.LastLevel == -1 ? null : row.LastLevel,
                Constants = new ProtocolConstants
                {
                    RampUpCycles = row.RampUpCycles,
                    NoRewardCycles = row.NoRewardCycles,
                    BlockDeposit = row.BlockDeposit,
                    BlockReward = row.BlockReward1 == 0
                        ? new List<long> { row.BlockReward0 }
                        : new List<long> { row.BlockReward0, row.BlockReward1 },
                    BlocksPerCommitment = row.BlocksPerCommitment,
                    BlocksPerCycle = row.BlocksPerCycle,
                    BlocksPerSnapshot = row.BlocksPerSnapshot,
                    BlocksPerVoting = row.BlocksPerVoting,
                    ByteCost = row.ByteCost,
                    EndorsementDeposit = row.EndorsementDeposit,
                    EndorsementReward = row.EndorsementReward1 == 0
                        ? new List<long> { row.EndorsementReward0 }
                        : new List<long> { row.EndorsementReward0, row.EndorsementReward1 },
                    EndorsersPerBlock = row.EndorsersPerBlock,
                    HardBlockGasLimit = row.HardBlockGasLimit,
                    HardOperationGasLimit = row.HardOperationGasLimit,
                    HardOperationStorageLimit = row.HardOperationStorageLimit,
                    OriginationSize = row.OriginationSize,
                    PreservedCycles = row.PreservedCycles,
                    RevelationReward = row.RevelationReward,
                    TimeBetweenBlocks = row.TimeBetweenBlocks,
                    TokensPerRoll = row.TokensPerRoll,
                    ProposalQuorum = row.ProposalQuorum / 100.0,
                    BallotQuorumMin = row.BallotQuorumMin / 100.0,
                    BallotQuorumMax = row.BallotQuorumMax / 100.0
                },
                Metadata = ProtocolMetadata[row.Hash]
            };
        }

        public async Task<Protocol> Get(string hash)
        {
            var sql = @"
                SELECT  *
                FROM    ""Protocols""
                WHERE   ""Hash"" = @hash::character(51)
                LIMIT   1";

            using var db = GetConnection();
            var row = await db.QueryFirstOrDefaultAsync(sql, new { hash });
            if (row == null) return null;

            return new Protocol
            {
                Code = row.Code,
                Hash = hash,
                FirstLevel = row.FirstLevel,
                LastLevel = row.LastLevel == -1 ? null : row.LastLevel,
                Constants = new ProtocolConstants
                {
                    RampUpCycles = row.RampUpCycles,
                    NoRewardCycles = row.NoRewardCycles,
                    BlockDeposit = row.BlockDeposit,
                    BlockReward = row.BlockReward1 == 0
                        ? new List<long> { row.BlockReward0 }
                        : new List<long> { row.BlockReward0, row.BlockReward1 },
                    BlocksPerCommitment = row.BlocksPerCommitment,
                    BlocksPerCycle = row.BlocksPerCycle,
                    BlocksPerSnapshot = row.BlocksPerSnapshot,
                    BlocksPerVoting = row.BlocksPerVoting,
                    ByteCost = row.ByteCost,
                    EndorsementDeposit = row.EndorsementDeposit,
                    EndorsementReward = row.EndorsementReward1 == 0
                        ? new List<long> { row.EndorsementReward0 }
                        : new List<long> { row.EndorsementReward0, row.EndorsementReward1 },
                    EndorsersPerBlock = row.EndorsersPerBlock,
                    HardBlockGasLimit = row.HardBlockGasLimit,
                    HardOperationGasLimit = row.HardOperationGasLimit,
                    HardOperationStorageLimit = row.HardOperationStorageLimit,
                    OriginationSize = row.OriginationSize,
                    PreservedCycles = row.PreservedCycles,
                    RevelationReward = row.RevelationReward,
                    TimeBetweenBlocks = row.TimeBetweenBlocks,
                    TokensPerRoll = row.TokensPerRoll,
                    ProposalQuorum = row.ProposalQuorum / 100.0,
                    BallotQuorumMin = row.BallotQuorumMin / 100.0,
                    BallotQuorumMax = row.BallotQuorumMax / 100.0
                },
                Metadata = ProtocolMetadata[row.Hash]
            };
        }

        public async Task<IEnumerable<Protocol>> Get(SortParameter sort, OffsetParameter offset, int limit)
        {
            var sql = new SqlBuilder(@"SELECT * FROM ""Protocols""")
                .Take(sort, offset, limit, x => x switch
                {
                    "code" => ("Id", "Code"),
                    "firstLevel" => ("Id", "FirstLevel"),
                    "lastLevel" => ("Id", "LastLevel"),
                    _ => ("Id", "Id")
                });

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            return rows.Select(row => new Protocol
            {
                Code = row.Code,
                Hash = row.Hash,
                FirstLevel = row.FirstLevel,
                LastLevel = row.LastLevel == -1 ? null : row.LastLevel,
                Constants = new ProtocolConstants
                {
                    RampUpCycles = row.RampUpCycles,
                    NoRewardCycles = row.NoRewardCycles,
                    BlockDeposit = row.BlockDeposit,
                    BlockReward = row.BlockReward1 == 0
                        ? new List<long> { row.BlockReward0 }
                        : new List<long> { row.BlockReward0, row.BlockReward1 },
                    BlocksPerCommitment = row.BlocksPerCommitment,
                    BlocksPerCycle = row.BlocksPerCycle,
                    BlocksPerSnapshot = row.BlocksPerSnapshot,
                    BlocksPerVoting = row.BlocksPerVoting,
                    ByteCost = row.ByteCost,
                    EndorsementDeposit = row.EndorsementDeposit,
                    EndorsementReward = row.EndorsementReward1 == 0
                        ? new List<long> { row.EndorsementReward0 }
                        : new List<long> { row.EndorsementReward0, row.EndorsementReward1 },
                    EndorsersPerBlock = row.EndorsersPerBlock,
                    HardBlockGasLimit = row.HardBlockGasLimit,
                    HardOperationGasLimit = row.HardOperationGasLimit,
                    HardOperationStorageLimit = row.HardOperationStorageLimit,
                    OriginationSize = row.OriginationSize,
                    PreservedCycles = row.PreservedCycles,
                    RevelationReward = row.RevelationReward,
                    TimeBetweenBlocks = row.TimeBetweenBlocks,
                    TokensPerRoll = row.TokensPerRoll,
                    ProposalQuorum = row.ProposalQuorum / 100.0,
                    BallotQuorumMin = row.BallotQuorumMin / 100.0,
                    BallotQuorumMax = row.BallotQuorumMax / 100.0
                },
                Metadata = ProtocolMetadata[row.Hash]
            });
        }
    }
}

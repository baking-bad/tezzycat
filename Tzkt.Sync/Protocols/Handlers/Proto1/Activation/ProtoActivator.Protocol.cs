﻿using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Tzkt.Data.Models;
using Tzkt.Sync.Utils;

namespace Tzkt.Sync.Protocols.Proto1
{
    partial class ProtoActivator : ProtocolCommit
    {
        public (Protocol, JToken) BootstrapProtocol(JsonElement rawBlock)
        {
            var protocol = new Protocol
            {
                Code = 1,
                Hash = rawBlock.Required("metadata").RequiredString("next_protocol"),
                FirstLevel = 2,
                LastLevel = -1
            };
            Db.Protocols.Add(protocol);
            Cache.Protocols.Add(protocol);

            var parameters = Bson.Parse(rawBlock
                .Required("header")
                .Required("content")
                .RequiredString("protocol_parameters")
                .Substring(8));

            SetParameters(protocol, parameters);
            return (protocol, parameters);
        }

        public async Task ClearProtocol()
        {
            await Db.Database.ExecuteSqlRawAsync(@"DELETE FROM ""Protocols"" WHERE ""Code"" = 1");
            Cache.Protocols.Reset();
        }

        protected virtual void SetParameters(Protocol protocol, JToken parameters)
        {
            protocol.RampUpCycles = parameters["security_deposit_ramp_up_cycles"]?.Value<int>() ?? 0;
            protocol.NoRewardCycles = parameters["no_reward_cycles"]?.Value<int>() ?? 0;
            protocol.BlockDeposit = parameters["block_security_deposit"]?.Value<long>() ?? 512_000_000;
            protocol.BlockReward0 = parameters["block_reward"]?.Value<long>() ?? 16_000_000;
            protocol.BlockReward1 = 0;
            protocol.BlocksPerCommitment = parameters["blocks_per_commitment"]?.Value<int>() ?? 32;
            protocol.BlocksPerCycle = parameters["blocks_per_cycle"]?.Value<int>() ?? 4096;
            protocol.BlocksPerSnapshot = parameters["blocks_per_roll_snapshot"]?.Value<int>() ?? 256;
            protocol.BlocksPerVoting = parameters["blocks_per_voting_period"]?.Value<int>() ?? 32_768;
            protocol.ByteCost = parameters["cost_per_byte"]?.Value<int>() ?? 1000;
            protocol.EndorsementDeposit = parameters["endorsement_security_deposit"]?.Value<long>() ?? 64_000_000;
            protocol.EndorsementReward0 = parameters["endorsement_reward"]?.Value<long>() ?? 2_000_000;
            protocol.EndorsementReward1 = 0;
            protocol.EndorsersPerBlock = parameters["endorsers_per_block"]?.Value<int>() ?? 32;
            protocol.HardBlockGasLimit = parameters["hard_gas_limit_per_block"]?.Value<int>() ?? 4_000_000;
            protocol.HardOperationGasLimit = parameters["hard_gas_limit_per_operation"]?.Value<int>() ?? 400_000;
            protocol.HardOperationStorageLimit = parameters["hard_storage_limit_per_operation"]?.Value<int>() ?? 60_000;
            protocol.OriginationSize = (parameters["origination_burn"]?.Value<int>() ?? 257_000) / protocol.ByteCost;
            protocol.PreservedCycles = parameters["preserved_cycles"]?.Value<int>() ?? 5;
            protocol.RevelationReward = parameters["seed_nonce_revelation_tip"]?.Value<long>() ?? 125_000;
            protocol.TimeBetweenBlocks = parameters["time_between_blocks"]?[0].Value<int>() ?? 60;
            protocol.TokensPerRoll = parameters["tokens_per_roll"]?.Value<long>() ?? 10_000_000_000;
            protocol.BallotQuorumMin = 0;
            protocol.BallotQuorumMax = 10000;
            protocol.ProposalQuorum = 0;
        }

        public async Task UpgradeProtocol(AppState state)
        {
            var protocol = new Protocol
            {
                Code = await Db.Protocols.CountAsync() - 1,
                Hash = state.NextProtocol,
                FirstLevel = state.Level + 1,
                LastLevel = -1,
                
            };
            Db.Protocols.Add(protocol);
            Cache.Protocols.Add(protocol);

            var prev = await Cache.Protocols.GetAsync(state.Protocol);
            Db.TryAttach(prev);
            prev.LastLevel = state.Level;

            UpgradeParameters(protocol, prev);
        }

        public async Task DowngradeProtocol(AppState state)
        {
            var current = await Cache.Protocols.GetAsync(state.NextProtocol);
            await Db.Database.ExecuteSqlRawAsync($@"DELETE FROM ""Protocols"" WHERE ""Code"" = {current.Code}");

            var prev = await Cache.Protocols.GetAsync(state.Protocol);
            Db.TryAttach(prev);
            prev.LastLevel = -1;

            Cache.Protocols.Reset();
        }

        protected virtual void UpgradeParameters(Protocol protocol, Protocol prev)
        {
            protocol.RampUpCycles = prev.RampUpCycles;
            protocol.NoRewardCycles = prev.NoRewardCycles;
            protocol.BlockDeposit = prev.BlockDeposit;
            protocol.BlockReward0 = prev.BlockReward0;
            protocol.BlockReward1 = prev.BlockReward1;
            protocol.BlocksPerCommitment = prev.BlocksPerCommitment;
            protocol.BlocksPerCycle = prev.BlocksPerCycle;
            protocol.BlocksPerSnapshot = prev.BlocksPerSnapshot;
            protocol.BlocksPerVoting = prev.BlocksPerVoting;
            protocol.ByteCost = prev.ByteCost;
            protocol.EndorsementDeposit = prev.EndorsementDeposit;
            protocol.EndorsementReward0 = prev.EndorsementReward0;
            protocol.EndorsementReward1 = prev.EndorsementReward1;
            protocol.EndorsersPerBlock = prev.EndorsersPerBlock;
            protocol.HardBlockGasLimit = prev.HardBlockGasLimit;
            protocol.HardOperationGasLimit = prev.HardOperationGasLimit;
            protocol.HardOperationStorageLimit = prev.HardOperationStorageLimit;
            protocol.OriginationSize = prev.OriginationSize;
            protocol.PreservedCycles = prev.PreservedCycles;
            protocol.RevelationReward = prev.RevelationReward;
            protocol.TimeBetweenBlocks = prev.TimeBetweenBlocks;
            protocol.TokensPerRoll = prev.TokensPerRoll;
            protocol.BallotQuorumMin = prev.BallotQuorumMin;
            protocol.BallotQuorumMax = prev.BallotQuorumMax;
            protocol.ProposalQuorum = prev.ProposalQuorum;
        }
    }
}

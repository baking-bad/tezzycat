﻿using System.Linq;
using System.Text.Json;
using Tzkt.Data.Models;

namespace Tzkt.Sync.Protocols.Proto9
{
    class RevelationPenaltyCommit : Proto6.RevelationPenaltyCommit
    {
        public RevelationPenaltyCommit(ProtocolHandler protocol) : base(protocol) { }

        protected override bool HasPanltiesUpdates(Block block, JsonElement rawBlock)
        {
            var cycle = (block.Level - 1) / block.Protocol.BlocksPerCycle;
            return rawBlock
                .Required("metadata")
                .RequiredArray("balance_updates")
                .EnumerateArray()
                .Where(x => x.RequiredString("origin")[0] == 'b')
                .Skip(cycle < block.Protocol.NoRewardCycles || rawBlock.Required("operations")[0].Count() == 0 ? 2 : 3)
                .Any(x => x.RequiredString("kind")[0] == 'f' && GetFreezerCycle(x) != cycle - block.Protocol.PreservedCycles);
        }
    }
}

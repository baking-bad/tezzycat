﻿using System.Text.Json;
using System.Threading.Tasks;
using Tzkt.Sync.Services;

namespace Tzkt.Sync.Protocols.Proto6
{
    class Rpc : Proto1.Rpc
    {
        public Rpc(TezosNode node) : base(node) { }

        public override Task<JsonElement> GetBakingRightsAsync(int level, int cycle)
            => Node.GetAsync($"chains/main/blocks/{level}/helpers/baking_rights?cycle={cycle}&max_priority=7&all=true");

        public override Task<JsonElement> GetLevelBakingRightsAsync(int level, int maxPriority)
            => Node.GetAsync($"chains/main/blocks/{level}/helpers/baking_rights?level={level}&max_priority={maxPriority}&all=true");
    }
}

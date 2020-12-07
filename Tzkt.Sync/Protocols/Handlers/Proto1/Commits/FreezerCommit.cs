﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tzkt.Data.Models;

namespace Tzkt.Sync.Protocols.Proto1
{
    class FreezerCommit : ProtocolCommit
    {
        public IEnumerable<JsonElement> FreezerUpdates { get; private set; }

        public FreezerCommit(ProtocolHandler protocol) : base(protocol) { }

        public virtual Task Apply(Block block, JsonElement rawBlock)
        {
            if (block.Events.HasFlag(BlockEvents.CycleEnd))
            {
                FreezerUpdates = GetFreezerUpdates(block, rawBlock);
            }

            if (FreezerUpdates == null) return Task.CompletedTask;

            foreach (var update in FreezerUpdates)
            {
                #region entities
                var delegat = Cache.Accounts.GetDelegate(update.RequiredString("delegate"));

                Db.TryAttach(delegat);
                #endregion

                var change = update.RequiredInt64("change");
                switch (update.RequiredString("category")[0])
                {
                    case 'd':
                        delegat.FrozenDeposits += change;
                        break;
                    case 'r':
                        delegat.FrozenRewards += change;
                        delegat.StakingBalance -= change;
                        break;
                    case 'f':
                        delegat.FrozenFees += change;
                        break;
                    default:
                        throw new Exception("unexpected freezer balance update type");
                }
            }

            return Task.CompletedTask;
        }

        public virtual async Task Revert(Block block)
        {
            if (block.Events.HasFlag(BlockEvents.CycleEnd))
            {
                var rawBlock = await Proto.Rpc.GetBlockAsync(block.Level);
                FreezerUpdates = GetFreezerUpdates(block, rawBlock);
            }

            if (FreezerUpdates == null) return;

            foreach (var update in FreezerUpdates)
            {
                #region entities
                var delegat = Cache.Accounts.GetDelegate(update.RequiredString("delegate"));

                Db.TryAttach(delegat);
                #endregion

                var change = update.RequiredInt64("change");
                switch (update.RequiredString("category")[0])
                {
                    case 'd':
                        delegat.FrozenDeposits -= change;
                        break;
                    case 'r':
                        delegat.FrozenRewards -= change;
                        delegat.StakingBalance += change;
                        break;
                    case 'f':
                        delegat.FrozenFees -= change;
                        break;
                    default:
                        throw new Exception("unexpected freezer balance update type");
                }
            }
        }

        public override Task Apply() => Task.CompletedTask;
        public override Task Revert() => Task.CompletedTask;

        protected virtual int GetFreezerCycle(JsonElement el) => el.RequiredInt32("level");

        protected virtual IEnumerable<JsonElement> GetFreezerUpdates(Block block, JsonElement rawBlock)
        {
            var cycle = (block.Level - 1) / block.Protocol.BlocksPerCycle;
            return rawBlock.Required("metadata").Required("balance_updates").EnumerateArray().Skip(block.Protocol.BlockReward0 > 0 ? 3 : 2)
                .Where(x => x.RequiredString("kind")[0] == 'f' && GetFreezerCycle(x) == cycle - block.Protocol.PreservedCycles);
        }
    }
}

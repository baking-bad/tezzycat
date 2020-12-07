﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Tzkt.Data.Models;

namespace Tzkt.Sync.Protocols.Proto1
{
    class BakingRightsCommit : ProtocolCommit
    {
        public List<BakingRight> CurrentRights { get; protected set; }
        public IEnumerable<JsonElement> FutureBakingRights { get; protected set; }
        public IEnumerable<JsonElement> FutureEndorsingRights { get; protected set; }

        public BakingRightsCommit(ProtocolHandler protocol) : base(protocol) { }

        public virtual async Task Apply(Block block)
        {
            var cycle = (block.Level - 1) / block.Protocol.BlocksPerCycle;

            #region current rights
            CurrentRights = await Cache.BakingRights.GetAsync(cycle, block.Level);
            var sql = string.Empty;

            // TODO: better use protocol of the block where the endorsing rights were generated
            if (block.Priority == 0 && block.Validations == block.Protocol.EndorsersPerBlock)
            {
                CurrentRights.RemoveAll(x => x.Type == BakingRightType.Baking && x.Priority > 0);
                CurrentRights.ForEach(x => x.Status = BakingRightStatus.Realized);

                sql = $@"
                    DELETE  FROM ""BakingRights""
                    WHERE   ""Level"" = {block.Level}
                    AND     ""Type"" = {(int)BakingRightType.Baking}
                    AND     ""Priority"" > 0;

                    UPDATE  ""BakingRights""
                    SET     ""Status"" = {(int)BakingRightStatus.Realized}
                    WHERE   ""Level"" = {block.Level};";
            }
            else
            {
                #region load missed priority
                var maxExistedPriority = CurrentRights
                    .Where(x => x.Type == BakingRightType.Baking)
                    .Select(x => x.Priority)
                    .Max();

                if (maxExistedPriority < block.Priority)
                {
                    var bakingRights = await Proto.Rpc.GetLevelBakingRightsAsync(block.Level, block.Priority);
                    //bakingRights = bakingRights.OrderBy(x => x.Priority);

                    var sqlInsert = @"
                        INSERT INTO ""BakingRights"" (""Cycle"", ""Level"", ""BakerId"", ""Type"", ""Status"", ""Priority"", ""Slots"") VALUES ";

                    foreach (var bakingRight in bakingRights.EnumerateArray().SkipWhile(x => x.RequiredInt32("priority") <= maxExistedPriority))
                    {
                        var delegat = Cache.Accounts.GetDelegateOrDefault(bakingRight.RequiredString("delegate"));
                        if (delegat == null) continue; // WTF: [level:28680] - Baking rights were given to non-baker account

                        sqlInsert += $@"
                            ({cycle}, {block.Level}, {delegat.Id}, {(int)BakingRightType.Baking}, {(int)BakingRightStatus.Future}, {bakingRight.RequiredInt32("priority")}, null),";
                    }

                    await Db.Database.ExecuteSqlRawAsync(sqlInsert[..^1]);

                    //TODO: execute sql with RETURNS to get identity
                    var addedRights = await Db.BakingRights
                        .Where(x => x.Level == block.Level && x.Type == BakingRightType.Baking && x.Priority > maxExistedPriority)
                        .ToListAsync();

                    CurrentRights.AddRange(addedRights);
                }
                #endregion

                #region remove excess
                if (CurrentRights.RemoveAll(x => x.Type == BakingRightType.Baking && x.Priority > block.Priority) > 0)
                {
                    sql += $@"
                        DELETE  FROM ""BakingRights""
                        WHERE   ""Level"" = {block.Level}
                        AND     ""Type"" = {(int)BakingRightType.Baking}
                        AND     ""Priority"" > {block.Priority};";
                }
                #endregion

                #region remove weird
                var weirdRights = CurrentRights
                    .Where(x => !Cache.Accounts.DelegateExists(x.BakerId))
                    .ToList();

                if (weirdRights.Count > 0)
                {
                    foreach (var wr in weirdRights)
                        CurrentRights.Remove(wr);

                    sql += $@"
                        DELETE  FROM ""BakingRights""
                        WHERE   ""Level"" = {block.Level}
                        AND     ""Id"" = ANY(ARRAY[{string.Join(',', weirdRights.Select(x => x.Id))}]);";
                }
                #endregion

                foreach (var cr in CurrentRights)
                    cr.Status = BakingRightStatus.Missed;

                CurrentRights.First(x => x.Priority == block.Priority).Status = BakingRightStatus.Realized;
                
                if (block.Endorsements != null)
                {
                    var endorsers = new HashSet<int>(block.Endorsements.Select(x => x.Delegate.Id));
                    foreach (var er in CurrentRights.Where(x => x.Type == BakingRightType.Endorsing && endorsers.Contains(x.BakerId)))
                        er.Status = BakingRightStatus.Realized;
                }

                foreach (var cr in CurrentRights.Where(x => x.Status == BakingRightStatus.Missed))
                {
                    var baker = Cache.Accounts.GetDelegate(cr.BakerId);
                    var available = baker.Balance - baker.FrozenDeposits - baker.FrozenRewards - baker.FrozenFees;
                    var required = cr.Type == BakingRightType.Baking ? block.Protocol.BlockDeposit : block.Protocol.EndorsementDeposit;

                    if (available < required)
                        cr.Status = BakingRightStatus.Uncovered;
                }

                var realized = CurrentRights.Where(x => x.Status == BakingRightStatus.Realized);
                var uncovered = CurrentRights.Where(x => x.Status == BakingRightStatus.Uncovered);
                var missed = CurrentRights.Where(x => x.Status == BakingRightStatus.Missed);

                sql += $@"
                    UPDATE  ""BakingRights""
                    SET     ""Status"" = {(int)BakingRightStatus.Realized}
                    WHERE   ""Level"" = {block.Level}
                    AND     ""Id"" = ANY(ARRAY[{string.Join(',', realized.Select(x => x.Id))}]);";

                if (uncovered.Any())
                {
                    sql += $@"
                        UPDATE  ""BakingRights""
                        SET     ""Status"" = {(int)BakingRightStatus.Uncovered}
                        WHERE   ""Level"" = {block.Level}
                        AND     ""Id"" = ANY(ARRAY[{string.Join(',', uncovered.Select(x => x.Id))}]);";
                }

                if (missed.Any())
                {
                    sql += $@"
                        UPDATE  ""BakingRights""
                        SET     ""Status"" = {(int)BakingRightStatus.Missed}
                        WHERE   ""Level"" = {block.Level}
                        AND     ""Id"" = ANY(ARRAY[{string.Join(',', missed.Select(x => x.Id))}]);";
                }
            }

            await Db.Database.ExecuteSqlRawAsync(sql);
            #endregion

            #region new cycle
            if (block.Events.HasFlag(BlockEvents.CycleBegin))
            {
                var futureCycle = cycle + block.Protocol.PreservedCycles;

                FutureBakingRights = (await Proto.Rpc.GetBakingRightsAsync(block.Level, futureCycle)).EnumerateArray();
                FutureEndorsingRights = (await Proto.Rpc.GetEndorsingRightsAsync(block.Level, futureCycle)).EnumerateArray();

                //foreach (var er in FutureEndorsingRights)
                //    if (!await Cache.Accounts.ExistsAsync(er.RequiredString("delegate")))
                //        throw new Exception($"Account {er.RequiredString("delegate")} doesn't exist");

                //foreach (var br in FutureBakingRights)
                //    if (!await Cache.Accounts.ExistsAsync(br.RequiredString("delegate")))
                //        throw new Exception($"Account {br.RequiredString("delegate")} doesn't exist");

                var conn = Db.Database.GetDbConnection() as NpgsqlConnection;
                using var writer = conn.BeginBinaryImport(@"COPY ""BakingRights"" (""Cycle"", ""Level"", ""BakerId"", ""Type"", ""Status"", ""Priority"", ""Slots"") FROM STDIN (FORMAT BINARY)");

                foreach (var er in FutureEndorsingRights)
                {
                    // WTF: [level:28680] - Baking rights were given to non-baker account
                    var acc = await Cache.Accounts.GetAsync(er.RequiredString("delegate"));
                    
                    writer.StartRow();
                    writer.Write(er.RequiredInt32("level") / block.Protocol.BlocksPerCycle, NpgsqlTypes.NpgsqlDbType.Integer); // level + 1 (shifted)
                    writer.Write(er.RequiredInt32("level") + 1, NpgsqlTypes.NpgsqlDbType.Integer);                             // level + 1 (shifted)
                    writer.Write(acc.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write((byte)BakingRightType.Endorsing, NpgsqlTypes.NpgsqlDbType.Smallint);
                    writer.Write((byte)BakingRightStatus.Future, NpgsqlTypes.NpgsqlDbType.Smallint);
                    writer.WriteNull();
                    writer.Write(er.RequiredArray("slots").Count(), NpgsqlTypes.NpgsqlDbType.Integer);
                }

                foreach (var br in FutureBakingRights)
                {
                    // WTF: [level:28680] - Baking rights were given to non-baker account
                    var acc = await Cache.Accounts.GetAsync(br.RequiredString("delegate"));

                    writer.StartRow();
                    writer.Write(futureCycle, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(br.RequiredInt32("level"), NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write(acc.Id, NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.Write((byte)BakingRightType.Baking, NpgsqlTypes.NpgsqlDbType.Smallint);
                    writer.Write((byte)BakingRightStatus.Future, NpgsqlTypes.NpgsqlDbType.Smallint);
                    writer.Write(br.RequiredInt32("priority"), NpgsqlTypes.NpgsqlDbType.Integer);
                    writer.WriteNull();
                }

                writer.Complete();
            }
            #endregion
        }

        public virtual async Task Revert(Block block)
        {
            block.Protocol ??= await Cache.Protocols.GetAsync(block.ProtoCode);
            var cycle = (block.Level - 1) / block.Protocol.BlocksPerCycle;

            #region current rights
            CurrentRights = await Cache.BakingRights.GetAsync(cycle, block.Level);

            foreach (var cr in CurrentRights)
                cr.Status = BakingRightStatus.Future;

            await Db.Database.ExecuteSqlRawAsync($@"
                UPDATE  ""BakingRights""
                SET     ""Status"" = {(int)BakingRightStatus.Future}
                WHERE   ""Level"" = {block.Level}");
            #endregion

            #region new cycle
            if (block.Events.HasFlag(BlockEvents.CycleBegin))
            {
                await Db.Database.ExecuteSqlRawAsync($@"
                    DELETE FROM ""BakingRights""
                    WHERE   ""Cycle"" = {cycle + block.Protocol.PreservedCycles} AND ""Type"" = 0
                    OR      ""Level"" > {(cycle + block.Protocol.PreservedCycles) * block.Protocol.BlocksPerCycle + 1}");
            }
            #endregion
        }

        public override Task Apply() => Task.CompletedTask;
        public override Task Revert() => Task.CompletedTask;
    }
}

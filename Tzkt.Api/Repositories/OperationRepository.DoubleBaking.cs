﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Tzkt.Api.Models;

namespace Tzkt.Api.Repositories
{
    public partial class OperationRepository : DbConnection
    {
        public async Task<int> GetDoubleBakingsCount(
            Int32Parameter level,
            DateTimeParameter timestamp)
        {
            var sql = new SqlBuilder(@"SELECT COUNT(*) FROM ""DoubleBakingOps""")
                .Filter("Level", level)
                .Filter("Timestamp", timestamp);

            using var db = GetConnection();
            return await db.QueryFirstAsync<int>(sql.Query, sql.Params);
        }

        public async Task<IEnumerable<DoubleBakingOperation>> GetDoubleBakings(string hash, Symbols quote)
        {
            var sql = @"
                SELECT      o.""Id"", o.""Level"", o.""Timestamp"", o.""AccusedLevel"", o.""AccuserId"", o.""AccuserReward"",
                            o.""OffenderId"", o.""OffenderLostDeposit"", o.""OffenderLostReward"", o.""OffenderLostFee"", b.""Hash""
                FROM        ""DoubleBakingOps"" as o
                INNER JOIN  ""Blocks"" as b 
                        ON  b.""Level"" = o.""Level""
                WHERE       o.""OpHash"" = @hash::character(51)
                LIMIT       1";

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql, new { hash });

            return rows.Select(row => new DoubleBakingOperation
            {
                Id = row.Id,
                Level = row.Level,
                Block = row.Hash,
                Timestamp = row.Timestamp,
                Hash = hash,
                AccusedLevel = row.AccusedLevel,
                Accuser = Accounts.GetAlias(row.AccuserId),
                AccuserRewards = row.AccuserReward,
                Offender = Accounts.GetAlias(row.OffenderId),
                OffenderLostDeposits = row.OffenderLostDeposit,
                OffenderLostRewards = row.OffenderLostReward,
                OffenderLostFees = row.OffenderLostFee,
                Quote = Quotes.Get(quote, row.Level)
            });
        }

        public async Task<IEnumerable<DoubleBakingOperation>> GetDoubleBakings(Block block, Symbols quote)
        {
            var sql = @"
                SELECT      ""Id"", ""Timestamp"", ""OpHash"", ""AccusedLevel"", ""AccuserId"", ""AccuserReward"",
                            ""OffenderId"", ""OffenderLostDeposit"", ""OffenderLostReward"", ""OffenderLostFee""
                FROM        ""DoubleBakingOps""
                WHERE       ""Level"" = @level
                ORDER BY    ""Id""";

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql, new { level = block.Level });

            return rows.Select(row => new DoubleBakingOperation
            {
                Id = row.Id,
                Level = block.Level,
                Block = block.Hash,
                Timestamp = row.Timestamp,
                Hash = row.OpHash,
                AccusedLevel = row.AccusedLevel,
                Accuser = Accounts.GetAlias(row.AccuserId),
                AccuserRewards = row.AccuserReward,
                Offender = Accounts.GetAlias(row.OffenderId),
                OffenderLostDeposits = row.OffenderLostDeposit,
                OffenderLostRewards = row.OffenderLostReward,
                OffenderLostFees = row.OffenderLostFee,
                Quote = Quotes.Get(quote, block.Level)
            });
        }

        public async Task<IEnumerable<DoubleBakingOperation>> GetDoubleBakings(
            AnyOfParameter anyof,
            AccountParameter accuser,
            AccountParameter offender,
            Int32Parameter level,
            DateTimeParameter timestamp,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            Symbols quote)
        {
            var sql = new SqlBuilder(@"SELECT o.*, b.""Hash"" FROM ""DoubleBakingOps"" AS o INNER JOIN ""Blocks"" as b ON b.""Level"" = o.""Level""")
                .Filter(anyof, x => x == "accuser" ? "AccuserId" : "OffenderId")
                .Filter("AccuserId", accuser, x => "OffenderId")
                .Filter("OffenderId", offender, x => "AccuserId")
                .FilterA(@"o.""Level""", level)
                .FilterA(@"o.""Timestamp""", timestamp)
                .Take(sort, offset, limit, x => x switch
                {
                    "level" => ("Id", "Level"),
                    "accusedLevel" => ("AccusedLevel", "AccusedLevel"),
                    "accuserRewards" => ("AccuserReward", "AccuserReward"),
                    "offenderLostDeposits" => ("OffenderLostDeposit", "OffenderLostDeposit"),
                    "offenderLostRewards" => ("OffenderLostReward", "OffenderLostReward"),
                    "offenderLostFees" => ("OffenderLostFee", "OffenderLostFee"),
                    _ => ("Id", "Id")
                }, "o");

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            return rows.Select(row => new DoubleBakingOperation
            {
                Id = row.Id,
                Level = row.Level,
                Block = row.Hash,
                Timestamp = row.Timestamp,
                Hash = row.OpHash,
                AccusedLevel = row.AccusedLevel,
                Accuser = Accounts.GetAlias(row.AccuserId),
                AccuserRewards = row.AccuserReward,
                Offender = Accounts.GetAlias(row.OffenderId),
                OffenderLostDeposits = row.OffenderLostDeposit,
                OffenderLostRewards = row.OffenderLostReward,
                OffenderLostFees = row.OffenderLostFee,
                Quote = Quotes.Get(quote, row.Level)
            });
        }

        public async Task<object[][]> GetDoubleBakings(
            AnyOfParameter anyof,
            AccountParameter accuser,
            AccountParameter offender,
            Int32Parameter level,
            DateTimeParameter timestamp,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            string[] fields,
            Symbols quote)
        {
            var columns = new HashSet<string>(fields.Length);
            var joins = new HashSet<string>(1);

            foreach (var field in fields)
            {
                switch (field)
                {
                    case "id": columns.Add(@"o.""Id"""); break;
                    case "level": columns.Add(@"o.""Level"""); break;
                    case "timestamp": columns.Add(@"o.""Timestamp"""); break;
                    case "hash": columns.Add(@"o.""OpHash"""); break;
                    case "accusedLevel": columns.Add(@"o.""AccusedLevel"""); break;
                    case "accuser": columns.Add(@"o.""AccuserId"""); break;
                    case "accuserRewards": columns.Add(@"o.""AccuserReward"""); break;
                    case "offender": columns.Add(@"o.""OffenderId"""); break;
                    case "offenderLostDeposits": columns.Add(@"o.""OffenderLostDeposit"""); break;
                    case "offenderLostRewards": columns.Add(@"o.""OffenderLostReward"""); break;
                    case "offenderLostFees": columns.Add(@"o.""OffenderLostFee"""); break;
                    case "block":
                        columns.Add(@"b.""Hash""");
                        joins.Add(@"INNER JOIN ""Blocks"" as b ON b.""Level"" = o.""Level""");
                        break;
                    case "quote": columns.Add(@"o.""Level"""); break;
                }
            }

            if (columns.Count == 0)
                return Array.Empty<object[]>();

            var sql = new SqlBuilder($@"SELECT {string.Join(',', columns)} FROM ""DoubleBakingOps"" as o {string.Join(' ', joins)}")
                .Filter(anyof, x => x == "accuser" ? "AccuserId" : "OffenderId")
                .Filter("AccuserId", accuser, x => "OffenderId")
                .Filter("OffenderId", offender, x => "AccuserId")
                .FilterA(@"o.""Level""", level)
                .FilterA(@"o.""Timestamp""", timestamp)
                .Take(sort, offset, limit, x => x switch
                {
                    "level" => ("Id", "Level"),
                    "accusedLevel" => ("AccusedLevel", "AccusedLevel"),
                    "accuserRewards" => ("AccuserReward", "AccuserReward"),
                    "offenderLostDeposits" => ("OffenderLostDeposit", "OffenderLostDeposit"),
                    "offenderLostRewards" => ("OffenderLostReward", "OffenderLostReward"),
                    "offenderLostFees" => ("OffenderLostFee", "OffenderLostFee"),
                    _ => ("Id", "Id")
                }, "o");

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            var result = new object[rows.Count()][];
            for (int i = 0; i < result.Length; i++)
                result[i] = new object[fields.Length];

            for (int i = 0, j = 0; i < fields.Length; j = 0, i++)
            {
                switch (fields[i])
                {
                    case "id":
                        foreach (var row in rows)
                            result[j++][i] = row.Id;
                        break;
                    case "level":
                        foreach (var row in rows)
                            result[j++][i] = row.Level;
                        break;
                    case "block":
                        foreach (var row in rows)
                            result[j++][i] = row.Hash;
                        break;
                    case "timestamp":
                        foreach (var row in rows)
                            result[j++][i] = row.Timestamp;
                        break;
                    case "hash":
                        foreach (var row in rows)
                            result[j++][i] = row.OpHash;
                        break;
                    case "accusedLevel":
                        foreach (var row in rows)
                            result[j++][i] = row.AccusedLevel;
                        break;
                    case "accuser":
                        foreach (var row in rows)
                            result[j++][i] = await Accounts.GetAliasAsync(row.AccuserId);
                        break;
                    case "accuserRewards":
                        foreach (var row in rows)
                            result[j++][i] = row.AccuserReward;
                        break;
                    case "offender":
                        foreach (var row in rows)
                            result[j++][i] = await Accounts.GetAliasAsync(row.OffenderId);
                        break;
                    case "offenderLostDeposits":
                        foreach (var row in rows)
                            result[j++][i] = row.OffenderLostDeposit;
                        break;
                    case "offenderLostRewards":
                        foreach (var row in rows)
                            result[j++][i] = row.OffenderLostReward;
                        break;
                    case "offenderLostFees":
                        foreach (var row in rows)
                            result[j++][i] = row.OffenderLostFee;
                        break;
                    case "quote":
                        foreach (var row in rows)
                            result[j++][i] = Quotes.Get(quote, row.Level);
                        break;
                }
            }

            return result;
        }

        public async Task<object[]> GetDoubleBakings(
            AnyOfParameter anyof,
            AccountParameter accuser,
            AccountParameter offender,
            Int32Parameter level,
            DateTimeParameter timestamp,
            SortParameter sort,
            OffsetParameter offset,
            int limit,
            string field,
            Symbols quote)
        {
            var columns = new HashSet<string>(1);
            var joins = new HashSet<string>(1);

            switch (field)
            {
                case "id": columns.Add(@"o.""Id"""); break;
                case "level": columns.Add(@"o.""Level"""); break;
                case "timestamp": columns.Add(@"o.""Timestamp"""); break;
                case "hash": columns.Add(@"o.""OpHash"""); break;
                case "accusedLevel": columns.Add(@"o.""AccusedLevel"""); break;
                case "accuser": columns.Add(@"o.""AccuserId"""); break;
                case "accuserRewards": columns.Add(@"o.""AccuserReward"""); break;
                case "offender": columns.Add(@"o.""OffenderId"""); break;
                case "offenderLostDeposits": columns.Add(@"o.""OffenderLostDeposit"""); break;
                case "offenderLostRewards": columns.Add(@"o.""OffenderLostReward"""); break;
                case "offenderLostFees": columns.Add(@"o.""OffenderLostFee"""); break;
                case "block":
                    columns.Add(@"b.""Hash""");
                    joins.Add(@"INNER JOIN ""Blocks"" as b ON b.""Level"" = o.""Level""");
                    break;
                case "quote": columns.Add(@"o.""Level"""); break;
            }

            if (columns.Count == 0)
                return Array.Empty<object>();

            var sql = new SqlBuilder($@"SELECT {string.Join(',', columns)} FROM ""DoubleBakingOps"" as o {string.Join(' ', joins)}")
                .Filter(anyof, x => x == "accuser" ? "AccuserId" : "OffenderId")
                .Filter("AccuserId", accuser, x => "OffenderId")
                .Filter("OffenderId", offender, x => "AccuserId")
                .FilterA(@"o.""Level""", level)
                .FilterA(@"o.""Timestamp""", timestamp)
                .Take(sort, offset, limit, x => x switch
                {
                    "level" => ("Id", "Level"),
                    "accusedLevel" => ("AccusedLevel", "AccusedLevel"),
                    "accuserRewards" => ("AccuserReward", "AccuserReward"),
                    "offenderLostDeposits" => ("OffenderLostDeposit", "OffenderLostDeposit"),
                    "offenderLostRewards" => ("OffenderLostReward", "OffenderLostReward"),
                    "offenderLostFees" => ("OffenderLostFee", "OffenderLostFee"),
                    _ => ("Id", "Id")
                }, "o");

            using var db = GetConnection();
            var rows = await db.QueryAsync(sql.Query, sql.Params);

            //TODO: optimize memory allocation
            var result = new object[rows.Count()];
            var j = 0;

            switch (field)
            {
                case "id":
                    foreach (var row in rows)
                        result[j++] = row.Id;
                    break;
                case "level":
                    foreach (var row in rows)
                        result[j++] = row.Level;
                    break;
                case "block":
                    foreach (var row in rows)
                        result[j++] = row.Hash;
                    break;
                case "timestamp":
                    foreach (var row in rows)
                        result[j++] = row.Timestamp;
                    break;
                case "hash":
                    foreach (var row in rows)
                        result[j++] = row.OpHash;
                    break;
                case "accusedLevel":
                    foreach (var row in rows)
                        result[j++] = row.AccusedLevel;
                    break;
                case "accuser":
                    foreach (var row in rows)
                        result[j++] = await Accounts.GetAliasAsync(row.AccuserId);
                    break;
                case "accuserRewards":
                    foreach (var row in rows)
                        result[j++] = row.AccuserReward;
                    break;
                case "offender":
                    foreach (var row in rows)
                        result[j++] = await Accounts.GetAliasAsync(row.OffenderId);
                    break;
                case "offenderLostDeposits":
                    foreach (var row in rows)
                        result[j++] = row.OffenderLostDeposit;
                    break;
                case "offenderLostRewards":
                    foreach (var row in rows)
                        result[j++] = row.OffenderLostReward;
                    break;
                case "offenderLostFees":
                    foreach (var row in rows)
                        result[j++] = row.OffenderLostFee;
                    break;
                case "quote":
                    foreach (var row in rows)
                        result[j++] = Quotes.Get(quote, row.Level);
                    break;
            }

            return result;
        }
    }
}

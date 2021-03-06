﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Tzkt.Sync.Protocols
{
    static class OperationErrors
    {
        public static string Parse(JsonElement errors)
        {
            if (errors.ValueKind == JsonValueKind.Undefined)
                return null;

            var res = new List<object>();
            
            foreach (var error in errors.EnumerateArray())
            {
                var id = error.GetProperty("id").GetString();
                var type = id.Substring(id.IndexOf('.', id.IndexOf('.') + 1) + 1);

                res.Add(type switch
                {
                    "contract.balance_too_low" => new
                    {
                        type,
                        balance = long.Parse(error.GetProperty("balance").GetString()),
                        required = long.Parse(error.GetProperty("amount").GetString())
                    },
                    "contract.manager.unregistered_delegate" => new
                    {
                        type,
                        @delegate = error.GetProperty("hash").GetString()
                    },
                    "contract.non_existing_contract" => new
                    {
                        type,
                        contract = error.GetProperty("contract").GetString()
                    },
                    _ => new { type }
                });
            }

            return JsonSerializer.Serialize(res);
        }
    }
}

﻿using System;
using Tzkt.Data.Models;

namespace Tzkt.Api
{
    static class AccountTypes
    {
        public const string User = "user";
        public const string Delegate = "delegate";
        public const string Contract = "contract";
        public const string Empty = "empty";

        public static bool TryParse(string value, out int res)
        {
            res = value switch
            {
                User => (int)AccountType.User,
                Delegate => (int)AccountType.Delegate,
                Contract => (int)AccountType.Contract,
                _ => -1
            };
            return res != -1;
        }

        public static string ToString(int value) => value switch
        {
            (int)AccountType.User => User,
            (int)AccountType.Delegate => Delegate,
            (int)AccountType.Contract => Contract,
            _ => throw new Exception("invalid account type value")
        };
    }
}

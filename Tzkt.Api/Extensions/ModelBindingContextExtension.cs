﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tzkt.Api
{
    static class ModelBindingContextExtension
    {
        public static bool TryGetInt32(this ModelBindingContext bindingContext, string name, ref bool hasValue, out int? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (!int.TryParse(valueObject.FirstValue, out var value))
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid integer value.");
                        return false;
                    }

                    hasValue = true;
                    result = value;
                }
            }

            return true;
        }

        public static bool TryGetInt32List(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<int> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<int>(rawValues.Length);

                    foreach (var rawValue in rawValues)
                    {
                        if (!int.TryParse(rawValue, out var value))
                        {
                            bindingContext.ModelState.TryAddModelError(name, "List contains invalid integer value.");
                            return false;
                        }
                        result.Add(value);
                    }
                }
            }

            return true;
        }

        public static bool TryGetInt64(this ModelBindingContext bindingContext, string name, ref bool hasValue, out long? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (!long.TryParse(valueObject.FirstValue, out var value))
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid integer value.");
                        return false;
                    }

                    hasValue = true;
                    result = value;
                }
            }

            return true;
        }

        public static bool TryGetInt64List(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<long> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<long>(rawValues.Length);

                    foreach (var rawValue in rawValues)
                    {
                        if (!long.TryParse(rawValue, out var value))
                        {
                            bindingContext.ModelState.TryAddModelError(name, "List contains invalid integer value.");
                            return false;
                        }
                        result.Add(value);
                    }
                }
            }

            return true;
        }

        public static bool TryGetDateTime(this ModelBindingContext bindingContext, string name, ref bool hasValue, out DateTime? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (!DateTimeOffset.TryParse(valueObject.FirstValue, out var value))
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid datetime value.");
                        return false;
                    }

                    hasValue = true;
                    result = value.DateTime;
                }
            }

            return true;
        }

        public static bool TryGetDateTimeList(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<DateTime> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<DateTime>(rawValues.Length);

                    foreach (var rawValue in rawValues)
                    {
                        if (!DateTimeOffset.TryParse(rawValue, out var value))
                        {
                            bindingContext.ModelState.TryAddModelError(name, "List contains invalid datetime value.");
                            return false;
                        }
                        result.Add(value.DateTime);
                    }
                }
            }

            return true;
        }

        public static bool TryGetAccount(this ModelBindingContext bindingContext, string name, ref bool hasValue, out string result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (!Regex.IsMatch(valueObject.FirstValue, "^[0-9A-z]{36}$"))
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid account address.");
                        return false;
                    }

                    hasValue = true;
                    result = valueObject.FirstValue;
                }
            }

            return true;
        }

        public static bool TryGetAccountList(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<string> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<string>(rawValues.Length);

                    foreach (var rawValue in rawValues)
                    {
                        if (!Regex.IsMatch(rawValue, "^[0-9A-z]{36}$"))
                        {
                            bindingContext.ModelState.TryAddModelError(name, "List contains invalid account address.");
                            return false;
                        }

                        result.Add(rawValue);
                    }
                }
            }

            return true;
        }

        public static bool TryGetProtocol(this ModelBindingContext bindingContext, string name, ref bool hasValue, out string result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (!Regex.IsMatch(valueObject.FirstValue, "^P[0-9A-z]{50}$"))
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid protocol hash.");
                        return false;
                    }

                    hasValue = true;
                    result = valueObject.FirstValue;
                }
            }

            return true;
        }

        public static bool TryGetProtocolList(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<string> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<string>(rawValues.Length);

                    foreach (var rawValue in rawValues)
                    {
                        if (!Regex.IsMatch(rawValue, "^P[0-9A-z]{50}$"))
                        {
                            bindingContext.ModelState.TryAddModelError(name, "List contains invalid protocol hash.");
                            return false;
                        }

                        result.Add(rawValue);
                    }
                }
            }

            return true;
        }

        public static bool TryGetAccountType(this ModelBindingContext bindingContext, string name, ref bool hasValue, out int? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (valueObject.FirstValue == AccountTypes.User)
                    {
                        hasValue = true;
                        result = 0;
                    }
                    else if (valueObject.FirstValue == AccountTypes.Delegate)
                    {
                        hasValue = true;
                        result = 1;
                    }
                    else if (valueObject.FirstValue == AccountTypes.Contract)
                    {
                        hasValue = true;
                        result = 2;
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid account type.");
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryGetBakingRightType(this ModelBindingContext bindingContext, string name, ref bool hasValue, out int? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (valueObject.FirstValue == "baking")
                    {
                        hasValue = true;
                        result = 0;
                    }
                    else if (valueObject.FirstValue == "endorsing")
                    {
                        hasValue = true;
                        result = 1;
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid baking right type.");
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryGetBakingRightStatus(this ModelBindingContext bindingContext, string name, ref bool hasValue, out int? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (valueObject.FirstValue == "future")
                    {
                        hasValue = true;
                        result = 0;
                    }
                    else if (valueObject.FirstValue == "realized")
                    {
                        hasValue = true;
                        result = 1;
                    }
                    else if (valueObject.FirstValue == "uncovered")
                    {
                        hasValue = true;
                        result = 2;
                    }
                    else if (valueObject.FirstValue == "missed")
                    {
                        hasValue = true;
                        result = 3;
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid baking right status.");
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryGetContractKind(this ModelBindingContext bindingContext, string name, ref bool hasValue, out int? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (valueObject.FirstValue == ContractKinds.Delegator)
                    {
                        hasValue = true;
                        result = 0;
                    }
                    else if (valueObject.FirstValue == ContractKinds.SmartContract)
                    {
                        hasValue = true;
                        result = 1;
                    }
                    else if (valueObject.FirstValue == ContractKinds.Asset)
                    {
                        hasValue = true;
                        result = 2;
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid contract kind.");
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryGetVoterStatus(this ModelBindingContext bindingContext, string name, ref bool hasValue, out int? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    switch (valueObject.FirstValue)
                    {
                        case VoterStatuses.None:
                            hasValue = true;
                            result = (int)Data.Models.VoterStatus.None;
                            break;
                        case VoterStatuses.Upvoted:
                            hasValue = true;
                            result = (int)Data.Models.VoterStatus.Upvoted;
                            break;
                        case VoterStatuses.VotedYay:
                            hasValue = true;
                            result = (int)Data.Models.VoterStatus.VotedYay;
                            break;
                        case VoterStatuses.VotedNay:
                            hasValue = true;
                            result = (int)Data.Models.VoterStatus.VotedNay;
                            break;
                        case VoterStatuses.VotedPass:
                            hasValue = true;
                            result = (int)Data.Models.VoterStatus.VotedPass;
                            break;
                        default:
                            bindingContext.ModelState.TryAddModelError(name, "Invalid voter status.");
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool TryGetVoterStatusList(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<int> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<int>(rawValues.Length);

                    foreach (var rawValue in rawValues)
                    {
                        switch (rawValue)
                        {
                            case VoterStatuses.None:
                                hasValue = true;
                                result.Add((int)Data.Models.VoterStatus.None);
                                break;
                            case VoterStatuses.Upvoted:
                                hasValue = true;
                                result.Add((int)Data.Models.VoterStatus.Upvoted);
                                break;
                            case VoterStatuses.VotedYay:
                                hasValue = true;
                                result.Add((int)Data.Models.VoterStatus.VotedYay);
                                break;
                            case VoterStatuses.VotedNay:
                                hasValue = true;
                                result.Add((int)Data.Models.VoterStatus.VotedNay);
                                break;
                            case VoterStatuses.VotedPass:
                                hasValue = true;
                                result.Add((int)Data.Models.VoterStatus.VotedPass);
                                break;
                            default:
                                bindingContext.ModelState.TryAddModelError(name, "List contains invalid voter status.");
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool TryGetMigrationKind(this ModelBindingContext bindingContext, string name, ref bool hasValue, out int? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (valueObject.FirstValue == MigrationKinds.Bootstrap)
                    {
                        hasValue = true;
                        result = 0;
                    }
                    else if (valueObject.FirstValue == MigrationKinds.ActivateDelegate)
                    {
                        hasValue = true;
                        result = 1;
                    }
                    else if (valueObject.FirstValue == MigrationKinds.Airdrop)
                    {
                        hasValue = true;
                        result = 2;
                    }
                    else if (valueObject.FirstValue == MigrationKinds.ProposalInvoice)
                    {
                        hasValue = true;
                        result = 3;
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid migration kind.");
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryGetMigrationKindList(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<int> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<int>(rawValues.Length);

                    foreach (var rawValue in rawValues)
                    {
                        if (rawValue == MigrationKinds.Bootstrap)
                        {
                            hasValue = true;
                            result.Add(0);
                        }
                        else if (rawValue == MigrationKinds.ActivateDelegate)
                        {
                            hasValue = true;
                            result.Add(1);
                        }
                        else if (rawValue == MigrationKinds.Airdrop)
                        {
                            hasValue = true;
                            result.Add(2);
                        }
                        else if (rawValue == MigrationKinds.ProposalInvoice)
                        {
                            hasValue = true;
                            result.Add(3);
                        }
                        else
                        {
                            bindingContext.ModelState.TryAddModelError(name, "List contains invalid migration kind.");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static bool TryGetOperationStatus(this ModelBindingContext bindingContext, string name, ref bool hasValue, out int? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    if (valueObject.FirstValue == "applied")
                    {
                        hasValue = true;
                        result = 1;
                    }
                    else if (valueObject.FirstValue == "failed")
                    {
                        hasValue = true;
                        result = 4;
                    }
                    else if (valueObject.FirstValue == "backtracked")
                    {
                        hasValue = true;
                        result = 2;
                    }
                    else if (valueObject.FirstValue == "skipped")
                    {
                        hasValue = true;
                        result = 3;
                    }
                    else
                    {
                        bindingContext.ModelState.TryAddModelError(name, "Invalid operation status.");
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryGetBool(this ModelBindingContext bindingContext, string name, ref bool hasValue, out bool? result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                result = !(valueObject.FirstValue == "false" || valueObject.FirstValue == "0");
                hasValue = true;
            }

            return true;
        }

        public static bool TryGetString(this ModelBindingContext bindingContext, string name, ref bool hasValue, out string result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    hasValue = true;
                    result = valueObject.FirstValue;
                }
            }

            return true;
        }

        public static bool TryGetStringList(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<string> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue
                        .Replace("\\,", "ъуъ")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<string>(rawValues.Length);

                    foreach (var rawValue in rawValues)
                        result.Add(rawValue.Replace("ъуъ", ","));
                }
            }

            return true;
        }

        public static bool TryGetStringListSimple(this ModelBindingContext bindingContext, string name, ref bool hasValue, out List<string> result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = new List<string>(rawValues);
                }
            }

            return true;
        }

        public static bool TryGetStringArray(this ModelBindingContext bindingContext, string name, ref bool hasValue, out string[] result)
        {
            result = null;
            var valueObject = bindingContext.ValueProvider.GetValue(name);

            if (valueObject != ValueProviderResult.None)
            {
                bindingContext.ModelState.SetModelValue(name, valueObject);
                if (!string.IsNullOrEmpty(valueObject.FirstValue))
                {
                    var rawValues = valueObject.FirstValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (rawValues.Length == 0)
                    {
                        bindingContext.ModelState.TryAddModelError(name, "List should contain at least one item.");
                        return false;
                    }

                    hasValue = true;
                    result = rawValues;
                }
            }

            return true;
        }
    }
}

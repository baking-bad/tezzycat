﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tzkt.Api.Models
{
    public class TransactionOperation : Operation
    {
        public override string Type => OpTypes.Transaction;

        public override int Id { get; set; }

        public int Level { get; set; }

        public DateTime Timestamp { get; set; }

        public string Hash { get; set; }

        public int Counter { get; set; }

        public Alias Initiator { get; set; }

        public Alias Sender { get; set; }

        public int? Nonce { get; set; }

        public int GasLimit { get; set; }

        public int GasUsed { get; set; }

        public int StorageLimit { get; set; }

        public int StorageUsed { get; set; }

        public long BakerFee { get; set; }

        public long StorageFee { get; set; }

        public long AllocationFee { get; set; }

        public Alias Target { get; set; }

        public long Amount { get; set; }

        public string Parameters { get; set; }

        public string Status { get; set; }

        public List<OperationError> Errors { get; set; }

        public bool HasInternals { get; set; }
    }
}

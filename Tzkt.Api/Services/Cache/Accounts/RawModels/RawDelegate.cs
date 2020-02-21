﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tzkt.Api.Services.Cache
{
    public class RawDelegate : RawUser
    {
        public override string Type => AccountTypes.Delegate;

        public int ActivationLevel { get; set; }
        public int DeactivationLevel { get; set; }

        public long FrozenDeposits { get; set; }
        public long FrozenRewards { get; set; }
        public long FrozenFees { get; set; }

        public int DelegatorsCount { get; set; }
        public long StakingBalance { get; set; }

        public int BlocksCount { get; set; }
        public int EndorsementsCount { get; set; }
        public int BallotsCount { get; set; }
        public int ProposalsCount { get; set; }
        public int DoubleBakingCount { get; set; }
        public int DoubleEndorsingCount { get; set; }
        public int NonceRevelationsCount { get; set; }
        public int RevelationPenaltiesCount { get; set; }
    }
}

﻿using System;
using Microsoft.EntityFrameworkCore;

using Tezzycat.Data.Models;

namespace Tezzycat.Data
{
    public class ViewContext : DbContext
    {
        public DbSet<AppState> AppState { get; set; }

        public DbSet<Block> Blocks { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<BalanceSnapshot> BalanceSnapshots { get; set; }

        public DbSet<BakingRight> BakingRights { get; set; }
        public DbSet<EndorsingRight> EndorsingRights { get; set; }

        public DbSet<BakerStat> BakerStats { get; set; }
        public DbSet<DelegatorStat> DelegatorStats { get; set; }
        public DbSet<CycleStat> CycleStats { get; set; }

        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<Protocol> Protocols { get; set; }

        public DbSet<ActivationOperation> ActivationOps { get; set; }
        public DbSet<BallotOperation> BallotOps { get; set; }
        public DbSet<DelegationOperation> DelegationOps { get; set; }
        public DbSet<DoubleBakingOperation> DoubleBakingOps { get; set; }
        public DbSet<DoubleEndorsingOperation> DoubleEndorsingOps { get; set; }
        public DbSet<EndorsementOperation> EndorsementOps { get; set; }
        public DbSet<NonceRevelationOperation> NonceRevelationOps { get; set; }
        public DbSet<OriginationOperation> OriginationOps { get; set; }
        public DbSet<ProposalOperation> ProposalOps { get; set; }
        public DbSet<RevealOperation> RevealOps { get; set; }
        public DbSet<TransactionOperation> TransactionOps { get; set; }

        public ViewContext(DbContextOptions<ViewContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
    }
}
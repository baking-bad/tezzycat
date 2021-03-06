﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Tzkt.Data.Models
{
    public class Protocol
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Hash { get; set; }
        public int FirstLevel { get; set; }
        public int LastLevel { get; set; }

        public int RampUpCycles { get; set; }
        public int NoRewardCycles { get; set; }

        public int PreservedCycles { get; set; }

        public int BlocksPerCycle { get; set; }
        public int BlocksPerCommitment { get; set; }
        public int BlocksPerSnapshot { get; set; }
        public int BlocksPerVoting { get; set; }

        public int TimeBetweenBlocks { get; set; }
       
        public int EndorsersPerBlock { get; set; }
        public int HardOperationGasLimit { get; set; }
        public int HardOperationStorageLimit { get; set; }
        public int HardBlockGasLimit { get; set; }

        public long TokensPerRoll { get; set; }
        public long RevelationReward { get; set; }

        public long BlockDeposit { get; set; }
        public long BlockReward0 { get; set; }
        public long BlockReward1 { get; set; }

        public long EndorsementDeposit { get; set; }
        public long EndorsementReward0 { get; set; }
        public long EndorsementReward1 { get; set; }

        public int OriginationSize { get; set; }
        public int ByteCost { get; set; }

        public int ProposalQuorum { get; set; }
        public int BallotQuorumMin { get; set; }
        public int BallotQuorumMax { get; set; }

        public string Metadata { get; set; }
    }

    public static class ProtocolModel
    {
        public static void BuildProtocolModel(this ModelBuilder modelBuilder)
        {
            #region keys
            modelBuilder.Entity<Protocol>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Protocol>()
                .HasAlternateKey(x => x.Code);
            #endregion

            #region props
            modelBuilder.Entity<Protocol>()
                .Property(x => x.Hash)
                .IsFixedLength(true)
                .HasMaxLength(51)
                .IsRequired();

            modelBuilder.Entity<Protocol>()
                .Property(x => x.Metadata)
                .HasColumnType("jsonb");

            modelBuilder.Entity<Protocol>()
                .Property(x => x.Metadata)
                .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            #endregion
        }
    }
}

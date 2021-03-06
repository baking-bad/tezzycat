﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Tzkt.Data.Models
{
    public class MigrationOperation
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public DateTime Timestamp { get; set; }

        public int AccountId { get; set; }
        public MigrationKind Kind { get; set; }
        public long BalanceChange { get; set; }

        public int? OldScriptId { get; set; }
        public int? OldStorageId { get; set; }
        public int? NewScriptId { get; set; }
        public int? NewStorageId { get; set; }

        #region relations
        [ForeignKey(nameof(Level))]
        public Block Block { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; }

        [ForeignKey(nameof(OldScriptId))]
        public Script OldScript { get; set; }

        [ForeignKey(nameof(OldStorageId))]
        public Storage OldStorage { get; set; }

        [ForeignKey(nameof(NewScriptId))]
        public Script NewScript { get; set; }

        [ForeignKey(nameof(NewStorageId))]
        public Storage NewStorage { get; set; }
        #endregion
    }

    public enum MigrationKind
    {
        Bootstrap,
        ActivateDelegate,
        AirDrop,
        ProposalInvoice,
        CodeChange
    }

    public static class MigrationOperationModel
    {
        public static void BuildMigrationOperationModel(this ModelBuilder modelBuilder)
        {
            #region indexes
            modelBuilder.Entity<MigrationOperation>()
                .HasIndex(x => x.Level);

            modelBuilder.Entity<MigrationOperation>()
                .HasIndex(x => x.AccountId);
            #endregion
            
            #region keys
            modelBuilder.Entity<MigrationOperation>()
                .HasKey(x => x.Id);
            #endregion

            #region relations
            modelBuilder.Entity<MigrationOperation>()
                .HasOne(x => x.Block)
                .WithMany(x => x.Migrations)
                .HasForeignKey(x => x.Level)
                .HasPrincipalKey(x => x.Level);
            #endregion
        }
    }
}

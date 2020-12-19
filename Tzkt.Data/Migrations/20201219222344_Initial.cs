﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Tzkt.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppState",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KnownHead = table.Column<int>(nullable: false),
                    LastSync = table.Column<DateTime>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Protocol = table.Column<string>(nullable: true),
                    NextProtocol = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    VotingEpoch = table.Column<int>(nullable: false),
                    VotingPeriod = table.Column<int>(nullable: false),
                    AccountCounter = table.Column<int>(nullable: false),
                    OperationCounter = table.Column<int>(nullable: false),
                    ManagerCounter = table.Column<int>(nullable: false),
                    CommitmentsCount = table.Column<int>(nullable: false),
                    AccountsCount = table.Column<int>(nullable: false),
                    BlocksCount = table.Column<int>(nullable: false),
                    ProtocolsCount = table.Column<int>(nullable: false),
                    ActivationOpsCount = table.Column<int>(nullable: false),
                    BallotOpsCount = table.Column<int>(nullable: false),
                    DelegationOpsCount = table.Column<int>(nullable: false),
                    DoubleBakingOpsCount = table.Column<int>(nullable: false),
                    DoubleEndorsingOpsCount = table.Column<int>(nullable: false),
                    EndorsementOpsCount = table.Column<int>(nullable: false),
                    NonceRevelationOpsCount = table.Column<int>(nullable: false),
                    OriginationOpsCount = table.Column<int>(nullable: false),
                    ProposalOpsCount = table.Column<int>(nullable: false),
                    RevealOpsCount = table.Column<int>(nullable: false),
                    TransactionOpsCount = table.Column<int>(nullable: false),
                    MigrationOpsCount = table.Column<int>(nullable: false),
                    RevelationPenaltyOpsCount = table.Column<int>(nullable: false),
                    ProposalsCount = table.Column<int>(nullable: false),
                    CyclesCount = table.Column<int>(nullable: false),
                    QuoteLevel = table.Column<int>(nullable: false),
                    QuoteBtc = table.Column<double>(nullable: false),
                    QuoteEur = table.Column<double>(nullable: false),
                    QuoteUsd = table.Column<double>(nullable: false),
                    QuoteCny = table.Column<double>(nullable: false),
                    QuoteJpy = table.Column<double>(nullable: false),
                    QuoteKrw = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BakerCycles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cycle = table.Column<int>(nullable: false),
                    BakerId = table.Column<int>(nullable: false),
                    Rolls = table.Column<int>(nullable: false),
                    StakingBalance = table.Column<long>(nullable: false),
                    DelegatedBalance = table.Column<long>(nullable: false),
                    DelegatorsCount = table.Column<int>(nullable: false),
                    FutureBlocks = table.Column<int>(nullable: false),
                    OwnBlocks = table.Column<int>(nullable: false),
                    ExtraBlocks = table.Column<int>(nullable: false),
                    MissedOwnBlocks = table.Column<int>(nullable: false),
                    MissedExtraBlocks = table.Column<int>(nullable: false),
                    UncoveredOwnBlocks = table.Column<int>(nullable: false),
                    UncoveredExtraBlocks = table.Column<int>(nullable: false),
                    FutureEndorsements = table.Column<int>(nullable: false),
                    Endorsements = table.Column<int>(nullable: false),
                    MissedEndorsements = table.Column<int>(nullable: false),
                    UncoveredEndorsements = table.Column<int>(nullable: false),
                    FutureBlockRewards = table.Column<long>(nullable: false),
                    OwnBlockRewards = table.Column<long>(nullable: false),
                    ExtraBlockRewards = table.Column<long>(nullable: false),
                    MissedOwnBlockRewards = table.Column<long>(nullable: false),
                    MissedExtraBlockRewards = table.Column<long>(nullable: false),
                    UncoveredOwnBlockRewards = table.Column<long>(nullable: false),
                    UncoveredExtraBlockRewards = table.Column<long>(nullable: false),
                    FutureEndorsementRewards = table.Column<long>(nullable: false),
                    EndorsementRewards = table.Column<long>(nullable: false),
                    MissedEndorsementRewards = table.Column<long>(nullable: false),
                    UncoveredEndorsementRewards = table.Column<long>(nullable: false),
                    OwnBlockFees = table.Column<long>(nullable: false),
                    ExtraBlockFees = table.Column<long>(nullable: false),
                    MissedOwnBlockFees = table.Column<long>(nullable: false),
                    MissedExtraBlockFees = table.Column<long>(nullable: false),
                    UncoveredOwnBlockFees = table.Column<long>(nullable: false),
                    UncoveredExtraBlockFees = table.Column<long>(nullable: false),
                    DoubleBakingRewards = table.Column<long>(nullable: false),
                    DoubleBakingLostDeposits = table.Column<long>(nullable: false),
                    DoubleBakingLostRewards = table.Column<long>(nullable: false),
                    DoubleBakingLostFees = table.Column<long>(nullable: false),
                    DoubleEndorsingRewards = table.Column<long>(nullable: false),
                    DoubleEndorsingLostDeposits = table.Column<long>(nullable: false),
                    DoubleEndorsingLostRewards = table.Column<long>(nullable: false),
                    DoubleEndorsingLostFees = table.Column<long>(nullable: false),
                    RevelationRewards = table.Column<long>(nullable: false),
                    RevelationLostRewards = table.Column<long>(nullable: false),
                    RevelationLostFees = table.Column<long>(nullable: false),
                    FutureBlockDeposits = table.Column<long>(nullable: false),
                    BlockDeposits = table.Column<long>(nullable: false),
                    FutureEndorsementDeposits = table.Column<long>(nullable: false),
                    EndorsementDeposits = table.Column<long>(nullable: false),
                    ExpectedBlocks = table.Column<double>(nullable: false),
                    ExpectedEndorsements = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakerCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BakingRights",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cycle = table.Column<int>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    BakerId = table.Column<int>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    Priority = table.Column<int>(nullable: true),
                    Slots = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BakingRights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Commitments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(fixedLength: true, maxLength: 37, nullable: false),
                    Balance = table.Column<long>(nullable: false),
                    AccountId = table.Column<int>(nullable: true),
                    Level = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commitments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cycles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Index = table.Column<int>(nullable: false),
                    SnapshotIndex = table.Column<int>(nullable: false),
                    SnapshotLevel = table.Column<int>(nullable: false),
                    TotalRolls = table.Column<int>(nullable: false),
                    TotalStaking = table.Column<long>(nullable: false),
                    TotalDelegated = table.Column<long>(nullable: false),
                    TotalDelegators = table.Column<int>(nullable: false),
                    TotalBakers = table.Column<int>(nullable: false),
                    Seed = table.Column<string>(fixedLength: true, maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cycles", x => x.Id);
                    table.UniqueConstraint("AK_Cycles_Index", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "DelegatorCycles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cycle = table.Column<int>(nullable: false),
                    DelegatorId = table.Column<int>(nullable: false),
                    BakerId = table.Column<int>(nullable: false),
                    Balance = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegatorCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proposals",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: true),
                    InitiatorId = table.Column<int>(nullable: false),
                    FirstPeriod = table.Column<int>(nullable: false),
                    LastPeriod = table.Column<int>(nullable: false),
                    Epoch = table.Column<int>(nullable: false),
                    Upvotes = table.Column<int>(nullable: false),
                    Rolls = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proposals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Protocols",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    FirstLevel = table.Column<int>(nullable: false),
                    LastLevel = table.Column<int>(nullable: false),
                    RampUpCycles = table.Column<int>(nullable: false),
                    NoRewardCycles = table.Column<int>(nullable: false),
                    PreservedCycles = table.Column<int>(nullable: false),
                    BlocksPerCycle = table.Column<int>(nullable: false),
                    BlocksPerCommitment = table.Column<int>(nullable: false),
                    BlocksPerSnapshot = table.Column<int>(nullable: false),
                    BlocksPerVoting = table.Column<int>(nullable: false),
                    TimeBetweenBlocks = table.Column<int>(nullable: false),
                    EndorsersPerBlock = table.Column<int>(nullable: false),
                    HardOperationGasLimit = table.Column<int>(nullable: false),
                    HardOperationStorageLimit = table.Column<int>(nullable: false),
                    HardBlockGasLimit = table.Column<int>(nullable: false),
                    TokensPerRoll = table.Column<long>(nullable: false),
                    RevelationReward = table.Column<long>(nullable: false),
                    BlockDeposit = table.Column<long>(nullable: false),
                    BlockReward0 = table.Column<long>(nullable: false),
                    BlockReward1 = table.Column<long>(nullable: false),
                    EndorsementDeposit = table.Column<long>(nullable: false),
                    EndorsementReward0 = table.Column<long>(nullable: false),
                    EndorsementReward1 = table.Column<long>(nullable: false),
                    OriginationSize = table.Column<int>(nullable: false),
                    ByteCost = table.Column<int>(nullable: false),
                    ProposalQuorum = table.Column<int>(nullable: false),
                    BallotQuorumMin = table.Column<int>(nullable: false),
                    BallotQuorumMax = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protocols", x => x.Id);
                    table.UniqueConstraint("AK_Protocols_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Btc = table.Column<double>(nullable: false),
                    Eur = table.Column<double>(nullable: false),
                    Usd = table.Column<double>(nullable: false),
                    Cny = table.Column<double>(nullable: false),
                    Jpy = table.Column<double>(nullable: false),
                    Krw = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SnapshotBalances",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Balance = table.Column<long>(nullable: false),
                    AccountId = table.Column<int>(nullable: false),
                    DelegateId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotBalances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Software",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlocksCount = table.Column<int>(nullable: false),
                    FirstLevel = table.Column<int>(nullable: false),
                    LastLevel = table.Column<int>(nullable: false),
                    ShortHash = table.Column<string>(fixedLength: true, maxLength: 8, nullable: false),
                    CommitDate = table.Column<DateTime>(nullable: true),
                    CommitHash = table.Column<string>(fixedLength: true, maxLength: 40, nullable: true),
                    Version = table.Column<string>(nullable: true),
                    Tags = table.Column<List<string>>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Software", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Cycle = table.Column<int>(nullable: true),
                    Date = table.Column<DateTime>(nullable: true),
                    TotalBootstrapped = table.Column<long>(nullable: false),
                    TotalCommitments = table.Column<long>(nullable: false),
                    TotalActivated = table.Column<long>(nullable: false),
                    TotalCreated = table.Column<long>(nullable: false),
                    TotalBurned = table.Column<long>(nullable: false),
                    TotalVested = table.Column<long>(nullable: false),
                    TotalFrozen = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VotingPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Index = table.Column<int>(nullable: false),
                    Epoch = table.Column<int>(nullable: false),
                    FirstLevel = table.Column<int>(nullable: false),
                    LastLevel = table.Column<int>(nullable: false),
                    Kind = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    TotalBakers = table.Column<int>(nullable: true),
                    TotalRolls = table.Column<int>(nullable: true),
                    UpvotesQuorum = table.Column<int>(nullable: true),
                    ProposalsCount = table.Column<int>(nullable: true),
                    TopUpvotes = table.Column<int>(nullable: true),
                    TopRolls = table.Column<int>(nullable: true),
                    ParticipationEma = table.Column<int>(nullable: true),
                    BallotsQuorum = table.Column<int>(nullable: true),
                    Supermajority = table.Column<int>(nullable: true),
                    YayBallots = table.Column<int>(nullable: true),
                    YayRolls = table.Column<int>(nullable: true),
                    NayBallots = table.Column<int>(nullable: true),
                    NayRolls = table.Column<int>(nullable: true),
                    PassBallots = table.Column<int>(nullable: true),
                    PassRolls = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingPeriods", x => x.Id);
                    table.UniqueConstraint("AK_VotingPeriods_Index", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "VotingSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Period = table.Column<int>(nullable: false),
                    BakerId = table.Column<int>(nullable: false),
                    Rolls = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivationOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    AccountId = table.Column<int>(nullable: false),
                    Balance = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivationOps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BallotOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    Epoch = table.Column<int>(nullable: false),
                    Period = table.Column<int>(nullable: false),
                    ProposalId = table.Column<int>(nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    Rolls = table.Column<int>(nullable: false),
                    Vote = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BallotOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BallotOps_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    ProtoCode = table.Column<int>(nullable: false),
                    SoftwareId = table.Column<int>(nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    Validations = table.Column<int>(nullable: false),
                    Events = table.Column<int>(nullable: false),
                    Operations = table.Column<int>(nullable: false),
                    Deposit = table.Column<long>(nullable: false),
                    Reward = table.Column<long>(nullable: false),
                    Fees = table.Column<long>(nullable: false),
                    BakerId = table.Column<int>(nullable: true),
                    RevelationId = table.Column<int>(nullable: true),
                    ResetDeactivation = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.UniqueConstraint("AK_Blocks_Level", x => x.Level);
                    table.ForeignKey(
                        name: "FK_Blocks_Protocols_ProtoCode",
                        column: x => x.ProtoCode,
                        principalTable: "Protocols",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Blocks_Software_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Software",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Address = table.Column<string>(fixedLength: true, maxLength: 36, nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    FirstLevel = table.Column<int>(nullable: false),
                    LastLevel = table.Column<int>(nullable: false),
                    Balance = table.Column<long>(nullable: false),
                    Counter = table.Column<int>(nullable: false),
                    ContractsCount = table.Column<int>(nullable: false),
                    DelegationsCount = table.Column<int>(nullable: false),
                    OriginationsCount = table.Column<int>(nullable: false),
                    TransactionsCount = table.Column<int>(nullable: false),
                    RevealsCount = table.Column<int>(nullable: false),
                    MigrationsCount = table.Column<int>(nullable: false),
                    DelegateId = table.Column<int>(nullable: true),
                    DelegationLevel = table.Column<int>(nullable: true),
                    Staked = table.Column<bool>(nullable: false),
                    Kind = table.Column<byte>(nullable: true),
                    Spendable = table.Column<bool>(nullable: true),
                    CreatorId = table.Column<int>(nullable: true),
                    ManagerId = table.Column<int>(nullable: true),
                    WeirdDelegateId = table.Column<int>(nullable: true),
                    Activated = table.Column<bool>(nullable: true),
                    PublicKey = table.Column<string>(maxLength: 55, nullable: true),
                    Revealed = table.Column<bool>(nullable: true),
                    ActivationLevel = table.Column<int>(nullable: true),
                    DeactivationLevel = table.Column<int>(nullable: true),
                    FrozenDeposits = table.Column<long>(nullable: true),
                    FrozenRewards = table.Column<long>(nullable: true),
                    FrozenFees = table.Column<long>(nullable: true),
                    DelegatorsCount = table.Column<int>(nullable: true),
                    StakingBalance = table.Column<long>(nullable: true),
                    BlocksCount = table.Column<int>(nullable: true),
                    EndorsementsCount = table.Column<int>(nullable: true),
                    BallotsCount = table.Column<int>(nullable: true),
                    ProposalsCount = table.Column<int>(nullable: true),
                    DoubleBakingCount = table.Column<int>(nullable: true),
                    DoubleEndorsingCount = table.Column<int>(nullable: true),
                    NonceRevelationsCount = table.Column<int>(nullable: true),
                    RevelationPenaltiesCount = table.Column<int>(nullable: true),
                    SoftwareId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Accounts_DelegateId",
                        column: x => x.DelegateId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accounts_Blocks_FirstLevel",
                        column: x => x.FirstLevel,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accounts_Accounts_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accounts_Accounts_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accounts_Accounts_WeirdDelegateId",
                        column: x => x.WeirdDelegateId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accounts_Software_SoftwareId",
                        column: x => x.SoftwareId,
                        principalTable: "Software",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DelegationOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    Counter = table.Column<int>(nullable: false),
                    BakerFee = table.Column<long>(nullable: false),
                    StorageFee = table.Column<long>(nullable: true),
                    AllocationFee = table.Column<long>(nullable: true),
                    GasLimit = table.Column<int>(nullable: false),
                    GasUsed = table.Column<int>(nullable: false),
                    StorageLimit = table.Column<int>(nullable: false),
                    StorageUsed = table.Column<int>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    Errors = table.Column<string>(nullable: true),
                    InitiatorId = table.Column<int>(nullable: true),
                    Nonce = table.Column<int>(nullable: true),
                    DelegateId = table.Column<int>(nullable: true),
                    PrevDelegateId = table.Column<int>(nullable: true),
                    ResetDeactivation = table.Column<int>(nullable: true),
                    Amount = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegationOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DelegationOps_Accounts_DelegateId",
                        column: x => x.DelegateId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DelegationOps_Accounts_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DelegationOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DelegationOps_Accounts_PrevDelegateId",
                        column: x => x.PrevDelegateId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DelegationOps_Accounts_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoubleBakingOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    AccusedLevel = table.Column<int>(nullable: false),
                    AccuserId = table.Column<int>(nullable: false),
                    AccuserReward = table.Column<long>(nullable: false),
                    OffenderId = table.Column<int>(nullable: false),
                    OffenderLostDeposit = table.Column<long>(nullable: false),
                    OffenderLostReward = table.Column<long>(nullable: false),
                    OffenderLostFee = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoubleBakingOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoubleBakingOps_Accounts_AccuserId",
                        column: x => x.AccuserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoubleBakingOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoubleBakingOps_Accounts_OffenderId",
                        column: x => x.OffenderId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoubleEndorsingOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    AccusedLevel = table.Column<int>(nullable: false),
                    AccuserId = table.Column<int>(nullable: false),
                    AccuserReward = table.Column<long>(nullable: false),
                    OffenderId = table.Column<int>(nullable: false),
                    OffenderLostDeposit = table.Column<long>(nullable: false),
                    OffenderLostReward = table.Column<long>(nullable: false),
                    OffenderLostFee = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoubleEndorsingOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoubleEndorsingOps_Accounts_AccuserId",
                        column: x => x.AccuserId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoubleEndorsingOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoubleEndorsingOps_Accounts_OffenderId",
                        column: x => x.OffenderId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EndorsementOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    DelegateId = table.Column<int>(nullable: false),
                    Slots = table.Column<int>(nullable: false),
                    Reward = table.Column<long>(nullable: false),
                    Deposit = table.Column<long>(nullable: false),
                    ResetDeactivation = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EndorsementOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EndorsementOps_Accounts_DelegateId",
                        column: x => x.DelegateId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EndorsementOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MigrationOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    AccountId = table.Column<int>(nullable: false),
                    Kind = table.Column<int>(nullable: false),
                    BalanceChange = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MigrationOps_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MigrationOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NonceRevelationOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    BakerId = table.Column<int>(nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    RevealedLevel = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonceRevelationOps", x => x.Id);
                    table.UniqueConstraint("AK_NonceRevelationOps_RevealedLevel", x => x.RevealedLevel);
                    table.ForeignKey(
                        name: "FK_NonceRevelationOps_Accounts_BakerId",
                        column: x => x.BakerId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NonceRevelationOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NonceRevelationOps_Accounts_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OriginationOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    Counter = table.Column<int>(nullable: false),
                    BakerFee = table.Column<long>(nullable: false),
                    StorageFee = table.Column<long>(nullable: true),
                    AllocationFee = table.Column<long>(nullable: true),
                    GasLimit = table.Column<int>(nullable: false),
                    GasUsed = table.Column<int>(nullable: false),
                    StorageLimit = table.Column<int>(nullable: false),
                    StorageUsed = table.Column<int>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    Errors = table.Column<string>(nullable: true),
                    InitiatorId = table.Column<int>(nullable: true),
                    Nonce = table.Column<int>(nullable: true),
                    ManagerId = table.Column<int>(nullable: true),
                    DelegateId = table.Column<int>(nullable: true),
                    ContractId = table.Column<int>(nullable: true),
                    Balance = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OriginationOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OriginationOps_Accounts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OriginationOps_Accounts_DelegateId",
                        column: x => x.DelegateId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OriginationOps_Accounts_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OriginationOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OriginationOps_Accounts_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OriginationOps_Accounts_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposalOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    Epoch = table.Column<int>(nullable: false),
                    Period = table.Column<int>(nullable: false),
                    ProposalId = table.Column<int>(nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    Rolls = table.Column<int>(nullable: false),
                    Duplicated = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProposalOps_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProposalOps_Accounts_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevealOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    Counter = table.Column<int>(nullable: false),
                    BakerFee = table.Column<long>(nullable: false),
                    StorageFee = table.Column<long>(nullable: true),
                    AllocationFee = table.Column<long>(nullable: true),
                    GasLimit = table.Column<int>(nullable: false),
                    GasUsed = table.Column<int>(nullable: false),
                    StorageLimit = table.Column<int>(nullable: false),
                    StorageUsed = table.Column<int>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    Errors = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevealOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevealOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevealOps_Accounts_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RevelationPenaltyOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    BakerId = table.Column<int>(nullable: false),
                    MissedLevel = table.Column<int>(nullable: false),
                    LostReward = table.Column<long>(nullable: false),
                    LostFees = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevelationPenaltyOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RevelationPenaltyOps_Accounts_BakerId",
                        column: x => x.BakerId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RevelationPenaltyOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionOps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    OpHash = table.Column<string>(fixedLength: true, maxLength: 51, nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    Counter = table.Column<int>(nullable: false),
                    BakerFee = table.Column<long>(nullable: false),
                    StorageFee = table.Column<long>(nullable: true),
                    AllocationFee = table.Column<long>(nullable: true),
                    GasLimit = table.Column<int>(nullable: false),
                    GasUsed = table.Column<int>(nullable: false),
                    StorageLimit = table.Column<int>(nullable: false),
                    StorageUsed = table.Column<int>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    Errors = table.Column<string>(nullable: true),
                    InitiatorId = table.Column<int>(nullable: true),
                    Nonce = table.Column<int>(nullable: true),
                    TargetId = table.Column<int>(nullable: true),
                    ResetDeactivation = table.Column<int>(nullable: true),
                    Amount = table.Column<long>(nullable: false),
                    Parameters = table.Column<string>(nullable: true),
                    InternalOperations = table.Column<byte>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionOps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionOps_Accounts_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransactionOps_Blocks_Level",
                        column: x => x.Level,
                        principalTable: "Blocks",
                        principalColumn: "Level",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionOps_Accounts_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionOps_Accounts_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AppState",
                columns: new[] { "Id", "AccountCounter", "AccountsCount", "ActivationOpsCount", "BallotOpsCount", "BlocksCount", "CommitmentsCount", "CyclesCount", "DelegationOpsCount", "DoubleBakingOpsCount", "DoubleEndorsingOpsCount", "EndorsementOpsCount", "Hash", "KnownHead", "LastSync", "Level", "ManagerCounter", "MigrationOpsCount", "NextProtocol", "NonceRevelationOpsCount", "OperationCounter", "OriginationOpsCount", "ProposalOpsCount", "ProposalsCount", "Protocol", "ProtocolsCount", "QuoteBtc", "QuoteCny", "QuoteEur", "QuoteJpy", "QuoteKrw", "QuoteLevel", "QuoteUsd", "RevealOpsCount", "RevelationPenaltyOpsCount", "Timestamp", "TransactionOpsCount", "VotingEpoch", "VotingPeriod" },
                values: new object[] { -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "", 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), -1, 0, 0, "", 0, 0, 0, 0, 0, "", 0, 0.0, 0.0, 0.0, 0.0, 0.0, -1, 0.0, 0, 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, -1, -1 });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Address",
                table: "Accounts",
                column: "Address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_DelegateId",
                table: "Accounts",
                column: "DelegateId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_FirstLevel",
                table: "Accounts",
                column: "FirstLevel");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Id",
                table: "Accounts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Staked",
                table: "Accounts",
                column: "Staked");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Type",
                table: "Accounts",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CreatorId",
                table: "Accounts",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ManagerId",
                table: "Accounts",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_WeirdDelegateId",
                table: "Accounts",
                column: "WeirdDelegateId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Type_Kind",
                table: "Accounts",
                columns: new[] { "Type", "Kind" },
                filter: "\"Type\" = 2");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SoftwareId",
                table: "Accounts",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Type_Staked",
                table: "Accounts",
                columns: new[] { "Type", "Staked" },
                filter: "\"Type\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ActivationOps_AccountId",
                table: "ActivationOps",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivationOps_Level",
                table: "ActivationOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ActivationOps_OpHash",
                table: "ActivationOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_BakerCycles_BakerId",
                table: "BakerCycles",
                column: "BakerId");

            migrationBuilder.CreateIndex(
                name: "IX_BakerCycles_Cycle",
                table: "BakerCycles",
                column: "Cycle");

            migrationBuilder.CreateIndex(
                name: "IX_BakerCycles_Id",
                table: "BakerCycles",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BakerCycles_Cycle_BakerId",
                table: "BakerCycles",
                columns: new[] { "Cycle", "BakerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BakingRights_Cycle",
                table: "BakingRights",
                column: "Cycle");

            migrationBuilder.CreateIndex(
                name: "IX_BakingRights_Level",
                table: "BakingRights",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_BakingRights_Cycle_BakerId",
                table: "BakingRights",
                columns: new[] { "Cycle", "BakerId" });

            migrationBuilder.CreateIndex(
                name: "IX_BallotOps_Epoch",
                table: "BallotOps",
                column: "Epoch");

            migrationBuilder.CreateIndex(
                name: "IX_BallotOps_Level",
                table: "BallotOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_BallotOps_OpHash",
                table: "BallotOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_BallotOps_Period",
                table: "BallotOps",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_BallotOps_ProposalId",
                table: "BallotOps",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_BallotOps_SenderId",
                table: "BallotOps",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_BakerId",
                table: "Blocks",
                column: "BakerId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_Hash",
                table: "Blocks",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_Level",
                table: "Blocks",
                column: "Level",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_ProtoCode",
                table: "Blocks",
                column: "ProtoCode");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_RevelationId",
                table: "Blocks",
                column: "RevelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_SoftwareId",
                table: "Blocks",
                column: "SoftwareId");

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_Address",
                table: "Commitments",
                column: "Address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commitments_Id",
                table: "Commitments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cycles_Index",
                table: "Cycles",
                column: "Index",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DelegationOps_DelegateId",
                table: "DelegationOps",
                column: "DelegateId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegationOps_InitiatorId",
                table: "DelegationOps",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegationOps_Level",
                table: "DelegationOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_DelegationOps_OpHash",
                table: "DelegationOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_DelegationOps_PrevDelegateId",
                table: "DelegationOps",
                column: "PrevDelegateId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegationOps_SenderId",
                table: "DelegationOps",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatorCycles_Cycle",
                table: "DelegatorCycles",
                column: "Cycle");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatorCycles_DelegatorId",
                table: "DelegatorCycles",
                column: "DelegatorId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatorCycles_Cycle_BakerId",
                table: "DelegatorCycles",
                columns: new[] { "Cycle", "BakerId" });

            migrationBuilder.CreateIndex(
                name: "IX_DelegatorCycles_Cycle_DelegatorId",
                table: "DelegatorCycles",
                columns: new[] { "Cycle", "DelegatorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoubleBakingOps_AccuserId",
                table: "DoubleBakingOps",
                column: "AccuserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleBakingOps_Level",
                table: "DoubleBakingOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleBakingOps_OffenderId",
                table: "DoubleBakingOps",
                column: "OffenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleBakingOps_OpHash",
                table: "DoubleBakingOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleEndorsingOps_AccuserId",
                table: "DoubleEndorsingOps",
                column: "AccuserId");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleEndorsingOps_Level",
                table: "DoubleEndorsingOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleEndorsingOps_OffenderId",
                table: "DoubleEndorsingOps",
                column: "OffenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DoubleEndorsingOps_OpHash",
                table: "DoubleEndorsingOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_EndorsementOps_DelegateId",
                table: "EndorsementOps",
                column: "DelegateId");

            migrationBuilder.CreateIndex(
                name: "IX_EndorsementOps_Level",
                table: "EndorsementOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_EndorsementOps_OpHash",
                table: "EndorsementOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationOps_AccountId",
                table: "MigrationOps",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationOps_Level",
                table: "MigrationOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_NonceRevelationOps_BakerId",
                table: "NonceRevelationOps",
                column: "BakerId");

            migrationBuilder.CreateIndex(
                name: "IX_NonceRevelationOps_Level",
                table: "NonceRevelationOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_NonceRevelationOps_OpHash",
                table: "NonceRevelationOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_NonceRevelationOps_SenderId",
                table: "NonceRevelationOps",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_OriginationOps_ContractId",
                table: "OriginationOps",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_OriginationOps_DelegateId",
                table: "OriginationOps",
                column: "DelegateId");

            migrationBuilder.CreateIndex(
                name: "IX_OriginationOps_InitiatorId",
                table: "OriginationOps",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_OriginationOps_Level",
                table: "OriginationOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_OriginationOps_ManagerId",
                table: "OriginationOps",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_OriginationOps_OpHash",
                table: "OriginationOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_OriginationOps_SenderId",
                table: "OriginationOps",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalOps_Epoch",
                table: "ProposalOps",
                column: "Epoch");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalOps_Level",
                table: "ProposalOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalOps_OpHash",
                table: "ProposalOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalOps_Period",
                table: "ProposalOps",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalOps_ProposalId",
                table: "ProposalOps",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalOps_SenderId",
                table: "ProposalOps",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_Epoch",
                table: "Proposals",
                column: "Epoch");

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_Hash",
                table: "Proposals",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_Level",
                table: "Quotes",
                column: "Level",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RevealOps_Level",
                table: "RevealOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_RevealOps_OpHash",
                table: "RevealOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_RevealOps_SenderId",
                table: "RevealOps",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_RevelationPenaltyOps_BakerId",
                table: "RevelationPenaltyOps",
                column: "BakerId");

            migrationBuilder.CreateIndex(
                name: "IX_RevelationPenaltyOps_Level",
                table: "RevelationPenaltyOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotBalances_Level",
                table: "SnapshotBalances",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_Cycle",
                table: "Statistics",
                column: "Cycle",
                unique: true,
                filter: "\"Cycle\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_Date",
                table: "Statistics",
                column: "Date",
                unique: true,
                filter: "\"Date\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Statistics_Level",
                table: "Statistics",
                column: "Level",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOps_InitiatorId",
                table: "TransactionOps",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOps_Level",
                table: "TransactionOps",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOps_OpHash",
                table: "TransactionOps",
                column: "OpHash");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOps_SenderId",
                table: "TransactionOps",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOps_TargetId",
                table: "TransactionOps",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_VotingPeriods_Epoch",
                table: "VotingPeriods",
                column: "Epoch");

            migrationBuilder.CreateIndex(
                name: "IX_VotingPeriods_Id",
                table: "VotingPeriods",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VotingPeriods_Index",
                table: "VotingPeriods",
                column: "Index",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VotingSnapshots_Period",
                table: "VotingSnapshots",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_VotingSnapshots_Period_BakerId",
                table: "VotingSnapshots",
                columns: new[] { "Period", "BakerId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivationOps_Blocks_Level",
                table: "ActivationOps",
                column: "Level",
                principalTable: "Blocks",
                principalColumn: "Level",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivationOps_Accounts_AccountId",
                table: "ActivationOps",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BallotOps_Blocks_Level",
                table: "BallotOps",
                column: "Level",
                principalTable: "Blocks",
                principalColumn: "Level",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BallotOps_Accounts_SenderId",
                table: "BallotOps",
                column: "SenderId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Accounts_BakerId",
                table: "Blocks",
                column: "BakerId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_NonceRevelationOps_RevelationId",
                table: "Blocks",
                column: "RevelationId",
                principalTable: "NonceRevelationOps",
                principalColumn: "RevealedLevel",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Blocks_FirstLevel",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_NonceRevelationOps_Blocks_Level",
                table: "NonceRevelationOps");

            migrationBuilder.DropTable(
                name: "ActivationOps");

            migrationBuilder.DropTable(
                name: "AppState");

            migrationBuilder.DropTable(
                name: "BakerCycles");

            migrationBuilder.DropTable(
                name: "BakingRights");

            migrationBuilder.DropTable(
                name: "BallotOps");

            migrationBuilder.DropTable(
                name: "Commitments");

            migrationBuilder.DropTable(
                name: "Cycles");

            migrationBuilder.DropTable(
                name: "DelegationOps");

            migrationBuilder.DropTable(
                name: "DelegatorCycles");

            migrationBuilder.DropTable(
                name: "DoubleBakingOps");

            migrationBuilder.DropTable(
                name: "DoubleEndorsingOps");

            migrationBuilder.DropTable(
                name: "EndorsementOps");

            migrationBuilder.DropTable(
                name: "MigrationOps");

            migrationBuilder.DropTable(
                name: "OriginationOps");

            migrationBuilder.DropTable(
                name: "ProposalOps");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "RevealOps");

            migrationBuilder.DropTable(
                name: "RevelationPenaltyOps");

            migrationBuilder.DropTable(
                name: "SnapshotBalances");

            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.DropTable(
                name: "TransactionOps");

            migrationBuilder.DropTable(
                name: "VotingPeriods");

            migrationBuilder.DropTable(
                name: "VotingSnapshots");

            migrationBuilder.DropTable(
                name: "Proposals");

            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropTable(
                name: "Protocols");

            migrationBuilder.DropTable(
                name: "NonceRevelationOps");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Software");
        }
    }
}

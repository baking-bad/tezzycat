﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tzkt.Data.Models;

namespace Tzkt.Sync.Protocols.Proto3
{
    class BallotsCommit : ProtocolCommit
    {
        public BallotsCommit(ProtocolHandler protocol) : base(protocol) { }

        public virtual async Task Apply(Block block, JsonElement op, JsonElement content)
        {
            #region init
            var period = await Cache.Periods.GetAsync(content.RequiredInt32("period"));
            var proposal = await Cache.Proposals.GetAsync(content.RequiredString("proposal"));
            var sender = Cache.Accounts.GetDelegate(content.RequiredString("source"));

            var snapshot = await Db.VotingSnapshots
                .FirstOrDefaultAsync(x => x.Period == period.Index && x.BakerId == sender.Id)
                    ?? throw new ValidationException("Ballot sender is not on the voters list");

            var ballot = new BallotOperation
            {
                Id = Cache.AppState.NextOperationId(),
                Block = block,
                Level = block.Level,
                Timestamp = block.Timestamp,
                OpHash = op.RequiredString("hash"),
                Sender = sender,
                Rolls = snapshot.Rolls,
                Epoch = period.Epoch,
                Period = period.Index,
                Proposal = proposal,
                Vote = content.RequiredString("ballot") switch
                {
                    "yay" => Vote.Yay,
                    "nay" => Vote.Nay,
                    "pass" => Vote.Pass,
                    _ => throw new Exception("invalid ballot value")
                }
            };
            #endregion

            #region entities
            //Db.TryAttach(block);
            Db.TryAttach(period);
            Db.TryAttach(proposal);
            Db.TryAttach(sender);
            //Db.TryAttach(snapshot);
            #endregion

            #region apply operation
            if (ballot.Vote == Vote.Yay)
            {
                period.YayBallots++;
                period.YayRolls += ballot.Rolls;
                snapshot.Status = VoterStatus.VotedYay;
            }
            else if (ballot.Vote == Vote.Nay)
            {
                period.NayBallots++;
                period.NayRolls += ballot.Rolls;
                snapshot.Status = VoterStatus.VotedNay;
            }
            else
            {
                period.PassBallots++;
                period.PassRolls += ballot.Rolls;
                snapshot.Status = VoterStatus.VotedPass;
            }

            sender.BallotsCount++;

            block.Operations |= Operations.Ballots;
            #endregion

            Db.BallotOps.Add(ballot);
        }

        public virtual async Task Revert(Block block, BallotOperation ballot)
        {
            #region init
            ballot.Block ??= block;
            ballot.Sender ??= Cache.Accounts.GetDelegate(ballot.SenderId);
            ballot.Proposal ??= await Cache.Proposals.GetAsync(ballot.ProposalId);

            var snapshot = await Db.VotingSnapshots
                .FirstAsync(x => x.Period == ballot.Period && x.BakerId == ballot.Sender.Id);

            var period = await Cache.Periods.GetAsync(ballot.Period);
            #endregion

            #region entities
            var sender = ballot.Sender;

            //Db.TryAttach(block);
            Db.TryAttach(sender);
            Db.TryAttach(period);
            //Db.TryAttach(snapshot);
            #endregion

            #region revert operation
            if (ballot.Vote == Vote.Yay)
            {
                period.YayBallots--;
                period.YayRolls -= ballot.Rolls;
                snapshot.Status = VoterStatus.None;
            }
            else if (ballot.Vote == Vote.Nay)
            {
                period.NayBallots--;
                period.NayRolls -= ballot.Rolls;
                snapshot.Status = VoterStatus.None;
            }
            else
            {
                period.PassBallots--;
                period.PassRolls -= ballot.Rolls;
                snapshot.Status = VoterStatus.None;
            }

            sender.BallotsCount--;
            #endregion

            Db.BallotOps.Remove(ballot);
        }
    }
}

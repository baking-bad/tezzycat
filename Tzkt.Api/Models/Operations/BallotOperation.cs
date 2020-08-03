﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tzkt.Api.Models
{
    public class BallotOperation : Operation
    {
        /// <summary>
        /// Type of the operation, `ballot` - is used to vote for a proposal in a given voting cycle
        /// </summary>
        public override string Type => OpTypes.Ballot;

        /// <summary>
        /// Unique ID of the operation, stored in the TzKT indexer database
        /// </summary>
        public override int Id { get; set; }

        /// <summary>
        /// The height of the block from the genesis block, in which the operation was included
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Datetime of the block, in which the operation was included (ISO 8601, e.g. `2020-02-20T02:40:57Z`)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Hash of the block, in which the operation was included
        /// </summary>
        public string Block { get; set; }

        /// <summary>
        /// Hash of the operation
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Information about the voting period for which the ballot was submitted
        /// </summary>
        public PeriodInfo Period { get; set; }

        /// <summary>
        /// Information about the proposal for which ballot was submitted
        /// </summary>
        public ProposalAlias Proposal { get; set; }

        /// <summary>
        /// Information about the delegate (baker), submitted the ballot
        /// </summary>
        public Alias Delegate { get; set; }

        /// <summary>
        /// Number of baker's rolls (baker's voting power)
        /// </summary>
        public int Rolls { get; set; }

        /// <summary>
        /// Vote, given in the ballot (`yay`, `nay`, or `pass`)
        /// </summary>
        public string Vote { get; set; }

        #region injecting
        /// <summary>
        /// Injected historical quote at the time of operation
        /// </summary>
        public QuoteShort Quote { get; set; }
        #endregion
    }
}

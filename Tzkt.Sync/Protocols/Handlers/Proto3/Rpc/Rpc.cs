﻿using Tzkt.Sync.Services;

namespace Tzkt.Sync.Protocols.Proto3
{
    class Rpc : Proto1.Rpc
    {
        public Rpc(TezosNode node) : base(node) { }
    }
}

﻿using Tzkt.Data;

namespace Tzkt.Sync.Protocols.Proto6
{
    class Diagnostics : Proto5.Diagnostics
    {
        public Diagnostics(TzktContext db, IRpc rpc) : base(db, rpc) { }
    }
}

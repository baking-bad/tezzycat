﻿using System;
using Microsoft.Extensions.DependencyInjection;

using Tzkt.Sync.Protocols;

namespace Tzkt.Sync
{
    public static class TezosProtocols
    {
        public static void AddTezosProtocols(this IServiceCollection services)
        {
            services.AddScoped<GenesisHandler>();
            services.AddScoped<InitiatorHandler>();
            services.AddScoped<Proto1Handler>();
            services.AddScoped<Proto2Handler>();
            services.AddScoped<Proto3Handler>();
            services.AddScoped<Proto4Handler>();
            services.AddScoped<Proto5Handler>();
            services.AddScoped<Proto6Handler>();
            services.AddScoped<Proto7Handler>();
            services.AddScoped<Proto8Handler>();
        }

        public static ProtocolHandler GetProtocolHandler(this IServiceProvider services, int level, string protocol)
        {
            if (level > 1)
            {
                switch (protocol)
                {
                    case "PtCJ7pwoxe8JasnHY8YonnLYjcVHmhiARPJvqcC6VfHT5s8k8sY":
                        return services.GetRequiredService<Proto1Handler>();
                    case "PsYLVpVvgbLhAhoqAkMFUo6gudkJ9weNXhUYCiLDzcUpFpkk8Wt":
                        return services.GetRequiredService<Proto2Handler>();
                    case "PsddFKi32cMJ2qPjf43Qv5GDWLDPZb3T3bF6fLKiF5HtvHNU7aP":
                        return services.GetRequiredService<Proto3Handler>();
                    case "Pt24m4xiPbLDhVgVfABUjirbmda3yohdN82Sp9FeuAXJ4eV9otd":
                        return services.GetRequiredService<Proto4Handler>();
                    case "PsBabyM1eUXZseaJdmXFApDSBqj8YBfwELoxZHHW77EMcAbbwAS":
                        return services.GetRequiredService<Proto5Handler>();
                    case "PsCARTHAGazKbHtnKfLzQg3kms52kSRpgnDY982a9oYsSXRLQEb":
                        return services.GetRequiredService<Proto6Handler>();
                    case "PsDELPH1Kxsxt8f9eWbxQeRxkjfbxoqM52jvs5Y5fBxWWh4ifpo":
                        return services.GetRequiredService<Proto7Handler>();
                    case "PtEdoTezd3RHSC31mpxxo1npxFjoWWcFgQtxapi51Z8TLu6v6Uq":
                        return services.GetRequiredService<Proto8Handler>();
                    default:
                        throw new NotImplementedException($"Protocol '{protocol}' is not supported");
                }
            }
            else if (level == 1)
            {
                return services.GetRequiredService<InitiatorHandler>();
            }
            else
            {
                return services.GetRequiredService<GenesisHandler>();
            }
        }
    }
}

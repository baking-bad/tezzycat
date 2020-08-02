﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Tzkt.Sync.Services
{
    class CoingeckoProvider : DefaultQuotesProvider, IDisposable
    {
        public const string ProviderName = "Coingecko";

        readonly TzktClient Client;
        readonly CoingeckoProviderConfig Config;

        public CoingeckoProvider(IConfiguration config)
        {
            Config = config.GetCoingeckoProviderConfig();
            Client = new TzktClient(Config.BaseUrl, Config.Timeout);
        }

        public void Dispose()
            => Client.Dispose();

        public override async Task<IEnumerable<IDefaultQuote>> GetBtc(DateTime from, DateTime to)
            => await GetQuotes("btc", from, to);

        public override async Task<IEnumerable<IDefaultQuote>> GetEur(DateTime from, DateTime to)
            => await GetQuotes("eur", from, to);

        public override async Task<IEnumerable<IDefaultQuote>> GetUsd(DateTime from, DateTime to)
            => await GetQuotes("usd", from, to);

        async Task<List<CoingeckoQuote>> GetQuotes(string currency, DateTime from, DateTime to)
        {
            var _from = (long)(from - DateTime.UnixEpoch).TotalSeconds;
            var _to = (long)(to - DateTime.UnixEpoch).TotalSeconds;

            using var stream = await Client.GetStreamAsync(
                $"coins/tezos/market_chart/range?vs_currency={currency}&from={_from}&to={_to}");

            var res = await JsonSerializer.DeserializeAsync<CoingeckoQuotes>(stream);
            return res.Prices;
        }
    }

    public class CoingeckoQuotes
    {
        [JsonPropertyName("prices")]
        public List<CoingeckoQuote> Prices { get; set; }
    }

    public class CoingeckoQuote : List<double>, IDefaultQuote
    {
        public DateTime Timestamp => DateTime.UnixEpoch.AddSeconds((long)(this[0] / 1000));
        public double Price => this[1];
    }
}

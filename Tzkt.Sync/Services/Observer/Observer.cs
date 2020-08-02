﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Tzkt.Data;
using Tzkt.Data.Models;
using Tzkt.Sync.Protocols;

namespace Tzkt.Sync.Services
{
    public class Observer : BackgroundService
    {
        public AppState AppState { get; private set; }

        private readonly TezosNode Node;
        private readonly IServiceScopeFactory Services;
        private readonly ILogger Logger;

        public Observer(TezosNode node, IServiceScopeFactory services, ILogger<Observer> logger)
        {
            Node = node;
            Services = services;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            try
            {
                #region init database
                await InitDatabase();
                Logger.LogInformation("Database initialized");
                #endregion

                #region init state
                AppState = await ResetState();
                Logger.LogInformation($"State initialized: [{AppState.Level}:{AppState.Hash}]");
                #endregion

                #region init quotes
                await InitQuotes();
                Logger.LogInformation($"Quotes initialized: [{AppState.QuoteLevel}]");
                #endregion

                Logger.LogWarning("Observer is started");

                while (!cancelToken.IsCancellationRequested)
                {
                    #region wait for updates
                    try
                    {
                        if (!await WaitForUpdatesAsync(cancelToken))
                            break;

                        var head = await Node.GetHeaderAsync();
                        Logger.LogDebug($"New head is found [{head.Level}:{head.Hash}]");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogCritical($"Failed to check updates. {ex.Message}");
                        await Task.Delay(5000);
                        continue;
                    }
                    #endregion

                    #region apply updates
                    try
                    {
                        if (!await ApplyUpdatesAsync(cancelToken))
                            break;

                        Logger.LogDebug($"Current head [{AppState.Level}:{AppState.Hash}]");
                    }
                    catch (BaseException ex) when (ex.RebaseRequired)
                    {
                        Logger.LogError($"Failed to apply block: {ex.Message}. Rebase local branch...");

                        try
                        {
                            if (!await RebaseLocalBranchAsync(cancelToken))
                                break;
                        }
                        catch (Exception exx)
                        {
                            Logger.LogCritical($"Failed to rebase branch. {exx.Message}");
                            AppState = await ResetState();
                            await Task.Delay(5000);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogCritical($"Failed to apply updates. {ex.Message}");
                        AppState = await ResetState();
                        await Task.Delay(5000);
                        continue;
                    }
                    #endregion
                }

                Logger.LogWarning("Observer is stoped");
            }
            catch (Exception ex)
            {
                Logger.LogCritical($"Observer crashed: {ex.Message}");
                throw;
            }
        }

        private async Task<AppState> ResetState()
        {
            using var scope = Services.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<CacheService>();
            await cache.ResetAsync();

            return cache.AppState.Get();
        }

        private async Task InitQuotes()
        {
            using var scope = Services.CreateScope();
            var quotes = scope.ServiceProvider.GetRequiredService<QuotesService>();
            await quotes.Init();
        }

        private async Task InitDatabase()
        {
            try
            {
                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TzktContext>();
                var migrations = await db.Database.GetPendingMigrationsAsync();
                if (migrations.Any())
                {
                    Logger.LogWarning($"{migrations.Count()} database migrations were found. Applying migrations...");
                    await db.Database.MigrateAsync();
                }
            }
            catch
            {
                Logger.LogCritical($"Failed to migrate database. It seems like you were using too old version. Try to restore database from the latest snapshot. See https://github.com/baking-bad/tzkt");
                throw;
            }
        }

        private async Task<bool> WaitForUpdatesAsync(CancellationToken cancelToken)
        {
            while (!await Node.HasUpdatesAsync(AppState.Level))
            {
                if (cancelToken.IsCancellationRequested)
                    return false;

                await Task.Delay(1000);
            }
            return true;
        }

        private async Task<bool> RebaseLocalBranchAsync(CancellationToken cancelToken)
        {
            while (AppState.Level >= 0 && !cancelToken.IsCancellationRequested)
            {
                var header = await Node.GetHeaderAsync(AppState.Level);
                if (AppState.Hash == header.Hash) break;

                Logger.LogError($"Invalid head [{AppState.Level}:{AppState.Hash}]. Reverting...");

                using var scope = Services.CreateScope();
                var protoHandler = scope.ServiceProvider.GetProtocolHandler(AppState.Protocol);
                AppState = await protoHandler.RevertLastBlock(header.Predecessor);

                Logger.LogInformation($"Reverted to [{AppState.Level}:{AppState.Hash}]");
            }

            return !cancelToken.IsCancellationRequested;
        }

        private async Task<bool> ApplyUpdatesAsync(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                var sync = DateTime.UtcNow;
                var header = await Node.GetHeaderAsync();
                if (AppState.Level == header.Level) break;

                Logger.LogDebug($"Loading block {AppState.Level + 1}...");
                using var blockStream = await Node.GetBlockAsync(AppState.Level + 1);

                //if (AppState.Level >= 0)
                //{
                //    throw new ValidationException("Test", true);
                //}

                Logger.LogDebug($"Applying block...");

                using var scope = Services.CreateScope();
                var protocol = scope.ServiceProvider.GetProtocolHandler(AppState.NextProtocol);
                AppState = await protocol.CommitBlock(blockStream, header.Level, sync);

                Logger.LogInformation($"Applied {AppState.Level} of {AppState.KnownHead}");
            }

            return !cancelToken.IsCancellationRequested;
        }
    }
}

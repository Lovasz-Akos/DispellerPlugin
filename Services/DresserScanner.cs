namespace Dispeller.Services
{
    using System;
    using System.Collections.Generic;
    using Dalamud.Plugin.Services;
    using FFXIVClientStructs.FFXIV.Client.UI.Agent;

    public class DresserScanner : IDisposable
    {
        private const int PollIntervalFrames = 30;

        private static readonly object LockObject = new();
        private static List<PrismBoxItem> _cachedDresserItems = [];
        private static long _cachedSignature = -1;
        private static ulong _contentId = 0;

        private int _framesSincePoll = PollIntervalFrames;
        private bool _disposed = false;

        public DresserScanner() => Plugin.Framework.Update += this.OnFrameworkUpdate;

        private unsafe void OnFrameworkUpdate(IFramework framework)
        {
            try
            {
                ulong contentId = Plugin.ClientState.IsLoggedIn ? Plugin.PlayerState.ContentId : 0;
                if (contentId != _contentId)
                {
                    SwitchCharacter(contentId);
                }

                AgentMiragePrismPrismBox* agent = AgentMiragePrismPrismBox.Instance();
                if (agent == null || !agent->IsAddonReady() || agent->Data == null)
                {
                    this._framesSincePoll = PollIntervalFrames;
                    return;
                }

                if (++this._framesSincePoll < PollIntervalFrames)
                {
                    return;
                }

                this._framesSincePoll = 0;

                List<PrismBoxItem> items = ReadAll(agent);
                if (items.Count == 0)
                {
                    return;
                }

                long signature = SignatureOf(items);

                lock (LockObject)
                {
                    if (_cachedDresserItems.Count > 0 && signature == _cachedSignature)
                    {
                        return;
                    }

                    _cachedDresserItems = items;
                    _cachedSignature = signature;
                }

                Plugin.Log.Information($"OnFrameworkUpdate: Cached {items.Count} items from dresser");
            }
            catch
            {
            }
        }

        private static void SwitchCharacter(ulong contentId)
        {
            lock (LockObject)
            {
                _contentId = contentId;
                _cachedDresserItems = [];
                _cachedSignature = -1;
            }

            Plugin.Log.Information($"Dresser cache cleared for character change (content id {contentId})");
        }

        private static unsafe List<PrismBoxItem> ReadAll(AgentMiragePrismPrismBox* agent)
        {
            List<PrismBoxItem> result = [];

            foreach (FFXIVClientStructs.FFXIV.Client.UI.Agent.PrismBoxItem item in agent->Data->PrismBoxItems)
            {
                if (item.ItemId == 0)
                {
                    continue;
                }

                result.Add(new PrismBoxItem
                {
                    Name = string.Empty,
                    Slot = item.Slot,
                    ItemId = item.ItemId,
                    IconId = item.IconId,
                    Stain1 = item.Stains[0],
                    Stain2 = item.Stains[1],
                });
            }

            return result;
        }

        private static long SignatureOf(List<PrismBoxItem> items)
        {
            long sum = 0;
            foreach (PrismBoxItem item in items)
            {
                sum += item.ItemId;
            }

            return (items.Count * 1_000_000_007L) + sum;
        }

        public static List<PrismBoxItem> GetDresserItems()
        {
            lock (LockObject)
            {
                return [.. _cachedDresserItems];
            }
        }

        public static unsafe bool TryRefresh()
        {
            try
            {
                AgentMiragePrismPrismBox* agent = AgentMiragePrismPrismBox.Instance();
                if (agent == null)
                {
                    Plugin.Log.Debug("TryRefresh: AgentMiragePrismPrismBox.Instance() returned null - dresser not open");
                    return false;
                }

                if (!agent->IsAddonReady())
                {
                    Plugin.Log.Debug("TryRefresh: Agent is not ready (IsAddonReady = false) - dresser not open");
                    return false;
                }

                if (agent->Data == null)
                {
                    Plugin.Log.Debug("TryRefresh: Agent data is null - dresser not initialized");
                    return false;
                }

                List<PrismBoxItem> items = ReadAll(agent);

                lock (LockObject)
                {
                    _cachedDresserItems = items;
                    _cachedSignature = SignatureOf(items);
                }

                Plugin.Log.Information($"TryRefresh: Loaded {items.Count} items from dresser");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex, "Error in TryRefresh");
                return false;
            }
        }

        public static bool HasCachedData()
        {
            lock (LockObject)
            {
                return _cachedDresserItems.Count > 0;
            }
        }

        public static int GetCachedItemCount()
        {
            lock (LockObject)
            {
                return _cachedDresserItems.Count;
            }
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            Plugin.Framework.Update -= this.OnFrameworkUpdate;
            this._disposed = true;
        }
    }

    public class PrismBoxItem
    {
        public string Name { get; set; } = string.Empty;
        public uint Slot { get; set; }
        public uint ItemId { get; set; }
        public uint IconId { get; set; }
        public byte Stain1 { get; set; }
        public byte Stain2 { get; set; }
    }
}

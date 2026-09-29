namespace Dispeller.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Lumina.Excel;
    using Lumina.Excel.Sheets;

    internal static class SheetWarmup
    {
        private static readonly object Gate = new();
        private static bool started;
        private static CancellationTokenSource? cancellation;

        public static void Start()
        {
            CancellationToken token;

            lock (Gate)
            {
                if (started)
                {
                    return;
                }

                started = true;
                cancellation = new CancellationTokenSource();
                token = cancellation.Token;
            }

            _ = Task.Run(() => Run(token));
        }

        public static void Stop()
        {
            CancellationTokenSource? cts;

            lock (Gate)
            {
                if (!started)
                {
                    return;
                }

                started = false;
                cts = cancellation;
                cancellation = null;
            }

            cts?.Cancel();
            cts?.Dispose();
        }

        private static void Run(CancellationToken token)
        {
            try
            {
                Stopwatch total = Stopwatch.StartNew();
                long touched = WarmItems(token) + WarmCabinet(token);
                total.Stop();

                if (token.IsCancellationRequested)
                {
                    return;
                }

                Plugin.Log.Information($"Sheet warmup finished in {total.Elapsed.TotalMilliseconds:F1} ms (checksum {touched})");
            }
            catch (Exception ex)
            {
                Plugin.Log.Warning(ex, "Sheet warmup did not finish - the first scan will be slower");
            }
        }

        private static long WarmItems(CancellationToken token)
        {
            ExcelSheet<Item> sheet = Plugin.DataManager.GetExcelSheet<Item>()!;
            long touched = 0;
            int row = 0;

            foreach (Item entry in sheet)
            {
                if ((++row & 0x3FF) == 0 && token.IsCancellationRequested)
                {
                    break;
                }

                touched += (long)entry.ModelMain;
                touched += entry.Icon;
                touched += entry.DyeCount;
                touched += entry.Name.ByteLength;

                if (!entry.EquipSlotCategory.IsValid || entry.EquipSlotCategory.RowId == 0)
                {
                    continue;
                }

                EquipSlotCategory category = entry.EquipSlotCategory.Value;
                touched += category.MainHand + category.OffHand + category.Head + category.Body
                    + category.Gloves + category.Legs + category.Feet + category.Ears
                    + category.Neck + category.Wrists + category.FingerR + category.FingerL;
            }

            int sampled = 0;
            foreach (Item entry in sheet)
            {
                touched += entry.Name.ExtractText().Length;
                if (++sampled == 16)
                {
                    break;
                }
            }

            return touched;
        }

        private static long WarmCabinet(CancellationToken token)
        {
            ExcelSheet<Cabinet> sheet = Plugin.DataManager.GetExcelSheet<Cabinet>()!;
            long touched = 0;
            int row = 0;

            foreach (Cabinet entry in sheet)
            {
                if ((++row & 0xFF) == 0 && token.IsCancellationRequested)
                {
                    break;
                }

                touched += entry.Item.RowId;
                touched += entry.Category.RowId;
            }

            return touched;
        }
    }
}

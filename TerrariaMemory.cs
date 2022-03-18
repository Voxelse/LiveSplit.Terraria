using System;
using System.Collections.Generic;
using Voxif.AutoSplitter;
using Voxif.Helpers.MemoryHelper;
using Voxif.IO;
using Voxif.Memory;

namespace LiveSplit.Terraria {
    public class TerrariaMemory : Memory {

        protected override string[] ProcessNames => new string[] { "Terraria" };

        public Pointer<IntPtr> Npc { get; private set; }
        public Pointer<IntPtr> Bosses { get; private set; }
        public Pointer<IntPtr> Inventory { get; private set; }
        public Pointer<bool> IsGameMenu { get; private set; }
        public Pointer<bool> IsHardmode { get; private set; }
        public Pointer<bool> IsCrimson { get; private set; }

        public TerrariaVersion Version { get; private set; }
        
        private ScanHelperTask scanTask;

        private readonly ScannableData scanVerData = new ScannableData {
            {"", new Dictionary<string, ScanTarget> {
                { "gameVer", new ScanTarget(2, "8B 05 ???????? 8D 15 ???????? E8 ???????? 8B 05 ???????? 8D 15 ???????? E8") {
                    IsGoodMatch = (wrapper, ptr, ver) => {
                        return wrapper.ReadString(wrapper.Read(ptr, 0x0, 0x0, 0x8), EStringType.AutoSized).StartsWith("v1.");
                    }
                } },
            }
        } };

        public TerrariaMemory(Logger logger) : base(logger) {
            OnHook += async () => {
                scanTask = new ScanHelperTask(game, logger);

                await scanTask.Run(scanVerData, (resultVersion) => OnVersionFound(resultVersion));

                ScannableData scanData = new ScannableData {
                    {"", new Dictionary<string, ScanTarget> {
                        { "updateTime", new ScanTarget(0, Version.Signature) },
                        { "crimson", new ScanTarget(2, "80 3D ???????? 00 74 05 ?? 24000000") },
                    }
                } };

                await scanTask.Run(scanData, (result) => OnScansDone(result));
            };

            OnExit += () => {
                if(scanTask != null) {
                    scanTask?.Dispose();
                    scanTask = null;
                }
            };  
        }

        private void OnVersionFound(Dictionary<string, Dictionary<string, IntPtr>> result) {
            string stringVersion = game.ReadString(game.Read(result[""]["gameVer"], 0x0, 0x0, 0x8), EStringType.AutoSized);
            logger.Log("Game version: " + stringVersion);
            OnVersionDetected?.Invoke(stringVersion);
            Version gameVersion = new Version(stringVersion.Substring(1, stringVersion.Length > 8 ? 7 : stringVersion.Length-1));
            Version = TerrariaVersion.GetVersion(gameVersion);
        }

        private void OnScansDone(Dictionary<string, Dictionary<string, IntPtr>> result) {
            NestedPointerFactory ptrFactory = new NestedPointerFactory(game);

            IntPtr time = result[""]["updateTime"];
            IsGameMenu = ptrFactory.Make<bool>(game.Read<IntPtr>(time + Version.GameMenuAsmOffset));
            Bosses = ptrFactory.Make<IntPtr>(time + Version.BossAsmOffset);
            IsHardmode = ptrFactory.Make<bool>(game.Read<IntPtr>(time + Version.HardmodeAsmOffset));
            Inventory = ptrFactory.Make<IntPtr>(game.Read<IntPtr>(time + Version.PlayerAsmOffset), 0x8, Version.InventoryOffset);
            Npc = ptrFactory.Make<IntPtr>(game.Read<IntPtr>(time + Version.NpcAsmOffset));

            IsCrimson = ptrFactory.Make<bool>(game.Read<IntPtr>(result[""]["crimson"]));

            logger.Log(ptrFactory.ToString());

            scanTask = null;
        }

        public override bool Update() => base.Update() && scanTask == null;

        public bool IsBossBeaten(string name) {
            return Bosses != null && Version != null && Version.BossLookup.TryGetValue(name, out int offset)
                && game.Read<bool>(Bosses.New + offset);
        }

        public IEnumerable<int> ItemSequence() {
            for(int i = 0; i < game.Read<int>(Inventory.New + 0x4); i++) {
                yield return game.Read<int>(game.Read<IntPtr>(Inventory.New + 0x8 + 0x4 * i) + Version.InventoryTypeOffset);
            }
        }

        public IEnumerable<IntPtr> NpcSequence() {
            for(int i = 0; i < game.Read<int>(Npc.New + 0x4); i++) {
                yield return game.Read<IntPtr>(Npc.New + 0x8 + 0x4 * i);
            }
        }
        public bool NpcActive(IntPtr npcPtr) {
            return game.Read<bool>(npcPtr + Version.NpcActiveOffset);
        }
        public int NpcType(IntPtr npcPtr) {
            return game.Read<int>(npcPtr + Version.NpcTypeOffset);
        }
    }
}
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

        private ScanHelperTask scanTask;

        private readonly ScannableData scanDict = new ScannableData {
            {"", new Dictionary<string, ScanTarget> {
                { "updateTime", new ScanTarget(0, "55 8B EC 57 56 83 EC ?? 8D 7D ?? B9 ???????? 33 C0 F3 AB 80 3D ???????? 00 75 ?? 0FB6") },
                { "updateTimeDay", new ScanTarget(0, "55 8B EC 56 50 8B F1 33 D2") }
            }
        } };

        public TerrariaMemory(Logger logger) : base(logger) {
            OnHook += () => {
                scanTask = new ScanHelperTask(game, logger);
                scanTask.Run(scanDict, (result) => OnScansDone(result));
            };

            OnExit += () => {
                if(scanTask != null) {
                    scanTask?.Dispose();
                    scanTask = null;
                }
            };  
        }

        private void OnScansDone(Dictionary<string, Dictionary<string, IntPtr>> result) {
            NestedPointerFactory ptrFactory = new NestedPointerFactory(game);

            IntPtr time = result[""]["updateTime"];
            IsGameMenu = ptrFactory.Make<bool>(game.Read<IntPtr>(time + 0x90));

            Bosses = ptrFactory.Make<IntPtr>(time + 0x336);
            IsHardmode = ptrFactory.Make<bool>(game.Read<IntPtr>(time + 0x346));
            Inventory = ptrFactory.Make<IntPtr>(game.Read<IntPtr>(time + 0x36A), 0x8, 0xD0);
            Npc = ptrFactory.Make<IntPtr>(game.Read<IntPtr>(time + 0x81A));

            IsCrimson = ptrFactory.Make<bool>(game.Read<IntPtr>(result[""]["updateTimeDay"] + 0xD9));

            logger.Log(ptrFactory.ToString());

            scanTask = null;
        }

        public override bool Update() => base.Update() && scanTask == null;

        public bool IsBossBeaten(int offset) {
            return Bosses != null && game.Read<bool>(Bosses.New + offset);
        }

        public IEnumerable<int> ItemSequence() {
            for(int i = 0; i < 60; i++) {
                yield return game.Read<int>(game.Read<IntPtr>(Inventory.New + 0x8 + 0x4 * i) + 0x9C);
            }
        }

        public IEnumerable<IntPtr> NpcSequence() {
            for(int i = 0; i < 201; i++) {
                yield return game.Read<IntPtr>(Npc.New + 0x8 + 0x4 * i);
            }
        }
        public bool NpcActive(IntPtr npcPtr) {
            return game.Read<bool>(npcPtr + 0x20);
        }
        public int NpcType(IntPtr npcPtr) {
            return game.Read<int>(npcPtr + 0xDC);
        }
    }
}
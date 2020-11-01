using LiveSplit.Model;
using LiveSplit.VoxSplitter;
using System;
using System.Collections.Generic;

namespace LiveSplit.Terraria {
    public class TerrariaMemory : SignatureMemory {

        private enum EBosses {
            //Wall of Flesh
            EyeofCthulhu = -0x4,
            EaterofWorldsBrainofCthulhu,
            Skeletron,
            QueenBee,
            KingSlime = 0x0,
            GoblinArmy,//Event
            FrostLegion,//Event
            PirateInvasion,//Event
            SolarEclipse,//Event
            Plantera,
            Golem,
            MartianSaucer,//Event
            DukeFishron,
            MourningWood,//Event
            Pumpking,//Event
            IceQueen,//Event
            Everscream,//Event
            SantaNK1,//Event
            LunaticCultist,
            MoonLord,
            SolarPillar,
            VortexPillar,
            NebulaPillar,
            StardustPillar,
            EmpressofLight,
            QueenSlime,
            //offset 1*6
            TheDestroyer = 0x1C,
            TheTwins,
            SkeletronPrime
        }

        private enum EItems {
            NightsEdge = 273,
            TerraBlade = 757,
            Zenith = 4956,
            TorchGodsFavor = 5043
        }

        private Pointer<bool> gameMenu;
        private Pointer<IntPtr> inventory;
        private Pointer<IntPtr> bosses;
        private Pointer<bool> hardmode;

        private readonly Dictionary<int, string> bossOffsets = new Dictionary<int, string>();
        private readonly HashSet<int> itemIds = new HashSet<int>();
        private bool needHardmodeSplit = false;

        public TerrariaMemory(Logger logger) : base(logger) {
            processName = "Terraria";
            scanData.Add("", new Dictionary<string, SignatureHolder> {
                { "asm", new SignatureHolder(0, "55 8B EC 57 56 83 EC 34 8D 7D D0 B9") }
            });
        }

        protected override void OnScanDone() {
            NestedPointerFactory ptrFactory = new NestedPointerFactory(this);

            IntPtr asm = scanData[""]["asm"].Pointer;
            gameMenu = ptrFactory.Make<bool>(Game.Read<IntPtr>(asm + 0x90));
            bosses = ptrFactory.Make<IntPtr>(asm + 0x335);
            hardmode = ptrFactory.Make<bool>(Game.Read<IntPtr>(asm + 0x345));
            inventory = ptrFactory.Make<IntPtr>(Game.Read<IntPtr>(asm + 0x369), 0x8, 0xC4);

            Logger.Log(ptrFactory.ToString());
        }

        public override bool Start(int start) => gameMenu.Old && !gameMenu.New;

        public override void OnStart(TimerModel timer, HashSet<string> splits) {
            bossOffsets.Clear();
            itemIds.Clear();
            foreach(string split in splits) {
                if(split.StartsWith("Boss_")) {
                    string bossName = split.Substring(5);
                    bossOffsets.Add((int)Enum.Parse(typeof(EBosses), bossName), bossName);
                } else if(split.StartsWith("Item_")) {
                    itemIds.Add((int)Enum.Parse(typeof(EItems), split.Substring(5)));
                } else if(split.Equals("WallofFlesh")) {
                    needHardmodeSplit = true;
                }
            }
        }

        public override bool Split() {
            return SplitBosses() || SplitItems();

            bool SplitBosses() {
                if(needHardmodeSplit && hardmode.New) {
                    Logger.Log("Split Boss WallofFlesh");
                    needHardmodeSplit = false;
                    return true;
                }

                if(bossOffsets.Count == 0) { return false; }

                foreach(KeyValuePair<int, string> kvp in bossOffsets) {
                    if(Game.Read<bool>(bosses.New + kvp.Key)) {
                        Logger.Log("Split Boss "+kvp.Value);
                        return bossOffsets.Remove(kvp.Key);
                    }
                }

                return false;
            }

            bool SplitItems() {
                if(itemIds.Count == 0) { return false; }

                for(int i = 0; i < 60; i++) {
                    int itemId = Game.Read<int>(Game.Read<IntPtr>(inventory.New + 0x8 + 0x4 * i));
                    if(itemIds.Remove(itemId)) {
                        Logger.Log("Split Item " + itemId);
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
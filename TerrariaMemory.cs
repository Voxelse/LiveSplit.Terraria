using LiveSplit.Model;
using LiveSplit.VoxSplitter;
using System;
using System.Collections.Generic;

namespace LiveSplit.Terraria {
    public class TerrariaMemory : SignatureMemory {

        public Pointer<IntPtr> Npc { get; private set; }
        public Pointer<IntPtr> Bosses { get; private set; }
        public Pointer<IntPtr> Inventory { get; private set; }
        public Pointer<bool> IsGameMenu { get; private set; }
        public Pointer<bool> IsHardmode { get; private set; }
        public Pointer<bool> IsCrimson { get; private set; }

        private readonly HashSet<int> bossOffsets = new HashSet<int>();
        private bool needHardmodeSplit = false;
        private bool needCorruptionSplit = false;
        private bool needCrimsonSplit = false;
        private readonly HashSet<int> mechBossSplits = new HashSet<int>();
        private readonly HashSet<int> mechBossOffsets = new HashSet<int>();
        private int mechBossKills = 0;
        private bool needAllBossSplit = false;
        private readonly HashSet<int> trackedBossOffsets = new HashSet<int>();
        private bool trackHardmode = false;
        private int allBossKills = 0;
        private readonly HashSet<int> itemIds = new HashSet<int>();
        private readonly HashSet<int> npcIds = new HashSet<int>();

        public TerrariaBossChecklist bossChecklist;

        public TerrariaMemory(LiveSplitState state, Logger logger) : base(state, logger) {
            SetProcessNames("Terraria");
            scanData.Add("", new Dictionary<string, SignatureHolder> {
                { "updateTime", new SignatureHolder(0, "55 8B EC 57 56 83 EC 34 8D 7D D0 B9") },
                { "updateTimeDay", new SignatureHolder(0, "55 8B EC 56 50 8B F1 33 D2") }
            });
        }
        
        protected override void OnScanDone() {
            NestedPointerFactory ptrFactory = new NestedPointerFactory(this);

            IntPtr time = scanData[""]["updateTime"].Pointer;
            IsGameMenu = ptrFactory.Make<bool>(Game.Read<IntPtr>(time + 0x90));
            Bosses = ptrFactory.Make<IntPtr>(time + 0x335);
            IsHardmode = ptrFactory.Make<bool>(Game.Read<IntPtr>(time + 0x345));
            Inventory = ptrFactory.Make<IntPtr>(Game.Read<IntPtr>(time + 0x369), 0x8, 0xC4);
            Npc = ptrFactory.Make<IntPtr>(Game.Read<IntPtr>(time + 0x7ED));

            IsCrimson = ptrFactory.Make<bool>(Game.Read<IntPtr>(scanData[""]["updateTimeDay"].Pointer + 0xD9));

            Logger.Log(ptrFactory.ToString());
        }

        public override bool Update() {
            bossChecklist?.Update(this);
            return true;
        }

        public override bool Start(int start) => IsGameMenu.Old && !IsGameMenu.New;

        public override void OnStart(HashSet<string> splits) {
            bossOffsets.Clear();
            needHardmodeSplit = false;
            needCorruptionSplit = false;
            needCrimsonSplit = false;
            mechBossSplits.Clear();
            mechBossOffsets.Clear();
            mechBossKills = 0;
            needAllBossSplit = false;
            trackedBossOffsets.Clear();
            trackHardmode = false;
            allBossKills = 0;
            itemIds.Clear();
            npcIds.Clear();


            foreach(string split in splits) {
                if(split.StartsWith("Boss_")) {
                    bossOffsets.Add((int)Enum.Parse(typeof(EBosses), split.Substring(5)));
                } else if(split.StartsWith("Item_")) {
                    itemIds.Add((int)Enum.Parse(typeof(EItems), split.Substring(5)));
                } else if(split.StartsWith("Npc_")) {
                    npcIds.Add((int)Enum.Parse(typeof(ENpcs), split.Substring(4)));
                } else if(split.StartsWith("MechBoss_")) {
                    mechBossOffsets.Add((int)EBosses.TheDestroyer);
                    mechBossOffsets.Add((int)EBosses.TheTwins);
                    mechBossOffsets.Add((int)EBosses.SkeletronPrime);
                    mechBossSplits.Add(split[split.Length - 1] - '0');
                } else if(split.Equals("WallofFlesh")) {
                    needHardmodeSplit = true;
                } else if(split.Equals("EaterofWorlds")) {
                    needCorruptionSplit = true;
                } else if(split.Equals("BrainofCthulhu")) {
                    needCrimsonSplit = true;
                } else if(split.Equals("AllBosses")) {
                    needAllBossSplit = true;
                } else if(split.Equals("Boots")) {
                    itemIds.Add((int)EItems.HermesBoots);
                    itemIds.Add((int)EItems.DuneriderBoots);
                    itemIds.Add((int)EItems.FlurryBoots);
                    itemIds.Add((int)EItems.SailfishBoots);
                }
            }

            if(needAllBossSplit) {
                trackHardmode = !needHardmodeSplit;
                foreach(EBosses boss in AllBosses) {
                    if(!bossOffsets.Contains((int)boss)
                    && (!boss.ToString().Equals("EaterofWorldsBrainofCthulhu") || (!needCorruptionSplit && !needCrimsonSplit))) {
                        trackedBossOffsets.Add((int)boss);
                    }
                }
            }

            bossChecklist?.SetRunning(true);
        }

        public override bool Split() {
            return !IsGameMenu.New && (SplitBosses() || SplitItems() || SplitNpcs());

            bool SplitBosses() {
                return SplitBoss() || SplitWormOrBrain() || SplitWallOfFlesh() || SplitMech();

                bool SplitBoss() {
                    if(bossOffsets.Count == 0 && trackedBossOffsets.Count == 0) { return false; }

                    foreach(int offset in bossOffsets) {
                        if(Game.Read<bool>(Bosses.New + offset)) {
                            Logger.Log("Split Boss " + GetBossName(offset));
                            return bossOffsets.Remove(offset) | SplitAllBosses();
                        }
                    }
                    foreach(int offset in trackedBossOffsets) {
                        if(Game.Read<bool>(Bosses.New + offset)) {
                            trackedBossOffsets.Remove(offset);
                            Logger.Log("Track Boss " + GetBossName(offset));
                            return SplitAllBosses();
                        }
                    }
                    return false;
                }

                bool SplitWormOrBrain() {
                    if((needCorruptionSplit || needCrimsonSplit)
                    && Game.Read<bool>(Bosses.New + (int)EBosses.EaterofWorldsBrainofCthulhu)) {
                        if(IsCrimson.New) {
                            if(needCrimsonSplit) {
                                needCrimsonSplit = false;
                                Logger.Log("Split Boss BrainofCthulhu");
                                return true | SplitAllBosses();
                            }
                        } else {
                            if(needCorruptionSplit) {
                                needCorruptionSplit = false;
                                Logger.Log("Split Boss EaterofWorlds");
                                return true | SplitAllBosses();
                            }
                        }
                    }
                    return false;
                }

                bool SplitWallOfFlesh() {
                    if((needHardmodeSplit || trackHardmode) && IsHardmode.New) {
                        if(needHardmodeSplit) {
                            needHardmodeSplit = false;
                            Logger.Log("Split Boss WallofFlesh");
                            return true | SplitAllBosses();
                        } else {
                            trackHardmode = false;
                            Logger.Log("Track Boss WallofFlesh");
                            return SplitAllBosses();
                        }
                    }
                    return false;
                }

                bool SplitMech() {
                    if(mechBossOffsets.Count == 0) { return false; }

                    foreach(int offset in mechBossOffsets) {
                        bool isDown = Game.Read<bool>(Bosses.New + offset);
                        if(isDown && mechBossOffsets.Remove(offset) && mechBossSplits.Remove(++mechBossKills)) {
                            if(mechBossSplits.Count == 0) {
                                mechBossOffsets.Clear();
                            }
                            Logger.Log("Split Mech Boss " + mechBossKills + " " + GetBossName(offset));
                            return true;
                        }
                    }
                    return false;
                }

                bool SplitAllBosses() {
                    return needAllBossSplit && (++allBossKills == 16);
                }
            }

            bool SplitItems() {
                if(itemIds.Count == 0) { return false; }

                for(int i = 0; i < 60; i++) {
                    int type = Game.Read<int>(Game.Read<IntPtr>(Inventory.New + 0x8 + 0x4 * i) + 0x94);
                    if(type != 0 && itemIds.Remove(type)) {
                        if(type == (int)EItems.HermesBoots || type == (int)EItems.DuneriderBoots
                        || type == (int)EItems.FlurryBoots || type == (int)EItems.SailfishBoots) {
                            itemIds.Remove((int)EItems.HermesBoots);
                            itemIds.Remove((int)EItems.DuneriderBoots);
                            itemIds.Remove((int)EItems.FlurryBoots);
                            itemIds.Remove((int)EItems.SailfishBoots);
                            Logger.Log("Split Boots Item " + GetItemName(type));
                        } else {
                            Logger.Log("Split Item " + GetItemName(type));
                        }
                        return true;
                    }
                }
                return false;
            }

            bool SplitNpcs() {
                if(npcIds.Count == 0) { return false; }

                for(int i = 0; i < 201; i++) {
                    IntPtr npcPtr = Game.Read<IntPtr>(Npc.New + 0x8 + 0x4 * i);
                    bool isActive = Game.Read<bool>(npcPtr + 0x18);
                    if(!isActive) {
                        return false;
                    }
                    int type = Game.Read<int>(npcPtr + 0xD4);
                    if(npcIds.Remove(type)) {
                        Logger.Log("Split Npc " + GetNpcName(type));
                        return true;
                    }
                }
                return false;
            }
        }

        public override void OnReset() => bossChecklist?.SetRunning(false);

        public static string GetBossName(int value) => Enum.GetName(typeof(EBosses), value);
        public static string GetItemName(int value) => Enum.GetName(typeof(EItems), value);
        public static string GetNpcName(int value) => Enum.GetName(typeof(ENpcs), value);

        public static EBosses[] AllBosses {
            get {
                List<EBosses> bosses = new List<EBosses>();
                foreach(EBosses boss in Enum.GetValues(typeof(EBosses))) {
                    string name = boss.ToString();
                    if(!name.StartsWith("_") && !name.EndsWith("Pillar")) {
                        bosses.Add(boss);
                    }
                }
                return bosses.ToArray();
            }
        }

        public override void Dispose() {
            bossChecklist?.Dispose();
            base.Dispose();
        }
    }
}
using LiveSplit.Model;
using LiveSplit.VoxSplitter;
using System;
using System.Collections.Generic;

namespace LiveSplit.Terraria {
    public class TerrariaMemory : SignatureMemory {

        private Pointer<IntPtr> npc;
        private Pointer<IntPtr> bosses;
        private Pointer<IntPtr> inventory;
        private Pointer<bool> isGameMenu;
        private Pointer<bool> isHardmode;
        private Pointer<bool> isCrimson;

        private readonly HashSet<int> bossOffsets = new HashSet<int>();
        private bool needHardmodeSplit = false;
        private bool needCorruptionSplit = false;
        private bool needCrimsonSplit = false;
        private bool needEvilSplit = false;
        private readonly HashSet<int> mechBossSplits = new HashSet<int>();
        private readonly HashSet<int> mechBossOffsets = new HashSet<int>();
        private int mechBossKills = 0;
        private readonly HashSet<int> itemIds = new HashSet<int>();
        private readonly HashSet<int> npcIds = new HashSet<int>();

        private TerrariaBossChecklist bossChecklist;
        public void ShowBossChecklist() {
            if(bossChecklist?.IsDisposed ?? true) {
                bossChecklist = new TerrariaBossChecklist();
                bossChecklist.Show();
            } else {
                bossChecklist.BringToFront();
                Logger.Log(bossChecklist.Size.ToString());
            }
        }

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
            isGameMenu = ptrFactory.Make<bool>(Game.Read<IntPtr>(time + 0x90));
            bosses = ptrFactory.Make<IntPtr>(time + 0x335);
            isHardmode = ptrFactory.Make<bool>(Game.Read<IntPtr>(time + 0x345));
            inventory = ptrFactory.Make<IntPtr>(Game.Read<IntPtr>(time + 0x369), 0x8, 0xC4);
            npc = ptrFactory.Make<IntPtr>(Game.Read<IntPtr>(time + 0x7ED));

            isCrimson = ptrFactory.Make<bool>(Game.Read<IntPtr>(scanData[""]["updateTimeDay"].Pointer + 0xD9));

            Logger.Log(ptrFactory.ToString());
        }

        public override bool Start(int start) => isGameMenu.Old && !isGameMenu.New;

        public override void OnStart(HashSet<string> splits) {
            bossOffsets.Clear();
            needHardmodeSplit = false;
            needCorruptionSplit = false;
            needCrimsonSplit = false;
            needEvilSplit = false;
            mechBossSplits.Clear();
            mechBossOffsets.Clear();
            mechBossKills = 0;
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
                } else if(split.Equals("EaterofWorldsOrBrainofCthulhu")) {
                    needEvilSplit = true;
                } else if(split.Equals("AllBosses")) {
                    needHardmodeSplit = true;
                    needEvilSplit = true;
                    foreach(EBosses boss in Enum.GetValues(typeof(EBosses))) {
                        string name = boss.ToString();
                        if(!name.StartsWith("_") && !name.EndsWith("Pillar") && !name.Equals("EaterofWorldsBrainofCthulhu")) {
                            bossOffsets.Add((int)boss);
                        }
                    }
                } else if(split.Equals("Boots")) {
                    itemIds.Add((int)EItems.HermesBoots);
                    itemIds.Add((int)EItems.DuneriderBoots);
                    itemIds.Add((int)EItems.FlurryBoots);
                    itemIds.Add((int)EItems.SailfishBoots);
                }
            }

            bossChecklist?.ResetBosses();
        }

        public override bool Split() {
            return !isGameMenu.New && (SplitBosses() || SplitItems() || SplitNpcs());

            bool SplitBosses() {
                return SplitBoss() || SplitWormOrBrain() || SplitWallOfFlesh() || SplitMech();

                bool SplitBoss() {
                    if(bossOffsets.Count == 0) { return false; }

                    foreach(int offset in bossOffsets) {
                        if(Game.Read<bool>(bosses.New + offset)) {
                            string name = GetBossName(offset);
                            bossChecklist?.CheckBoss(name);
                            Logger.Log("Split Boss " + name);
                            return bossOffsets.Remove(offset);
                        }
                    }
                    return false;
                }

                bool SplitWormOrBrain() {
                    bool isDown = Game.Read<bool>(bosses.New + (int)EBosses.EaterofWorldsBrainofCthulhu);
                    if((needCorruptionSplit || needCrimsonSplit || needEvilSplit) && isDown) {
                        if(isCrimson.New) {
                            if(needCrimsonSplit || needEvilSplit) {
                                needCrimsonSplit = false;
                                needEvilSplit = false;
                                bossChecklist?.CheckBoss(EBosses.EaterofWorldsBrainofCthulhu.ToString());
                                Logger.Log("Split Boss BrainofCthulhu");
                                return true;
                            }
                        } else {
                            if(needCorruptionSplit || needEvilSplit) {
                                needCorruptionSplit = false;
                                needEvilSplit = false;
                                bossChecklist?.CheckBoss(EBosses.EaterofWorldsBrainofCthulhu.ToString());
                                Logger.Log("Split Boss EaterofWorlds");
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool SplitWallOfFlesh() {
                    if(needHardmodeSplit && isHardmode.New) {
                        needHardmodeSplit = false;
                        bossChecklist?.CheckBoss("WallofFlesh");
                        Logger.Log("Split Boss WallofFlesh");
                        return true;
                    }
                    return false;
                }

                bool SplitMech() {
                    if(mechBossOffsets.Count == 0) { return false; }

                    foreach(int offset in mechBossOffsets) {
                        bool isDown = Game.Read<bool>(bosses.New + offset);
                        if(isDown && mechBossOffsets.Remove(offset) && mechBossSplits.Remove(++mechBossKills)) {
                            if(mechBossSplits.Count == 0) {
                                mechBossOffsets.Clear();
                            }
                            string name = GetBossName(offset);
                            bossChecklist?.CheckBoss(name);
                            Logger.Log("Split Mech Boss " + mechBossKills + " " + name);
                            return true;
                        }
                    }
                    return false;
                }
            }

            bool SplitItems() {
                if(itemIds.Count == 0) { return false; }

                for(int i = 0; i < 60; i++) {
                    int type = Game.Read<int>(Game.Read<IntPtr>(inventory.New + 0x8 + 0x4 * i) + 0x94);
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
                    IntPtr npcPtr = Game.Read<IntPtr>(npc.New + 0x8 + 0x4 * i);
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

        public override void OnReset() => bossChecklist?.ResetBosses();

        public static string GetBossName(int value) => Enum.GetName(typeof(EBosses), value);
        public static string GetItemName(int value) => Enum.GetName(typeof(EItems), value);
        public static string GetNpcName(int value) => Enum.GetName(typeof(ENpcs), value);

        public override void Dispose() {
            bossChecklist?.Dispose();
            base.Dispose();
        }
    }
}
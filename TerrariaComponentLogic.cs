using System;
using System.Collections.Generic;

namespace LiveSplit.Terraria {
    public partial class TerrariaComponent {

        private readonly HashSet<int> bossOffsets = new HashSet<int>();
        private bool needHardmodeSplit = false;
        private bool needCorruptionSplit = false;
        private bool needCrimsonSplit = false;

        private readonly HashSet<int> mechBossSplits = new HashSet<int>();
        private readonly HashSet<int> mechBossOffsets = new HashSet<int>();
        private int mechBossKills = 0;
        
        private int allBossKills = 0;
        private int IncreaseBossKills() => allBossKills++;

        private bool needAllBossSplit = false;
        private bool trackHardmode = false;
        private readonly HashSet<int> trackedBossOffsets = new HashSet<int>();

        private readonly HashSet<int> itemIds = new HashSet<int>();
        
        private readonly HashSet<int> npcIds = new HashSet<int>();

        public override bool Update() {
            bossChecklist?.Update(memory);
            return memory.Update();
        }

        public override bool Start() => memory.IsGameMenu.Old && !memory.IsGameMenu.New;

        public override void OnStart() {
            bossOffsets.Clear();
            needHardmodeSplit = false;
            needCorruptionSplit = false;
            needCrimsonSplit = false;
            
            mechBossSplits.Clear();
            mechBossOffsets.Clear();
            mechBossKills = 0;
            
            allBossKills = 0;
            
            needAllBossSplit = false;
            trackHardmode = false;
            trackedBossOffsets.Clear();
            
            itemIds.Clear();
            
            npcIds.Clear();


            foreach(string split in settings.Splits) {
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
                foreach(EBosses boss in TerrariaEnums.AllBosses) {
                    if(!bossOffsets.Contains((int)boss)
                    && (boss != EBosses.EaterofWorldsBrainofCthulhu || (!needCorruptionSplit && !needCrimsonSplit))) {
                        trackedBossOffsets.Add((int)boss);
                    }
                }
            }

            bossChecklist?.SetRunning(true);
        }

        public override bool Split() {
            return !memory.IsGameMenu.New && (SplitBosses() || SplitItems() || SplitNpcs());

            bool SplitBosses() {
                return SplitBoss() || SplitWormOrBrain() || SplitWallOfFlesh() || SplitMech();

                bool SplitBoss() {
                    foreach(int offset in bossOffsets) {
                        if(memory.IsBossBeaten(offset)) {
                            IncreaseBossKills();
                            logger.Log("Split Boss " + TerrariaEnums.BossName(offset));
                            return bossOffsets.Remove(offset);
                        }
                    }
                    foreach(int offset in trackedBossOffsets) {
                        if(memory.IsBossBeaten(offset)) {
                            IncreaseBossKills();
                            trackedBossOffsets.Remove(offset);
                            logger.Log("Track Boss " + TerrariaEnums.BossName(offset));
                            return SplitAllBosses();
                        }
                    }
                    return false;
                }

                bool SplitWormOrBrain() {
                    if((needCorruptionSplit || needCrimsonSplit)
                    && memory.IsBossBeaten((int)EBosses.EaterofWorldsBrainofCthulhu)) {
                        if(memory.IsCrimson.New) {
                            if(needCrimsonSplit) {
                                IncreaseBossKills();
                                needCrimsonSplit = false;
                                logger.Log("Split Boss BrainofCthulhu");
                                return true;
                            }
                        } else {
                            if(needCorruptionSplit) {
                                IncreaseBossKills();
                                needCorruptionSplit = false;
                                logger.Log("Split Boss EaterofWorlds");
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool SplitWallOfFlesh() {
                    if((needHardmodeSplit || trackHardmode) && memory.IsHardmode.New) {
                        IncreaseBossKills();
                        if(needHardmodeSplit) {
                            needHardmodeSplit = false;
                            logger.Log("Split Boss WallofFlesh");
                            return true;
                        } else {
                            trackHardmode = false;
                            logger.Log("Track Boss WallofFlesh");
                            return SplitAllBosses();
                        }
                    }
                    return false;
                }

                bool SplitMech() {
                    foreach(int offset in mechBossOffsets) {
                        bool isDown = memory.IsBossBeaten(offset);
                        if(isDown && mechBossOffsets.Remove(offset) && mechBossSplits.Remove(++mechBossKills)) {
                            if(mechBossSplits.Count == 0) {
                                mechBossOffsets.Clear();
                            }
                            logger.Log("Split Mech Boss " + mechBossKills + " " + TerrariaEnums.BossName(offset));
                            return true;
                        }
                    }
                    return false;
                }

                bool SplitAllBosses() {
                    return needAllBossSplit && allBossKills == 16;
                }
            }

            bool SplitItems() {
                if(itemIds.Count == 0) {
                    return false;
                }
                foreach(int type in memory.ItemSequence()) {
                    if(type != 0 && itemIds.Remove(type)) {
                        if(type == (int)EItems.HermesBoots || type == (int)EItems.DuneriderBoots
                        || type == (int)EItems.FlurryBoots || type == (int)EItems.SailfishBoots) {
                            itemIds.Remove((int)EItems.HermesBoots);
                            itemIds.Remove((int)EItems.DuneriderBoots);
                            itemIds.Remove((int)EItems.FlurryBoots);
                            itemIds.Remove((int)EItems.SailfishBoots);
                            logger.Log("Split Boots Item " + TerrariaEnums.ItemName(type));
                        } else {
                            logger.Log("Split Item " + TerrariaEnums.ItemName(type));
                        }
                        return true;
                    }
                }
                return false;
            }

            bool SplitNpcs() {
                if(npcIds.Count == 0) {
                    return false;
                }
                foreach(IntPtr npcPtr in memory.NpcSequence()) {
                    if(!memory.NpcActive(npcPtr)) {
                        return false;
                    }
                    int type = memory.NpcType(npcPtr);
                    if(npcIds.Remove(type)) {
                        logger.Log("Split Npc " + TerrariaEnums.NpcName(type));
                        return true;
                    }
                }
                return false;
            }
        }

        public override void OnReset() => bossChecklist?.SetRunning(false);
    }
}
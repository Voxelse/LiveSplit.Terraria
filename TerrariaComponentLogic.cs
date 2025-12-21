using System;
using System.Collections.Generic;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LiveSplit.Terraria {
    public partial class TerrariaComponent {

        private readonly HashSet<string> bossToCheck = new HashSet<string>();
        private bool needHardmodeSplit = false;
        private bool needCorruptionSplit = false;
        private bool needCrimsonSplit = false;

        private readonly HashSet<int> mechBossSplits = new HashSet<int>();
        private readonly HashSet<string> mechBossToCheck = new HashSet<string>();
        private int mechBossKills = 0;

        private readonly HashSet<int> pillarSplits = new HashSet<int>();
        private readonly HashSet<string> pillarToCheck = new HashSet<string>();
        private int pillarKills = 0;

        private int allBossKills = 0;
        private int IncreaseBossKills() => allBossKills++;

        private bool needAllBossSplit = false;
        private bool trackHardmode = false;
        private readonly HashSet<string> trackedBossOffsets = new HashSet<string>();

        private readonly HashSet<int> itemIds = new HashSet<int>();
        
        private readonly HashSet<int> npcIds = new HashSet<int>();

        public override bool Update() {
            if(!memory.Update()) {
                return false;
            }
            bossChecklist?.Update(memory);
            return true;
        }

        public override bool Start() => memory.IsGameMenu.Old && !memory.IsGameMenu.New;

        public override void OnStart() {
            bossToCheck.Clear();
            needHardmodeSplit = false;
            needCorruptionSplit = false;
            needCrimsonSplit = false;

            mechBossSplits.Clear();
            mechBossToCheck.Clear();
            mechBossKills = 0;

            pillarSplits.Clear();
            pillarToCheck.Clear();
            pillarKills = 0;

            allBossKills = 0;
            
            needAllBossSplit = false;
            trackHardmode = false;
            trackedBossOffsets.Clear();
            
            itemIds.Clear();
            
            npcIds.Clear();


            foreach(string split in settings.Splits) {
                if(split.StartsWith("Boss_")) {
                    bossToCheck.Add(split.Substring(5));
                } else if(split.StartsWith("Item_")) {
                    itemIds.Add((int)Enum.Parse(typeof(EItems), split.Substring(5)));
                } else if(split.StartsWith("Npc_")) {
                    npcIds.Add((int)Enum.Parse(typeof(ENpcs), split.Substring(4)));
                } else if(split.StartsWith("MechBoss_")) {
                    mechBossToCheck.Add(EBosses.TheDestroyer.ToString());
                    mechBossToCheck.Add(EBosses.TheTwins.ToString());
                    mechBossToCheck.Add(EBosses.SkeletronPrime.ToString());
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
                } else if(split.Equals("Shellphone")) {
                    itemIds.Add((int)EItems.Shellphone);
                    itemIds.Add((int)EItems.ShellphoneSpawn);
                    itemIds.Add((int)EItems.ShellphoneOcean);
                    itemIds.Add((int)EItems.ShellphoneHell);
                } else if(split.Equals("All_Pillars_4")) {
                    pillarToCheck.Add(EBosses.StardustPillar.ToString());
                    pillarToCheck.Add(EBosses.VortexPillar.ToString());
                    pillarToCheck.Add(EBosses.SolarPillar.ToString());
                    pillarToCheck.Add(EBosses.NebulaPillar.ToString());
                    pillarSplits.Add(split[split.Length - 1] - '0');
                }

            }

            if(needAllBossSplit) {
                trackHardmode = !needHardmodeSplit;
                foreach(string name in memory.Version.AllBosses()) {
                    if(!bossToCheck.Contains(name)
                    && (name != EBosses.EaterofWorldsBrainofCthulhu.ToString() || (!needCorruptionSplit && !needCrimsonSplit))) {
                        trackedBossOffsets.Add(name);
                    }
                }
            }

            bossChecklist?.SetRunning(true, memory.Version.AllBosses());
        }

        public override bool Split() {
            return !memory.IsGameMenu.New && (SplitBosses() || SplitItems() || SplitNpcs());

            bool SplitBosses() {
                return SplitBoss() || SplitWormOrBrain() || SplitWallOfFlesh() || SplitMech() || SplitPillar();

                bool SplitBoss() {
                    foreach(string name in bossToCheck) {
                        if(memory.IsBossBeaten(name)) {
                            IncreaseBossKills();
                            logger.Log("Split Boss " + name);
                            return bossToCheck.Remove(name);
                        }
                    }
                    foreach(string name in trackedBossOffsets) {
                        if(memory.IsBossBeaten(name)) {
                            IncreaseBossKills();
                            trackedBossOffsets.Remove(name);
                            logger.Log("Track Boss " + name);
                            return SplitAllBosses();
                        }
                    }
                    return false;
                }

                bool SplitWormOrBrain() {
                    if((needCorruptionSplit || needCrimsonSplit)
                    && memory.IsBossBeaten(EBosses.EaterofWorldsBrainofCthulhu.ToString())) {
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
                    foreach(string name in mechBossToCheck) {
                        bool isDown = memory.IsBossBeaten(name);
                        if(isDown && mechBossToCheck.Remove(name) && mechBossSplits.Remove(++mechBossKills)) {
                            if(mechBossSplits.Count == 0) {
                                mechBossToCheck.Clear();
                            }
                            logger.Log("Split Mech Boss " + mechBossKills + " " + name);
                            return true;
                        }
                    }
                    return false;
                }

                bool SplitPillar() {
                    foreach(string name in pillarToCheck) {
                        bool isDown = memory.IsBossBeaten(name);
                        if(isDown && pillarToCheck.Remove(name) && pillarSplits.Remove(++pillarKills)) {
                            if(pillarSplits.Count == 0) {
                                pillarToCheck.Clear();
                            }
                            logger.Log("Split Pillar Boss " + pillarKills + " " + name);
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
                        } else if(type == (int)EItems.Shellphone || type == (int)EItems.ShellphoneHell
                        || type == (int)EItems.ShellphoneOcean || type == (int)EItems.ShellphoneSpawn) {
                            itemIds.Remove((int)EItems.Shellphone);
                            itemIds.Remove((int)EItems.ShellphoneSpawn);
                            itemIds.Remove((int)EItems.ShellphoneOcean);
                            itemIds.Remove((int)EItems.ShellphoneHell);
                            logger.Log("Split Shellphone Item " + TerrariaEnums.ItemName(type));
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
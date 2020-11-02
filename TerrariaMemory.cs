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
        private readonly HashSet<int> itemIds = new HashSet<int>();
        private readonly HashSet<int> npcIds = new HashSet<int>();

        public TerrariaMemory(Logger logger) : base(logger) {
            processName = "Terraria";
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

        public override void OnStart(TimerModel timer, HashSet<string> splits) {
            bossOffsets.Clear();
            needHardmodeSplit = false;
            needCorruptionSplit = false;
            needCrimsonSplit = false;
            itemIds.Clear();
            npcIds.Clear();

            foreach(string split in splits) {
                if(split.StartsWith("Boss_")) {
                    bossOffsets.Add((int)Enum.Parse(typeof(EBosses), split.Substring(5)));
                } else if(split.StartsWith("Item_")) {
                    itemIds.Add((int)Enum.Parse(typeof(EItems), split.Substring(5)));
                } else if(split.StartsWith("Npc_")) {
                    npcIds.Add((int)Enum.Parse(typeof(ENpcs), split.Substring(4)));
                } else if(split.Equals("WallofFlesh")) {
                    needHardmodeSplit = true;
                } else if(split.Equals("EaterofWorlds")) {
                    needCorruptionSplit = true;
                } else if(split.Equals("BrainofCthulhu")) {
                    needCrimsonSplit = true;
                }
            }
        }

        public override bool Split() {
            return !isGameMenu.New && (SplitBosses() || SplitItems() || SplitNpcs());

            bool SplitBosses() {
                return SplitBoss() || SplitWallOfFlesh() || SplitWormOrBrain();

                bool SplitBoss() {
                    if(bossOffsets.Count > 0) {
                        foreach(int offset in bossOffsets) {
                            if(Game.Read<bool>(bosses.New + offset)) {
                                Logger.Log("Split Boss " + Enum.GetName(typeof(EBosses), offset));
                                return bossOffsets.Remove(offset);
                            }
                        }
                    }
                    return false;
                }

                bool SplitWallOfFlesh() {
                    if(needHardmodeSplit && isHardmode.New) {
                        needHardmodeSplit = false;
                        Logger.Log("Split Boss WallofFlesh");
                        return true;
                    }
                    return false;
                }

                bool SplitWormOrBrain() {
                    if((needCorruptionSplit || needCrimsonSplit) && Game.Read<bool>(bosses.New + (int)EBosses.EaterofWorldsBrainofCthulhu)) {
                        if(isCrimson.New) {
                            if(needCrimsonSplit) {
                                needCrimsonSplit = false;
                                Logger.Log("Split Boss BrainofCthulhu");
                                return true;
                            }
                        } else {
                            if(needCorruptionSplit) {
                                needCorruptionSplit = false;
                                Logger.Log("Split Boss EaterofWorlds");
                                return true;
                            }
                        }
                    }
                    return false;
                }
            }

            bool SplitItems() {
                if(itemIds.Count > 0) {
                    for(int i = 0; i < 60; i++) {
                        int type = Game.Read<int>(Game.Read<IntPtr>(inventory.New + 0x8 + 0x4 * i) + 0x94);
                        if(type != 0 && itemIds.Remove(type)) {
                            Logger.Log("Split Item " + Enum.GetName(typeof(EItems), type));
                            return true;
                        }
                    }
                }
                return false;
            }

            bool SplitNpcs() {
                if(npcIds.Count > 0) {
                    for(int i = 0; i < 201; i++) {
                        IntPtr npcPtr = Game.Read<IntPtr>(npc.New + 0x8 + 0x4 * i);
                        bool isActive = Game.Read<bool>(npcPtr + 0x18);
                        if(!isActive) {
                            return false;
                        }
                        int type = Game.Read<int>(npcPtr + 0xD4);
                        if(npcIds.Remove(type)) {
                            Logger.Log("Split Npc " + Enum.GetName(typeof(ENpcs), type));
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.Terraria {

    public abstract class TerrariaVersion {
        public string Signature { get; protected set; }

        public Dictionary<string, int> BossLookup { get; protected set; }

        public int GameMenuAsmOffset { get; protected set; }

        public int BossAsmOffset { get; protected set; }
        public int HardmodeAsmOffset { get; protected set; }
        public int PlayerAsmOffset { get; protected set; }
        public int NpcAsmOffset { get; protected set; }

        public int InventoryOffset { get; protected set; }
        public int InventoryTypeOffset { get; protected set; }

        public int NpcActiveOffset { get; protected set; }
        public int NpcTypeOffset { get; protected set; }

        public IEnumerable<string> AllBosses() {
            return BossLookup.Keys.Where(key => !key.EndsWith("Pillar"));
        }

        public static TerrariaVersion GetVersion(Version ver) {
            if(ver.Minor == 4) {
                if(ver >= new Version(1, 4, 3, 3)) {
                    return new TerrariaVersion_1_4_3_3();
                } else if(ver >= new Version(1, 4, 3, 1)) {
                    return new TerrariaVersion_1_4_3_1();
                } else if(ver >= new Version(1, 4, 3)) {
                    return new TerrariaVersion_1_4_3();
                } else if(ver >= new Version(1, 4, 2)) {
                    return new TerrariaVersion_1_4_2();
                } else if(ver >= new Version(1, 4, 1)) {
                    return new TerrariaVersion_1_4_1();
                } else {
                    return new TerrariaVersion_1_4_0_1();
                }
            } else if(ver.Minor == 3) {
                if(ver >= new Version(1, 3, 5, 3)) {
                    return new TerrariaVersion_1_3_5_3();
                } else if(ver >= new Version(1, 3, 5)) {
                    return new TerrariaVersion_1_3_5();
                } else if(ver >= new Version(1, 3, 4, 3)) {
                    return new TerrariaVersion_1_3_4_3();
                } else if(ver >= new Version(1, 3, 4)) {
                    return new TerrariaVersion_1_3_4();
                } else if(ver >= new Version(1, 3, 3, 3)) {
                    return new TerrariaVersion_1_3_3_3();
                } else if(ver >= new Version(1, 3, 3)) {
                    return new TerrariaVersion_1_3_3();
                } else if(ver >= new Version(1, 3, 2)) {
                    return new TerrariaVersion_1_3_2();
                } else if(ver >= new Version(1, 3, 1)) {
                    return new TerrariaVersion_1_3_1();
                } else if(ver >= new Version(1, 3, 0, 8)) {
                    return new TerrariaVersion_1_3_0_8();
                } else if(ver >= new Version(1, 3, 0, 4)) {
                    return new TerrariaVersion_1_3_0_4();
                } else if(ver >= new Version(1, 3, 0, 3)) {
                    return new TerrariaVersion_1_3_0_3();
                } else {
                    return new TerrariaVersion_1_3_0_1();
                }
            } else if(ver.Minor == 2) {
                if(ver >= new Version(1, 2, 4)) {
                    return new TerrariaVersion_1_2_4();
                } else if(ver >= new Version(1, 2, 3, 1)) {
                    return new TerrariaVersion_1_2_3_1();
                } else if(ver >= new Version(1, 2, 3)) {
                    return new TerrariaVersion_1_2_3();
                } else if(ver >= new Version(1, 2, 2)) {
                    return new TerrariaVersion_1_2_2();
                } else if(ver >= new Version(1, 2, 1, 2)) {
                    return new TerrariaVersion_1_2_1_2();
                } else if(ver >= new Version(1, 2, 1)) {
                    return new TerrariaVersion_1_2_1();
                } else if(ver >= new Version(1, 2, 0, 3)) {
                    return new TerrariaVersion_1_2_0_3();
                } else {
                    return new TerrariaVersion_1_2();
                }
            }

            return new TerrariaVersion_1_4_3_3();
        }

        
        private class TerrariaVersion_1_2 : TerrariaVersion {
            public TerrariaVersion_1_2() {
                Signature = "55 8B EC 57 56 53 81 EC ???????? 8D BD ???????? B9 ???????? 33 C0 F3 AB 83 3D";

                BossLookup = new Dictionary<string, int>() {
                    { EBosses.EyeofCthulhu.ToString(), 0x0 },
                    { EBosses.EaterofWorldsBrainofCthulhu.ToString(), 0x1 },
                    { EBosses.Skeletron.ToString(), 0x2 },
                    { EBosses.QueenBee.ToString(), 0x3 },
                    //{ "_SavedGoblin", 0x4 },
                    //{ "_SavedWizard", 0x5 },
                    //{ "_SavedMechanic", 0x6 },
                    //{ "_GoblinArmy", 0x7 },
                    //{ "_FrostLegion", 0x8 },
                    //{ "_PirateInvasion", 0x9 },
                    //{ "_SolarEclipse", 0xA },
                    { EBosses.Plantera.ToString(), 0xB },
                    { EBosses.Golem.ToString(), 0xC },
                    //{ "_AnyMechanical", 0xD },
                    { EBosses.TheDestroyer.ToString(), 0xE },
                    { EBosses.TheTwins.ToString(), 0xF },
                    { EBosses.SkeletronPrime.ToString(), 0x10 },
                };

                GameMenuAsmOffset = 0x26;

                BossAsmOffset = 0x6BE;
                HardmodeAsmOffset = 0x422;
                PlayerAsmOffset = 0x1DC;
                NpcAsmOffset = 0x754;

                InventoryOffset = 0x3C;
                InventoryTypeOffset = 0x30;

                NpcActiveOffset = 0xF8;
                NpcTypeOffset = 0x6C;
            }
        }

        private class TerrariaVersion_1_2_0_3 : TerrariaVersion_1_2 {
            public TerrariaVersion_1_2_0_3() : base() {
                BossAsmOffset = 0x6CB;
                NpcAsmOffset = 0x761;
            }
        }

        private class TerrariaVersion_1_2_1 : TerrariaVersion_1_2_0_3 {
            public TerrariaVersion_1_2_1() : base() {
                Signature = "55 8B EC 57 56 53 81 EC ???????? 8D BD ???????? B9 ???????? 33 C0 F3 AB 80 3D ???????? 00 74 ?? C6 05 ???????? 00 83 3D";

                GameMenuAsmOffset = 0x36;

                BossAsmOffset = 0x72E;
                HardmodeAsmOffset = 0x43D;
                PlayerAsmOffset = 0x1EC;
                NpcAsmOffset = 0x7C4;
            }
        }

        private class TerrariaVersion_1_2_1_2 : TerrariaVersion_1_2_1 {
            public TerrariaVersion_1_2_1_2() : base() {
                BossAsmOffset = 0x736;
                NpcAsmOffset = 0x7CC;
            }
        }

        private class TerrariaVersion_1_2_2 : TerrariaVersion_1_2_1_2 {
            public TerrariaVersion_1_2_2() : base() {
                Signature = "55 8B EC 57 56 53 81 EC ???????? 8D BD ???????? B9 ???????? 33 C0 F3 AB 80 3D ???????? 00 74 ?? C6 05 ???????? 00 C6 05";

                GameMenuAsmOffset = 0x4D;

                BossAsmOffset = 0x744;
                HardmodeAsmOffset = 0x454;
                PlayerAsmOffset = 0x203;
                NpcAsmOffset = 0x7DA;
            }
        }

        private class TerrariaVersion_1_2_3 : TerrariaVersion_1_2_2 {
            public TerrariaVersion_1_2_3() : base() {
                BossLookup = new Dictionary<string, int>() {
                    { EBosses.EyeofCthulhu.ToString(), 0x0 },
                    { EBosses.EaterofWorldsBrainofCthulhu.ToString(), 0x1 },
                    { EBosses.Skeletron.ToString(), 0x2 },
                    { EBosses.QueenBee.ToString(), 0x3 },
                    //{ "_SavedGoblin", 0x4 },
                    //{ "_SavedWizard", 0x5 },
                    //{ "_SavedMechanic", 0x6 },
                    //{ "_SavedStylist", 0x7 },
                    //{ "_GoblinArmy", 0x8 },
                    //{ "_FrostLegion", 0x9 },
                    //{ "_PirateInvasion", 0xA },
                    //{ "_SolarEclipse", 0xB },
                    { EBosses.Plantera.ToString(), 0xC },
                    { EBosses.Golem.ToString(), 0xD },
                    //{ "_AnyMechanical", 0xE },
                    { EBosses.TheDestroyer.ToString(), 0xF },
                    { EBosses.TheTwins.ToString(), 0x10 },
                    { EBosses.SkeletronPrime.ToString(), 0x11 },
                };

                BossAsmOffset = 0x87E;
                HardmodeAsmOffset = 0x589;
                PlayerAsmOffset = 0x338;
                NpcAsmOffset = 0x2AD;

                InventoryOffset = 0x44;

                NpcActiveOffset = 0xFF;
            }
        }

        private class TerrariaVersion_1_2_3_1 : TerrariaVersion_1_2_3 {
            public TerrariaVersion_1_2_3_1() : base() {
                BossAsmOffset = 0x8CB;
                HardmodeAsmOffset = 0x5D6;
                PlayerAsmOffset = 0x33C;
                NpcAsmOffset = 0x2AE;
            }
        }

        private class TerrariaVersion_1_2_4 : TerrariaVersion_1_2_3_1 {
            public TerrariaVersion_1_2_4() : base() {
                BossLookup = new Dictionary<string, int>() {
                    { EBosses.EyeofCthulhu.ToString(), 0x0 },
                    { EBosses.EaterofWorldsBrainofCthulhu.ToString(), 0x1 },
                    { EBosses.Skeletron.ToString(), 0x2 },
                    { EBosses.QueenBee.ToString(), 0x3 },
                    //{ "_SavedGoblin", 0x4 },
                    //{ "_SavedWizard", 0x5 },
                    //{ "_SavedMechanic", 0x6 },
                    //{ "_SavedAngler", 0x7 },
                    //{ "_SavedStylist", 0x8 },
                    //{ "_GoblinArmy", 0x9 },
                    //{ "_FrostLegion", 0xA },
                    //{ "_PirateInvasion", 0xB },
                    //{ "_SolarEclipse", 0xC },
                    { EBosses.Plantera.ToString(), 0xD },
                    { EBosses.Golem.ToString(), 0xE },
                    //{ "_AnyMechanical", 0xF },
                    { EBosses.TheDestroyer.ToString(), 0x10 },
                    { EBosses.TheTwins.ToString(), 0x11 },
                    { EBosses.SkeletronPrime.ToString(), 0x12 },
                };

                BossAsmOffset = 0x91A;
                HardmodeAsmOffset = 0x618;
                PlayerAsmOffset = 0x378;
                NpcAsmOffset = 0x2EA;

                InventoryTypeOffset = 0x38;

                NpcActiveOffset = 0x102;
            }
        }


        private class TerrariaVersion_1_3_0_1 : TerrariaVersion_1_2_4 {
            public TerrariaVersion_1_3_0_1() : base() {
                BossLookup = new Dictionary<string, int>() {
                    { EBosses.EyeofCthulhu.ToString(), -0x4 },
                    { EBosses.EaterofWorldsBrainofCthulhu.ToString(), -0x3 },
                    { EBosses.Skeletron.ToString(), -0x2 },
                    { EBosses.QueenBee.ToString(), -0x1 },
                    { EBosses.KingSlime.ToString(), 0x0 },
                    //{ "_GoblinArmy", 0x1 },
                    //{ "_FrostLegion", 0x2 },
                    //{ "_PirateInvasion", 0x3 },
                    //{ "_SolarEclipse", 0x4 },
                    { EBosses.Plantera.ToString(), 0x5 },
                    { EBosses.Golem.ToString(), 0x6 },
                    //{ "_MartianSaucer", 0x7 },
                    { EBosses.DukeFishron.ToString(), 0x8 },
                    //{ "_MourningWood", 0x9 },
                    //{ "_Pumpking", 0xA },
                    //{ "_IceQueen", 0xB },
                    //{ "_Everscream", 0xC },
                    //{ "_SantaNK1", 0xD },
                    { EBosses.LunaticCultist.ToString(), 0xE },
                    { EBosses.MoonLord.ToString(), 0xF },
                    { EBosses.SolarPillar.ToString(), 0x10 },
                    { EBosses.VortexPillar.ToString(), 0x11 },
                    { EBosses.NebulaPillar.ToString(), 0x12 },
                    { EBosses.StardustPillar.ToString(), 0x13 },
                    //{ "_ActiveSolar", 0x14 },
                    //{ "_ActiveVortex", 0x15 },
                    //{ "_ActiveNebula", 0x16 },
                    //{ "_ActiveStardust", 0x17 },
                    //{ "_ApocalypseUp", 0x18 },
                    //{ "_AnyMechanical", 0x19 },
                    { EBosses.TheDestroyer.ToString(), 0x1A },
                    { EBosses.TheTwins.ToString(), 0x1B },
                    { EBosses.SkeletronPrime.ToString(), 0x1C },
                };

                BossAsmOffset = 0x2E0;
                HardmodeAsmOffset = 0x2F0;
                PlayerAsmOffset = 0x321;
                NpcAsmOffset = 0x8EC;

                InventoryOffset = 0xA8;
                InventoryTypeOffset = 0x6C;

                NpcActiveOffset = 0x1C;
                NpcTypeOffset = 0xB0;
            }
        }

        private class TerrariaVersion_1_3_0_3 : TerrariaVersion_1_3_0_1 {
            public TerrariaVersion_1_3_0_3() : base() {
                InventoryOffset = 0xAC;
            }
        }

        private class TerrariaVersion_1_3_0_4 : TerrariaVersion_1_3_0_3 {
            public TerrariaVersion_1_3_0_4() : base() {
                NpcTypeOffset = 0xB4;
            }
        }

        private class TerrariaVersion_1_3_0_8 : TerrariaVersion_1_3_0_4 {
            public TerrariaVersion_1_3_0_8() : base() {
                BossAsmOffset = 0x2DB;
                HardmodeAsmOffset = 0x2EB;
                PlayerAsmOffset = 0x31C;
                NpcAsmOffset = 0x8E5;
            }
        }

        private class TerrariaVersion_1_3_1 : TerrariaVersion_1_3_0_8 {
            public TerrariaVersion_1_3_1() : base() {
                BossAsmOffset = 0x2DE;
                HardmodeAsmOffset = 0x2EE;
                PlayerAsmOffset = 0x31F;
                NpcAsmOffset = 0x8DF;

                InventoryOffset = 0xBC;

                NpcTypeOffset = 0xB8;
            }
        }

        private class TerrariaVersion_1_3_2 : TerrariaVersion_1_3_1 {
            public TerrariaVersion_1_3_2() : base() {
                NpcAsmOffset = 0x8E4;

                NpcTypeOffset = 0xBC;
            }
        }

        private class TerrariaVersion_1_3_3 : TerrariaVersion_1_3_2 {
            public TerrariaVersion_1_3_3() : base() {
                BossAsmOffset = 0x2E3;
                HardmodeAsmOffset = 0x2F3;
                PlayerAsmOffset = 0x324;
                NpcAsmOffset = 0x8F2;

            }
        }

        private class TerrariaVersion_1_3_3_3 : TerrariaVersion_1_3_3 {
            public TerrariaVersion_1_3_3_3() : base() {
                NpcAsmOffset = 0x8F9;
            }
        }

        private class TerrariaVersion_1_3_4 : TerrariaVersion_1_3_3_3 {
            public TerrariaVersion_1_3_4() : base() {
                BossAsmOffset = 0x293;
                HardmodeAsmOffset = 0x2A3;
                PlayerAsmOffset = 0x2D4;
                NpcAsmOffset = 0x8A4;

                InventoryTypeOffset = 0x70;

                NpcTypeOffset = 0xD0;
            }
        }

        private class TerrariaVersion_1_3_4_3 : TerrariaVersion_1_3_4 {
            public TerrariaVersion_1_3_4_3() : base() {
                NpcTypeOffset = 0xD4;
            }
        }

        private class TerrariaVersion_1_3_5 : TerrariaVersion_1_3_4_3 {
            public TerrariaVersion_1_3_5() : base() {
                BossAsmOffset = 0x263;
                HardmodeAsmOffset = 0x273;
                PlayerAsmOffset = 0x2A3;
                NpcAsmOffset = 0x96F;

                InventoryTypeOffset = 0x6C;

                NpcActiveOffset = 0x18;
                NpcTypeOffset = 0xD0;
            }
        }

        private class TerrariaVersion_1_3_5_3 : TerrariaVersion_1_3_5 {
            public TerrariaVersion_1_3_5_3() : base() {
                BossAsmOffset = 0x257;
                HardmodeAsmOffset = 0x267;
                PlayerAsmOffset = 0x297;
                NpcAsmOffset = 0x907;
            }
        }


        private class TerrariaVersion_1_4_0_1 : TerrariaVersion_1_3_5_3 {
            public TerrariaVersion_1_4_0_1() : base() {
                Signature = "55 8B EC 57 56 83 EC ?? 8D 7D ?? B9 ???????? 33 C0 F3 AB 80 3D ???????? 00 75 ?? 0FB6";

                BossLookup = new Dictionary<string, int>() {
                    { EBosses.EyeofCthulhu.ToString(), -0x4 },
                    { EBosses.EaterofWorldsBrainofCthulhu.ToString(), -0x3 },
                    { EBosses.Skeletron.ToString(), -0x2 },
                    { EBosses.QueenBee.ToString(), -0x1 },
                    { EBosses.KingSlime.ToString(), 0x0 },
                    //{ "_GoblinArmy", 0x1 },
                    //{ "_FrostLegion", 0x2 },
                    //{ "_PirateInvasion", 0x3 },
                    //{ "_SolarEclipse", 0x4 },
                    { EBosses.Plantera.ToString(), 0x5 },
                    { EBosses.Golem.ToString(), 0x6 },
                    //{ "_MartianSaucer", 0x7 },
                    { EBosses.DukeFishron.ToString(), 0x8 },
                    //{ "_MourningWood", 0x9 },
                    //{ "_Pumpking", 0xA },
                    //{ "_IceQueen", 0xB },
                    //{ "_Everscream", 0xC },
                    //{ "_SantaNK1", 0xD },
                    { EBosses.LunaticCultist.ToString(), 0xE },
                    { EBosses.MoonLord.ToString(), 0xF },
                    { EBosses.SolarPillar.ToString(), 0x10 },
                    { EBosses.VortexPillar.ToString(), 0x11 },
                    { EBosses.NebulaPillar.ToString(), 0x12 },
                    { EBosses.StardustPillar.ToString(), 0x13 },
                    { EBosses.EmpressofLight.ToString(), 0x14 },
                    { EBosses.QueenSlime.ToString(), 0x15 },
                    //{ "_ActiveSolar", 0x16 },
                    //{ "_ActiveVortex", 0x17 },
                    //{ "_ActiveNebula", 0x18 },
                    //{ "_ActiveStardust", 0x19 },
                    //{ "_ApocalypseUp", 0x1A },
                    //{ "_AnyMechanical", 0x1B },
                    { EBosses.TheDestroyer.ToString(), 0x1C },
                    { EBosses.TheTwins.ToString(), 0x1D },
                    { EBosses.SkeletronPrime.ToString(), 0x1E },
                };

                GameMenuAsmOffset = 0x90;

                BossAsmOffset = 0x320;
                HardmodeAsmOffset = 0x330;
                PlayerAsmOffset = 0x354;
                NpcAsmOffset = 0x7C1;

                InventoryOffset = 0xC4;
                InventoryTypeOffset = 0x90;

                NpcTypeOffset = 0xD4;
            }
        }

        private class TerrariaVersion_1_4_1 : TerrariaVersion_1_4_0_1 {
            public TerrariaVersion_1_4_1() : base() {
                BossAsmOffset = 0x335;
                HardmodeAsmOffset = 0x345;
                PlayerAsmOffset = 0x369;
                NpcAsmOffset = 0x7ED;

                InventoryTypeOffset = 0x94;
            }
        }

        private class TerrariaVersion_1_4_2 : TerrariaVersion_1_4_1 {
            public TerrariaVersion_1_4_2() : base() {
                BossAsmOffset = 0x342;
                HardmodeAsmOffset = 0x352;
                PlayerAsmOffset = 0x376;
                NpcAsmOffset = 0x7F1;

                InventoryOffset = 0xD0;
                InventoryTypeOffset = 0x9C;

                NpcActiveOffset = 0x20;
                NpcTypeOffset = 0xDC;
            }
        }

        private class TerrariaVersion_1_4_3 : TerrariaVersion_1_4_2 {
            public TerrariaVersion_1_4_3() : base() {
                BossLookup = new Dictionary<string, int>() {
                    { EBosses.EyeofCthulhu.ToString(), -0x4 },
                    { EBosses.EaterofWorldsBrainofCthulhu.ToString(), -0x3 },
                    { EBosses.Skeletron.ToString(), -0x2 },
                    { EBosses.QueenBee.ToString(), -0x1 },
                    { EBosses.KingSlime.ToString(), 0x0 },
                    //{ "_GoblinArmy", 0x1 },
                    //{ "_FrostLegion", 0x2 },
                    //{ "_PirateInvasion", 0x3 },
                    //{ "_SolarEclipse", 0x4 },
                    { EBosses.Plantera.ToString(), 0x5 },
                    { EBosses.Golem.ToString(), 0x6 },
                    //{ "_MartianSaucer", 0x7 },
                    { EBosses.DukeFishron.ToString(), 0x8 },
                    //{ "_MourningWood", 0x9 },
                    //{ "_Pumpking", 0xA },
                    //{ "_IceQueen", 0xB },
                    //{ "_Everscream", 0xC },
                    //{ "_SantaNK1", 0xD },
                    { EBosses.LunaticCultist.ToString(), 0xE },
                    { EBosses.MoonLord.ToString(), 0xF },
                    { EBosses.SolarPillar.ToString(), 0x10 },
                    { EBosses.VortexPillar.ToString(), 0x11 },
                    { EBosses.NebulaPillar.ToString(), 0x12 },
                    { EBosses.StardustPillar.ToString(), 0x13 },
                    { EBosses.EmpressofLight.ToString(), 0x14 },
                    { EBosses.QueenSlime.ToString(), 0x15 },
                    { EBosses.Deerclops.ToString(), 0x16 },
                    //{ "_ActiveSolar", 0x17 },
                    //{ "_ActiveVortex", 0x18 },
                    //{ "_ActiveNebula", 0x19 },
                    //{ "_ActiveStardust", 0x1A },
                    //{ "_ApocalypseUp", 0x1B },
                    //{ "_AnyMechanical", 0x1C },
                    { EBosses.TheDestroyer.ToString(), 0x1D },
                    { EBosses.TheTwins.ToString(), 0x1E },
                    { EBosses.SkeletronPrime.ToString(), 0x1F },
                };

                BossAsmOffset = 0x336;
                HardmodeAsmOffset = 0x346;
                PlayerAsmOffset = 0x36A;
                NpcAsmOffset = 0x81C;
            }
        }

        private class TerrariaVersion_1_4_3_1 : TerrariaVersion_1_4_3 {
            public TerrariaVersion_1_4_3_1() : base() {
                NpcAsmOffset = 0x81A;
            }
        }

        private class TerrariaVersion_1_4_3_3 : TerrariaVersion_1_4_3_1 {
            public TerrariaVersion_1_4_3_3() : base() {
                BossAsmOffset = 0x353;
                HardmodeAsmOffset = 0x363;
                PlayerAsmOffset = 0x387;
                NpcAsmOffset = 0x833;
            }
        }
    }
}
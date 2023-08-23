using System;

namespace LiveSplit.Terraria {

    public static class TerrariaEnums {
        public static string ItemName(int value) => Enum.GetName(typeof(EItems), value);
        public static string NpcName(int value) => Enum.GetName(typeof(ENpcs), value);
    }

    public enum EBosses {
        //Wall of Flesh
        EyeofCthulhu,
        EaterofWorldsBrainofCthulhu,
        Skeletron,
        QueenBee,
        KingSlime,
        //_GoblinArmy,
        //_FrostLegion,
        //_PirateInvasion,
        //_SolarEclipse,
        Plantera,
        Golem,
        //_MartianSaucer,
        DukeFishron,
        //_MourningWood,
        //_Pumpking,
        //_IceQueen,
        //_Everscream,
        //_SantaNK1,
        LunaticCultist,
        MoonLord,
        SolarPillar,
        VortexPillar,
        NebulaPillar,
        StardustPillar,
        EmpressofLight,
        QueenSlime,
        Deerclops,
        TheDestroyer,
        TheTwins,
        SkeletronPrime
    }

    public enum EItems {
        LightsBane = 46,
        HermesBoots = 54,
        PlatinumCoin = 74,
        Musket = 96,
        MusketBall = 97,
        FieryGreatsword = 121,
        MoltenPickaxe = 122,
        Muramasa = 155,
        Grenade = 168,
        BladeofGrass = 190,
        Bed = 224,
        NightsEdge = 273,
        MythrilAnvil = 525,
        Shotgun = 534,
        Jetpack = 748,
        TerraBlade = 757,
        BloodButcherer = 795,
        TheUndertaker = 800,
        Boomstick = 964,
        OrichalcumAnvil = 1220,
        ChlorophyteShotbow = 1229,
        FlurryBoots = 1579,
        TheHorsemansBlade = 1826,
        TruffleWorm = 2673,
        InfluxWaver = 2880,
        SailfishBoots = 3200,
        DuneriderBoots = 4055,
        MagicConch = 4263,
        GoatSkull = 4795,
        Zenith = 4956,
        GelatinCrystal = 4988,
        TorchGodsFavor = 5043,
        BeeHive = 5066,
        BastStatue = 4276,
        OcramsRazor = 5334
    }

    public enum ENpcs {
        Merchant = 17,
        Nurse = 18,
        ArmsDealer = 19,
        Demolitionist = 38,
    }
}

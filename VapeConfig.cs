using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace VapeLoot
{
    public static class VapeConfig
    {
        public static bool EnableRarityRings = true;
        public static int StatMultiplierMin = 2;
        public static int StatMultiplierMax = 4;
        public static int StatUniqueMultiplierMin = 6;
        public static int StatUniqueMultiplierMax = 9;
        public static int UncommonChance = 15;
        public static int RareChance = 10;
        public static int EpicChance = 5;

        private static readonly string ConfigPath = Path.Combine(Main.SavePath, "Mod Configs", "VapeLoot.json");

        private static readonly Preferences ConfigFile = new Preferences(ConfigPath);

        public static void Load()
        {
            if (!ReadConfig(ConfigFile))
            {
                ErrorLogger.Log("Failed to read config file: VapeLoot.json! Recreating config...");
                CreateConfig(ConfigFile);
            }
        }

        private static bool ReadConfig(Preferences conf)
        {
            if (conf.Load())
            {
                conf.Get("EnableRarityRings", ref EnableRarityRings);
                conf.Get("StatMultiplierMin", ref StatMultiplierMin);
                conf.Get("StatMultiplierMax", ref StatMultiplierMax);
                conf.Get("StatUniqueMultiplierMin", ref StatUniqueMultiplierMin);
                conf.Get("StatUniqueMultiplierMax", ref StatUniqueMultiplierMax);
                conf.Get("UncommonChance", ref UncommonChance);
                conf.Get("RareChance", ref RareChance);
                conf.Get("EpicChance", ref EpicChance);
                return true;
            }
            return false;
        }

        private static void CreateConfig(Preferences conf)
        {
            conf.Clear();

            conf.Put("EnableRarityRings", EnableRarityRings);
            conf.Put("StatMultiplierMin", StatMultiplierMin);
            conf.Put("StatMultiplierMax", StatMultiplierMax);
            conf.Put("StatUniqueMultiplierMin", StatUniqueMultiplierMin);
            conf.Put("StatUniqueMultiplierMax", StatUniqueMultiplierMax);
            conf.Put("UncommonChance", UncommonChance);
            conf.Put("RareChance", RareChance);
            conf.Put("EpicChance", EpicChance);

            conf.Save();
        }
    }
}

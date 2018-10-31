using System.Text;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using System;
using System.Diagnostics;

namespace VapeLoot
{
    class VapeLoot : Mod
    {
        internal static bool VapeRpgLoaded;
        internal static Mod VapeRpgMod;

        public VapeLoot()
        {
            Properties = new ModProperties()
            {
                Autoload = true,
                AutoloadGores = true,
                AutoloadSounds = true
            };
        }

        public override void Load()
        {
            VapeConfig.Load();

            VapeRpgMod = ModLoader.GetMod("VapeRPG");
            VapeRpgLoaded = VapeRpgMod != null;
            if(!VapeRpgLoaded)
            {
                ErrorLogger.Log("[VapeLoot]: VapeRPG not found. Reverting to vanilla bonuses...");
            }
        }

        public static string GetStatName(string field)
        {
            StringBuilder sb = new StringBuilder();

            if (field.Contains("Life"))
            {
                return "Life";
            }
            else if (field.Contains("Mana"))
            {
                return "Mana";
            }

            sb.Append(char.ToUpper(field[0]));
            for (int i = 1; i < field.Length; i++)
            {
                sb.Append(field[i]);

                if (i < field.Length - 1 && char.IsUpper(field[i + 1]))
                {
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }
    }
}

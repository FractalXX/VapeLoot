using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.IO;

namespace VapeLoot
{
    public enum ItemQuality { Unique = -1, Common, Uncommon, Rare, Epic }

    class VapeGlobalItem : GlobalItem
    {
        private static Random rnd = new Random();

        /// <summary>
        /// The quality of the item.
        /// </summary>
        public ItemQuality quality;

        /// <summary>
        /// Stores the stat bonuses this item gives for the player.
        /// </summary>
        private Dictionary<string, int> statBonus;

        /// <summary>
        /// Returns true if the item was already qualified.
        /// </summary>
        public bool wasQualified;

        public bool tooltipVisible;

        // Quality Colors
        private static Color uncommonColor = Color.LimeGreen;
        private static Color rareColor = Color.Blue;
        private static Color epicColor = Color.BlueViolet;
        private static Color uniqueColor = Color.SkyBlue;

        // Stat paris for unique items
        private static string[,] uniqueStatPairs =
        {
            {"meleeDamage", "meleeSpeed" },
            {"meleeDamage", "meleeCrit" },
            {"meleeCrit", "meleeSpeed" },
            {"rangedDamage", "rangedCrit" },
            {"magicDamage", "magicCrit" }
        };

        private static string[] baseStats =
        {
            "meleeDamage",
            "meleeSpeed",
            "meleeCrit",
            "rangedDamage",
            "rangedCrit",
            "magicDamage",
            "magicCrit",
            "moveSpeed",
            "minionDamage",
            "maxMinions",
            "thrownDamage",
            "thrownCrit",
            "thrownVelocity",
            "lifeRegen",
            "manaRegen",
            "statLifeMax2",
            "statManaMax2"
        };

        public override bool InstancePerEntity
        {
            get
            {
                return true;
            }
        }

        public VapeGlobalItem()
        {
            this.statBonus = new Dictionary<string, int>();
            this.wasQualified = false;
            this.quality = ItemQuality.Common;
            this.tooltipVisible = false;
        }

        private static bool IsQualifiable(Item item)
        {
            return item.accessory || item.defense > 0;
        }

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            VapeGlobalItem global = (VapeGlobalItem)base.Clone(item, itemClone);
            global.quality = this.quality;
            global.wasQualified = this.wasQualified;
            global.statBonus = new Dictionary<string, int>();
            global.tooltipVisible = this.tooltipVisible;

            foreach (var x in this.statBonus)
            {
                global.statBonus[x.Key] = x.Value;
            }

            return global;
        }

        public override void SetDefaults(Item item)
        {
            if (!this.wasQualified)
            {
                this.quality = ItemQuality.Common;

                if (IsQualifiable(item))
                {
                    int chance = rnd.Next(0, 100);

                    if (chance <= 5)
                    {
                        this.Qualify(item, ItemQuality.Epic);
                    }
                    else if (chance <= 15)
                    {
                        this.Qualify(item, ItemQuality.Rare);
                    }
                    else if (chance <= 30)
                    {
                        this.Qualify(item, ItemQuality.Uncommon);
                    }
                }

                this.wasQualified = true;
            }
        }

        public override void OnCraft(Item item, Recipe recipe)
        {
            this.quality = ItemQuality.Common;
            this.statBonus.Clear();
            this.tooltipVisible = true;

            if (IsQualifiable(item))
            {
                int chance = rnd.Next(0, 100);

                if (chance <= VapeConfig.EpicChance)
                {
                    this.Qualify(item, ItemQuality.Epic);
                }
                else if (chance <= VapeConfig.EpicChance + VapeConfig.RareChance)
                {
                    this.Qualify(item, ItemQuality.Rare);
                }
                else if (chance <= VapeConfig.EpicChance + VapeConfig.RareChance + VapeConfig.UncommonChance)
                {
                    this.Qualify(item, ItemQuality.Uncommon);
                }
            }

            this.wasQualified = true;
        }

        public override bool OnPickup(Item item, Player player)
        {
            this.tooltipVisible = true;
            return base.OnPickup(item, player);
        }

        public void Qualify(Item item, ItemQuality newQuality)
        {
            this.quality = newQuality;
            this.GenerateStatBonuses(item, newQuality);
        }

        private void GenerateStatBonuses(Item item, ItemQuality newQuality)
        {
            if (newQuality != ItemQuality.Common)
            {
                string[] statPool = baseStats;

                if (VapeLoot.VapeRpgLoaded)
                {
                    statPool = (string[])VapeLoot.VapeRpgMod.GetType().GetField("BaseStats", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                }

                if (newQuality == ItemQuality.Unique)
                {
                    this.statBonus.Clear();

                    string[,] uniqueStatPool = uniqueStatPairs;
                    string lifeStat = "statLifeMax2";

                    if (VapeLoot.VapeRpgLoaded)
                    {
                        GlobalItem VapeRPGGlobalItem = VapeLoot.VapeRpgMod.GetGlobalItem("VapeGlobalItem");
                        uniqueStatPool = (string[,])VapeRPGGlobalItem.GetType().GetField("uniqueStatPairs", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
                        lifeStat = "Vitality";
                    }

                    int statMin = (item.rare + 1) * VapeConfig.StatUniqueMultiplierMin;
                    int statMax = (item.rare + 1) * VapeConfig.StatUniqueMultiplierMax;
                    int statPairIndex = rnd.Next(0, uniqueStatPool.GetLength(0));

                    // Fargo fix
                    if (statMin < 0) statMin *= -1;
                    if (statMax < 0) statMax *= -1;

                    this.statBonus[uniqueStatPool[statPairIndex, 0]] = RollNewStatBonus(uniqueStatPool[statPairIndex, 0], statMin, statMax);
                    this.statBonus[uniqueStatPool[statPairIndex, 1]] = RollNewStatBonus(uniqueStatPool[statPairIndex, 1], statMin, statMax);

                    if (!VapeLoot.VapeRpgLoaded)
                    {
                        this.statBonus["moveSpeed"] = RollNewStatBonus("moveSpeed", statMin, statMax);
                    }

                    this.statBonus[lifeStat] = rnd.Next(statMin * 3, statMax * 3);

                    item.expert = true;
                }
                else
                {
                    int statNumChance = rnd.Next(0, 100);

                    int stat = 0;
                    int statMin = (item.rare + 1) * VapeConfig.StatMultiplierMin;
                    int statMax = (item.rare + 1 + ((int)newQuality - 1)) * VapeConfig.StatMultiplierMax;

                    if (statMin < 0) statMin *= -1;
                    if (statMax < 0) statMax *= -1;

                    for (int i = 0; i < (int)newQuality; i++)
                    {
                        do
                        {
                            stat = rnd.Next(0, statPool.Length);
                        }
                        while (this.statBonus.ContainsKey(baseStats[stat]));

                        this.statBonus[statPool[stat]] = RollNewStatBonus(statPool[stat], statMin, statMax);
                    }

                    int newstat = 0;
                    if (statNumChance <= 20)
                    {
                        while (newstat == stat)
                        {
                            newstat = rnd.Next(0, statPool.Length);
                        }
                        this.statBonus[statPool[newstat]] = RollNewStatBonus(statPool[newstat], statMin, statMax);
                    }
                    else if (statNumChance <= 5)
                    {
                        int newstat2 = 0;
                        while (newstat2 == stat || newstat2 == newstat)
                        {
                            newstat2 = RollNewStatBonus(statPool[newstat2], statMin, statMax);
                        }
                        this.statBonus[statPool[newstat2]] = rnd.Next(statMin, statMax);
                    }
                }
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (this.quality != ItemQuality.Common)
            {
                TooltipLine itemQuality = new TooltipLine(this.mod, "Quality", this.quality.ToString());
                Color qualityColor = Color.White;
                switch (this.quality)
                {
                    case ItemQuality.Uncommon:
                        qualityColor = uncommonColor;
                        break;

                    case ItemQuality.Rare:
                        qualityColor = rareColor;
                        break;

                    case ItemQuality.Epic:
                        qualityColor = epicColor;
                        break;

                    case ItemQuality.Unique:
                        qualityColor = uniqueColor;
                        break;
                }

                itemQuality.overrideColor = qualityColor;
                tooltips.Add(itemQuality);
            }

            foreach (var x in this.statBonus)
            {
                if (x.Value > 0)
                {
                    TooltipLine bonus = new TooltipLine(this.mod, x.Key, String.Format("+{0}{1} {2}", x.Value, IsIntegerStat(x.Key) || VapeLoot.VapeRpgLoaded ? "" : "%", VapeLoot.GetStatName(x.Key)));
                    bonus.overrideColor = Color.Yellow;
                    tooltips.Add(bonus);
                }
            }
        }


        public override bool NeedsSaving(Item item)
        {
            return true;
        }

        public override TagCompound Save(Item item)
        {
            TagCompound itemTC = new TagCompound();
            TagCompound statBonusTC = new TagCompound();

            foreach (var x in this.statBonus)
            {
                statBonusTC.Add(x.Key, x.Value);
            }

            itemTC.Add("Quality", Convert.ToString(this.quality));
            itemTC.Add("StatBonuses", statBonusTC);
            itemTC.Add("WasQualified", this.wasQualified);

            return itemTC;
        }

        public override void Load(Item item, TagCompound tag)
        {
            TagCompound statBonusTC = tag.GetCompound("StatBonuses");
            this.wasQualified = tag.GetBool("WasQualified");
            this.statBonus.Clear();

            if (this.wasQualified)
            {
                this.quality = (ItemQuality)Enum.Parse(ItemQuality.Common.GetType(), tag.GetString("Quality"));
                foreach (var x in statBonusTC)
                {
                    this.statBonus.Add(x.Key, (int)x.Value);
                }

                if (this.quality == ItemQuality.Unique)
                {
                    item.expert = true;
                }
            }
        }

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
        {
            if (VapeConfig.EnableRarityRings && IsQualifiable(item))
            {
                int dustType = 63;
                switch (this.quality)
                {
                    case ItemQuality.Uncommon:
                        dustType = 61;
                        break;

                    case ItemQuality.Rare:
                        dustType = 59;
                        break;

                    case ItemQuality.Epic:
                        dustType = 62;
                        break;

                    case ItemQuality.Unique:
                        dustType = 64;
                        break;
                }

                int dustCount = 360;
                for (int i = 0; i < dustCount; i += 2)
                {
                    double angle = i * Math.PI / 180;
                    Vector2 dustPosition = new Vector2(item.position.X + item.width / 2 + item.width * (float)Math.Cos(angle), item.position.Y + item.height / 2 + item.height * (float)Math.Sin(angle));

                    Dust dust = Dust.NewDustPerfect(dustPosition, dustType, Vector2.Zero);
                    dust.noGravity = true;
                }
            }
        }

        public override void UpdateEquip(Item item, Player player)
        {
            if (VapeLoot.VapeRpgLoaded)
            {
                // TODO: Cache reflection infos
                ModPlayer modPlayer = player.GetModPlayer(VapeLoot.VapeRpgMod, "VapePlayer");
                PropertyInfo property = modPlayer.GetType().GetProperty("EffectiveStats", BindingFlags.Public | BindingFlags.Instance);
                IDictionary<string, int> bonuses = property.GetValue(modPlayer) as IDictionary<string, int>;

                if (this.statBonus.Count > 0)
                {
                    foreach (var x in this.statBonus)
                    {
                        bonuses[x.Key] += x.Value;
                    }
                }
            }
            else
            {
                FieldInfo[] fields = typeof(Player).GetFields(BindingFlags.Public | BindingFlags.Instance);
                this.tooltipVisible = true;

                foreach (var x in fields)
                {
                    if (this.statBonus.ContainsKey(x.Name))
                    {
                        if (x.Name.Contains("Crit") || x.Name.Contains("stat") || x.Name.Contains("Minions") || x.Name.Contains("Regen"))
                        {
                            x.SetValue(player, Convert.ToInt32(x.GetValue(player)) + this.statBonus[x.Name]);
                        }
                        else
                        {
                            x.SetValue(player, (float)Convert.ToDouble(x.GetValue(player)) + this.statBonus[x.Name] / 100f);
                        }
                    }
                }
            }
        }

        public override void UpdateInventory(Item item, Player player)
        {
            this.tooltipVisible = true;
        }

        public override bool NewPreReforge(Item item)
        {
            return this.quality == ItemQuality.Common;
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write((int)this.quality);
            writer.Write(this.wasQualified);
            writer.Write(this.statBonus.Count);

            foreach (var x in this.statBonus)
            {
                writer.Write(String.Format("{0}:{1}", x.Key, x.Value));
            }

            base.NetSend(item, writer);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            this.quality = (ItemQuality)reader.ReadInt32();
            this.wasQualified = reader.ReadBoolean();
            int statCount = reader.ReadInt32();

            this.statBonus.Clear();

            for (int i = 0; i < statCount; i++)
            {
                string[] keyValuePair = reader.ReadString().Split(':');
                this.statBonus[keyValuePair[0]] = int.Parse(keyValuePair[1]);
            }

            base.NetReceive(item, reader);
        }

        private static int RollNewStatBonus(string stat, int statMin, int statMax)
        {
            int value = rnd.Next(statMin, statMax);

            if (stat.Contains("Damage"))
            {
                value = (int)Math.Ceiling(value / 2f);
            }
            else if (stat.Contains("Crit") || stat.Contains("Defense") || stat.Contains("maxMinions"))
            {
                value = (int)Math.Ceiling(value / 10f);
            }
            else if (stat.Contains("Life") || stat.Contains("Mana"))
            {
                value *= rnd.Next(2, 4);
            }
            else if (stat.Contains("Regen") || stat.Contains("thrownVelocity"))
            {
                value /= (int)Math.Ceiling(value / 5f);
            }

            return value;
        }

        private static bool IsIntegerStat(string stat)
        {
            return stat.Contains("stat") ||
                stat.Contains("Regen") ||
                stat.Contains("maxMinions");
        }
    }
}

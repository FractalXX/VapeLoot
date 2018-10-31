using System;
using Terraria;
using Terraria.ModLoader;

namespace VapeLoot
{
    class VapePlayer : ModPlayer
    {
        public override void OnEnterWorld(Player player)
        {
            if(player.inventory.Length > 0)
            {
                foreach(Item item in player.inventory)
                {
                    try
                    {
                        item.GetGlobalItem<VapeGlobalItem>().tooltipVisible = true;
                    }
                    catch(Exception) { }
                }
            }
        }
    }
}

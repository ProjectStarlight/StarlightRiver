﻿using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Items.Armor;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Starwood
{
    //makes star items start starwood empowerment, starting it just doesn't do anything is the player does not have the set equiped
    internal class ManastarPickup : GlobalItem
    {
        public override bool OnPickup(Item item, Player player)
        {
            if (new int[] { ItemID.Star, ItemID.SoulCake, ItemID.SugarPlum }.Contains(item.type))
            {
                StarlightPlayer mp = player.GetModPlayer<StarlightPlayer>();
                mp.StartStarwoodEmpowerment();
            }
            return base.OnPickup(item, player);
        }
    }

    //makes npcs drop stars if the bool the armor-set sets is true
    internal class ManastarDrops : GlobalNPC
    {
        public bool DropStar = false;
        public override bool InstancePerEntity => true;
        public override void NPCLoot(NPC npc)
        {
            if (DropStar && Main.rand.Next(2) == 0)
            {
                Item.NewItem(npc.Center, ItemID.Star);
            }
        }
    }
}

//modplayer to handle empowerment

namespace StarlightRiver.Core
{
    public partial class StarlightPlayer : ModPlayer
    {
        public bool empowered = false;
        public int empowermentTimer = 0;
        public void StartStarwoodEmpowerment()
        {

        }
    }
}
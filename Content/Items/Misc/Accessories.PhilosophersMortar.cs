using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.WorldGeneration;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Content.Items.Misc
{
    public class PhilosophersMortar : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public PhilosophersMortar() : base("Philosopher's Mortar", "Life hearts decrease duration of Potion Sickness by 10 seconds, but heal half as much") { }

        public override void SafeSetDefaults()
        {
            Item.value = 1;
            Item.rare = ItemRarityID.LightRed;
        }

        public override void SafeUpdateEquip(Player Player)
        {
            Player.GetModPlayer<PhilMortarPlayer>().equipped = true;
        }
    }

    public class PhilMortarPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects()
        {
            equipped = false;
        }
    }

    public class PhilMortarGItem : GlobalItem
    {
        public override bool OnPickup(Item item, Player player)
        {
            if (!player.GetModPlayer<PhilMortarPlayer>().equipped)
                return base.OnPickup(item, player);

            if (item.type == ItemID.Heart || item.type == ItemID.CandyApple || item.type == ItemID.CandyCane)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, item.Center);
                player.HealEffect(10);
                player.statLife = (int)MathHelper.Min(player.statLife + 10, player.statLifeMax2);

                int buffIndex = player.FindBuffIndex(BuffID.PotionSickness);
                if (buffIndex != -1)
                {
                    player.buffTime[buffIndex] = (int)MathHelper.Max(0, player.buffTime[buffIndex] - 600);
                }
                item.active = false;
                return false;
            }

            return base.OnPickup(item, player);
        }
    }
}
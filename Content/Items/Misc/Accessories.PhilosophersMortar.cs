using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.Items.Misc
{
    public class PhilosophersMortar :  CursedAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public PhilosophersMortar() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "PhilosophersMortar").Value) { }

        public override void Load()
        {
            StarlightItem.OnPickupEvent += OnPickup;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Philosopher's Mortar");
            Tooltip.SetDefault("Life hearts decrease duration of Potion Sickness by 10 seconds, but heal half as much");
        }

        public override void SafeSetDefaults()
        {
            Item.value = 1;
            Item.rare = ItemRarityID.LightRed;
        }

        private bool OnPickup(Item item, Player player)
        {
            if (!Equipped(player))
                return true;

            if (item.type == ItemID.Heart || item.type == ItemID.CandyApple || item.type == ItemID.CandyCane)
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, item.Center);
                player.HealEffect(10);
                player.statLife = (int)MathHelper.Min(player.statLife + 10, player.statLifeMax2);

                int buffIndex = player.FindBuffIndex(BuffID.PotionSickness);

                if (buffIndex != -1)
                    player.buffTime[buffIndex] = (int)MathHelper.Max(0, player.buffTime[buffIndex] - 600);

                item.active = false;
                return false;
            }

            return true;
        }
    }
}
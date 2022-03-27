using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class Jetwelder : ModItem
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
            Tooltip.SetDefault("I shall update this later");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.knockBack = 3f;
            item.mana = 10;
            item.width = 32;
            item.height = 32;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item44;

            item.noMelee = true;
            item.summon = true;
            item.shoot = ModContent.ProjectileType<JetwelderCrawler>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            switch (Main.rand.Next(2))
            {
                case 0:
                    type = ModContent.ProjectileType<JetwelderCrawler>();
                    break;
                case 1:
                    type = ModContent.ProjectileType<JetwelderGatler>();
                    break;
            }
            return true;
        }
    }
}
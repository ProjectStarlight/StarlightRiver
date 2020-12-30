using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
    class PressureBow : ModItem
    {
        public override string Texture => AssetDirectory.CaveTempleItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Daedalus 'Slight Atmospheric Pressure Disturbance' Bow");
            Tooltip.SetDefault("Shoots AN arrow from the sky\nHopefully less overpowered than it's older brother");
        }

        public override void SetDefaults()
        {
            item.magic = true;
            item.width = 32;
            item.height = 32;
            item.damage = 10;
            item.crit = 15;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useTime = 30;
            item.useAnimation = 30;
            item.noMelee = true;
            item.knockBack = 2;
            item.rare = ItemRarityID.Green;
            item.useAmmo = AmmoID.Arrow;
            item.shoot = ProjectileID.PurificationPowder;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            float rot = Main.rand.NextFloat(-0.2f, 0.2f);
            Vector2 pos = Main.MouseWorld + Vector2.UnitY.RotatedBy(rot) * -900;
            Vector2 vel = Vector2.UnitY.RotatedBy(rot) * 5;

            Projectile.NewProjectile(pos, vel, type, damage, knockBack);

            return false;
        }
    }
}
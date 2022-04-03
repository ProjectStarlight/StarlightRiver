using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
            Item.magic = true;
            Item.width = 32;
            Item.height = 32;
            Item.damage = 10;
            Item.crit = 15;
            Item.useStyle = ItemUseStyleID.HoldingOut;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.rare = ItemRarityID.Green;
            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.PurificationPowder;
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            float rot = Main.rand.NextFloat(-0.2f, 0.2f);
            Vector2 pos = Main.MouseWorld + Vector2.UnitY.RotatedBy(rot) * -900;
            Vector2 vel = Vector2.UnitY.RotatedBy(rot) * 5;

            Projectile.NewProjectile(pos, vel, type, damage, knockBack);

            return false;
        }
    }
}
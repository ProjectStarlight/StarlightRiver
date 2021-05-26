using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
    public class RefractiveBlade : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Refractive Blade");
            Tooltip.SetDefault("Swing in any direction \nHold down to launch a laser");
        }

        public override void SetDefaults()
        {
            item.channel = true;
            item.damage = 26;
            item.width = 60;
            item.height = 60;
            item.useTime = 60;
            item.useAnimation = 60;
            item.useStyle = ItemUseStyleID.HoldingOut;
            Item.staff[item.type] = true;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 7;
            item.useTurn = false;
            item.value = Terraria.Item.sellPrice(0, 2, 20, 0);
            item.rare = 4;
            item.autoReuse = true;
            item.shoot = ModContent.ProjectileType<RefractiveBladeProj>();
            item.shootSpeed = 6f;
            item.noUseGraphic = true;
        }
    }
    public class RefractiveBladeProj : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + "RefractiveBlade";
        public sealed override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.melee = true;
            projectile.width = projectile.height = 80;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.alpha = 255;
        }
    }
}
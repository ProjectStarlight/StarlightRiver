using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;

using System;

namespace StarlightRiver.Content.Items.Misc
{
    internal class Sandscript : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sand Scripts");
            Tooltip.SetDefault("Manifests a blade of sand");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 43;
            item.useTime = 43;
            item.shootSpeed = 1f;
            item.knockBack = 7f;
            item.damage = 12;
            item.shoot = ProjectileType<SandSlash>();
            item.rare = ItemRarityID.Blue;
            item.noMelee = true;
            item.magic = true;
            item.mana = 10;

            item.UseSound = SoundID.Item45;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int i = Projectile.NewProjectile(player.Center, Vector2.Normalize(Main.MouseWorld - player.Center) * 25, type, damage, knockBack, player.whoAmI);
            Main.projectile[i].rotation = (Main.MouseWorld - player.Center).ToRotation();
            return false;
        }
    }

    internal class SandSlash : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.aiStyle = -1;
            projectile.timeLeft = 45;
            projectile.width = 64;
            projectile.height = 64;
            projectile.tileCollide = false;
            projectile.extraUpdates = 2;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            projectile.ai[0]++;

            if (projectile.ai[0] == 30) projectile.knockBack *= 0;

            Vector2 relativeRot = new Vector2
            {
                X = (float)Math.Cos(projectile.ai[0] / 60 * 6.28f) * 3f,
                Y = (float)Math.Sin(projectile.ai[0] / 60 * 6.28f) * 10f
            };
            projectile.velocity = relativeRot.RotatedBy(projectile.rotation - 1.57f);

            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Stamina>(), projectile.velocity * Main.rand.NextFloat(0.2f, 1.1f), 0, default, 1f);
            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Sand>(), projectile.velocity * Main.rand.NextFloat(0.8f, 1.2f), 140, default, 0.7f);

            Lighting.AddLight(projectile.Center, new Vector3(0.3f, 0.2f, 0));
        }
    }
}
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricYoyo : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Yoyo");
            Tooltip.SetDefault("Splits");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 26;
            item.value = Terraria.Item.sellPrice(0, 10, 0, 0);
            item.rare = ItemRarityID.Green;
            item.crit += 4;
            item.damage = 18;
            item.knockBack = 4f;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useTime = 25;
            item.useAnimation = 25;
            item.melee = true;
            item.channel = true;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.shootSpeed = 12f;
            item.shoot = ModContent.ProjectileType<VitricYoyoProj>();
            item.UseSound = SoundID.Item1;
        }
    }
     public class VitricYoyoProj : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + "VitricYoyoProj";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Yoyo");
            Main.projFrames[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.Valor);
            projectile.damage = 104;
            projectile.width = projectile.height = 18;
            aiType = ProjectileID.Valor;
        }
        //projectile.ai[1] = shatter timer
        int shattertimer = 0;
        public override void AI()
        {
            if (shattertimer > 0)
            {
                shattertimer--;
                projectile.frame = 1;
                projectile.friendly = false;
            }
            else
            {
                projectile.friendly = true;
                projectile.frame = 0;
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (shattertimer == 0)
            {
                shattertimer = 60;
                for (int i = 0; i < 3; i++)
                {
                    Projectile.NewProjectile(projectile.position, Vector2.Zero, ModContent.ProjectileType<VitricYoyoShard>(), projectile.damage, projectile.knockBack, projectile.owner, projectile.whoAmI, i * 120);
                }
                for (int j = 0; j < 20; j++)
                {
                    Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Content.Dusts.GlassGravity>());
                }
            }
        }
    }
    public class VitricYoyoShard : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + "VitricYoyoShard";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Yoyo");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.hostile = false;
            projectile.friendly = true;
            projectile.damage = 13;
            projectile.width = projectile.height = 11;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
            projectile.timeLeft = 60;

        }
        public override void AI()
        {
            Projectile parent = Main.projectile[(int)projectile.ai[0]];
            if (!parent.active)
            {
                projectile.active = false;
            }
            double dist = (900 - ((projectile.timeLeft - 30) * (projectile.timeLeft - 30))) / 11;
            double deg = (float)(dist * 3) + (float)(projectile.ai[1]);
            double rad = deg * (Math.PI / 180); //Convert degrees to radians

            projectile.position.Y = parent.Center.Y - (int)(Math.Sin(rad) * dist) - projectile.height / 2;
            projectile.position.X = parent.Center.X - (int)(Math.Cos(rad) * dist) - projectile.width / 2;

            projectile.rotation = (float)((dist * 9) + (projectile.ai[1])) * (float)(Math.PI / 180);
        }
    }
}

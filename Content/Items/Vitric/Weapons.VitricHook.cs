using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
    class VitricHook : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ripshatter");
            Tooltip.SetDefault("Fires a hook, allowing you to grapple to an enemy and bounce off of them\nCan only be used while airborn");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.melee = true;
            item.noMelee = true;
            item.useTime = 20;
            item.useAnimation = 20;
            item.shoot = ProjectileType<VitricHookProjectile>();
            item.shootSpeed = 20;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.noUseGraphic = true;
            item.knockBack = 3;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.velocity.Y == 0 || Main.projectile.Any(n => n.active && n.type == ProjectileType<VitricHookProjectile>() && n.owner == player.whoAmI))
                return false;

            return base.CanUseItem(player);
        }

        //public override void HoldItem(Player player)
        //{
        //    if (Main.projectile.Any(n => n.type == ProjectileType<VitricHookProjectile>() && n.owner == player.whoAmI))
        //        player.itemAnimation = 5;
        //}
    }

    class VitricHookProjectile : ModProjectile 
    {
        NPC hooked;
        Vector2 startPos;
        bool struck;

        ref float Progress => ref projectile.ai[0];
        ref float Distance => ref projectile.ai[1];
        bool Retracting => projectile.timeLeft < 30;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.timeLeft = 60;
            projectile.aiStyle = -1;
            projectile.penetrate = 2;
        }


        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            projectile.rotation = projectile.velocity.ToRotation();
            if(projectile.timeLeft < 40)//slows down the projectile by 8%, for about 10 ticks before it retracts
            {
                projectile.velocity *= 0.92f;
            }

            if(projectile.timeLeft == 30)
            {
                startPos = projectile.Center;
                projectile.velocity *= 0;
            }

            if(Retracting)
            {
                projectile.Center = Vector2.Lerp(player.Center, startPos, projectile.timeLeft / 30f);
            }

            if(hooked != null && !struck)
            {
                projectile.timeLeft = 32;
                projectile.Center = hooked.Center;
                player.velocity = Vector2.Zero;//resets wings / double jumps

                Progress += 20f / Distance;
                player.Center = Vector2.Lerp(startPos, hooked.Center, Progress);

                if(player.Hitbox.Intersects(hooked.Hitbox))
                {
                    struck = true;
                    projectile.timeLeft = 20;

                    player.immune = true;
                    player.immuneTime = 20;
                    player.velocity = Vector2.Normalize(startPos - hooked.Center) * 15;
                    player.GetModPlayer<StarlightPlayer>().Shake += 15;

                    hooked.StrikeNPC(projectile.damage, projectile.knockBack, player.Center.X < hooked.Center.X ? -1 : 1);

                    for(int k = 0; k < 30; k++)
                        Dust.NewDustPerfect(player.Center, DustType<Dusts.GlassNoGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4f), 0, default, Main.rand.NextFloat(1f, 2f));
                    Main.PlaySound(SoundID.Shatter);
                }
            }

            if(struck)
            {
                player.fullRotation = (projectile.timeLeft / 20f) * 3.14f * player.direction;
                player.fullRotationOrigin = player.Size / 2;
                player.velocity *= 0.95f;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];

            if (target.life <= 0) return;

            if (!Retracting && hooked is null)
            {
                hooked = target;
                projectile.velocity *= 0;
                startPos = player.Center;
                Distance = Vector2.Distance(startPos, target.Center);
                projectile.friendly = false;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage /= 4;
            knockback /= 4f;
            crit = false;
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if(struck) 
                return false;

            Texture2D chainTex1 = GetTexture(AssetDirectory.VitricItem + "VitricHookChain1");
            Texture2D chainTex2 = GetTexture(AssetDirectory.VitricItem + "VitricHookChain2");
            Player player = Main.player[projectile.owner];

            float dist = Vector2.Distance(player.Center, projectile.Center);
            float rot = (player.Center - projectile.Center).ToRotation() + (float)Math.PI / 2f;


            float length = 1f / dist * chainTex1.Height;
            for (int k = 0; k * length < 1; k++)
            {
                var pos = Vector2.Lerp(projectile.Center, player.Center, k * length);
                if(k % 2 == 0)
                    spriteBatch.Draw(chainTex1, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
                else
                    spriteBatch.Draw(chainTex2, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
            }

            Texture2D hook = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(hook, projectile.Center - Main.screenPosition, null, lightColor, rot + ((float)Math.PI * 0.75f), hook.Size() / 2, 1, 0, 0);

            return false;
        }
    }
}

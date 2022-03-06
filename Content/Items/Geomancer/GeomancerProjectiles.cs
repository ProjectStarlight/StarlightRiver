using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Dyes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Graphics;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Geomancer
{
    public class EmeraldHeart : ModItem
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Emerald Heart");
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 24;
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statLifeMax2 - player.statLife, 5);
            player.HealEffect(5);
            player.statLife += healAmount;

            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            return false;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 100);
    }

    public class TopazShieldFade : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.GeomancerItem + "TopazShield";

        private float progress => (30 - projectile.timeLeft) / 30f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Topaz Shield");
        }

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.width = 32;
            projectile.height = 32;
            projectile.penetrate = -1;
            projectile.hide = true;
            projectile.timeLeft = 30;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            Vector2 direction = Main.MouseWorld - player.Center;
            direction.Normalize();

            projectile.rotation = direction.ToRotation();

            projectile.Center = player.Center + (direction * 35);
        }
        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            float transparency = (float)Math.Pow(1 - progress, 2);
            float scale = 1f + (progress * 2);

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * transparency, projectile.rotation, tex.Size() / 2, projectile.scale * scale, SpriteEffects.None, 0f);
        }
    }

    public class TopazShield : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.GeomancerItem + "TopazShield";

        private const int EXPLOSIONTIME = 2;

        public int shieldLife = 100;

        public float shieldSpring = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Topaz Shield");
        }

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.width = 32;
            projectile.height = 32;
            projectile.penetrate = -1;
            projectile.hide = true;
            projectile.timeLeft = 60;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            Vector2 direction = Main.MouseWorld - player.Center;
            direction.Normalize();

            projectile.rotation = direction.ToRotation();

            shieldSpring *= 0.9f;

            projectile.Center = player.Center + (direction * MathHelper.Lerp(35, 26, shieldSpring));

            if (player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.Topaz || player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All)
            {
                if (projectile.timeLeft > EXPLOSIONTIME)
                    projectile.timeLeft = EXPLOSIONTIME + 2;
            }
            else
                projectile.active = false;

            if (projectile.timeLeft > EXPLOSIONTIME)
            {
                for (int k = 0; k < Main.maxProjectiles; k++)
                {
                    var proj = Main.projectile[k];

                    if (proj.active && proj.hostile && proj.damage > 1 && proj.Hitbox.Intersects(projectile.Hitbox))
                    {
                        var diff = proj.damage - shieldLife;

                        if (diff <= 0)
                        {
                            shieldSpring = 1;
                            proj.penetrate -= 1;
                            proj.friendly = true;
                            shieldLife -= proj.damage;
                            CombatText.NewText(projectile.Hitbox, Color.Yellow, proj.damage);
                        }
                        else
                        {
                            CombatText.NewText(projectile.Hitbox, Color.Yellow, shieldLife);
                            proj.damage -= (int)shieldLife;
                            projectile.timeLeft = EXPLOSIONTIME;
                            return;
                        }
                    }
                }
            }
            else
            {
                /*float progress = EXPLOSIONTIME - projectile.timeLeft;

                float deviation = (float)Math.Sqrt(progress) * 0.08f;
                projectile.rotation += Main.rand.NextFloat(-deviation,deviation);

                Vector2 dustDir = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Dust.NewDustPerfect(projectile.Center - (dustDir * 50), DustID.TopazBolt, dustDir * 10, 0, default, (float)Math.Sqrt(progress) * 0.3f).noGravity = true;*/
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, tex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (projectile.timeLeft <= EXPLOSIONTIME)
                return false;
            return base.CanHitNPC(target);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            hitDirection = Math.Sign(projectile.Center.X - Main.player[projectile.owner].Center.X);

            shieldLife -= target.damage;
            shieldSpring = 1;
            CombatText.NewText(projectile.Hitbox, Color.Yellow, target.damage);

            if (shieldLife <= 0)
                projectile.timeLeft = EXPLOSIONTIME;
        }

        public override void Kill(int timeLeft)
        {
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<TopazShieldFade>(), 0, 0, projectile.owner);
            Main.player[projectile.owner].GetModPlayer<StarlightPlayer>().Shake += 4;
            Vector2 direction = Vector2.Normalize(Main.MouseWorld - Main.player[projectile.owner].Center);
            for (int i = 0; i < 4; i++)
                Projectile.NewProjectile(projectile.Center, direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(0.6f, 1f) * 15, ModContent.ProjectileType<TopazShard>(), projectile.damage * 2, projectile.knockBack, projectile.owner);
        }
    }

    public class TopazShard : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.GeomancerItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Topaz Shard");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.Size = new Vector2(8, 8);
            projectile.penetrate = 1;
            projectile.hide = true;
            projectile.timeLeft = 60;
        }

        public override void AI()
        {
            var target = Main.npc.Where(x => x.active && !x.friendly && !x.townNPC && projectile.Distance(x.Center) < 150).OrderBy(x => projectile.Distance(x.Center)).FirstOrDefault();
            if (target != default)
                projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.DirectionTo(target.Center) * 30, 0.05f);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                float rand = Main.rand.NextFloat(0.3f);
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.TopazBolt, projectile.velocity.X * rand, projectile.velocity.Y * rand);
            }
        }
        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, tex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);
        }

    }

    public class AmethystShard : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoAmethyst";

        private bool initialized = false;
        private Vector2 offset = Vector2.Zero;

        private int fadeIn;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Amethyst Shard");
        }

        public override void SetDefaults()
        {
            projectile.friendly = false;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.Size = new Vector2(16, 16);
            projectile.penetrate = 1;
            projectile.hide = true;
            projectile.timeLeft = 3;
        }

        public override void AI()
        {
            NPC target = Main.npc[(int)projectile.ai[1]];
            if (!initialized)
            {
                initialized = true;
                offset = projectile.Center - target.Center;
            }
            if (fadeIn < 15)
                fadeIn++;

            projectile.scale = (fadeIn / 15f);

            if (!target.active || target.life <= 0 || projectile.ai[0] >= target.GetGlobalNPC<GeoNPC>().amethystDebuff)
                return;
            projectile.timeLeft = 2;

            projectile.Center = target.Center + offset;

            Vector2 direction = target.Center - projectile.Center;
            projectile.rotation = direction.ToRotation();
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            for (int k = projectile.oldPos.Length - 1; k > 0; k--) //TODO: Clean this shit up
            {
                Vector2 drawPos = projectile.oldPos[k] + (new Vector2(projectile.width, projectile.height) / 2);
                Color color = Color.White * (float)(((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length));
                    spriteBatch.Draw(tex, drawPos - Main.screenPosition, null, color, projectile.rotation, tex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White * (fadeIn / 15f), projectile.rotation, tex.Size() / 2, projectile.scale, SpriteEffects.None, 0f);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
                Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.AmethystBolt).velocity *= 1.4f;
        }
    }

    public class RubyDagger : ModProjectile,IDrawAdditive
    {
        public override string Texture => AssetDirectory.GeomancerItem + "GeoRuby";

        Vector2 direction = Vector2.Zero;

        private List<float> oldRotation = new List<float>();

        private bool launched = false;
        private bool lockedOn = false;
        private int launchCounter = 15;
        private float rotation = 0f;

        private float radiansToSpin = 0f;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ruby Dagger");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.magic = true;
            projectile.tileCollide = false;
            projectile.Size = new Vector2(16, 16);
            projectile.penetrate = 1;
            radiansToSpin = 6.28f * Main.rand.Next(2, 5) * Math.Sign(Main.rand.Next(-100,100));
            projectile.hide = true;

        }

        public override void AI()
        {

            oldRotation.Add(projectile.rotation);
            while (oldRotation.Count > projectile.oldPos.Length)
            {
                oldRotation.RemoveAt(0);
            }


            NPC target = Main.npc[(int)projectile.ai[0]];

            if (target.active && target.life > 0 && !lockedOn)
                direction = Vector2.Normalize(target.Center - projectile.Center);

            if (!launched)
            {
                projectile.timeLeft = 50;
                projectile.velocity *= 0.96f;
                rotation = MathHelper.Lerp(rotation, direction.ToRotation() + radiansToSpin, 0.02f);
                projectile.rotation = rotation + 1.57f; //using a variable here cause I think projectile.rotation automatically caps at 2 PI?

                float difference = Math.Abs(rotation - (direction.ToRotation() + radiansToSpin));
                if (difference < 0.7f || lockedOn)
                {
                    projectile.velocity *= 0.8f;
                    if (difference > 0.2f)
                    {
                        rotation += 0.1f * Math.Sign((direction.ToRotation() + radiansToSpin) - rotation);
                    }
                    else
                        rotation = direction.ToRotation() + radiansToSpin;
                    lockedOn = true;
                    launchCounter--;
                    projectile.Center -= direction * 3;
                    if (launchCounter <= 0)
                        launched = true;
                }
                else
                {
                    rotation += 0.15f * Math.Sign((direction.ToRotation() + radiansToSpin) - rotation);
                }
            }
            else
            {
                if (target.active && target.life > 0)
                    direction = Vector2.Normalize(target.Center - projectile.Center);
                projectile.velocity = direction * 30;
                projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];

            Vector2 origin = new Vector2(tex.Width / 2, tex.Height);

            SpriteEffects effects = Math.Sign(radiansToSpin) == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            for (int k = projectile.oldPos.Length - 1; k > 0; k--) //TODO: Clean this shit up
            {
                Vector2 drawPos = projectile.oldPos[k] + (new Vector2(projectile.width, projectile.height) / 2);
                Color color = Color.White * (float)(((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length));
                if (k > 0 && k < oldRotation.Count)
                    spriteBatch.Draw(tex, drawPos - Main.screenPosition, null, color, oldRotation[k], tex.Size() / 2, projectile.scale, effects, 0f);
            }

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, tex.Size() / 2, projectile.scale, effects, 0f);
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
                Dust.NewDustPerfect(projectile.Center, DustID.RubyBolt, (projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(0.4f)), 0, default, 1.25f).noGravity = true; ;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;
    }
}
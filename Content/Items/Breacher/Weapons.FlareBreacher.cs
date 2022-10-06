using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Breacher
{
    public class FlareBreacher : ModItem
    {
        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flare Breacher");
            Tooltip.SetDefault("Fires explosive flares that embed in enemies, blasting shrapnel through and behind them");
        }

        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.knockBack = 2f;
            Item.UseSound = SoundID.Item11;
            Item.width = 24;
            Item.height = 28;
            Item.damage = 23;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.useAmmo = AmmoID.Flare;
            Item.DamageType = DamageClass.Ranged;
            Item.shoot = ModContent.ProjectileType<ExplosiveFlare>();
            Item.shootSpeed = 17;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(2, 0);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<ExplosiveFlare>();
            position.Y -= 4;
        }

        public override bool Shoot(Player Player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 direction = velocity;

            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(position + (direction * 1.35f), 6, (direction.RotatedBy(Main.rand.NextFloat(-1, 1)) / 5f) * Main.rand.NextFloat());
                dust.noGravity = true;
            }

            Helper.PlayPitched("Guns/FlareFire", 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), position);
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Content.Items.SpaceEvent.Astroscrap>(), 12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    internal class ExplosiveFlare : ModProjectile
    {
        bool red;
        bool stuck;

        int explosionTimer = 100;
        int enemyID;
        int blinkCounter;

        Vector2 offset = Vector2.Zero;

        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.aiStyle = 1;
            AIType = 163;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Flare");
            Main.projFrames[Projectile.type] = 2;
        }

        public override bool PreAI()
        {
            Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3());
            Vector2 direction = (Projectile.rotation + 1.57f + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2();

            if (stuck)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<BreacherDust>(), direction * Main.rand.NextFloat(3, 4));
                dust.scale = 1.15f;
                dust.noGravity = true;
                NPC target = Main.npc[enemyID];
                Projectile.position = target.position + offset;
                explosionTimer--;

                blinkCounter++;
                int timerVal = 3 + (int)Math.Sqrt(explosionTimer);

                if (blinkCounter > timerVal)
                {
                    if (!red)
                    {
                        red = true;
                        Helper.PlayPitched("Effects/Bleep", 1, 1 - (explosionTimer / 100f), Projectile.Center);
                        blinkCounter = 0;
                    }
                    else
                    {
                        red = false;
                        blinkCounter = 0;
                    }
                }

                if (explosionTimer <= 0 || !target.active)
                    Explode(target);

                return false;
            }
            else
            {
                for (float i = -1; i < 0; i += 0.25f)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center - (Projectile.velocity * i), ModContent.DustType<BreacherDustFour>(), direction * Main.rand.NextFloat(3, 4));
                    dust.scale = 0.85f;
                    dust.noGravity = true;
                }

                Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
            }

            return base.PreAI();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Dust.NewDustPerfect(Projectile.Center + oldVelocity, ModContent.DustType<FlareBreacherDust>(), Vector2.Zero, 60, default, 0.7f).rotation = Main.rand.NextFloat(6.28f);
            return base.OnTileCollide(oldVelocity);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!stuck && target.life > 0)
            {
                stuck = true;
                Projectile.friendly = false;
                Projectile.tileCollide = false;
                enemyID = target.whoAmI;
                offset = Projectile.position - target.position;
                offset -= Projectile.velocity;
                Projectile.netUpdate = true;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(stuck);
            writer.WritePackedVector2(offset);
            writer.Write(enemyID);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            stuck = reader.ReadBoolean();
            offset = reader.ReadPackedVector2();
            enemyID = reader.ReadInt32();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Projectile.type].Value;
            var source = new Rectangle(0, 0, Projectile.width, 16);

            if (stuck)
                source.Y += 16 * (red ? 1 : 0);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, lightColor, Projectile.rotation, (tex.Size() / 2) * new Vector2(1, 0.5f), Projectile.scale, 0, 0);

            return false;
        }

        private void Explode(NPC target)
        {
            Helper.PlayPitched("Guns/FlareBoom", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);

            if (!target.immortal && !target.dontTakeDamage)
                target.StrikeNPC(Projectile.damage, 0f, 0);

            Core.Systems.CameraSystem.Shake = 10;
            int numberOfProjectiles = Main.rand.Next(4, 6);

            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < numberOfProjectiles; i++)
                {
                    float offsetRad = MathHelper.Lerp(0, 0.5f, i / (float)numberOfProjectiles);
                    var pos = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f) * target.width;
                    var velocity = Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(0 - offsetRad, offsetRad) - 1.57f) * Main.rand.NextFloat(9, 11);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, velocity, ModContent.ProjectileType<FlareShrapnel>(), Projectile.damage / 4, Projectile.knockBack, Projectile.owner, target.whoAmI);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(5, 10);
                dust.scale = Main.rand.NextFloat(0.4f, 0.7f);
                dust.alpha = 40 + Main.rand.Next(40);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f), 0, 0, ModContent.DustType<FlareBreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(10, 20);
                dust.scale = Main.rand.NextFloat(0.75f, 1f);
                dust.alpha = 40 + Main.rand.Next(40);
                dust.rotation = Main.rand.NextFloat(6.28f);
            }

            for (int i = 0; i < 24; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 1.57f), 0, 0, ModContent.DustType<BreacherDust>());
                dust.velocity = Vector2.UnitX.RotatedBy(Projectile.rotation + Main.rand.NextFloat(-0.3f, 0.3f) - 1.57f) * Main.rand.NextFloat(1, 5);
                dust.scale = Main.rand.NextFloat(0.75f, 1.1f);
            }

            Projectile.active = false;
        }
    }

    internal class FlareShrapnel : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        private NPC source => Main.npc[(int)Projectile.ai[0]];

        public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = Main.rand.Next(50, 70);
            Projectile.extraUpdates = 4;
            Projectile.alpha = 255;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Shrapnel");
            Main.projFrames[Projectile.type] = 2;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.96f;
            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target == source)
                return false;

            return base.CanHitNPC(target);
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 20; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 20)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(40 * 4), factor => factor * 6, factor =>
            {
                return new Color(255, 50, 180);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["ShrapnelTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            effect.Parameters["progress"].SetValue(MathHelper.Lerp(Projectile.timeLeft / 60f, 0, 0.3f));

            trail?.Render(effect);
        }
    }
}
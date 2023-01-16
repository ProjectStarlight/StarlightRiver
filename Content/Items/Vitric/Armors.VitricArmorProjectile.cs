using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
    internal class VitricArmorProjectileIdle : ModProjectile
    {
        public Vector2 offset = Vector2.Zero;
        public float rotOffset = 0;
        public VitricHead parent;
        
        public float maxSize;

        Vector2 posTarget;
        float rotTarget;
        float prevRotTarget;

        public float timer;
        public ref float State => ref Projectile.ai[0];

        float prevState;
        public ref float Index => ref Projectile.ai[1];
        public Player Owner => Main.player[Projectile.owner];

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void AI()
        {
            if (Owner.dead)
            {
                if (Main.myPlayer == Owner.whoAmI)
                {
                    parent.shardCount = 0;
                    parent.shardTimer = 0;
                    parent.loaded = false;
                }
                    
                Projectile.Kill();
            }

            offset = new Vector2(0, -50).RotatedBy(0.2f + (Index - 1) / -2f * 1.5f);
            rotOffset = 0.2f + (Index - 1) / -2f * 1.6f;
            maxSize = Index == 1 ? 0.9f : 0.8f;

            timer++;

            if (State != 2) //persist
                Projectile.timeLeft = 15;

            var flippedOffset = offset;
            flippedOffset.X = offset.X * -Owner.direction;

            posTarget = Owner.Center + flippedOffset + Vector2.UnitY * Owner.gfxOffY;
            Projectile.Center += (posTarget - Projectile.Center) * 0.18f;

            if (Helper.CompareAngle(Projectile.rotation, rotTarget) > 0.1f)
                Projectile.rotation += Helper.CompareAngle(Projectile.rotation, rotTarget) * 0.1f;
            else
                Projectile.rotation = rotTarget;

            if (Owner.armor[0].type != ModContent.ItemType<VitricHead>())
            {
                State = 2;
                timer = 0;
            }

            if(State == 0)
			{
                rotTarget = (float)Math.Sin(timer * 0.02f) * 0.2f + rotOffset * -Owner.direction;

                if(timer <= 30)
                    Projectile.scale = timer / 30f * maxSize;

                if (Main.myPlayer == Owner.whoAmI && parent != null && parent.loaded)
				{
                    if (timer < 30)
                        Projectile.active = false;

                    State = 1;
                    timer = 0;
				}
            }

            if(State == 1 && Main.myPlayer == Owner.whoAmI) //loaded
			{

                rotTarget = Helpers.Helper.LerpFloat(Projectile.rotation, (Owner.Center - Main.MouseWorld).ToRotation() + 1.57f, Math.Min(1, timer / 30f));
                if (Math.Abs(rotTarget - prevRotTarget) > 0.1f)
                {
                    prevRotTarget = rotTarget;
                    Projectile.netUpdate = true;
                }

                if (parent != null && !parent.loaded)
                {
                    State = 0;
                    timer = 30;
                }

                if (parent != null && parent.shardCount <= Index)
				{
                    State = 2;
                    timer = 0;
				}
			}

            if(State == 2)
			{
                Projectile.scale = Projectile.timeLeft / 15f * maxSize;
			}

            if (Main.myPlayer == Owner.whoAmI && prevState != State)
            {
                prevState = State;
                Projectile.netUpdate = true;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(rotTarget);
            writer.Write(timer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            rotTarget = reader.ReadSingle();
            timer = reader.ReadInt32();
        }

        public override bool PreDraw(ref Color lightColor)
		{
            var spriteBatch = Main.spriteBatch;

            var tex = ModContent.Request<Texture2D>(Texture).Value;
            var texGlow = ModContent.Request<Texture2D>(Texture + "Glow").Value;
            var texHot = ModContent.Request<Texture2D>(Texture + "Hot").Value;

            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, 0, 0);

            if(State == 0)
                spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, Helper.MoltenVitricGlow(timer / 30f * 110), Projectile.rotation, texGlow.Size() / 2, Projectile.scale, 0, 0);

            if (State == 1)
                spriteBatch.Draw(texHot, Projectile.Center - Main.screenPosition, null, Color.White * Math.Min(1, timer / 30f), Projectile.rotation, texHot.Size() / 2, Projectile.scale, 0, 0);

            return false;
		}
	}

    internal class VitricArmorProjectile : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public ref float state => ref Projectile.ai[0];

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 5;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Glass");
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

            if (Main.rand.Next(5) == 0)
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * Main.rand.NextFloat(-2, -1), 0, new Color(255, 150, 50), 0.6f);

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			for(int k = 0; k < 20; k++)
			{
                float rot = Projectile.rotation - 1.57f;
                Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Glow>(), Vector2.UnitX.RotatedBy(rot + Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(5), 0, new Color(255, 150, 50), 0.4f);
			}

            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
            Core.Systems.CameraSystem.Shake += 5;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
            Projectile.velocity *= 0;
            Projectile.friendly = false;
            return false;
		}

		private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 90; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 90)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 90, new TriangularTip(20 * 4), factor => factor * 20, factor =>
            {
                if (factor.X > 0.95f)
                    return Color.White * 0;

                float alpha = 1;

                if (Projectile.timeLeft < 20)
                    alpha = Projectile.timeLeft / 20f;

                return new Color(255, 175 + (int)((float)Math.Sin(factor.X * 3.14f * 5) * 25), 100) * (float)Math.Sin(factor.X * 3.14f) * alpha;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);
        }
    }
}
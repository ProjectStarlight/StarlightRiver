using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Moonstone
{
	public class DreambeastProj : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.MoonstoneNPC + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dream Shard");
		}

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 45;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.damage = 66;
		}

		public override void AI()
		{
			Projectile.velocity *= 1.06f;

			if (Main.rand.NextBool())
				Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<Dusts.MoonstoneShimmer>());

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.timeLeft < 15)
				Projectile.Opacity -= 0.066f;
		}

		public override bool CanHitPlayer(Player target)
		{
			return Main.LocalPlayer.GetModPlayer<LunacyPlayer>().Insane;
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.GetModPlayer<LunacyPlayer>().ReturnSanity(8);
			modifiers.FinalDamage *= target.GetModPlayer<LunacyPlayer>().GetInsanityDamageMult();

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				SanityHitPacket packet = new SanityHitPacket(target.whoAmI, 8);
				packet.Send();
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int k = 0; k < 5; k++)
			{
				Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<Dusts.Stone>(), Projectile.velocity.X / 4, Projectile.velocity.Y / 4);
				Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), Projectile.velocity.X / 4, Projectile.velocity.Y / 4, 35, new Color(150, 120, 255) * 0.5f, Main.rand.NextFloat(1f, 1.2f));
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			if (Main.LocalPlayer.GetModPlayer<LunacyPlayer>().Insane)
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), Color.White, Projectile.rotation, tex.Size() / 2, 1.2f, 0, 0);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture + "_Bloom").Value;
			var glowColor = new Color(78, 87, 191);

			if (Main.LocalPlayer.GetModPlayer<LunacyPlayer>().Insane)
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), glowColor * 0.8f, Projectile.rotation, tex.Size() / 2, 1.2f, 0, 0);
		}
	}

	public class DreambeastProjHome : ModProjectile, IDrawPrimitive, IDrawAdditive
	{

		private List<Vector2> cache;
		private Trail trail;
		public ref float target => ref Projectile.ai[0];
		public Player Target => Main.player[(int)target];

		public Vector2 PathCenter
		{
			get => new(Projectile.ai[1], Projectile.ai[2]);
			set
			{
				Projectile.ai[1] = value.X;
				Projectile.ai[2] = value.Y;
			}
		}

		public override string Texture => AssetDirectory.SquidBoss + "InkBlob";

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 36;
			Projectile.height = 20;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.damage = 66;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dream Essence");
		}

		public override void OnSpawn(IEntitySource source)
		{
			PathCenter = Projectile.Center;
		}

		public override void AI()
		{
			float homingMult = Math.Min(1 - (Projectile.timeLeft - 120f) / 60f, 1);

			Projectile.velocity = Vector2.Lerp(Projectile.velocity, PathCenter.DirectionTo(Target.position) * 15, 0.03f * homingMult);

			Projectile.rotation += 0.05f;

			PathCenter += Projectile.velocity;

			float wave = (float)Math.Sin(Projectile.timeLeft / 5f) * 20f * homingMult;

			Projectile.Center = PathCenter + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * wave;

			var glowColor = new Color(78, 87, 191);
			Lighting.AddLight(Projectile.Center, glowColor.ToVector3());

			if (Main.rand.NextBool(10))
			{
				Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 35, new Color(150, 120, 255) * 0.5f, Main.rand.NextFloat(0.6f, 0.8f));
				Dust.NewDust(Projectile.Center, 0, 0, ModContent.DustType<Dusts.MoonstoneShimmer>(), 0, 0, 35, new Color(150, 120, 255, 0) * 0.5f, Main.rand.NextFloat(0.4f, 0.5f));
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			if (Projectile.timeLeft < 30)
				Projectile.Opacity -= 0.033f;
		}

		public override bool CanHitPlayer(Player target)
		{
			return Main.LocalPlayer.GetModPlayer<LunacyPlayer>().Insane;
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.GetModPlayer<LunacyPlayer>().ReturnSanity(4);
			modifiers.FinalDamage *= target.GetModPlayer<LunacyPlayer>().GetInsanityDamageMult();

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				SanityHitPacket packet = new SanityHitPacket(target.whoAmI, 4);
				packet.Send();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			var color = new Color(78, 87, 191);

			if (Main.LocalPlayer.GetModPlayer<LunacyPlayer>().Insane)
			{
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), color * Projectile.Opacity, Projectile.rotation, tex.Size() / 2, 0.5f, 0, 0);
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), Color.White * Projectile.Opacity, Projectile.rotation, tex.Size() / 2, 0.4f, 0, 0);
			}

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			var color = new Color(78, 87, 191);

			if (Main.LocalPlayer.GetModPlayer<LunacyPlayer>().Insane)
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), color * 0.8f * Projectile.Opacity, Projectile.rotation, tex.Size() / 2, 1.2f, 0, 0);
		}

		public void DrawPrimitives()
		{
			if (Main.LocalPlayer.GetModPlayer<LunacyPlayer>().Insane)
			{
				Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

				var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
				Matrix view = Main.GameViewMatrix.TransformationMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
				effect.Parameters["repeats"].SetValue(2f);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
				trail?.Render(effect);

				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);
				trail?.Render(effect);
			}
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 30; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 30, new TriangularTip(40 * 4), factor => factor * 32, factor =>
			{
				float alpha = factor.X * Projectile.Opacity;

				if (factor.X == 1)
					alpha = 0;

				var color = new Color(78, 87, 191);

				return color * alpha;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}
	}
}

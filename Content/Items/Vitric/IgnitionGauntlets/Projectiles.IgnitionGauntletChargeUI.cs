using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Vitric
{ 
	public class IgnitionGauntletChargeUI : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.VitricItem + Name;
		private Player owner => Main.player[Projectile.owner];

		public int Radius => 25;

		private List<Vector2> cache;
		private List<Vector2> cache2;

		private Trail trail;
		private Trail trail2;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 20;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void AI()
		{

			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = owner.Center;
			Lighting.AddLight(owner.Center, Color.OrangeRed.ToVector3() * modPlayer.charge / 150f * Main.rand.NextFloat());
			if (owner.HeldItem.type == ModContent.ItemType<IgnitionGauntlets>())
				Projectile.timeLeft = 2;
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Main.spriteBatch.End();
			Effect effect = Filters.Scene["IgnitionFlames"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);


			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Back").Value);
			effect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>(Texture + "_Noise2").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>(Texture + "_Noise").Value);
			effect.Parameters["stretch"].SetValue(0.002f * modPlayer.charge);
			effect.Parameters["stretch2"].SetValue(0.0011f * modPlayer.charge);
			effect.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.035f);
			effect.Parameters["opacity"].SetValue((float)Math.Sqrt(modPlayer.charge / 150f));
			effect.Parameters["pallette"].SetValue(ModContent.Request<Texture2D>(Texture + "_Pallette").Value);


			trail?.Render(effect);

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		private void ManageCaches()
		{
			cache = new List<Vector2>();
			cache2 = new List<Vector2>();
			float radius = Radius;
			for (int i = 0; i < 17; i++) //TODO: Cache offsets, to improve performance
			{
				double rad = (i / 32f) * 6.28f;
				Vector2 offset = new Vector2((float)Math.Cos(rad), 0);
				offset *= radius;
				cache.Add(Projectile.Center + offset);
			}
			for (int i = 17; i > 0; i--) //TODO: Cache offsets, to improve performance
			{
				double rad = (i / 32f) * 6.28f;
				Vector2 offset = new Vector2((float)Math.Cos(rad), 0);
				offset *= radius;
				cache2.Add(Projectile.Center + offset);
			}

			while (cache.Count > 17)
				cache.RemoveAt(0);
			while (cache2.Count > 17)
				cache2.RemoveAt(0);
		}

		private void ManageTrail()
		{

			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 17, new TriangularTip(1), factor => 55, factor =>
			{
				return Color.White;
			});

			float nextplace = 17f / 32f;
			Vector2 offset = new Vector2((float)Math.Cos(nextplace), 0);
			offset *= Radius;

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + offset;

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 17, new TriangularTip(1), factor => 55, factor =>
			{
				return Color.White;
			});
			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + offset;
		}

		public void DrawPrimitives()
		{
			return;
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Effect effect = Filters.Scene["IgnitionFlames"].GetShader().Shader;


			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture).Value);
			effect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>(Texture + "_Noise2").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>(Texture + "_Noise").Value);
			effect.Parameters["stretch"].SetValue(0.002f * modPlayer.charge);
			effect.Parameters["stretch2"].SetValue(0.0011f * modPlayer.charge);
			effect.Parameters["pallette"].SetValue(ModContent.Request<Texture2D>(Texture + "_Pallette").Value);
			effect.Parameters["uTime"].SetValue(-Main.GameUpdateCount * 0.035f);
			effect.Parameters["opacity"].SetValue((float)Math.Sqrt(modPlayer.charge / 150f));

			trail2?.Render(effect);
		}
	}
}
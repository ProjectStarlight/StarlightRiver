using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Infusions;
using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Tiles.Underground;
using StarlightRiver.Content.Tiles.Vitric.Temple;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.Hint
{
	internal class Wellspring : HintAbility
	{
		public override bool OnHint(Vector2 pos, bool defaultHint)
		{
			Tile tile = Framing.GetTileSafely(pos / 16);
			if (defaultHint && !tile.HasTile && Player.GetHandler().Stamina > 2f)
			{
				Player.GetHandler().Stamina -= 1.75f;

				foreach (Projectile proj in Main.ActiveProjectiles)
				{
					if (proj.type == ModContent.ProjectileType<WellspringProjectile>() && proj.owner == Player.whoAmI && proj.timeLeft > 20)
						proj.timeLeft = 20;
				}

				Projectile.NewProjectile(null, pos + Vector2.UnitY * 20, new Vector2(0, -5), ModContent.ProjectileType<WellspringProjectile>(), 0, 0, Player.whoAmI);
				return false;
			}

			return true;
		}
	}

	internal class WellspringProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public static int anySprings;

		public override void Load()
		{
			StarlightRiverBackground.DrawMapEvent += DrawWellspringMaps;
			StarlightRiverBackground.DrawOverlayEvent += DrawWellsprings;
			StarlightRiverBackground.CheckIsActiveEvent += AnyWellsprings;
		}

		private void DrawWellspringMaps(SpriteBatch sb)
		{
			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				if (proj.type == Type)
				{
					Texture2D glow = Assets.Keys.GlowAlpha.Value;

					float alpha = 1f;

					if (proj.timeLeft > 280)
						alpha = 1f - (proj.timeLeft - 280) / 20f;

					if (proj.timeLeft < 20)
						alpha = proj.timeLeft / 20f;

					Main.spriteBatch.Draw(glow, proj.Center - Main.screenPosition, null, new Color(1f, 1f, 1f, 0) * alpha, 0, glow.Size() / 2f, 1200f / glow.Width, 0, 0);
				}
			}
		}

		private void DrawWellsprings(GameTime gameTime, ScreenTarget starsMap, ScreenTarget starsTarget)
		{
			Effect mapEffect = Filters.Scene["StarMap"].GetShader().Shader;
			mapEffect.Parameters["map"].SetValue(starsMap.RenderTarget);
			mapEffect.Parameters["background"].SetValue(starsTarget.RenderTarget);

			Main.spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, mapEffect, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(starsMap.RenderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), new Color(1f, 1f, 1f, 0));

			Main.spriteBatch.End();
		}

		private bool AnyWellsprings()
		{
			bool value = anySprings > 0;
			anySprings--;
			return value;
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			anySprings = 5;

			float alpha = 1f;

			if (Projectile.timeLeft > 280)
				alpha = 1f - (Projectile.timeLeft - 280) / 20f;

			if (Projectile.timeLeft < 20)
				alpha = Projectile.timeLeft / 20f;

			Lighting.AddLight(Projectile.Center, new Vector3(0.2f, 0.6f, 0.8f) * alpha);

			Projectile.velocity *= 0.9f;

			foreach (Player player in Main.ActivePlayers)
			{
				if (Projectile.timeLeft % 30 == 0 && Vector2.Distance(player.Center, Projectile.Center) < 256)
				{
					player.Heal(5);
					player.statMana += 5;
					player.ManaEffect(5);
				}
			}

			if (Main.rand.NextBool(10))
			{
				int d = Dust.NewDust(Projectile.position, 32, 32, ModContent.DustType<Dusts.Aurora>(), 0, -1, 0, new Color(200, 220, 255), 1);
				Main.dust[d].customData = Main.rand.NextFloat(0.3f, 0.6f);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				float dist = Vector2.Distance(player.Center, Projectile.Center);
				if (dist < 256)
				{
					DrawLine(Main.spriteBatch, player.Center);
				}
			}

			Texture2D tex = Assets.Abilities.HintCursor.Value;
			Texture2D glow = Assets.Keys.GlowAlpha.Value;
			Texture2D star = Assets.StarTexture.Value;
			Texture2D ring = Assets.Misc.GlowRing.Value;
			var frame = new Rectangle(0, 120, 50, 30);

			float alpha = 1f;

			if (Projectile.timeLeft > 280)
				alpha = 1f - (Projectile.timeLeft - 280) / 20f;

			if (Projectile.timeLeft < 20)
				alpha = Projectile.timeLeft / 20f;

			Main.spriteBatch.Draw(ring, Projectile.Center - Main.screenPosition, null, new Color(0, 140, 255, 0) * alpha * (0.03f + (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.01f), 0, ring.Size() / 2f, 512f / ring.Width, 0, 0);
			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, new Color(0, 140, 255, 0) * alpha * 0.25f, 0, glow.Size() / 2f, 512f / glow.Width, 0, 0);

			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, new Color(160, 190, 255, 0) * alpha * 0.5f, 0, glow.Size() / 2f, 1f, 0, 0);
			Main.spriteBatch.Draw(star, Projectile.Center - Main.screenPosition, null, new Color(90, 220, 255, 0) * alpha * 0.35f, 0, star.Size() / 2f, 0.65f, 0, 0);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255, 0) * alpha, 0, frame.Size() / 2f, 1f, 0, 0);

			return false;
		}

		public void DrawLine(SpriteBatch spriteBatch, Vector2 endPoint)
		{
			Texture2D texBeam = Assets.EnergyTrail.Value;

			float rotation = Projectile.Center.DirectionTo(endPoint).ToRotation();
			float distance = Vector2.Distance(Projectile.Center, endPoint);

			int sin = (int)(Math.Sin(StarlightWorld.visualTimer * 3) * 20f);

			float alpha = 1f;

			if (Projectile.timeLeft > 280)
				alpha = 1f - (Projectile.timeLeft - 280) / 20f;

			if (Projectile.timeLeft < 20)
				alpha = Projectile.timeLeft / 20f;

			Color color = new Color(100, 160 + sin, 220 + sin) * (1 - distance / 256f) * alpha;
			color.A = 0;

			var origin = new Vector2(0, texBeam.Height / 2);

			Effect effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/Wiggle").Value;

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 2f);

			effect.Parameters["freq1"].SetValue(2.0f);
			effect.Parameters["speed1"].SetValue(0.03f);
			effect.Parameters["amp1"].SetValue(0.14f);

			effect.Parameters["freq2"].SetValue(3.5f);
			effect.Parameters["speed2"].SetValue(0.1f);
			effect.Parameters["amp2"].SetValue(0.14f);

			effect.Parameters["colorIn"].SetValue(color.ToVector4());

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

			float height = texBeam.Height / 10f;
			int width = (int)(Projectile.Center - endPoint).Length();

			Vector2 pos = Projectile.Center - Main.screenPosition;

			var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);
			var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.4f));

			var source = new Rectangle((int)(Main.GameUpdateCount * -0.14f * texBeam.Width), 0, (int)(distance / texBeam.Width * texBeam.Width * 6), texBeam.Height);
			var source2 = new Rectangle((int)(Main.GameUpdateCount * -0.09f * texBeam.Width), 0, (int)(distance / texBeam.Width * texBeam.Width * 6), texBeam.Height);

			spriteBatch.Draw(texBeam, target, source, color, rotation, origin, 0, 0);
			spriteBatch.Draw(texBeam, target2, source2, color, rotation, origin, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			for (int i = 0; i < width; i += 10)
			{
				Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(rotation) * i + Main.screenPosition, color.ToVector3() * height * 0.010f);
			}

			float opacity = height / (texBeam.Height / 2f) * 0.75f;

			Texture2D impactTex = Assets.Keys.GlowAlpha.Value;
			Texture2D glowTex = Assets.GlowTrail.Value;

			spriteBatch.Draw(glowTex, target, source, color * 0.05f, rotation, new Vector2(0, glowTex.Height / 2), 0, 0);

			spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color * (height * 0.012f), 0, impactTex.Size() / 2, 0.8f, 0, 0);
			spriteBatch.Draw(impactTex, pos, null, color * (height * 0.02f), 0, impactTex.Size() / 2, 0.4f, 0, 0);
		}
	}

	class WellspringItem : InfusionItem<HintAbility, Wellspring>
	{
		public override InfusionTier Tier => InfusionTier.Bronze;
		public override string Texture => "StarlightRiver/Assets/Abilities/Wellspring";
		public override string FrameTexture => "StarlightRiver/Assets/Abilities/DefaultFrame";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wellspring");
			Tooltip.SetDefault("Starsight Infusion\nCreates a restorative wellspring when using starsight on the air\nSpend 2 starlight to do this");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 14;
			Item.rare = ItemRarityID.Green;

			color = new Color(80, 250, 250);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<BasicInfusion>(1);
			recipe.AddIngredient(ItemID.HealingPotion, 10);
			recipe.AddIngredient(ItemID.ManaPotion, 10);
			recipe.AddTile(ModContent.TileType<MainForge>());
			recipe.Register();
		}
	}
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Misc
{
	public class MagmaGunManager : ILoadable
    {
		public float Priority => 1.4f;

		public int oldScreenWidth = 0;
		public int oldScreenHeight = 0;

		public RenderTarget2D Target { get; protected set; }
		public RenderTarget2D TmpTarget { get; protected set; }

		public Color outlineColor => new Color(255, 210, 0);
		public Color insideColor => new Color(255, 120, 20);

		public void Load()
        {
			if (Main.dedServ)
				return;

			if (Main.graphics.GraphicsDevice != null)
				UpdateWindowSize(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
			On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles;
			Main.OnPreDraw += Main_OnPreDraw;
		}
		public void Unload()
        {
			On.Terraria.Main.DrawProjectiles -= Main_DrawProjectiles;
			Main.OnPreDraw -= Main_OnPreDraw;
		}

		public void UpdateWindowSize(GraphicsDevice graphicsDevice, int width, int height)
		{
			Target = new RenderTarget2D(graphicsDevice, width, height);
			TmpTarget = new RenderTarget2D(graphicsDevice, width, height);
			oldScreenWidth = width;
			oldScreenHeight = height;
		}

		private void Main_DrawProjectiles(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
		{
			orig(self);
			DrawTarget(Main.spriteBatch);
		}

		private void Main_OnPreDraw(GameTime obj)
		{
			if (Main.spriteBatch != null && Main.graphics.GraphicsDevice != null && CheckForBalls())
				DrawToTarget(Main.spriteBatch, Main.graphics.GraphicsDevice);

			if (Main.graphics.GraphicsDevice != null)
            {
				if (Main.screenWidth != oldScreenWidth || Main.screenHeight != oldScreenHeight)
                {
					UpdateWindowSize(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
				}
            }
		}


		private void DrawToTarget(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
			graphicsDevice.SetRenderTarget(Target);
			graphicsDevice.Clear(Color.Transparent);

			Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;
			borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
			borderNoise.CurrentTechnique.Passes[0].Apply();

			foreach (Projectile proj in Main.projectile)
            {
				if (proj.modProjectile is MagmaGunProj modProj && proj.active)
                {
					borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + modProj.rotationConst);
					spriteBatch.Draw(Main.projectileTexture[proj.type], (proj.Center - Main.screenPosition) / 2, null, Color.White, 0f, Vector2.One * 256f, proj.scale / 32f, SpriteEffects.None, 0);
				}
            }

			spriteBatch.End();

			Effect metaballColorCode = Filters.Scene["MetaballColorCode"].GetShader().Shader;
			metaballColorCode.Parameters["codedColor"].SetValue(new Color(0,255,0).ToVector4());
			AddEffect(spriteBatch, graphicsDevice, Target, metaballColorCode);

			Effect metaballEdgeDetection = Filters.Scene["MetaballEdgeDetection"].GetShader().Shader;
			metaballEdgeDetection.Parameters["width"].SetValue((float)Main.screenWidth / 2);
			metaballEdgeDetection.Parameters["height"].SetValue((float)Main.screenHeight / 2);
			metaballEdgeDetection.Parameters["border"].SetValue(outlineColor.ToVector4());
			metaballEdgeDetection.Parameters["codedColor"].SetValue(insideColor.ToVector4());

			AddEffect(spriteBatch, graphicsDevice, Target, metaballEdgeDetection);
		}

		private void AddEffect(SpriteBatch sB, GraphicsDevice graphicsDevice, RenderTarget2D target, Effect effect)
		{
			graphicsDevice.SetRenderTarget(TmpTarget);
			graphicsDevice.Clear(Color.Transparent);

			sB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

			effect.CurrentTechnique.Passes[0].Apply();

			sB.Draw(target, position: Vector2.Zero, color: Color.White);

			sB.End();

			graphicsDevice.SetRenderTarget(target);
			graphicsDevice.Clear(Color.Transparent);

			sB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

			sB.Draw(TmpTarget, position: Vector2.Zero, color: Color.White);

			sB.End();
		}

		private static bool CheckForBalls()
		{
			foreach (Projectile proj in Main.projectile)
			{
				if (proj.modProjectile is MagmaGunProj modProj && proj.active)
				{
					return true;
				}
			}
			return false;
		}

		private void DrawTarget(SpriteBatch spriteBatch)
        {
			if (!CheckForBalls())
				return;
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			spriteBatch.Draw(Target, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

			spriteBatch.End();
		}
    }
	public class MagmaGun : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Gun");
			Tooltip.SetDefault("Update this later");

		}

		public override void SetDefaults()
		{
			item.damage = 30;
			item.ranged = true;
			item.width = 24;
			item.height = 24;
			item.useTime = 2;
			item.useAnimation = 8;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 0;
			item.rare = ItemRarityID.Blue;
			item.shoot = ModContent.ProjectileType<MagmaGunProj>();
			item.shootSpeed = 12f;
			item.autoReuse = true;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			Vector2 direction = new Vector2(speedX, speedY).RotatedByRandom(0.2f);
			direction *= Main.rand.NextFloat(0.9f, 1.15f);
			Projectile.NewProjectile(position + (Vector2.Normalize(new Vector2(speedX,speedY)) * 40), direction, type, damage, knockBack, player.whoAmI);

			return true;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-15, 0);
		}
	}
	public class MagmaGunProj : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		private Player owner => Main.player[projectile.owner];

		public float rotationConst;

		private int embedTimer = 4;
		private bool touchingTile = false;
		private bool stoppedInTile = false;

		private float endScale = 1;
		private float fadeIn = 0f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Glob");
		}
		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Shuriken);
			projectile.width = 18;
			projectile.damage = 0;
			projectile.height = 18;
			projectile.ranged = true;
			projectile.timeLeft = 200;
			projectile.aiStyle = 14;
			projectile.friendly = true;
			endScale = Main.rand.NextFloat(0.7f, 2f);
			projectile.penetrate = -1;
			rotationConst = (float)Main.rand.NextDouble() * 6.28f;
		}

        public override void AI()
        {
			if (fadeIn < 1)
				fadeIn += 0.1f;
			projectile.scale = endScale * fadeIn;

			Lighting.AddLight(projectile.Center, Color.Orange.ToVector3() * 0.4f * projectile.scale);
            if (touchingTile)
            {
				embedTimer--;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Color color = Color.Orange;
			color.A = 0;
			Texture2D tex = ModContent.GetTexture(Texture + "_Glow");
			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color, 0f, tex.Size() / 2, projectile.scale * 0.33f, SpriteEffects.None, 0f);
			return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stoppedInTile)
            {
				touchingTile = true;
				projectile.velocity = Vector2.Normalize(oldVelocity) * 9;
				if (embedTimer < 0)
                {
					stoppedInTile = true;
					projectile.velocity = Vector2.Zero;
					projectile.aiStyle = -1;
                }
            }
			return false;
        }
    }
}
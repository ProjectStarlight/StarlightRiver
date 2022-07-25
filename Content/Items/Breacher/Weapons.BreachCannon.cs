//TODO:
//Implement laser start and end things
//Implement laser explosion
//Make it break if you break the tile
//Sell price
//Rarity
//Obtainment
//Balance
//Improve tile finding
//Add rotation easing
//Remove main.newtext
//Improve item usestyle
//Dust


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Items.Breacher
{
	class BreachCannon : ModItem
	{
        public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breach Cannon");
			Tooltip.SetDefault("Summons a sentry that shoots a laser towards the cursor");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.QueenSpiderStaff);
			Item.damage = 19;
			Item.mana = 12;
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 0, 80, 0);
			Item.rare = ItemRarityID.Green;
			Item.knockBack = 2.5f;
			Item.UseSound = SoundID.Item25;
			Item.shoot = ModContent.ProjectileType<BreachCannonSentry>();
			Item.shootSpeed = 0f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			position = Main.MouseWorld;
			//0 = right
			//1 = bottom
			//2 = left
			//3 = top

			int minDistance = 99;
			int minDirection = 0;

			Vector2 originTile = Vector2.Zero;
			for (int k = 0; k < 4; k++)
            {
				Vector2 testPosition = position / 16;
				Vector2 direction = Vector2.UnitX.RotatedBy(k * 1.57f);
				int testDistance = 0;
				while (testDistance < 20)
                {
					testPosition += direction;
					testDistance++;
					if (testDistance > minDistance)
						break;

					int i = (int)testPosition.X;
					int j = (int)testPosition.Y;
					Tile testTile = Main.tile[i, j];
					if (testTile.HasTile && Main.tileSolid[testTile.TileType])
                    {
						minDistance = testDistance;
						minDirection = k;
						originTile = testPosition;
						break;
                    }
				}
            }

			if (minDistance == 99)
			{
				Main.NewText("Placement failed");
				return false;
			}
			Projectile proj = Projectile.NewProjectileDirect(source, (originTile - Vector2.UnitX.RotatedBy(minDirection * 1.57f)) * 16, velocity, type, damage, knockback, player.whoAmI, minDirection);
			var mp = proj.ModProjectile as BreachCannonSentry;
			mp.tileOrigin = originTile;
			proj.originalDamage = Item.damage;
			player.UpdateMaxTurrets();
			Main.NewText(minDirection.ToString());
			return false;
		}
	}

	public class BreachCannonSentry : ModProjectile
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		private Player owner => Main.player[Projectile.owner];


		//0 = right
		//1 = bottom
		//2 = left
		//3 = top
		private int originAngle => (int)Projectile.ai[0];

		private float originAngleRad => originAngle * 1.57f;

		public Vector2 tileOrigin = Vector2.Zero;

		public Vector2 tileWorldPos => (tileOrigin * 16) + new Vector2(8,8);

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private float laserLength => (laserEndpoint - laserStartpoint).Length();

		private Vector2 laserStartpoint;
		private Vector2 laserEndpoint;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Breach Cannon");
		}

		public override void SetDefaults()
		{
			Projectile.width = 58;
			Projectile.height = 58;
			Projectile.timeLeft = Projectile.SentryLifeTime;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.sentry = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.ignoreWater = true;
		}

        public override void AI()
        {
			Projectile.Center = tileWorldPos - originAngleRad.ToRotationVector2() * 26;
			Projectile.rotation = Projectile.DirectionTo(Main.MouseWorld).ToRotation();

			laserStartpoint = Projectile.Center + (Projectile.DirectionTo(Main.MouseWorld).RotatedBy(InvertRotation() ? 0.4f : -0.4f) * 26);
			Vector2 offset = Vector2.Zero;
			for (int k = 0; k < 150; k++)
			{
				offset = Projectile.DirectionTo(Main.MouseWorld) * k * 16;

				int i = (int)((laserStartpoint.X + offset.X) / 16);
				int j = (int)((laserStartpoint.Y + offset.Y) / 16);
				Tile testTile = Main.tile[i, j];
				if (testTile.HasTile && Main.tileSolid[testTile.TileType])
				{
					break;
				}
			}
			laserEndpoint = laserStartpoint + offset;

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

        public override bool PreDraw(ref Color lightColor)
        {

			var spriteBatch = Main.spriteBatch;

			DrawTrail(spriteBatch);
			Texture2D cannonTex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D baseTex = ModContent.Request<Texture2D>(Texture + "_Base").Value;

            float baseRotation = -1.57f + originAngleRad;
			Vector2 baseOrigin = new Vector2(baseTex.Width / 2, baseTex.Height + 8);

			Main.spriteBatch.Draw(baseTex, tileWorldPos - Main.screenPosition, null, lightColor, baseRotation, baseOrigin, Projectile.scale, SpriteEffects.None, 0f);

			float cannonRotation = Projectile.rotation;
            Vector2 cannonOrigin = new Vector2(cannonTex.Width / 2, cannonTex.Height * 0.75f);
			SpriteEffects cannonEffects = SpriteEffects.None;
			if (InvertRotation())
            {
				cannonEffects = SpriteEffects.FlipHorizontally;
				cannonRotation -= 3.14f;
            }
			Main.spriteBatch.Draw(cannonTex, Projectile.Center - Main.screenPosition, null, lightColor, cannonRotation, cannonOrigin, Projectile.scale, cannonEffects, 0f);

			return false;
        }

		private void ManageCaches()
        {
			cache = new List<Vector2>();
			for (int i = 0; i < 14; i++)
            {
				cache.Add(Vector2.Lerp(laserStartpoint, laserEndpoint, i / 14f));
            }
			cache.Add(laserEndpoint);
		}

		private void ManageTrail()
        {
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 30, factor =>
			{
				return Color.Cyan;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = laserEndpoint;

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(4), factor => 15, factor =>
			{
				return Color.White;
			});

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = laserEndpoint;
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["BreachLaser"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["noiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.25f);
			effect.Parameters["stretch"].SetValue(2f / laserLength);

			effect.Parameters["dilation"].SetValue(0.8f);
			effect.Parameters["falloff"].SetValue(1);

			trail?.Render(effect);
			trail2?.Render(effect);

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private bool InvertRotation()
        {
			Vector2 originVector = Projectile.rotation.ToRotationVector2();
			switch (originAngle)
            {
				case 0:
					return originVector.Y > 0;
				case 1:
					return originVector.X < 0;
				case 2:
					return originVector.Y < 0;
				case 3:
					return originVector.X > 0;
			}
			return false;
        }
    }
}

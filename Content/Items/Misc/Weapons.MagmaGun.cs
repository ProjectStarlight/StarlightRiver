using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Vitric;
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

		public Color outlineColor => new Color(255, 255, 100);
		public Color outlineColor2 => Color.White;

		public Color tileOutlineColor => Color.OrangeRed;
		public Color insideColor2 => new Color(255, 70, 10);
		public Color insideColor => new Color(255, 190, 30);

		public void Load()
        {
			if (Main.dedServ)
				return;

			if (Main.graphics.GraphicsDevice != null)
				UpdateWindowSize(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
			On.Terraria.Main.DrawNPCs += Main_DrawNPCs;
			Main.OnPreDraw += Main_OnPreDraw;
		}
		public void Unload()
        {
			On.Terraria.Main.DrawNPCs -= Main_DrawNPCs;
			Main.OnPreDraw -= Main_OnPreDraw;
		}

		public void UpdateWindowSize(GraphicsDevice graphicsDevice, int width, int height)
		{
			Target = new RenderTarget2D(graphicsDevice, width, height);
			TmpTarget = new RenderTarget2D(graphicsDevice, width, height);
			oldScreenWidth = width;
			oldScreenHeight = height;
		}

		private void Main_DrawNPCs(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles = false)
		{
			DrawTarget(Main.spriteBatch);
			orig(self, behindTiles);
		}

		private void Main_OnPreDraw(GameTime obj)
		{
			if (Main.graphics.GraphicsDevice != null)
			{
				if (Main.screenWidth != oldScreenWidth || Main.screenHeight != oldScreenHeight)
				{
					UpdateWindowSize(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
				}
			}

			if (Main.spriteBatch != null && Main.graphics.GraphicsDevice != null && CheckForBalls())
				DrawToTarget(Main.spriteBatch, Main.graphics.GraphicsDevice);
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
				/*if (proj.modProjectile is MagmaGunProj modProj && proj.active)
				{
					borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + modProj.rotationConst);
					spriteBatch.Draw(Main.projectileTexture[proj.type], (proj.Center - Main.screenPosition) / 2, null, Color.White, 0f, Vector2.One * 256f, proj.scale / 32f, SpriteEffects.None, 0);
				}*/

				if (proj.modProjectile is MagmaGunPhantomProj modProj && proj.active)
                {
					foreach (MagmaGlob glob in modProj.Globs)
                    {
						if (glob.active)
                        {
							borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + glob.rotationConst);
							spriteBatch.Draw(Main.projectileTexture[proj.type], (glob.Center - Main.screenPosition) / 2, null, Color.White, 0f, Vector2.One * 256f, glob.scale / 32f, SpriteEffects.None, 0);
						}
                    }
                }
			}


			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == ModContent.DustType<MagmaGunDust>())
				{
					borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + dust.rotation);
					spriteBatch.Draw(ModContent.GetTexture(AssetDirectory.MiscItem + "MagmaGunProj"), (dust.position - Main.screenPosition) / 2, null, Color.White, 0f, Vector2.One * 256f, dust.scale / 64f, SpriteEffects.None, 0);
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

			Effect metaballEdgeDetection2 = Filters.Scene["MetaballEdgeDetection2"].GetShader().Shader;
			metaballEdgeDetection2.Parameters["width"].SetValue((float)Main.screenWidth / 2);
			metaballEdgeDetection2.Parameters["height"].SetValue((float)Main.screenHeight / 2);
			metaballEdgeDetection2.Parameters["border"].SetValue(outlineColor2.ToVector4());

			AddEffect(spriteBatch, graphicsDevice, Target, metaballEdgeDetection2);

			Effect magmaNoise = Filters.Scene["MagmaNoise"].GetShader().Shader;
			magmaNoise.Parameters["noiseScale"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight) / 200);
			magmaNoise.Parameters["offset"].SetValue(Vector2.Zero);
			magmaNoise.Parameters["codedColor"].SetValue(insideColor.ToVector4());
			magmaNoise.Parameters["newColor"].SetValue(insideColor2.ToVector4());
			magmaNoise.Parameters["distort"].SetValue(ModContent.GetTexture(AssetDirectory.Assets + "Noise/ShaderNoiseLooping"));

			AddEffect(spriteBatch, graphicsDevice, Target, magmaNoise);

			/*if (TileDrawOverLoader.tileTarget != null)
			{
				Effect magmaTiles = Filters.Scene["MagmaTileShader"].GetShader().Shader;
				magmaTiles.Parameters["TileTarget"].SetValue(TileDrawOverLoader.tileTarget);
				magmaTiles.Parameters["transparency"].SetValue(0f);
				magmaTiles.Parameters["tileScale"].SetValue(2);
				magmaTiles.Parameters["width"].SetValue((float)Main.screenWidth / 2);
				magmaTiles.Parameters["height"].SetValue((float)Main.screenHeight / 2);
				magmaTiles.Parameters["border"].SetValue(tileOutlineColor.ToVector4());
				AddEffect(spriteBatch, graphicsDevice, Target, magmaTiles);
			}*/
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
				if (proj.modProjectile is MagmaGunPhantomProj modProj && proj.active)
				{
					return true;
				}
			}
			foreach (Dust dust in Main.dust)
            {
				if (dust.active && dust.type == ModContent.DustType<MagmaGunDust>())
					return true;
            }
			return false;
		}

		private void DrawTarget(SpriteBatch spriteBatch)
        {
			if (!CheckForBalls())
				return;
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			if (TileDrawOverLoader.tileTarget != null)
			{
				Effect magmaTiles = Filters.Scene["MagmaTileShader"].GetShader().Shader;
				magmaTiles.Parameters["TileTarget"].SetValue(TileDrawOverLoader.tileTarget);
				magmaTiles.Parameters["transparency"].SetValue(0f);
				magmaTiles.Parameters["tileScale"].SetValue(2);
				magmaTiles.Parameters["width"].SetValue((float)Main.screenWidth / 2);
				magmaTiles.Parameters["height"].SetValue((float)Main.screenHeight / 2);
				magmaTiles.Parameters["border"].SetValue(tileOutlineColor.ToVector4());
				magmaTiles.Parameters["oldBorder"].SetValue(outlineColor.ToVector4());
				magmaTiles.CurrentTechnique.Passes[0].Apply();
			}

			spriteBatch.Draw(Target, Vector2.Zero, null, Color.White, 0, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
		}
    }
	public class MagmaGun : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		private Projectile proj;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Gun");
			Tooltip.SetDefault("Launches hot globs of magma that stick to enemies and tiles");

		}

		public override void SetDefaults()
		{
			item.damage = 30;
			item.ranged = true;
			item.width = 24;
			item.height = 24;
			item.useTime = 1;
			item.useAnimation = 8;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.knockBack = 0;
			item.rare = ItemRarityID.Orange;
			item.value = Item.sellPrice(0, 3, 0, 0);
			item.shoot = ModContent.ProjectileType<MagmaGunPhantomProj>();
			item.shootSpeed = 12f;
			item.autoReuse = true;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			for (int i = 0; i < 3; i++)
			{
				if (proj != null && proj.active)
				{
					proj.damage = damage;
					var mp = proj.modProjectile as MagmaGunPhantomProj;
					Vector2 direction = new Vector2(speedX, speedY).RotatedByRandom(0.1f);
					direction *= Main.rand.NextFloat(0.9f, 1.15f);
					position += Vector2.Normalize(new Vector2(speedX, speedY)) * 20;

					mp.CreateGlob(position, direction);
				}
			}
			return false;
		}

        public override void UpdateInventory(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MagmaGunPhantomProj>()] == 0)
            {
				proj = Projectile.NewProjectileDirect(player.Center, Vector2.Zero, ModContent.ProjectileType<MagmaGunPhantomProj>(), 0, 0, player.whoAmI);
            }
        }

        public override Vector2? HoldoutOffset()
		{
			return new Vector2(-15, 0);
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.HellstoneBar, 15);
			recipe.AddIngredient(ModContent.ItemType<SandstoneChunk>(), 8);
			recipe.AddIngredient(ModContent.ItemType<MagmaCore>(), 1);
			recipe.AddTile(TileID.Anvils);

			recipe.SetResult(this);

			recipe.AddRecipe();
		}
	}
	public class MagmaGlob
	{
		public float rotationConst;

		public int embedTimer = 1;
		public bool touchingTile = false;
		public bool stoppedInTile = false;
		public int timeLeft = 200;

		public bool stoppedInEnemy = false;
		public Vector2 enemyOffset = Vector2.Zero;
		public NPC enemy;
		public bool friendly = true;

		public float endScale = 1;
		public float fadeIn = 0f;

		public int width => 18;
		public int height => 18;
		public Vector2 Position;

		public Vector2 Velocity;
		public Vector2 oldVel;
		public Vector2 Size => new Vector2(width, height);
		public Vector2 Center
		{
			get
			{
				return Position + (Size / 2);
			}
			set
            {
				Position = value - (Size / 2);
            }
		}

		public float scale;

		public bool active = true;

		public MagmaGlob(Vector2 velocity, Vector2 position)
        {
			Velocity = velocity;
			Center = position;
			endScale = Main.rand.NextFloat(0.7f, 2f) * 0.75f;
			rotationConst = (float)Main.rand.NextDouble() * 6.28f;
		}

		public void Update()
        {
			timeLeft--;

			if (fadeIn < 1)
				fadeIn += 0.1f;
			scale = Math.Min(endScale, endScale * (timeLeft / 20f)) * fadeIn;

			if (Main.rand.NextBool(100))
            {
				Dust.NewDustPerfect(Center, ModContent.DustType<Dusts.MagmaSmoke>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), (int)(Main.rand.Next(15, 45)), Color.White, Main.rand.NextFloat(0.4f, 1f));
			}

			Color lightColor = Color.OrangeRed;
			lightColor.B += 50;
			lightColor.R -= 50;
			Lighting.AddLight(Center, lightColor.ToVector3() * 1f * scale);
			if (!stoppedInEnemy)
				CheckIfTouchingTiles();
			else
            {
				if (enemy != null && enemy.active)
				{
					Center = enemy.Center + enemyOffset;
					enemy.AddBuff(BuffID.OnFire, 5);
				}
				else
					active = false;
            }

			if (touchingTile)
			{
				embedTimer--;
			}
			if (stoppedInTile && Main.rand.NextBool(200))
			{
				Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 4);
				Dust.NewDust(Position, width, height, ModContent.DustType<MagmaGunDust>(), dir.X, dir.Y);
			}

			if (timeLeft < 20)
            {
				Position -= oldVel * 0.1f;
            }

			if (!stoppedInTile)
				Velocity.Y += 0.1f;

			Center += Velocity;
			if (timeLeft <= 0)
				active = false;
        }

		public void CheckIfTouchingTiles()
        {
			bool breakOut = false;
			for (int i = (int)Position.X; i < (int)(Position.X + Size.X) && !breakOut; i += 16)
            {
				for (int j = (int)Position.Y; j < (int)(Position.Y + Size.Y) && !breakOut; j += 16)
				{
					if (WorldGen.InWorld(i / 16, j / 16))
					{
						Tile tile = Main.tile[i / 16, j / 16];
						if (tile.active() && Main.tileSolid[tile.type])
						{
							if (!stoppedInTile)
							{
								touchingTile = true;
								Velocity = Vector2.Normalize(Velocity) * 9;
								if (embedTimer < 0)
								{
									stoppedInTile = true;
									oldVel = Velocity;
									Velocity = Vector2.Zero;
									int k = 0;
									int d = 0;
									if (Main.rand.NextBool())
									{
										Vector2 bubbleDir = -Vector2.Normalize(oldVel).RotatedByRandom(0.8f) * Main.rand.NextFloat(2, 3);
										d = Dust.NewDust(Position + (bubbleDir * 4), width, height, ModContent.DustType<Dusts.LavaSpark>(), 0, 0, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.2f, 0.3f));
										Main.dust[d].velocity = bubbleDir;
									}
									else
									{
										Vector2 bubbleDir = -Vector2.Normalize(oldVel).RotatedByRandom(0.2f) * 6;
										Vector2 pos = Position + new Vector2(Main.rand.Next(width), Main.rand.Next(height));
										pos += (bubbleDir * 4);

										if (WorldGen.InWorld((int)(pos.X / 16), (int)(pos.Y / 16)))
										{
											Tile tile2 = Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)];
											if (!Main.tileSolid[tile2.type] || !tile2.active())
											{
												Gore.NewGoreDirect(pos, bubbleDir, ModGore.GetGoreSlot("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore"), Main.rand.NextFloat(0.5f, 0.8f));
											}
										}
									}

									for (k = 0; k < 4; k++)
                                    {
										Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 4);
										Dust.NewDust(Position, width, height, ModContent.DustType<MagmaGunDust>(), dir.X, dir.Y, default, default, 2);
									}

								}
							}
							breakOut = true;
						}
					}
				}
			}
        }

		public void Draw(SpriteBatch spriteBatch, Texture2D tex)
		{
			Color color = new Color(255, 70, 10);
			color.A = 0;
			spriteBatch.Draw(tex, Center - Main.screenPosition, null, color * 0.425f, 0f, tex.Size() / 2, scale * 0.34f, SpriteEffects.None, 0f);
		}

		/*public void DrawSinge(SpriteBatch spriteBatch, Texture2D tex)
		{
			Color color = Color.DarkGray;
			color.A = 0;
			spriteBatch.Draw(tex, Center - Main.screenPosition, null, color * 0.0425f, 0f, tex.Size() / 2, scale * 0.34f, SpriteEffects.None, 0f);
		}*/
	}

	public class MagmaGunPhantomProj : ModProjectile
    {
		public override string Texture => AssetDirectory.MiscItem + "MagmaGunProj";

		public List<MagmaGlob> Globs = new List<MagmaGlob>();

		private Player owner => Main.player[projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Glob");
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.Shuriken);
			projectile.width = 2;
			projectile.height = 2;
			projectile.ranged = true;
			projectile.timeLeft = 200;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.hide = true;
			projectile.ignoreWater = true;
		}

		public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
		{
			drawCacheProjsBehindNPCsAndTiles.Add(index);
		}

		public override void AI()
        {
			projectile.timeLeft = 2;
			projectile.Center = owner.Center;

			foreach (MagmaGlob glob in Globs)
            {
				glob.Update();
            }

			foreach (MagmaGlob glob in Globs.ToArray())
			{
				if (!glob.active)
					Globs.Remove(glob);
			}

			if (owner.HeldItem.type != ModContent.ItemType<MagmaGun>() && Globs.Count == 0)
				projectile.active = false;
		}

		public void CreateGlob(Vector2 pos, Vector2 vel)
        {
			Globs.Add(new MagmaGlob(vel, pos));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			foreach (MagmaGlob glob in Globs)
            {
				if (glob.active)
                {
					glob.Draw(spriteBatch, ModContent.GetTexture(Texture + "_Glow"));
                }
            }
			return false;
        }

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return true;
		}

        public override bool? CanHitNPC(NPC target)
        {
			foreach (MagmaGlob glob in Globs)
			{
				if (glob.stoppedInEnemy)
					continue;
				if (glob.active)
				{
					if (Collision.CheckAABBvAABBCollision(target.position, target.Size, glob.Position, glob.Size))
					{
						if (!glob.stoppedInTile)
						{
							glob.stoppedInEnemy = true;
							glob.enemy = target;
							glob.enemyOffset = glob.Center - target.Center;
						}
						target.AddBuff(BuffID.OnFire, 30);
						return true;
					}
				}
			}
			return false;
		}

		/*public void DrawOverTiles(SpriteBatch spriteBatch)
        {
			foreach (MagmaGlob glob in Globs)
			{
				if (glob.active)
				{
					glob.DrawSinge(spriteBatch, ModContent.GetTexture(Texture + "_Glow"));
				}
			}
		}*/
    }

	public class MagmaGunDust : ModDust
	{
		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.Assets + "Invisible";
			return true;
		}
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = true;
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity.Y += 0.2f;
			if (Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].active() && Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].collisionType == 1)
				dust.velocity *= -0.5f;

			dust.rotation = dust.velocity.ToRotation();
			dust.scale *= 0.96f;
			if (dust.scale < 0.5f)
				dust.active = false;
			return false;
		}
	}
}
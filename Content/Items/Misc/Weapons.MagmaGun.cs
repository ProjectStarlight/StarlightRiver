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
using Terraria.Enums;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class MagmaGunManager : IOrderedLoadable
    {
		public float Priority => 1.4f;

		public int oldScreenWidth = 0;
		public int oldScreenHeight = 0;

		public RenderTarget2D Target { get; protected set; }
		public RenderTarget2D TmpTarget { get; protected set; }

		public Color outlineColor => new Color(255, 255, 100);
		public Color outlineColor2 => Color.White;

		public Color tileOutlineColor => Color.Lerp(Color.OrangeRed, Color.Red, 0.5f);
		public Color insideColor2 => new Color(255, 70, 10);
		public Color insideColor => new Color(255, 190, 30);

		public void Load()
        {
			if (Main.dedServ)
				return;

			if (Main.graphics.GraphicsDevice != null)
				UpdateWindowSize(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
			On.Terraria.Main.DrawNPCs += Main_DrawNPCs;
			On.Terraria.Main.CheckMonoliths += Main_CheckMonoliths;
		}
		public void Unload()
        {
			On.Terraria.Main.DrawNPCs -= Main_DrawNPCs;
			On.Terraria.Main.CheckMonoliths -= Main_CheckMonoliths;
		}

		public void UpdateWindowSize(GraphicsDevice graphicsDevice, int width, int height)
		{
			Main.QueueMainThreadAction(() =>
			{
				Target = new RenderTarget2D(graphicsDevice, width, height);
				TmpTarget = new RenderTarget2D(graphicsDevice, width, height);
			});

			oldScreenWidth = width;
			oldScreenHeight = height;
		}

		private void Main_DrawNPCs(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles = false)
		{
			if (behindTiles)
				DrawTarget(Main.spriteBatch);
			orig(self, behindTiles);
		}

		private void Main_CheckMonoliths(On.Terraria.Main.orig_CheckMonoliths orig)
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

			orig();
		}


		private void DrawToTarget(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
			graphicsDevice.SetRenderTarget(Target);
			graphicsDevice.Clear(Color.Transparent);

			Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;

			if (borderNoise is null)
				return;

			borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
			borderNoise.CurrentTechnique.Passes[0].Apply();

			foreach (Projectile proj in Main.projectile)
			{
				if (proj.ModProjectile is MagmaGunPhantomProj modProj && proj.active)
                {
					foreach (MagmaGlob glob in modProj.Globs)
                    {
						if (glob.active)
                        {
							borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + glob.rotationConst);
							spriteBatch.Draw(TextureAssets.Projectile[proj.type].Value, (glob.Center - Main.screenPosition) / 2, null, Color.White, 0f, Vector2.One * 256f, glob.scale / 32f, SpriteEffects.None, 0);
						}
                    }
                }
			}


			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == ModContent.DustType<MagmaGunDust>())
				{
					borderNoise.Parameters["offset"].SetValue((float)Main.time / 1000f + dust.rotation);
					spriteBatch.Draw(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "MagmaGunProj").Value, (dust.position - Main.screenPosition) / 2, null, Color.White, 0f, Vector2.One * 256f, dust.scale / 64f, SpriteEffects.None, 0);
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
			magmaNoise.Parameters["offset"].SetValue(2 * Main.screenPosition / new Vector2(Main.screenWidth, Main.screenHeight));
			magmaNoise.Parameters["codedColor"].SetValue(insideColor.ToVector4());
			magmaNoise.Parameters["newColor"].SetValue(insideColor2.ToVector4());
			magmaNoise.Parameters["distort"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/ShaderNoiseLooping").Value);

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
				if (proj.ModProjectile is MagmaGunPhantomProj modProj && proj.active)
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

		private int counter;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pyroclastic Flow");
			Tooltip.SetDefault("Blasts a torrential stream of lava that sticks to tiles and enemies");

		}

		public override void SetDefaults()
		{
			Item.damage = 30;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 1;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 1;
			Item.useAnimation = 8;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.shoot = ModContent.ProjectileType<MagmaGunPhantomProj>();
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			counter++;
			foreach (Projectile Projectile in Main.projectile)
            {
				if (Projectile.owner == player.whoAmI && Projectile.type == type && Projectile.active)
					proj = Projectile;
            }
			if (counter % 5 == 0)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.SplashWeak with { PitchRange = (0.45f, 0.55f) }, position);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item13, position);
			}
			for (int i = 0; i < 3; i++)
			{
				if (proj != null && proj.active)
				{
					proj.damage = damage / 2;
					var mp = proj.ModProjectile as MagmaGunPhantomProj;
					Vector2 direction = velocity.RotatedByRandom(0.1f);
					direction *= Main.rand.NextFloat(0.9f, 1.15f);
					Vector2 position2 = position;
					position2 += Vector2.Normalize(velocity) * 20;
					position2 += Vector2.Normalize(velocity.RotatedBy(1.57f * -player.direction)) * 5;
					mp.CreateGlob(position2, direction);
				}
			}
			return false;
		}

        public override void UpdateInventory(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MagmaGunPhantomProj>()] == 0)
            {
				proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<MagmaGunPhantomProj>(), 0, 0, player.whoAmI);
            }
        }

        public override Vector2? HoldoutOffset()
		{
			return new Vector2(-15, 0);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HellstoneBar, 15);
			recipe.AddIngredient(ModContent.ItemType<SandstoneChunk>(), 8);
			recipe.AddIngredient(ModContent.ItemType<MagmaCore>(), 1);
			recipe.AddTile(TileID.Anvils);
		}
	}
	public class MagmaGlob
	{
		public float rotationConst;

		public int embedTimer = 1;
		public bool touchingTile = false;
		public bool stoppedInTile = false;
		public int timeLeft = 700;

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

			if (Main.rand.NextBool(2000))
				Terraria.Audio.SoundEngine.PlaySound(SoundID.SplashWeak, Center);
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
				int dustID = Dust.NewDust(Position, width, height, ModContent.DustType<MagmaGunDust>(), dir.X, dir.Y);
				Main.dust[dustID].noGravity = false;
			}
			else if (stoppedInTile && Main.rand.NextBool(500))
			{
				Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(20);
				int dustID = Dust.NewDust(Position + dir, width, height, ModContent.DustType<MagmaGunDust>(), 0,0);
				Main.dust[dustID].noGravity = true;
			}
			else if (Velocity == Vector2.Zero && Main.rand.NextBool(700))
            {
				Vector2 bubbleDir = -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(2, 3);
				int d = Dust.NewDust(Position + (bubbleDir * 4), width, height, ModContent.DustType<Dusts.LavaSpark>(), 0, 0, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.2f, 0.3f));
				Main.dust[d].velocity = bubbleDir;
			}

			if (timeLeft < 20)
            {
				Position -= oldVel * 0.1f;
            }

			if (!stoppedInTile)
				Velocity.Y += 0.1f;

			if (stoppedInTile)
			{
				Velocity = Vector2.Zero;
				int i = 0;
				int j = 0;
				if (!TouchingTiles(ref i, ref j))
				{
					embedTimer = 1;
					stoppedInTile = false;
					touchingTile = false;
				}
			}
			if (stoppedInEnemy)
				Velocity = Vector2.Zero;

			Center += Velocity;
			if (timeLeft <= 0)
				active = false;
        }

		public void CheckIfTouchingTiles()
        {
			int i = 0;
			int j = 0;
			if (TouchingTiles(ref i, ref j) && !stoppedInTile)
            {
				touchingTile = true;
				Velocity = Vector2.Normalize(Velocity) * 9;
				if (embedTimer < 0)
				{
					if (Main.rand.NextBool(40))
						Terraria.Audio.SoundEngine.PlaySound(SoundID.SplashWeak, Center);
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
							if (!Main.tileSolid[tile2.TileType] || !tile2.HasTile)
							{
								Gore.NewGoreDirect(new EntitySource_Misc("Spawned from magma gun"), pos, bubbleDir, StarlightRiver.Instance.Find<ModGore>("StarlightRiver/Assets/NPCs/Vitric/MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.8f));
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
        }

		private bool TouchingTiles(ref int i, ref int j)
        {
			for(i = (int)Position.X; i < (int)(Position.X + Size.X); i += 16) //Todo: Break this godawful nested statement hell into its own methods
            {
				for (j = (int)Position.Y; j < (int)(Position.Y + Size.Y); j += 16)
				{
					if (WorldGen.InWorld(i / 16, j / 16))
					{
						Tile tile = Main.tile[i / 16, j / 16];
						if (tile.HasTile && Main.tileSolid[tile.TileType] && !TileID.Sets.Platforms[tile.TileType])
						{
							return true;
						}
					}
				}
			}
			return false;
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

		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Glob");
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Shuriken);
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.timeLeft = 200;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.ignoreWater = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override void AI()
        {
			foreach (NPC target in Main.npc)
			{
				if (target.townNPC || !target.active)
					continue;
				foreach (MagmaGlob glob in Globs)
				{
					if (glob.stoppedInEnemy && glob.enemy != target)
						continue;
					if (glob.active)
					{
						if (Collision.CheckAABBvAABBCollision(target.position, target.Size, glob.Position, glob.Size))
						{

							if (!glob.stoppedInTile && !glob.stoppedInEnemy)
							{
								glob.stoppedInEnemy = true;
								glob.enemy = target;
								glob.enemyOffset = glob.Center - target.Center;
							}
						}
					}
				}
			}

			Projectile.timeLeft = 2;
			Projectile.Center = owner.Center;

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
				Projectile.active = false;
		}

		public void CreateGlob(Vector2 pos, Vector2 vel)
        {
			Globs.Add(new MagmaGlob(vel, pos));
        }

        public override bool PreDraw(ref Color lightColor)
        {
			foreach (MagmaGlob glob in Globs)
            {
				if (glob.active)
                {
					glob.Draw(Main.spriteBatch, ModContent.Request<Texture2D>(Texture + "_Glow").Value);
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
			if (target.townNPC || target.immune[Projectile.owner] > 0)
				return false;
			foreach (MagmaGlob glob in Globs)
			{
				if (glob.stoppedInEnemy && glob.enemy != target)
					continue;
				if (glob.active)
				{
					if (Collision.CheckAABBvAABBCollision(target.position, target.Size, glob.Position, glob.Size))
					{

						if (!glob.stoppedInTile && !glob.stoppedInEnemy)
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

		public override bool? CanCutTiles()
		{
			return true;
		}
		public override void CutTiles()
		{
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			foreach (MagmaGlob glob in Globs)
			{
				if (glob.active)
				{
					Utils.PlotTileLine(glob.Center - new Vector2((glob.width / 2) * glob.scale, 0), glob.Center + new Vector2((glob.width / 2) * glob.scale, 0), glob.height * glob.scale, DelegateMethods.CutTiles);
				}
			}
		}

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			foreach (MagmaGlob glob in Globs) //TODO: Merge with canhitNPC similar code into method to reduce boilerplate
			{
				if (glob.stoppedInEnemy)
					continue;
				if (glob.active)
				{
					if (Collision.CheckAABBvAABBCollision(target.position, target.Size, glob.Position, glob.Size))
					{
						damage *= 2;
						break;
					}
				}
			}
		}

        /*public void DrawOverTiles(SpriteBatch spriteBatch)
        {
			foreach (MagmaGlob glob in Globs)
			{
				if (glob.active)
				{
					glob.DrawSinge(spriteBatch, ModContent.Request<Texture2D>(Texture + "_Glow").Value);
				}
			}
		}*/
    }

	public class MagmaGunDust : ModDust
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";
		public override void OnSpawn(Dust dust)
		{
			dust.noLight = true;
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			if (dust.noGravity)
			{
				dust.velocity = new Vector2(0, -1f);
			}
			else
			{
				dust.velocity.Y += 0.2f;
				if (Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].HasTile && Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].BlockType == BlockType.Solid && Main.tileSolid[Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].TileType])
					dust.velocity *= -0.5f;
			}

			dust.rotation = dust.velocity.ToRotation();
			dust.scale *= 0.96f;
			if (dust.noGravity)
				dust.scale *= 0.96f;

			if (dust.scale < 0.2f)
				dust.active = false;
			return false;
		}
	}
}
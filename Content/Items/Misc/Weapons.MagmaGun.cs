using StarlightRiver.Content.Items.Vitric;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class MagmaGun : ModItem
	{
		private Projectile proj;

		private int counter;
		private float rotation;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pyroclastic Flow");
			Tooltip.SetDefault("Blasts a torrential stream of magma that sticks to tiles and enemies");
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
			if (rotation == -1)
				rotation = velocity.ToRotation();

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

			if (proj != null && proj.active && counter % 2 == 0)
			{
				proj.damage = damage / 2;
				var mp = proj.ModProjectile as MagmaGunPhantomProj;

				if (mp is null)
				{
					proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<MagmaGunPhantomProj>(), 0, 0, player.whoAmI);
					return false;
				}

				for (int i = 0; i < 2; i++)
				{
					rotation += Helpers.Helper.RotationDifference(velocity.ToRotation(), rotation) * 0.175f;
					velocity = Vector2.UnitX.RotatedBy(rotation) * velocity.Length();

					Vector2 direction = velocity.RotatedByRandom(0.1f);
					direction *= Main.rand.NextFloat(0.9f, 1.15f);

					Vector2 position2 = position;
					position2 += Vector2.Normalize(velocity) * 20;
					position2 += Vector2.Normalize(velocity.RotatedBy(1.57f * -player.direction)) * 5;
					mp.CreateGlob(position2, direction * 0.5f);
				}
			}
			else
			{
				proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<MagmaGunPhantomProj>(), 0, 0, player.whoAmI);
			}

			return false;
		}

		public override void UpdateInventory(Player player)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<MagmaGunPhantomProj>()] == 0)
				proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ModContent.ProjectileType<MagmaGunPhantomProj>(), 0, 0, player.whoAmI);

			if (!Main.mouseLeft)
				rotation = -1;
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
			recipe.Register();
		}
	}

	public class MagmaGlob
	{
		public const int WIDTH = 18;
		public const int HEIGHT = 18;

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

		public Vector2 position;

		public Vector2 velocity;
		public Vector2 oldVel;

		public float scale;
		public bool active = true;

		public Vector2 size = new(WIDTH, HEIGHT);

		public Vector2 Center
		{
			get => position + size / 2;
			set => position = value - size / 2;
		}

		public MagmaGlob(Vector2 velocity, Vector2 position)
		{
			this.velocity = velocity;
			Center = position;
			endScale = Main.rand.NextFloat(0.8f, 1.4f) * 0.75f;
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
				Dust.NewDustPerfect(Center, ModContent.DustType<Dusts.MagmaSmoke>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), Main.rand.Next(15, 45), Color.White, Main.rand.NextFloat(0.4f, 1f));

			Color lightColor = Color.OrangeRed;
			lightColor.B += 50;
			lightColor.R -= 50;
			Lighting.AddLight(Center, lightColor.ToVector3() * 1f * scale);

			if (!stoppedInEnemy)
			{
				CheckIfTouchingTiles();
			}
			else
			{
				if (enemy != null && enemy.active)
				{
					Center = enemy.Center + enemyOffset;
					enemy.AddBuff(BuffID.OnFire, 5);
				}
				else
				{
					active = false;
				}
			}

			if (touchingTile)
				embedTimer--;

			if (stoppedInTile && Main.rand.NextBool(200))
			{
				Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 4);
				int dustID = Dust.NewDust(position, WIDTH, HEIGHT, ModContent.DustType<MagmaGunDust>(), dir.X, dir.Y);
				Main.dust[dustID].noGravity = false;
			}

			else if (stoppedInTile && Main.rand.NextBool(500))
			{
				Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(20);
				int dustID = Dust.NewDust(position + dir, WIDTH, HEIGHT, ModContent.DustType<MagmaGunDust>(), 0, 0);
				Main.dust[dustID].noGravity = true;
			}

			else if (velocity == Vector2.Zero && Main.rand.NextBool(700))
			{
				Vector2 bubbleDir = -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(2, 3);
				int d = Dust.NewDust(position + bubbleDir * 4, WIDTH, HEIGHT, ModContent.DustType<Dusts.LavaSpark>(), 0, 0, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.2f, 0.3f));
				Main.dust[d].velocity = bubbleDir;
			}

			if (timeLeft < 20)
				position -= oldVel * 0.1f;

			if (!stoppedInTile)
				velocity.Y += 0.1f;

			if (stoppedInTile)
			{
				velocity = Vector2.Zero;
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
				velocity = Vector2.Zero;

			Center += velocity;

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
				velocity = Vector2.Normalize(velocity) * 9;

				if (embedTimer < 0)
				{
					if (Main.rand.NextBool(40))
						Terraria.Audio.SoundEngine.PlaySound(SoundID.SplashWeak, Center);

					stoppedInTile = true;
					oldVel = velocity;
					velocity = Vector2.Zero;

					if (Main.rand.NextBool())
					{
						Vector2 bubbleDir = -Vector2.Normalize(oldVel).RotatedByRandom(0.8f) * Main.rand.NextFloat(2, 3);
						int d = Dust.NewDust(position + bubbleDir * 4, WIDTH, HEIGHT, ModContent.DustType<Dusts.LavaSpark>(), 0, 0, 0, new Color(255, 150, 50), Main.rand.NextFloat(0.2f, 0.3f));
						Main.dust[d].velocity = bubbleDir;
					}
					else
					{
						Vector2 bubbleDir = -Vector2.Normalize(oldVel).RotatedByRandom(0.2f) * 6;
						Vector2 pos = position + new Vector2(Main.rand.Next(WIDTH), Main.rand.Next(HEIGHT));
						pos += bubbleDir * 4;

						if (WorldGen.InWorld((int)(pos.X / 16), (int)(pos.Y / 16)))
						{
							Tile tile2 = Main.tile[(int)(pos.X / 16), (int)(pos.Y / 16)];

							if (!Main.tileSolid[tile2.TileType] || !tile2.HasTile)
							{
								Gore.NewGoreDirect(new EntitySource_Misc("Spawned from magma gun"), pos, bubbleDir, StarlightRiver.Instance.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.8f));
							}
						}
					}

					for (int k = 0; k < 4; k++)
					{
						Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 4);
						Dust.NewDust(position, WIDTH, HEIGHT, ModContent.DustType<MagmaGunDust>(), dir.X, dir.Y, default, default, 2);
					}
				}
			}
		}

		private bool TouchingTiles(ref int i, ref int j)
		{
			for (i = (int)position.X; i < (int)(position.X + size.X); i += 16) //Todo: Break this godawful nested statement hell into its own methods
			{
				for (j = (int)position.Y; j < (int)(position.Y + size.Y); j += 16)
				{
					if (WorldGen.InWorld(i / 16, j / 16))
					{
						Tile tile = Main.tile[i / 16, j / 16];

						if (tile.HasTile && Main.tileSolid[tile.TileType] && !TileID.Sets.Platforms[tile.TileType])
							return true;
					}
				}
			}

			return false;
		}

		public void Draw(SpriteBatch spriteBatch, Texture2D tex)
		{
			var color = new Color(255, 70, 10)
			{
				A = 0
			};
			spriteBatch.Draw(tex, Center - Main.screenPosition, null, color * 0.425f, 0f, tex.Size() / 2, scale * 0.24f, SpriteEffects.None, 0f);
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

		public List<MagmaGlob> Globs = new();

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
				if (target.townNPC || !target.active || target.dontTakeDamage || target.immortal || target.friendly)
					continue;

				foreach (MagmaGlob glob in Globs)
				{
					if (glob.stoppedInEnemy && glob.enemy != target)
						continue;

					if (glob.active && Collision.CheckAABBvAABBCollision(target.position, target.Size, glob.position, glob.size))
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
					glob.Draw(Main.spriteBatch, ModContent.Request<Texture2D>(Texture + "_Glow").Value);
			}

			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return true;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.townNPC || target.friendly || target.immune[Projectile.owner] > 0)
				return false;

			foreach (MagmaGlob glob in Globs)
			{
				if (glob.stoppedInEnemy && glob.enemy != target)
					continue;

				if (glob.active)
				{
					if (Collision.CheckAABBvAABBCollision(target.position, target.Size, glob.position, glob.size))
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
					Utils.PlotTileLine(glob.Center - new Vector2(MagmaGlob.WIDTH / 2 * glob.scale, 0), glob.Center + new Vector2(MagmaGlob.WIDTH / 2 * glob.scale, 0), MagmaGlob.HEIGHT * glob.scale, DelegateMethods.CutTiles);
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			foreach (MagmaGlob glob in Globs) //TODO: Merge with canhitNPC similar code into method to reduce boilerplate
			{
				if (glob.stoppedInEnemy)
					continue;

				if (glob.active)
				{
					if (Collision.CheckAABBvAABBCollision(target.position, target.Size, glob.position, glob.size))
					{
						modifiers.SourceDamage *= 2f;
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

				Tile tile = Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16];

				if (tile.HasTile && tile.BlockType == BlockType.Solid && Main.tileSolid[tile.TileType])
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
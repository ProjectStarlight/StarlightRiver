using StarlightRiver.Content.Physics;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	class MagmiteBomb : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magmite Bomb");
			Tooltip.SetDefault("A ball of cuteness turned into an instrument of destruction\nYou monster...");
		}

		public override void SetDefaults()
		{
			Item.damage = 54;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 6;
			Item.width = 32;
			Item.height = 48;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = false;
			Item.useTurn = true;
			Item.shoot = ModContent.ProjectileType<MagmiteBombProjectile>();
			Item.shootSpeed = 8.5f;
		}

		public override void AddRecipes()
		{
			CreateRecipe().
				AddIngredient(ModContent.ItemType<MagmiteBottle>()).
				AddIngredient(ItemType<SandstoneChunk>(), 5).
				AddIngredient(ItemID.Hellstone, 5).
				Register();
		}
	}

	class MagmiteBombProjectile : ModProjectile
	{
		public const int NUM_SEGMENTS = 12;

		private Trail trail;
		private List<Vector2> cache = new();

		public VerletChain Chain;
		public bool chainUpdated = false; // tracks if chain is updated to prevent renders when AI() is not called before

		public Vector2 ChainStart => Projectile.Center + new Vector2(15, 0).RotatedBy(-MathHelper.Pi / 3 + Projectile.rotation);

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 30;
			Projectile.timeLeft = 240;
			Projectile.friendly = true;
			Projectile.damage = 60;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Chain = new VerletChain(NUM_SEGMENTS, true, ChainStart, 4, true)
			{
				forceGravity = new Vector2(0, 0.01f),
				constraintRepetitions = 10,
				drag = 1.2f,
				parent = Projectile,
				customDistances = true, // Using custom distance so it can be shortened smoothly
				segmentDistances = new List<int> {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5}
			};

			Chain.Start(); // Manually initialize chain so dust can be drawn immediately

			for (int i = 1; i < NUM_SEGMENTS; i++)
			{
				Chain.ropeSegments[i].posNow = Projectile.Center; // Avoid chain clipping into floor
			}
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.4f;
			Projectile.rotation += Projectile.velocity.X * 0.05f;

			chainUpdated = false;

			// Shorten fuse as it burns
			if (Projectile.timeLeft % 4 == 0)
			{
				for (int i = NUM_SEGMENTS - 1; i >= 0; i--)
				{
					if (Chain.segmentDistances[i] <= 0)
						continue;

					Chain.segmentDistances[i] -= 1;
					break;
				}
			}
			
			Vector2 chainEnd = Chain.ropeSegments[NUM_SEGMENTS - 1].posNow;
			Dust.NewDust(chainEnd, 0, 0, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
			Dust.NewDust(chainEnd, 0, 0, DustID.Torch, 0, 0, 0, default, 0.75f);
		}

		public override bool PreDrawExtras()
		{
			if (!chainUpdated)
			{
				Chain.UpdateChain(ChainStart);

				// Make sure rope segments after first do not intersect bomb
				for (int i = 1; i < NUM_SEGMENTS; i++)
				{
					Vector2 segPos = Chain.ropeSegments[i].posNow;
					Vector2 diff = segPos - Projectile.Center;
					if (diff.Length() < 18)
					{
						float diffFactor = (18 - diff.Length()) / diff.Length();
						Vector2 offset = diff * diffFactor;
						Chain.ropeSegments[i].posNow += offset;
					}
				}

				chainUpdated = true;
			}

			if (!Main.dedServ)
			{
				ManageCache();
				ManageTrail();
			}

			DrawChain();
			return base.PreDrawExtras();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity.X *= 0.95f;

			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y * 0.2f;

				if (Projectile.velocity.Y < -1f)
				{
					for (int i = 0; i < 5; i++)
					{
						Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(0.1f, 0.2f), Mod.Find<ModGore>("MagmiteGore").Type);
					}

					SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.position);
				}
			}

			return false;
		}

		public override void Kill(int timeLeft)
		{
			for (int k = 0; k < 60; k++)
			{
				Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-16, -1)).RotatedByRandom(0.8f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(1.0f, 1.4f));
			}

			for (int k = 0; k < 10; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.MagmaSmoke>(), (Vector2.UnitY * Main.rand.NextFloat(-3f, -2f)).RotatedByRandom(MathHelper.ToRadians(75f)), 100, Color.Black, Main.rand.NextFloat(0.7f, 0.9f));
			}

			for (int x = -10; x < 10; x++)
			{
				for (int y = -10; y < 10; y++)
				{
					Tile tile = Main.tile[(int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y];
					if (tile.HasTile && Main.tileSolid[tile.TileType] && Helpers.Helper.IsEdgeTile((int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y))
					{
						Vector2 pos = new Vector2((int)Projectile.Center.X / 16 + x, (int)Projectile.Center.Y / 16 + y) * 16 + Vector2.One * 8;

						if (!Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<MagmaBottleBurn>() && n.Center == pos))
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<MagmaBottleBurn>(), 25, 0, Projectile.owner);
						else
							Main.projectile.FirstOrDefault(n => n.active && n.type == ModContent.ProjectileType<MagmaBottleBurn>() && n.Center == pos).timeLeft = 180;
					}
				}

				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
			}

			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			SoundEngine.PlaySound(SoundID.DD2_GoblinHurt, Projectile.Center);
		}

		private void ManageCache()
		{
			cache = new List<Vector2>
			{
				ChainStart
			};

			float pointLength = TotalLength(GetChainPoints()) / NUM_SEGMENTS;
			float pointCounter = 0;
			int presision = 30; //This normalizes length between points so it doesnt squash super weirdly on certain parts

			for (int i = 0; i < NUM_SEGMENTS - 1; i++)
			{
				for (int j = 0; j < presision; j++)
				{
					pointCounter += (Chain.ropeSegments[i].posNow - Chain.ropeSegments[i + 1].posNow).Length() / presision;

					while (pointCounter > pointLength)
					{
						float lerper = j / (float)presision;
						cache.Add(Vector2.Lerp(Chain.ropeSegments[i].posNow, Chain.ropeSegments[i + 1].posNow, lerper));
						pointCounter -= pointLength;
					}
				}
			}

			while (cache.Count < NUM_SEGMENTS)
			{
				cache.Add(Chain.ropeSegments[NUM_SEGMENTS - 1].posNow);
			}

			while (cache.Count > NUM_SEGMENTS)
			{
				cache.RemoveAt(cache.Count - 1);
			}
		}

		private List<Vector2> GetChainPoints()
		{
			var points = new List<Vector2>();

			foreach (RopeSegment ropeSegment in Chain.ropeSegments)
				points.Add(ropeSegment.posNow);

			return points;
		}

		private void DrawChain()
		{
			if (trail == null || trail == default)
				return;

			Main.spriteBatch.End();

			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(1);

			trail?.Render(effect);

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, NUM_SEGMENTS, new TriangularTip(1), factor => 4, factor => Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)));

			List<Vector2> positions = cache;
			trail.NextPosition = positions[NUM_SEGMENTS - 1];

			trail.Positions = positions.ToArray();
		}

		private float TotalLength(List<Vector2> points)
		{
			float ret = 0;

			for (int i = 1; i < points.Count; i++)
			{
				ret += (points[i] - points[i - 1]).Length();
			}

			return ret;
		}
	}
}

using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class HolyAmulet : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public HolyAmulet() : base("Holy Amulet", "Releases bursts of homing energy for every 25 HP healed") { }

		public override void Load()
		{
			Terraria.On_Player.HealEffect += HealEffect;
		}

		public override void Unload()
		{
			Terraria.On_Player.HealEffect -= HealEffect;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<HolyAmuletHealingTracker>().item = Item;
		}

		private void HealEffect(Terraria.On_Player.orig_HealEffect orig, Player self, int healAmount, bool broadcast)
		{
			if (Equipped(self))
				self.GetModPlayer<HolyAmuletHealingTracker>().Healed(healAmount);

			orig(self, healAmount, broadcast);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.GoldBar, 10);
			recipe.AddIngredient(ItemID.LifeCrystal);
			recipe.AddRecipeGroup("StarlightRiver:Gems", 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.PlatinumBar, 10);
			recipe.AddIngredient(ItemID.LifeCrystal);
			recipe.AddRecipeGroup("StarlightRiver:Gems", 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class HolyAmuletHealingTracker : ModPlayer
	{
		private int cumulativeAmountHealed;

		public Item item;

		public void Healed(int amount)
		{
			cumulativeAmountHealed += amount;
		}

		public override void PreUpdate()
		{
			int amountOfProjectiles = cumulativeAmountHealed / 25;

			float step = MathHelper.TwoPi / amountOfProjectiles;

			float rotation = 0;

			while (cumulativeAmountHealed >= 25)
			{
				Projectile.NewProjectile(Player.GetSource_Accessory(item), Player.Center, Vector2.UnitX.RotatedBy(rotation) * 16, ModContent.ProjectileType<HolyAmuletOrb>(), 10, 2.5f, Player.whoAmI);

				rotation += step;

				cumulativeAmountHealed -= 25;
			}
		}
	}

	// This is basically a carbon copy of the astroflora bow. Consider making a base "TrailProjectile" to cut down on the boilerplate.
	public class HolyAmuletOrb : ModProjectile, IDrawPrimitive
	{
		private const float detectionRange = 320;         // 20 Tiles.
		private const int oldPositionCacheLength = 24;
		private const int trailMaxWidth = 4;

		private Trail trail;
		private List<Vector2> cache;

		private int TargetNPCIndex
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private bool HitATarget
		{
			get => (int)Projectile.ai[1] == 1;
			set => Projectile.ai[1] = value ? 1 : 0;
		}

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			// Sync its target.
			Projectile.netUpdate = true;

			ManageCaches();
			ManageTrail();

			if (Projectile.timeLeft < 30)
				Projectile.alpha += 8;

			if (!HitATarget && TargetNPCIndex == -1)
			{
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC NPC = Main.npc[i];

					if (NPC.CanBeChasedBy() && NPC.DistanceSQ(Projectile.Center) < detectionRange * detectionRange)
					{
						TargetNPCIndex = i;
						break;
					}
				}
			}
			else if (TargetNPCIndex != -1)
			{
				if (!Main.npc[TargetNPCIndex].CanBeChasedBy())
				{
					TargetNPCIndex = -1;
					return;
				}

				Homing(Main.npc[TargetNPCIndex]);
			}
		}

		private void Homing(NPC target)
		{
			Vector2 move = target.Center - Projectile.Center;
			AdjustMagnitude(ref move);

			Projectile.velocity = (10 * Projectile.velocity + move) / 11f;
			AdjustMagnitude(ref Projectile.velocity);
		}

		private void AdjustMagnitude(ref Vector2 vector)
		{
			float adjustment = 24;
			float magnitude = vector.Length();

			if (magnitude > adjustment)
				vector *= adjustment / magnitude;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < oldPositionCacheLength; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > oldPositionCacheLength)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, oldPositionCacheLength, new TriangularTip(trailMaxWidth * 4), factor => factor * trailMaxWidth, factor =>
			{
				// 1 = full opacity, 0 = transparent.
				float normalisedAlpha = 1 - Projectile.alpha / 255f;

				// Scales opacity with the Projectile alpha as well as the distance from the beginning of the trail.
				return Color.Crimson * normalisedAlpha * factor.X;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["Primitives"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);

			trail?.Render(effect);
		}

		public override bool? CanHitNPC(NPC target)
		{
			return TargetNPCIndex != -1 && !HitATarget && Main.npc[TargetNPCIndex] == target;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.timeLeft = 30;
			HitATarget = true;

			// This is hacky, but it lets the Projectile keep its rotation without having to make an extra variable to cache it after it hits a target and "stops".
			Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.0001f;
		}

		public override void Kill(int timeLeft)
		{
			trail?.Dispose();
		}
	}
}
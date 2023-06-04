using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TemplePick : ModItem
	{
		private int charge;

		private int direction;

		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Whirlwind Pickaxe");
			Tooltip.SetDefault("Hold right click to charge up a spinning pickaxe dash, breaking anything in your way");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Green;
			Item.pick = 45;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.damage = 8;
			Item.autoReuse = true;
			Item.channel = true;
			Item.UseSound = SoundID.Item1;
			Item.DamageType = DamageClass.Melee;
		}

		public override bool AltFunctionUse(Player Player)
		{
			return true;
		}

		public override bool CanUseItem(Player Player)
		{
			return !Spinning(Player) && charge == 0;
		}

		public override bool? UseItem(Player Player)
		{
			if (Player.altFunctionUse == 2)
			{
				Item.noUseGraphic = true;
				Item.noMelee = true;
			}
			else
			{
				Item.noUseGraphic = false;
				Item.noMelee = false;
			}

			return true;
		}

		private bool Spinning(Player player)
		{
			return Main.projectile.Any(n => n.active && n.type == ProjectileType<TemplePickProjectile>() && n.owner == player.whoAmI);
		}

		public override void UpdateInventory(Player Player)
		{
			if (Player.HeldItem == Item)
			{
				if (Main.mouseRight && charge < 120)
				{
					if (charge % 15 == 0)
					{
						var d = Dust.NewDustPerfect(Player.Center, DustType<Dusts.PickCharge>(), Vector2.UnitY.RotatedBy(charge / 120f * 6.28f) * 45, 0, new Color(255, 255, 50), 2);
						d.customData = Player.whoAmI;
					}

					if (charge == 119)
					{
						Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);

						for (int k = 0; k < 100; k++)
						{
							Dust.NewDustPerfect(Player.Center, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
						}
					}
				}

				if (!Main.mouseRight && !Spinning(Player) && charge == 120)
				{
					direction = Main.MouseWorld.X > Player.Center.X ? 1 : -1;
					Projectile.NewProjectile(Item.GetSource_FromThis(), Player.Center, Vector2.Zero, ProjectileType<TemplePickProjectile>(), 0, 0, Player.whoAmI, 80, direction);

					charge = 0;
				}
			}

			if (!Main.mouseRight && charge > 0 && !Spinning(Player) || Player.HeldItem != Item)
				charge = 0;

			if (Main.mouseRight && Player.HeldItem == Item && charge < 120)
				charge++;
		}
	}

	class TemplePickProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public ref float Timer => ref Projectile.ai[0];
		public ref float Direction => ref Projectile.ai[1];

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 999;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Timer--;

			if (Timer <= 0)
				Projectile.timeLeft = 0;

			Player Player = Main.player[Projectile.owner];

			Projectile.Center = Player.Center;

			Player.velocity.X = Direction * 8;
			Player.UpdateRotation(Timer / 20f * 6.28f);

			if (Timer % 4 == 0)
			{
				for (int k = 0; k < 3; k++)
				{
					int i = (int)(Player.Center.X / 16 + Direction);
					int j = (int)(Player.position.Y / 16) + k;
					Player.PickTile(i, j, 45);
				}
			}

			if (Timer % 10 == 0)
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item63, Player.Center);

			if (Timer > 30 && Timer % 4 == 0)
				Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Whirl>(), 0, 0, Projectile.owner);
		}
	}

	internal class Whirl : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		private float offset;
		private float vertical;
		private float radius;
		private float speed;

		public float Progress => Projectile.timeLeft / 60f;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			offset = Main.rand.NextFloat(6.28f);
			vertical = Main.rand.NextFloat(-10, 10);
			radius = Main.rand.NextFloat(1f, 1.5f);
			speed = Main.rand.Next(8, 15);
		}

		public override void AI()
		{
			Projectile.Center = Main.player[Projectile.owner].Center;
			ManageCache(ref cache);
			ManageTrail(ref trail, cache);
		}

		private void ManageCache(ref List<Vector2> cache)
		{
			if (cache == null || cache.Count < 120)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 120; i++)
				{
					cache.Add(Projectile.Center + new Vector2(0, 100));
				}
			}

			float rad = 1;

			if (Projectile.timeLeft < 10)
				rad = Projectile.timeLeft / 10f;

			if (Projectile.timeLeft > 50)
				rad = 1 - (Projectile.timeLeft - 50) / 10f;

			rad *= radius;

			for (int i = 0; i < 120; i++)
			{
				cache[i] = Projectile.Center + new Vector2((float)Math.Cos(Progress * speed + i / 60f + offset) * 48 * rad, (float)Math.Sin(Progress * speed + i / 60f + offset) * 16 * rad + vertical);
			}

			while (cache.Count > 120)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail(ref Trail trail, List<Vector2> cache)
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 120, new TriangularTip(40 * 4), factor => 5, factor =>
			{
				if (factor.X >= 0.95f)
					return Color.White * 0;

				return new Color(155, 155, 155) * (float)Math.Sin(factor.X * 3.14f) * 0.15f * radius;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			if (effect is null)
				return;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.01f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/GlowTrailNoEnd").Value);

			trail?.Render(effect);
		}
	}
}
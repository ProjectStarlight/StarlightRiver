using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using StarlightRiver.Content.Items.SpaceEvent;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Buffs;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Breacher
{
	public class SupplyBeacon : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Supply Beacon");
			Tooltip.SetDefault("Taking over 50 damage summons a supply drop \nStand near the supply drop to buff either damage, regeneration, or defense \n10 second cooldown");

		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.rare = 3;
			Item.value = Item.buyPrice(0, 4, 0, 0);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			SupplyBeaconPlayer modPlayer = Player.GetModPlayer<SupplyBeaconPlayer>();
			modPlayer.active = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(Mod);
			recipe.AddIngredient(ModContent.ItemType<Astroscrap>(), 10);
			recipe.AddIngredient(ItemID.Wire, 15);
			recipe.AddTile(TileID.Anvils);

			recipe.SetResult(this);

			recipe.AddRecipe();
		}
	}

	public class SupplyBeaconPlayer : ModPlayer
	{
		public bool active = false;

		int cooldown = 0;
		int damageTicker = 0;
		int launchCounter = 0;

		public override void ResetEffects()
		{
			active = false;
		}

		public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
			if (cooldown <= 0 && active)
			{
				damageTicker += (int)damage;
				if (damageTicker >= 50)
				{
					damageTicker = 0;
					cooldown = 600;
					launchCounter = 125;
					Helper.PlayPitched("AirstrikeIncoming", 0.6f, 0);
				}
			}
		}
		public override void PreUpdate()
		{
			cooldown--;
			if (active)
			{
				launchCounter--;
				if (launchCounter == 1)
					SummonDrop(Player);
			}
			else
				launchCounter = 0;
		}

		private static void SummonDrop(Player Player)
		{
			Vector2 direction = new Vector2(0, -1);
			direction = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));
			Projectile.NewProjectile(Player.Center + (direction * 800) + new Vector2(Main.rand.Next(-300, 300), 0), direction * -15, ModContent.ProjectileType<SupplyBeaconProj>(), 0, 0, Player.whoAmI, Main.rand.Next(3));
		}
	}
	internal class SupplyBeaconProj : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		private bool landed = false;
		private bool ableToLand = false;

		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;

		private float startupCounter = 0f;

		private int state => (int)Projectile.ai[0]; //0 is defense, 1 is healing, 2 is attack

		private Player owner => Main.player[Projectile.owner];

		private float trailAlpha => Math.Max(0, 1 - (startupCounter * 5));

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.extraUpdates = 2;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Supply Beacon");
		}

		public override void AI()
		{
			if (!landed)
			{
				if (Main.netMode != NetmodeID.Server)
					ManageCaches();


				if (Projectile.Center.Y > owner.Center.Y - 100 && !Main.tile[(int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16].HasTile)
				{
					ableToLand = true;
				}

				if (ableToLand)
					Projectile.tileCollide = true;
			}
			else
			{
				Projectile.velocity.Y += 0.3f;
				if (startupCounter < 1)
					startupCounter += 0.02f;

				Lighting.AddLight(Projectile.Center - new Vector2(0, 40 + (7 * (float)Math.Sin(Main.GlobalTime * 0.6f))), GetColor().ToVector3() * startupCounter);
				BuffPlayers();
			}

			if (Main.netMode != NetmodeID.Server)
				ManageTrail();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 position = (Projectile.Center - Main.screenPosition);
			if (landed)
			{
				Texture2D displayTex = ModContent.Request<Texture2D>(Texture + "_Display").Value;
				Texture2D symbolTex = ModContent.Request<Texture2D>(Texture + "_Symbol").Value;

				Color displayColor = GetColor();
				displayColor.A = 0;

				Color symbolColor = Color.White;

				Rectangle symbolFrame = new Rectangle(
					state * symbolTex.Width / 3,
					(Main.GlobalTime * 100) % 10 > 5 ? 0 : symbolTex.Height / 2,
					symbolTex.Width / 3,
					symbolTex.Height / 2);

				spriteBatch.Draw(displayTex, position, null, displayColor, 0, new Vector2(displayTex.Width / 2, displayTex.Height), startupCounter, SpriteEffects.None, 0);
				spriteBatch.Draw(symbolTex, position - new Vector2(0, 40 + (7 * (float)Math.Sin(Main.GlobalTime * 0.6f))), symbolFrame, symbolColor, 0, new Vector2(symbolTex.Width / 6, symbolTex.Height / 4), startupCounter, SpriteEffects.None, 0);
			}
			spriteBatch.Draw(mainTex, position, null, lightColor, Projectile.rotation, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);

			Texture2D starTex = ModContent.Request<Texture2D>(Texture + "_Star").Value;
			Color color = GetColor();
			color.A = 0;
			Color color2 = Color.White;
			color2.A = 0;
			spriteBatch.Draw(starTex, position, null,
							 color * trailAlpha * 0.33f, Projectile.rotation, starTex.Size() / 2, Projectile.scale * 2, SpriteEffects.None, 0);
			spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null,
							 color * trailAlpha, Projectile.rotation, starTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
			spriteBatch.Draw(starTex, Projectile.Center - Main.screenPosition, null,
							 color2 * trailAlpha, Projectile.rotation, starTex.Size() / 2, Projectile.scale * 0.75f, SpriteEffects.None, 0);
			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (ableToLand)
			{
				if (!landed)
				{
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42, Projectile.Center);
					landed = true;
					Projectile.timeLeft = 700;
					owner.GetModPlayer<StarlightPlayer>().Shake += 12;
				}
				Projectile.extraUpdates = 0;
				Projectile.velocity = Vector2.Zero;
			}
			return false;
		}

		private void BuffPlayers()
		{
			for (int i = 0; i < Main.player.Length; i++)
			{
				Player Player = Main.player[i];
				if (Player.active && Player.Distance(Projectile.Center) < 150 && !Player.dead)
				{
					int buffType = -1;
					switch (state)
					{
						case 0:
							buffType = ModContent.BuffType<SupplyBeaconDefense>();
							break;
						case 1:
							buffType = ModContent.BuffType<SupplyBeaconHeal>();
							break;
						case 2:
							buffType = ModContent.BuffType<SupplyBeaconDamage>();
							break;
					}
					Player.AddBuff(buffType, 2);
				}
			}
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 100; i++)
				{
					cache.Add(Projectile.Center);
				}
			}
			cache.Add(Projectile.Center);

			while (cache.Count > 100)
			{
				cache.RemoveAt(0);
			}
		}

		private Color GetColor()
		{
			switch (state)
			{
				case 0:
					return Color.Orange;
				case 1:
					return Color.Green;
				case 2:
					return Color.Red;
			}
			return Color.White;
		}

		private void ManageTrail()
		{

			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(16), factor => factor * MathHelper.Lerp(11, 22, factor), factor =>
			{
				return GetColor();
			});
			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(16), factor => factor * MathHelper.Lerp(6, 12, factor), factor =>
			{
				return Color.White;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(trailAlpha);

			trail?.Render(effect);

			trail2?.Render(effect);
		}
	}
	class SupplyBeaconDefense : SmartBuff
	{
		public SupplyBeaconDefense() : base("Supply Beacon", "Defense increased", false) { }

		public override void SafeSetDetafults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			base.SafeSetDetafults();
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Main.rand.NextBool(7))
				Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<SupplyBeaconDefenseDust>());
			Player.statDefense += 15;
		}
	}

	class SupplyBeaconHeal : SmartBuff
	{
		public SupplyBeaconHeal() : base("Supply Beacon", "Regeneration increased", false) { }

		public override void SafeSetDetafults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			base.SafeSetDetafults();
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Main.rand.NextBool(7))
				Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<SupplyBeaconHealDust>());
			Player.lifeRegen += 10;
			Player.manaRegen += 10;
		}
	}

	class SupplyBeaconDamage : SmartBuff
	{
		public SupplyBeaconDamage() : base("Supply Beacon", "Damage increased", false) { }

		public override void SafeSetDetafults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			base.SafeSetDetafults();
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Main.rand.NextBool(7))
				Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<SupplyBeaconAttackDust>());
			Player.meleeDamage += 0.2f;
			Player.rangedDamage += 0.2f;
			Player.magicDamage += 0.2f;
			Player.minionDamage += 0.2f;
		}
	}
	public class SupplyBeaconDefenseDust : ModDust
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void OnSpawn(Dust dust)
        {
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 10, 10);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White * ((255 - dust.alpha) / 255f);


		public override bool Update(Dust dust)
        {
			dust.position.Y -= 2;
			dust.alpha += 15;
			if (dust.alpha > 255)
				dust.active = false;
			return false;
        }
    }

	public class SupplyBeaconHealDust : SupplyBeaconDefenseDust { }

	public class SupplyBeaconAttackDust : SupplyBeaconDefenseDust { }
}
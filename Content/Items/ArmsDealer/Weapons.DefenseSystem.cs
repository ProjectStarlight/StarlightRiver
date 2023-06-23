using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.ArmsDealer
{
	internal class DefenseSystem : ModItem
	{
		public enum GunType
		{
			Pistol,
			Shotgun,
			Minigun
		}

		public GunType selected = GunType.Pistol;

		public float radialRotation;

		public static Texture2D PistolTex => Terraria.GameContent.TextureAssets.Item[ItemID.FlintlockPistol].Value;
		public static Texture2D ShotgunTex => Terraria.GameContent.TextureAssets.Item[ItemID.Boomstick].Value;
		public static Texture2D MinigunTex => Terraria.GameContent.TextureAssets.Item[ItemID.Minishark].Value;

		public override string Texture => AssetDirectory.Debug;

		public override void Load()
		{
			StarlightPlayer.PostDrawEvent += DrawRadial;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Arms Dealer's Defense System");

			Tooltip.SetDefault("Summons a gun-toting turret\n" +
				"Right click to cycle between different guns\n" +
				$"[i/s1:{ItemID.FlintlockPistol}] Modest damage with good range\n" +
				$"[i/s1:{ItemID.Boomstick}] Great damage with short range\n" +
				$"[i/s1:{ItemID.Minishark}] Light damage with great fire rate\n");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.value = Item.sellPrice(gold: 30);
			Item.rare = ItemRarityID.Green;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.DamageType = DamageClass.Summon;
			Item.damage = 12;
			Item.UseSound = SoundID.Item44;
			Item.shoot = 10;
			Item.shootSpeed = 1;
			Item.sentry = true;
			Item.mana = 20;
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool? UseItem(Player player)
		{
			if (player.altFunctionUse == 2)
			{
				selected = selected switch
				{
					GunType.Pistol => GunType.Shotgun,
					GunType.Shotgun => GunType.Minigun,
					GunType.Minigun => GunType.Pistol,
					_ => GunType.Pistol
				};

				return true;
			}

			return null;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = player.GetModPlayer<ControlsPlayer>().mouseWorld;

			velocity = Vector2.Zero;

			switch (selected)
			{
				case GunType.Pistol:
					type = ModContent.ProjectileType<PistolTurret>();
					break;

				case GunType.Shotgun:
					type = ModContent.ProjectileType<ShotgunTurret>();
					break;

				case GunType.Minigun:
					type = ModContent.ProjectileType<MinigunTurret>();
					break;
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse != 2)
			{
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
				player.UpdateMaxTurrets();
			}

			return false;
		}

		/// <summary>
		/// This handles drawing the radial indicator for which gun is selected
		/// </summary>
		/// <param name="player"></param>
		/// <param name="spriteBatch"></param>
		private void DrawRadial(Player player, SpriteBatch spriteBatch)
		{
			// Get the held instance, if it exists
			var held = player.HeldItem.ModItem as DefenseSystem;

			if (held is null)
				return;

			// Rotate as appropriate based on gun selection
			float rotTarget = held.selected switch
			{
				GunType.Pistol => 0,
				GunType.Shotgun => -MathHelper.Pi * 2 / 3,
				GunType.Minigun => -MathHelper.Pi * 2 / 3 * 2,
				_ => 0
			};

			held.radialRotation += Helpers.Helper.RotationDifference(rotTarget, held.radialRotation) * 0.1f;
			held.radialRotation %= 6.28f;

			// Draw all 3 guns
			for (int k = 0; k < 3; k++)
			{
				bool selected = k == (int)held.selected;
				Vector2 pos = player.Center + -Vector2.UnitY.RotatedBy(held.radialRotation + k / 3f * 6.28f) * 48 - Main.screenPosition;

				Texture2D tex = k switch
				{
					0 => PistolTex,
					1 => ShotgunTex,
					2 => MinigunTex,
					_ => null
				};

				if (tex is null)
					return;

				spriteBatch.Draw(tex, pos, null, selected ? Color.White : Color.LightGray * 0.5f, 0, tex.Size() / 2f, selected ? 1 : 0.8f, 0, 0);
			}
		}
	}

	/// <summary>
	/// Abstract class for the various turrets that can be summoned
	/// </summary>
	internal abstract class DefenseSystemTurret : ModProjectile
	{
		/// <summary>
		/// How many frames between firing this turret should wait
		/// </summary>
		public int delay;

		/// <summary>
		/// The radius of the circle around the turret it should check for targets
		/// </summary>
		public int range;

		/// <summary>
		/// The targeted NPC
		/// </summary>
		public NPC target;

		// Used for the visuals of the gun
		public float gunRotation;
		public Texture2D gunTex;

		public Player Owner => Main.player[Projectile.owner];

		public ref float Timer => ref Projectile.ai[0];
		public ref float Placed => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public DefenseSystemTurret(int delay, int range, Texture2D gunTex)
		{
			this.delay = delay;
			this.range = range;
			this.gunTex = gunTex;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gun turret");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.sentry = true;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		/// <summary>
		/// This should contain the logic that runs when a turret fires, most likely shooting bullets in some way
		/// </summary>
		/// <param name="target">Represents the direction in which bullets should be fired</param>
		public abstract void Fire(Vector2 target);

		/// <summary>
		/// Selects an NPC for the turret to target, and stores it into the target variable
		/// </summary>
		public void PickTarget()
		{
			// Check player's target first, and prioritize that if possible
			if (Owner?.HasMinionAttackTargetNPC ?? false)
			{
				NPC npc = Main.npc[Owner.MinionAttackTargetNPC];

				if (Vector2.Distance(Projectile.Center, npc.Center) <= range)
				{
					target = npc;
					return;
				}
			}

			// Else, scan for other valid NPCs
			foreach (NPC npc in Main.npc)
			{
				if (!npc.active || npc.friendly || !npc.CanBeChasedBy(this))
					continue;

				if (Vector2.Distance(Projectile.Center, npc.Center) <= range)
				{
					target = npc;
					return;
				}
			}
		}

		public sealed override void AI()
		{
			// Find sentry position
			if (Placed < 1)
			{
				Owner.FindSentryRestingSpot(Projectile.whoAmI, out int worldX, out int worldY, out int pushYUp);
				Projectile.position = new Vector2(worldX, worldY - pushYUp);

				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * 24, ModContent.DustType<Dusts.BuzzSpark>(), Main.rand.NextVector2Circular(3, 3), 0, Color.Yellow);
					Helpers.Helper.PlayPitched("Impacts/StabTiny", 0.5f, 0, Projectile.Center);
				}

				Placed = 1;
			}

			// invalidate target if out of range
			if (target != null && Vector2.Distance(target.Center, Projectile.Center) > range)
				target = null;

			// Scan for targets if we dont have one
			if (target is null || !target.active)
				PickTarget();

			// If we found one, aim at it
			if (target != null && target.active)
			{
				// Increment the timer
				Timer++;

				// Rotate the gun to aim it
				float targetAngle = (target.Center - Projectile.Center).ToRotation();
				gunRotation += Helpers.Helper.CompareAngle(targetAngle, gunRotation) * 0.075f;

				// If the delay time has passed, fire a shot and reset
				if (Timer >= delay)
				{
					Fire(Vector2.UnitX.RotatedBy(gunRotation));
					Timer = 0;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D baseTex = ModContent.Request<Texture2D>(AssetDirectory.Debug).Value;

			SpriteEffects effects = SpriteEffects.None;

			if (Vector2.UnitX.RotatedBy(gunRotation).X < 0)
				effects = SpriteEffects.FlipVertically;

			Main.spriteBatch.Draw(baseTex, Projectile.Center - Main.screenPosition, null, lightColor, 0, baseTex.Size() / 2f, 1, 0, 0);
			Main.spriteBatch.Draw(gunTex, Projectile.Center - Main.screenPosition, null, lightColor, gunRotation, gunTex.Size() / 2f, 1, effects, 0);

			return false;
		}
	}

	/// <summary>
	/// The pistol turret, which has a modest delay and damage at good range
	/// </summary>
	internal class PistolTurret : DefenseSystemTurret
	{
		public PistolTurret() : base(34, 1200, DefenseSystem.PistolTex) { }

		public override void Fire(Vector2 target)
		{
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, target * 14, ProjectileID.Bullet, 25, 1, Projectile.owner);
			SoundEngine.PlaySound(SoundID.Item11, Projectile.Center);
		}
	}

	/// <summary>
	/// The shotgun turret, which slowly fires large bursts of damage
	/// </summary>
	internal class ShotgunTurret : DefenseSystemTurret
	{
		public ShotgunTurret() : base(67, 400, DefenseSystem.ShotgunTex) { }

		public override void Fire(Vector2 target)
		{
			for (int k = 0; k < 8; k++)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, target.RotatedByRandom(0.3f) * Main.rand.NextFloat(6, 8), ProjectileID.Bullet, 11, 1, Projectile.owner);
			}

			SoundEngine.PlaySound(SoundID.Item36, Projectile.Center);
		}
	}

	/// <summary>
	/// The minigun turret, which rapidly fires low damage bullets
	/// </summary>
	internal class MinigunTurret : DefenseSystemTurret
	{
		public MinigunTurret() : base(9, 600, DefenseSystem.MinigunTex) { }

		public override void Fire(Vector2 target)
		{
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, target.RotatedByRandom(0.05f) * 8, ProjectileID.Bullet, 7, 1, Projectile.owner);
			SoundEngine.PlaySound(SoundID.Item11, Projectile.Center);
		}
	}
}

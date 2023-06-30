using StarlightRiver.Helpers;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Forest
{
	public class SlimePrinceMinion : ModProjectile
	{
		public const int MAX_LIFE = 400;

		public int life = MAX_LIFE;

		public Vector2 startPoint;
		public NPC target;

		public Player Owner => Main.player[Projectile.owner];
		public bool Merged => State == 3;

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];
		public ref float HealTimer => ref Projectile.ai[2];

		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slime Prince");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.timeLeft = 2;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Timer++;
			HealTimer++;

			Projectile.timeLeft = 2;

			switch (State)
			{
				case 0: PassiveBehavior(); break;
				case 1: AttackBehavior(); break;
				case 2: FuseAnimation(); break;
				case 3: FusedBehavior(); break;
			}
		}

		/// <summary>
		/// Behavior this minion should follow when they are idle, following the player
		/// and trying to keep up with them.
		/// </summary>
		public void PassiveBehavior()
		{
			if (HealTimer % 2 == 0 && life < MAX_LIFE)
				life++;

			Vector2 targetPos = Owner.Center + new Vector2(32 * Owner.direction * -1, -32);

			Projectile.velocity = (targetPos - Projectile.Center) * 0.1f;

			if (Projectile.velocity.Length() > 15)
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 14.99f;

			// Check target
			target = Projectile.TargetStrongestNPC(800, true, true);

			// Switch to attack mode if applicable
			if (target != null)
			{
				State = 1;
				Timer = 0;
			}
		}

		/// <summary>
		/// Behavior the minion should follow when it has an active, valid target and is 
		/// attacking them passively.
		/// </summary>
		public void AttackBehavior()
		{
			if (HealTimer % 2 == 0 && life < MAX_LIFE)
				life++;

			// If we need to retarget, do so and reset the attack
			if (!Projectile.TargetValid(800, true, true, target))
			{
				target = Projectile.TargetStrongestNPC(800, true, true);
				Timer = 0;
			}

			// return to idle if no target
			if (target is null)
			{
				State = 0;
				Timer = 0;
				return;
			}

			// Otherwise, we follow the attack cycle
			if (Timer == 1)
			{
				startPoint = Projectile.Center;
			}

			// Position above the target
			if (Timer > 1 && Timer < 30)
			{
				Projectile.velocity *= 0;
				Vector2 targetPos = target.Center + Vector2.UnitY * (target.height * -0.5f - 64);
				Projectile.Center = Vector2.SmoothStep(startPoint, targetPos, Timer / 30f);
			}

			// Fall onto them
			if (Timer > 30 && Timer < 90)
			{
				Projectile.velocity.Y += 0.4f;
				Projectile.Center = new Vector2(target.Center.X, Projectile.Center.Y);

				// Bounce off the enemy once it's been stomped on
				if (Projectile.Hitbox.Intersects(target.Hitbox))
				{
					Projectile.velocity.Y = -7;
					Projectile.velocity.X = Main.rand.NextFloat(-5, 5);
					Timer = 90;
				}
			}

			// Continue to experience gravity, albeit reduced
			if (Timer >= 90)
				Projectile.velocity.Y += 0.2f;

			// Fire a thorn at them
			if (Timer == 110)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.Center.DirectionTo(target.Center) * 11, ModContent.ProjectileType<SlimeThorn>(), 20, 0.5f, Projectile.owner);
				SoundEngine.PlaySound(SoundID.DD2_DrakinShot, Projectile.Center);
			}

			// Do it all over again
			if (Timer >= 120)
				Timer = 0;
		}

		/// <summary>
		/// The animation for when the minion is fusing with the player via the set bonus
		/// </summary>
		public void FuseAnimation()
		{
			if (Timer == 1)
				startPoint = Projectile.Center;

			if (Timer > 1 && Timer <= 60)
				Projectile.Center = Vector2.SmoothStep(startPoint, Owner.Center + Vector2.UnitY * Owner.gfxOffY, Timer / 60f);

			if (Timer == 60)
			{
				State = 3;
				Timer = 0;
			}
		}

		/// <summary>
		/// Behavior the minion should follow when it is fused to the player, via the set
		/// bonus. This should stick hard to the player and constantly spawn spikes.
		/// </summary>
		public void FusedBehavior()
		{
			Projectile.velocity *= 0;
			Projectile.Center = Owner.Center + Vector2.UnitY * Owner.gfxOffY;

			life--;

			if (life <= 0)
			{
				State = 0;
				life = 0;
				Timer = 0;
			}
		}

		public override void PostDraw(Color lightColor)
		{
			float fill = life / (float)MAX_LIFE;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "SmallBar1").Value;
			Texture2D tex2 = ModContent.Request<Texture2D>(AssetDirectory.GUI + "SmallBar0").Value;

			var pos = (Projectile.Center + new Vector2(-tex.Width / 2, -50) + Vector2.UnitY * Projectile.height / 2f - Main.screenPosition).ToPoint();
			var target = new Rectangle(pos.X, pos.Y, (int)(fill * tex.Width), tex.Height);
			var source = new Rectangle(0, 0, (int)(fill * tex.Width), tex.Height);
			var target2 = new Rectangle(pos.X, pos.Y + 2, tex2.Width, tex2.Height);
			var color = Vector3.Lerp(Color.Red.ToVector3(), Color.Green.ToVector3(), fill);

			Main.spriteBatch.Draw(tex2, target2, new Color(40, 40, 40));
			Main.spriteBatch.Draw(tex, target, source, new Color(color.X, color.Y, color.Z));
		}
	}

	public class SlimeThorn : ModProjectile
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.timeLeft = 120;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;

			Projectile.velocity.Y += 0.01f;
			Dust.NewDust(Projectile.position, 16, 16, DustID.t_Slime, 0, 0, 0, Color.Purple * 0.5f);
		}
	}
}

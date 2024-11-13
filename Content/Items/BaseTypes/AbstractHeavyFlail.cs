using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BaseTypes
{
	internal abstract class AbstractHeavyFlail : ModItem
	{
		public abstract int ProjType { get; }

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.useAnimation = 10;
			Item.useTime = 10;
			Item.channel = true;
			Item.shootSpeed = 12;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Swing;

			Item.shoot = ProjType;
		}

		public override bool CanShoot(Player player)
		{
			return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjType && (n.ModProjectile as AbstractHeavyFlailProjectile).State < 4);
		}
	}

	internal abstract class AbstractHeavyFlailProjectile : ModProjectile
	{
		public abstract Asset<Texture2D> ChainAsset { get; }
		protected Asset<Texture2D> BallAsset;

		readonly List<Vector2> chainPos = [];
		readonly List<Vector2> chainTarget = [];

		protected bool slowing;

		/// <summary>
		/// The maximum length this flail can extend out to if it has room.
		/// </summary>
		public virtual int MaxLength { get; set; } = 200;

		/// <summary>
		/// The amount of times alternate chain styles should be repeated at the end of the chain.
		/// </summary>
		public virtual int AlternateStyleRepeats => 3;

		public ref float Timer => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];
		public ref float Length => ref Projectile.ai[2];

		protected Player Owner => Main.player[Projectile.owner];

		/// <summary>
		/// What the flail should do when it impacts tiles or an enemy
		/// </summary>
		public virtual void OnImpact(bool wasTile) { }

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 180;
			Projectile.penetrate = -1;

			BallAsset = ModContent.Request<Texture2D>(Texture);
		}

		public override void AI()
		{
			//Owner.itemAnimation = 10;

			if (Owner.channel)
			{
				Projectile.timeLeft = 180;
			}
			else if (State != 4)
			{
				Projectile.velocity *= 0.5f;
				Timer = 0;
				State = 4;
			}

			if (State == 0) // Extending out
			{
				Projectile.velocity *= 0.99f;
				Projectile.velocity.Y += 0.6f;

				if (Vector2.Distance(Owner.Center, Projectile.Center) >= MaxLength)
				{
					Projectile.velocity *= 0;
					Length = MaxLength;
					State = 1;
				}
			}
			else if (State == 1) // Establish chain
			{
				int segments = (int)(Length / ChainAsset.Width());

				if (segments <= 2)
				{
					State = 4;
					return;
				}

				for (int k = 0; k < segments; k++)
				{
					chainPos.Add(Vector2.Lerp(Owner.Center, Projectile.Center, k / (float)segments));
					chainTarget.Add(Vector2.Lerp(Owner.Center, Projectile.Center, k / (float)segments));
				}

				State = Projectile.Center.X < Owner.Center.X ? 3 : 2;
			}
			else if (State == 2 || State == 3) // Swinging
			{
				// Accelerate if we're not past the far point yet
				if (State == 2 && Projectile.Center.X > Owner.Center.X - Length * 0.9f || State == 3 && Projectile.Center.X < Owner.Center.X + Length * 0.9f)
				{
					if (Timer < 60)
						Timer++;
				}
				else // We're past the far point
				{
					slowing = true;
				}

				// Set velocity based on if were past the far point or not
				if (!slowing)
					Projectile.velocity = Projectile.DirectionTo(Owner.Center).RotatedBy(1.57f * (State == 2 ? 1 : -1)) * (Timer / 60f) * 46.7f;
				else
					Projectile.velocity.X *= 0.8f;

				// Update chains
				for (int k = 0; k < chainPos.Count; k++)
				{
					chainTarget[k] = Vector2.Lerp(Owner.Center, Projectile.Center, k / (float)chainPos.Count);
					chainTarget[k] += Projectile.velocity * (float)Math.Sin(k / (float)chainPos.Count * 3.14f) * Length / 150f;
				}

				// Correct distance
				if (Vector2.Distance(Owner.Center, Projectile.Center) > Length)
					Projectile.Center = Owner.Center + Owner.Center.DirectionTo(Projectile.Center) * Length;

				// Force side switch if going too far
				if (State == 2 && slowing && Math.Abs(Projectile.velocity.X) < 0.01f)
					SwitchSides();

				if (State == 3 && slowing && Math.Abs(Projectile.velocity.X) < 0.01f)
					SwitchSides();

				// Set player animations
				Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.Center.DirectionTo(chainPos[1]).ToRotation() - 1.57f);
				Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Owner.Center.DirectionTo(chainPos[1]).ToRotation() - 1.57f - 0.1f);
				Owner.direction = Projectile.Center.X > Owner.Center.X ? 1 : -1;
			}
			else if (State == 4) // Retract
			{
				Projectile.Center = chainPos.Count > 0 ? chainPos[^1] : Owner.Center;

				for (int k = 0; k < chainPos.Count; k++)
				{
					chainTarget[k] = Owner.Center;
				}

				if (chainPos.Count > 0)
				{
					chainPos.RemoveAt(chainPos.Count - 1);
					chainTarget.RemoveAt(chainTarget.Count - 1);
				}
				else
				{
					Projectile.timeLeft = 0;
				}
			}

			// Update chain
			for (int k = 0; k < chainPos.Count; k++)
			{
				chainPos[k] += (chainTarget[k] - chainPos[k]) * Math.Min(1f, Timer / 10f);
			}
		}

		private void SwitchSides()
		{
			Projectile.velocity *= 0;

			Timer = 0;
			slowing = false;
			State = State == 2 ? 3 : 2;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (State == 0)
			{
				OnImpact(true);
				Projectile.velocity *= 0;
				Length = Vector2.Distance(Owner.Center, Projectile.Center);
				State = 1;
			}
			else if (State == 2 || State == 3)
			{
				if (oldVelocity.Length() >= 2)
					OnImpact(true);

				SwitchSides();
			}
			else if (State == 4)
			{
				if (Math.Abs(Projectile.velocity.Y) > 0.1f)
					Projectile.velocity.Y *= -0.95f;
				else
					Projectile.velocity.Y *= 0;
			}

			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			projHitbox.Inflate(30, 30);

			if (targetHitbox.Intersects(projHitbox))
				return true;

			for (int k = 0; k < chainPos.Count; k += 4)
			{
				if (k < chainPos.Count)
				{
					var hitbox = new Rectangle((int)chainPos[k].X - 4, (int)chainPos[k].Y - 4, 8, 8);

					if (hitbox.Intersects(targetHitbox))
						return true;
				}
			}

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Rectangle ball = Projectile.Hitbox;
			ball.Inflate(30, 30);

			if (ball.Intersects(target.Hitbox))
				OnImpact(false);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			int extraStyles = (ChainAsset.Height() - 44) / 22;

			if (State == 0)
			{
				int max = (int)(Vector2.Distance(Owner.Center, Projectile.Center) / ChainAsset.Width());

				for (int k = 0; k < max; k++)
				{
					var pos = Vector2.Lerp(Owner.Center, Projectile.Center, k / (float)max);
					Rectangle source = new Rectangle(0, 0, 8, 22);

					if (k > 1)
					{
						source.Y += 22;

						if (extraStyles > 0 && (k + 3) >= (max - extraStyles * AlternateStyleRepeats))
							source.Y += 22 * (extraStyles - (max - (k + 3)) / AlternateStyleRepeats);
					}

					Main.EntitySpriteDraw(ChainAsset.Value, pos - Main.screenPosition, source, new Color(Lighting.GetSubLight(pos)), Owner.Center.DirectionTo(pos).ToRotation() - 1.57f, new Vector2(4, 11), 1f, 0, 0);
				}
			}

			for (int k = 1; k < chainPos.Count; k++)
			{
				Vector2 prev = chainPos[k - 1];
				Vector2 curr = chainPos[k];

				Rectangle source = new Rectangle(0, 0, 8, 22);

				if (k > 1)
				{
					source.Y += 22;

					if (extraStyles > 0 && (k + 3) >= (chainPos.Count - extraStyles * AlternateStyleRepeats))
						source.Y += 22 * (extraStyles - (chainPos.Count - (k + 3)) / AlternateStyleRepeats);
				}

				Main.EntitySpriteDraw(ChainAsset.Value, curr - Main.screenPosition, source, new Color(Lighting.GetSubLight(curr)), curr.DirectionTo(prev).ToRotation() - 1.57f, new Vector2(4, 11), 1f, 0, 0);
			}

			Vector2 ballPos = Projectile.oldPosition + Projectile.Size / 2f;
			Main.EntitySpriteDraw(BallAsset.Value, ballPos - Main.screenPosition, null, lightColor, Projectile.rotation, BallAsset.Size() / 2f, Projectile.scale, 0, 0);

			return false;
		}
	}
}
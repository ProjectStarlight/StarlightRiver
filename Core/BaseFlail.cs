using System;
using System.IO;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Core
{
	public abstract class BaseFlailItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.DamageType = DamageClass.Melee;
			Item.channel = true;
			Item.UseSound = SoundID.Item19;
			SafeSetDefaults();
		}

		public virtual void SafeSetDefaults() { }

		public override bool CanUseItem(Player Player)
		{
			return Player.ownedProjectileCounts[Item.shoot] == 0;
		}
	}

	public abstract class BaseFlailProj : ModProjectile
	{
		internal bool released = false;
		internal bool falling = false;
		internal bool strucktile = false;

		private float Timer
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private float ChargeTime
		{
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public float MaxChargeTime;
		public Vector2 SpeedMult;
		public Vector2 DamageMult;
		public float spinningdistance;
		public float degreespertick;

		public BaseFlailProj(Vector2 SpeedMult, Vector2 DamageMult, float MaxChargeTime = 2, float spinningdistance = 50, float degreespertick = 10)
		{
			this.SpeedMult = SpeedMult;
			this.DamageMult = DamageMult;
			this.MaxChargeTime = MaxChargeTime;
			this.spinningdistance = spinningdistance;
			this.degreespertick = degreespertick;
		}

		public override void SetDefaults()
		{
			Projectile.Size = new Vector2(34, 34);
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(released);
			writer.Write(falling);
			writer.Write(strucktile);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			released = reader.ReadBoolean();
			falling = reader.ReadBoolean();
			strucktile = reader.ReadBoolean();
		}

		public override void AI()
		{
			Player Owner = Main.player[Projectile.owner];
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;
			Owner.heldProj = Projectile.whoAmI;

			if (!Owner.channel && !released) //check to see if Player stops channelling
			{
				released = true;
				Timer = 0;
				Projectile.netUpdate = true;
			}

			if (Owner.channel && !released) //spinning around the Player
			{
				Owner.itemRotation = 0;
				Projectile.velocity = Vector2.Zero;
				Projectile.tileCollide = false;
				Projectile.rotation = Projectile.AngleFrom(Owner.MountedCenter);

				if (++Timer % 20 == 0)
					SoundEngine.PlaySound(SoundID.Item19 with { Volume = 0.5f, PitchVariance = 0.1f }, Projectile.Center);

				ChargeTime = MathHelper.Clamp(Timer / 60, MaxChargeTime / 6, MaxChargeTime);

				float radians = MathHelper.ToRadians(degreespertick * Timer * (1 + ChargeTime / MaxChargeTime)) * Owner.direction;
				float distfromPlayer = spinningdistance * ((float)Math.Abs(Math.Cos(radians) / 5) + 0.8f); //use a cosine function based on the amount of rotation the flail has gone through to create an ellipse-like pattern
				Vector2 spinningoffset = new Vector2(distfromPlayer, 0).RotatedBy(radians);
				Projectile.Center = Owner.MountedCenter + spinningoffset;

				if (Owner.whoAmI == Main.myPlayer)
					Owner.ChangeDir(Math.Sign(Main.MouseWorld.X - Owner.Center.X));

				SpinExtras(Owner);
			}
			else
			{
				Projectile.rotation += Projectile.velocity.X * 0.03f;
				Owner.ChangeDir(Math.Sign(Projectile.Center.X - Owner.Center.X));
				Owner.itemRotation = MathHelper.WrapAngle(Projectile.AngleFrom(Owner.MountedCenter) - ((Owner.direction < 0) ? MathHelper.Pi : 0));
			}

			float launchspeed = Owner.HeldItem.shootSpeed * MathHelper.Lerp(SpeedMult.X, SpeedMult.Y, ChargeTime / MaxChargeTime);

			if (released && !falling) //basic flail launch, returns after a while
			{
				Projectile.tileCollide = true;

				if (++Timer == 1 && Owner.whoAmI == Main.myPlayer)
				{
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item19, Projectile.Center);
					Projectile.Center = Owner.MountedCenter;
					Projectile.velocity = Owner.DirectionTo(Main.MouseWorld) * launchspeed;
					OnLaunch(Owner);
				}

				if (Timer >= MathHelper.Min(60 * (ChargeTime / MaxChargeTime), 30)) //max out on time halfway through charge
					Return(launchspeed, Owner);
				else
					LaunchExtras(Owner);

				if (Owner.controlUseItem)
				{
					Projectile.netUpdate = true;
					falling = true;
					Timer = 0;
				}
			}

			if (falling) //falling towards ground, returns after hitting ground
			{
				if (strucktile || ++Timer >= 180)
				{
					Return(launchspeed, Owner);
				}
				else
				{
					FallingExtras(Owner);
					Projectile.tileCollide = true;

					if (Projectile.velocity.Y < 16f)
						Projectile.velocity.Y += 0.5f;

					Projectile.velocity.X *= 0.98f;
				}
			}
		}

		private void Return(float launchspeed, Player Owner)
		{
			Projectile.tileCollide = false;
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Owner.Center) * launchspeed * 1.5f, 0.07f);

			if (Projectile.Hitbox.Intersects(Owner.Hitbox))
				Projectile.Kill();

			ReturnExtras(Owner);
		}

		#region extra hooks
		public virtual void SpinExtras(Player Player) { }

		public virtual void NotSpinningExtras(Player Player) { }

		public virtual void OnLaunch(Player Player) { }

		public virtual void LaunchExtras(Player Player) { NotSpinningExtras(Player); }

		public virtual void FallingExtras(Player Player) { NotSpinningExtras(Player); }

		public virtual void ReturnExtras(Player Player) { NotSpinningExtras(Player); }

		public virtual void SafeTileCollide(Vector2 oldVelocity) { }

		public virtual void FallingTileCollide(Vector2 oldVelocity) { }
		#endregion

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (ChargeTime > 0)
				damage = (int)(damage * MathHelper.Lerp(DamageMult.X, DamageMult.Y, ChargeTime / MaxChargeTime));
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (falling)
			{
				strucktile = true;
				FallingTileCollide(oldVelocity);
			}

			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			Projectile.velocity = new Vector2((Projectile.velocity.X != Projectile.oldVelocity.X) ?
				-Projectile.oldVelocity.X / 5 : Projectile.velocity.X,
				(Projectile.velocity.Y != Projectile.oldVelocity.Y) ?
				-Projectile.oldVelocity.Y / 5 : Projectile.velocity.Y);
			SafeTileCollide(oldVelocity);
			Timer = 30;

			return false;
		}

		public override bool PreDrawExtras()
		{
			Texture2D ChainTexture = ModContent.Request<Texture2D>(Texture + "_chain").Value;
			Player Owner = Main.player[Projectile.owner];
			int timestodrawchain = Math.Max((int)(Projectile.Distance(Owner.MountedCenter) / ChainTexture.Width), 1);

			for (int i = 0; i < timestodrawchain; i++)
			{
				var chaindrawpos = Vector2.Lerp(Owner.MountedCenter, Projectile.Center, i / (float)timestodrawchain);
				float scaleratio = Projectile.Distance(Owner.MountedCenter) / ChainTexture.Width / timestodrawchain;
				var chainscale = new Vector2(scaleratio, 1);
				Color lightColor = Lighting.GetColor((int)chaindrawpos.X / 16, (int)chaindrawpos.Y / 16);
				Main.spriteBatch.Draw(ChainTexture, chaindrawpos - Main.screenPosition, null, lightColor, Projectile.AngleFrom(Owner.MountedCenter), new Vector2(0, ChainTexture.Height / 2), chainscale, SpriteEffects.None, 0);
			}

			return true;
		}
	}
}
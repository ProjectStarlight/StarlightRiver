using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public abstract class BaseFlailItem : ModItem
	{
		public override void SetDefaults()
		{
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noUseGraphic = true;
			item.melee = true;
			item.channel = true;
			item.UseSound = SoundID.Item19;
			SafeSetDefaults();
		}

		public virtual void SafeSetDefaults() { }

		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[item.shoot] == 0;
	}

	public abstract class BaseFlailProj : ModProjectile
	{
		internal bool released = false;
		internal bool falling = false;
		internal bool strucktile = false;

		private float Timer
		{
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		private float ChargeTime
		{
			get => projectile.ai[1];
			set => projectile.ai[1] = value;
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
			projectile.netUpdate = true;
		}

		public override void SetDefaults()
		{
			projectile.Size = new Vector2(34, 34);
			projectile.friendly = true;
			projectile.melee = true;
			projectile.penetrate = -1;
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
			Player Owner = Main.player[projectile.owner];
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;
			Owner.heldProj = projectile.whoAmI;
			if (!Owner.channel && !released) //check to see if player stops channelling
			{
				released = true;
				Timer = 0;
				projectile.netUpdate = true;
			}

			if (Owner.channel && !released) //spinning around the player
			{
				Owner.itemRotation = 0;
				projectile.velocity = Vector2.Zero;
				projectile.tileCollide = false;
				projectile.rotation = projectile.AngleFrom(Owner.MountedCenter);
				if (++Timer % 20 == 0)
					Main.PlaySound(new LegacySoundStyle(SoundID.Item, 19).WithPitchVariance(0.1f).WithVolume(0.5f), projectile.Center);

				ChargeTime = MathHelper.Clamp(Timer / 60, MaxChargeTime / 6, MaxChargeTime);

				float radians = MathHelper.ToRadians(degreespertick * Timer * (1 + ChargeTime/MaxChargeTime)) * Owner.direction;
				float distfromplayer = spinningdistance * ((float)Math.Abs(Math.Cos(radians) / 5) + 0.8f); //use a cosine function based on the amount of rotation the flail has gone through to create an ellipse-like pattern
				Vector2 spinningoffset = new Vector2(distfromplayer, 0).RotatedBy(radians);
				projectile.Center = Owner.MountedCenter + spinningoffset;
				if (Owner.whoAmI == Main.myPlayer)
					Owner.ChangeDir(Math.Sign(Main.MouseWorld.X - Owner.Center.X));

				SpinExtras(Owner);
			}

			else
			{
				projectile.rotation += projectile.velocity.X * 0.03f;
				Owner.ChangeDir(Math.Sign(projectile.Center.X - Owner.Center.X));
				Owner.itemRotation = MathHelper.WrapAngle(projectile.AngleFrom(Owner.MountedCenter) - ((Owner.direction < 0) ? MathHelper.Pi : 0));
			}

			float launchspeed = Owner.HeldItem.shootSpeed * MathHelper.Lerp(SpeedMult.X, SpeedMult.Y, ChargeTime / MaxChargeTime);
			if (released && !falling) //basic flail launch, returns after a while
			{
				projectile.tileCollide = true;
				if(++Timer == 1 && Owner.whoAmI == Main.myPlayer)
				{
					Main.PlaySound(SoundID.Item19, projectile.Center);
					projectile.Center = Owner.MountedCenter;
					projectile.velocity = Owner.DirectionTo(Main.MouseWorld) * launchspeed;
					OnLaunch(Owner);
				}

				if (Timer >= MathHelper.Min(60 * (ChargeTime / MaxChargeTime), 30)) //max out on time halfway through charge
					Return(launchspeed, Owner);

				else
					LaunchExtras(Owner);

				if (Owner.controlUseItem)
				{
					projectile.netUpdate = true;
					falling = true;
					Timer = 0;
				}
			}

			if(falling) //falling towards ground, returns after hitting ground
			{
				if(strucktile || ++Timer >= 180)
					Return(launchspeed, Owner);

				else
				{
					FallingExtras(Owner);
					projectile.tileCollide = true;
					if(projectile.velocity.Y < 16f)
						projectile.velocity.Y += 0.5f;

					projectile.velocity.X *= 0.98f;
				}
			}
		}

		private void Return(float launchspeed, Player Owner)
		{
			projectile.tileCollide = false;
			projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.DirectionTo(Owner.Center) * launchspeed * 1.5f, 0.07f);
			if (projectile.Hitbox.Intersects(Owner.Hitbox))
				projectile.Kill();
			ReturnExtras(Owner);
		}

        #region extra hooks
        public virtual void SpinExtras(Player player) { }

		public virtual void NotSpinningExtras(Player player) { }

		public virtual void OnLaunch(Player player) { }

		public virtual void LaunchExtras(Player player) { NotSpinningExtras(player); }

		public virtual void FallingExtras(Player player) { NotSpinningExtras(player); }

		public virtual void ReturnExtras(Player player) { NotSpinningExtras(player); }

		public virtual void SafeTileCollide(Vector2 oldVelocity) { }

		public virtual void FallingTileCollide(Vector2 oldVelocity) { }
		#endregion

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(ChargeTime > 0)
				damage = (int)(damage * MathHelper.Lerp(DamageMult.X, DamageMult.Y, ChargeTime / MaxChargeTime));
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (falling)
			{
				strucktile = true;
				FallingTileCollide(oldVelocity);
			}
			Main.PlaySound(SoundID.Dig, projectile.Center);
			Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
			projectile.velocity = new Vector2((projectile.velocity.X != projectile.oldVelocity.X) ?
				-projectile.oldVelocity.X / 5 : projectile.velocity.X,
				(projectile.velocity.Y != projectile.oldVelocity.Y) ?
				-projectile.oldVelocity.Y / 5 : projectile.velocity.Y);
			SafeTileCollide(oldVelocity);
			Timer = 30;
			return false;
		}

		public override bool PreDrawExtras(SpriteBatch spriteBatch)
		{
			Texture2D ChainTexture = mod.GetTexture(Texture.Remove(0, mod.Name.Length + 1) + "_chain");
			Player Owner = Main.player[projectile.owner];
			int timestodrawchain = Math.Max((int)(projectile.Distance(Owner.MountedCenter) / ChainTexture.Width), 1);
			for (int i = 0; i < timestodrawchain; i++)
			{
				Vector2 chaindrawpos = Vector2.Lerp(Owner.MountedCenter, projectile.Center, (i / (float)timestodrawchain));
				float scaleratio = projectile.Distance(Owner.MountedCenter) / ChainTexture.Width / timestodrawchain;
				Vector2 chainscale = new Vector2(scaleratio, 1);
				Color lightColor = Lighting.GetColor((int)chaindrawpos.X / 16, (int)chaindrawpos.Y / 16);
				spriteBatch.Draw(ChainTexture, chaindrawpos - Main.screenPosition, null, lightColor, projectile.AngleFrom(Owner.MountedCenter), new Vector2(0, ChainTexture.Height / 2), chainscale, SpriteEffects.None, 0);
			}
			return true;
		}
	}
}

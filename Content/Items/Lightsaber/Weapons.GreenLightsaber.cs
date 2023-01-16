using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberProj_Green : LightsaberProj
	{
		bool jumped = false;

		private int soundTimer = 0;

		protected override Vector3 BladeColor => Color.Green.ToVector3();

		protected override void RightClickBehavior()
		{
			if (!jumped)
			{
				jumped = true;
				Owner.velocity = Owner.DirectionTo(Main.MouseWorld) * 20;
				Owner.GetModPlayer<LightsaberPlayer>().jumping = true;
			}

			if (soundTimer++ % 100 == 0)
			{
				hit = new List<NPC>();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Owner.Center);
			}

			Owner.itemTime = Owner.itemAnimation = 2;
			Projectile.timeLeft = 200;
			afterImageLength = 30;
			Projectile.rotation += 0.06f * Owner.direction;
			rotVel = 0.02f;
			midRotation = Owner.velocity.ToRotation();
			squish = 0.7f;
			hide = false;
			canHit = true;
			anchorPoint = Owner.Center - Main.screenPosition;
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Owner.heldProj = Projectile.whoAmI;

			if (!Owner.GetModPlayer<LightsaberPlayer>().jumping)
			{
				CameraSystem.shake += 10;

				for (int i = 0; i < 30; i++)
					Dust.NewDustPerfect(Owner.Bottom, ModContent.DustType<LightsaberGlow>(), Main.rand.NextVector2Circular(10, 10), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(1.95f, 2.35f));

				Projectile.active = false;

				Tile tile = Main.tile[(Owner.Bottom / 16 + new Vector2(0, 1)).ToPoint()];
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX, ModContent.ProjectileType<GreenLightsaberShockwave>(), (int)(Projectile.damage * 1.3f), 0, Owner.whoAmI, 0, 10);
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.UnitX * -1, ModContent.ProjectileType<GreenLightsaberShockwave>(), (int)(Projectile.damage * 1.3f), 0, Owner.whoAmI, tile.TileType, -10);

				var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Owner.Bottom, Vector2.Zero, ModContent.ProjectileType<LightsaberImpactRing>(), 0, 0, Owner.whoAmI, 160, 1.57f);
				(proj.ModProjectile as LightsaberImpactRing).outerColor = new Color(BladeColor.X, BladeColor.Y, BladeColor.Z);
				(proj.ModProjectile as LightsaberImpactRing).ringWidth = 40;
				(proj.ModProjectile as LightsaberImpactRing).timeLeftStart = 50;
				(proj.ModProjectile as LightsaberImpactRing).additive = true;
				proj.timeLeft = 50;
			}
		}
	}

	class GreenLightsaberShockwave : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		private int TileType => (int)Projectile.ai[0];
		private int ShockwavesLeft => (int)Projectile.ai[1];//Positive and Negitive

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shockwave");
		}

		private bool createdLight = false;

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 1060;
			Projectile.tileCollide = true;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.idStaticNPCHitCooldown = 20;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.extraUpdates = 5;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			if (Projectile.timeLeft > 1000)
			{
				if (Projectile.timeLeft < 1002 && Projectile.timeLeft > 80)
					Projectile.Kill();

				Projectile.velocity.Y = 4f;
			}
			else
			{
				Projectile.velocity.Y = Projectile.timeLeft <= 10 ? 1f : -1f;

				if (Projectile.timeLeft == 19 && Math.Abs(ShockwavesLeft) > 0)
				{
					var proj = Projectile.NewProjectileDirect(Terraria.Entity.InheritSource(Projectile), new Vector2((int)Projectile.Center.X / 16 * 16 + 16 * Math.Sign(ShockwavesLeft)
					, (int)Projectile.Center.Y / 16 * 16 - 32),
					Vector2.Zero, Projectile.type, Projectile.damage, 0, Main.myPlayer, TileType, Projectile.ai[1] - Math.Sign(ShockwavesLeft));
					proj.extraUpdates = (int)(Math.Abs(ShockwavesLeft) / 3f);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft < 21)
				Main.spriteBatch.Draw(TextureAssets.Tile[TileType].Value, Projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.timeLeft > 800)
			{
				var point = new Point16((int)((Projectile.Center.X + Projectile.width / 3f * Projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)(Projectile.Center.Y / 16) + 1));
				Tile tile = Framing.GetTileSafely(point.X, point.Y);

				if (!createdLight)
				{
					createdLight = true;
					Dust.NewDustPerfect(point.ToVector2() * 16, ModContent.DustType<LightsaberLight>(), Vector2.Zero, 0, Color.Green, 1);
				}

				if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.HasTile && Main.tileSolid[tile.TileType])
				{
					Projectile.timeLeft = 20;
					Projectile.ai[0] = tile.TileType;
					Projectile.tileCollide = false;
					Projectile.position.Y += 16;

					for (float num315 = 0.50f; num315 < 3; num315 += 0.25f)
					{
						float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
						Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 2f;
						int dustID = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, (int)(Projectile.height / 2f), ModContent.DustType<LightsaberGlow>(), 0f, 0f, 50, Color.Green, Main.rand.NextFloat(0.45f, 0.95f));
						Main.dust[dustID].velocity = vecangle;
					}
				}
			}

			return false;
		}
	}
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassSword : ModProjectile
	{
		private readonly int[] slashTime = new int[] { 70, 125, 160 };

		private Vector2 gripPos;
		public int Variant;

		public override string Texture => AssetDirectory.Glassweaver + Name;

		public ref float Timer => ref Projectile.ai[0];

		public NPC Parent => Main.npc[(int)Projectile.ai[1]];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glass Sword");
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 180;
			Projectile.height = 140;
			Projectile.hostile = true;
			Projectile.aiStyle = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.manualDirectionChange = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Helpers.Helper.PlayPitched("GlassMiniboss/WeavingShort", 1f, 0f, Projectile.Center);
		}

		public override void AI()
		{
			if (!Parent.active || Parent.type != NPCType<Glassweaver>())
				Projectile.Kill();

			Timer++;

			Projectile.Center = Parent.Center + new Vector2(30 * Parent.direction, -20);
			Projectile.velocity = Parent.velocity;

			Lighting.AddLight(Projectile.Center, Glassweaver.GlassColor.ToVector3());
			Projectile.direction = -1;

			Vector2 swordOff = Vector2.Zero;
			float swordTargetRot = 4.2f;
			const int rotateOff = -5;
			switch (Variant)
			{
				case 0:
					swordOff = new Vector2(38, 15);
					swordTargetRot = 4f;

					if (Timer > slashTime[0])
					{
						swordOff = new Vector2(32, -55);
						Projectile.direction = -1;
					}
					else
						Projectile.direction = -1;

					if (Timer > slashTime[1])
						swordOff = new Vector2(34, 25);
					if (Timer > slashTime[2])
						swordOff = new Vector2(48, -25);

					if (Timer > slashTime[0] + rotateOff)
						swordTargetRot = -0.4f;
					if (Timer > slashTime[1] + rotateOff)
						swordTargetRot = 3.5f;
					if (Timer > slashTime[2] + rotateOff)
						swordTargetRot = -0.1f;

					break;

				case 1:
					swordOff = new Vector2(-24, 15);
					swordTargetRot = 4.2f;
					Projectile.direction = 1;

					if (Timer > slashTime[0])
						swordOff = new Vector2(24, 5);
					if (Timer > slashTime[1])
						swordOff = new Vector2(-38, 0);
					if (Timer > slashTime[2])
						swordOff = new Vector2(48, -25);

					if (Timer > slashTime[0] + rotateOff)
						swordTargetRot = 0.44f;
					if (Timer > slashTime[1] + rotateOff)
						swordTargetRot = 4.2f;
					if (Timer > slashTime[2] + rotateOff)
						swordTargetRot = -0.44f;

					break;

				case 2:
					swordOff = new Vector2(-36, 5);
					swordTargetRot = 4.4f;

					if (Timer > slashTime[0])
						swordOff = new Vector2(-36, 20);
					if (Timer > slashTime[1])
						swordOff = new Vector2(-24, 12);
					if (Timer > slashTime[2])
						swordOff = new Vector2(48, -25);

					if (Timer > slashTime[0] + rotateOff)
						swordTargetRot = 4.1f;
					if (Timer > slashTime[1] + rotateOff)
						swordTargetRot = 3.9f;
					if (Timer > slashTime[2] + rotateOff)
						swordTargetRot = -0.8f;

					break;
			}

			swordOff.X *= Parent.direction;
			//swordTargetRot *= Parent.direction;

			gripPos = Parent.Center + swordOff.RotatedBy(Parent.rotation);
			Projectile.rotation = MathHelper.Lerp(Projectile.rotation, swordTargetRot, 0.1f) + Parent.rotation;

			if (Timer > slashTime[(int)Variant] && Math.Abs(Projectile.rotation - swordTargetRot) > 0.05f)
				Dust.NewDustPerfect(gripPos + new Vector2(0, -80).RotatedBy(Projectile.rotation * Parent.direction), DustType<Dusts.Cinder>(), Parent.velocity * 0.2f, 0, Glassweaver.GlassColor, 0.7f);

			if (Timer < 60)
			{
				Vector2 vel = new Vector2(0, -Main.rand.NextFloat(5f)).RotatedBy(Projectile.rotation).RotatedByRandom(0.2f);
				Dust.NewDustPerfect(gripPos, DustType<Dusts.Cinder>(), vel, 0, Glassweaver.GlowDustOrange, 1f);
			}

			int extraTime = 27;

			if (Variant == 1)
				extraTime = 22;

			if (Timer > extraTime + slashTime[(int)Variant])
				Projectile.Kill();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Timer > slashTime[(int)Variant] && projHitbox.Intersects(targetHitbox);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Math.Abs(Parent.velocity.X) > 0 && Timer < slashTime[2])
				Parent.velocity.X = -oldVelocity.X;

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float rot = Projectile.rotation * Parent.direction;

			Asset<Texture2D> sword = Request<Texture2D>(Texture);
			Rectangle frame = sword.Frame(2, 1, 0);
			Rectangle hotFrame = sword.Frame(2, 1, 1);
			Vector2 origin = frame.Size() * new Vector2(0.5f, 0.84f);

			SpriteEffects dir = Projectile.direction * Parent.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			float scaleIn = Projectile.scale * Helpers.Helper.BezierEase(Utils.GetLerpValue(50, 70, Timer, true));

			Color fadeIn = Color.Lerp(lightColor, Color.White, Utils.GetLerpValue(100, 60, Timer, true));
			Main.EntitySpriteDraw(sword.Value, gripPos - Main.screenPosition, frame, fadeIn, rot, origin, scaleIn, dir, 0);

			Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(75, 60, Timer, true);
			Main.EntitySpriteDraw(sword.Value, gripPos - Main.screenPosition, hotFrame, hotFade, rot, origin, scaleIn, dir, 0);

			Asset<Texture2D> slash = Request<Texture2D>(Texture + "Slash");
			Rectangle slashFill = slash.Frame(1, 2, 0, 0);
			Rectangle slashLine = slash.Frame(1, 2, 0, 1);

			Color slashColor = Glassweaver.GlassColor * Utils.GetLerpValue(slashTime[(int)Variant] - 8, slashTime[(int)Variant] + 1, Timer, true) * Utils.GetLerpValue(slashTime[(int)Variant] + 10, slashTime[(int)Variant] + 4, Timer, true);
			slashColor.A = 0;

			Vector2 slashScale = new Vector2(1.2f, 1.5f) + new Vector2(Utils.GetLerpValue(slashTime[(int)Variant] - 7, slashTime[(int)Variant] + 10, Timer, true));

			Main.EntitySpriteDraw(slash.Value, gripPos - Main.screenPosition, slashFill, slashColor * 0.9f, (MathHelper.Pi / 3f * Parent.direction) + rot * 0.4f, slashFill.Size() * new Vector2(0.5f, 0.33f), slashScale, 0, 0);
			Main.EntitySpriteDraw(slash.Value, gripPos - Main.screenPosition, slashLine, slashColor * 1.25f, (MathHelper.Pi / 3f * Parent.direction) + rot * 0.4f, slashFill.Size() * new Vector2(0.5f, 0.33f), slashScale * 0.98f, 0, 0);

			return false;
		}
	}
}

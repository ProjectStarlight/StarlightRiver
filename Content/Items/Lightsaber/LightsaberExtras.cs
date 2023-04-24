using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberGProj : GlobalProjectile
	{
		public Entity parent = default;

		public override bool InstancePerEntity => true;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_Parent spawnSource)
				parent = spawnSource.Entity;
		}
	}

	public class LightsaberPlayer : ModPlayer
	{
		public int whiteCooldown = -1;
		public bool dashing = false;

		public bool jumping = false;
		public Vector2 jumpVelocity = Vector2.Zero;

		public float storedBodyRotation = 0f;

		public override void ResetEffects()
		{
			if (whiteCooldown > 1 || Player.itemAnimation == 0)
				whiteCooldown--;
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (dashing)
				base.ModifyHurt(ref modifiers);
		}

		public override void PreUpdate()
		{
			if (dashing || jumping)
				Player.maxFallSpeed = 2000f;

			if (whiteCooldown == 0)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Player.Center);
				var dust = Dust.NewDustPerfect(Player.Center, ModContent.DustType<LightsaberStar>(), Vector2.Zero, 0, new Color(200, 200, 255, 0), 0.3f);
				dust.customData = Player.whoAmI;
			}
		}

		public override void PostUpdate()
		{
			if (jumping)
			{
				Player.mount?.Dismount(Player);
				storedBodyRotation += 0.3f * Player.direction;
				Player.fullRotation = storedBodyRotation;
				Player.fullRotationOrigin = Player.Size / 2;
			}

			if (Player.velocity.X == 0 || Player.velocity.Y == 0)
				dashing = false;

			if (Player.velocity.Y == 0)
			{
				storedBodyRotation = 0;
				Player.fullRotation = 0;
				jumping = false;
			}
			else
			{
				jumpVelocity = Player.velocity;
			}
		}
	}

	public class LightsaberLight : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.96f;
			if (dust.scale < 0.05f)
				dust.active = false;
			Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 2);
			return false;
		}
	}

	public class LightsaberStar : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Keys/GlowStar";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 74, 74);
			dust.noLight = true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * (1 - dust.alpha / 255f);
		}

		public override bool Update(Dust dust)
		{
			Player owner = Main.player[(int)dust.customData];

			dust.position = owner.position + new Vector2(9 * owner.direction, 19);

			dust.alpha += 10;

			if (!dust.noLight)
				Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.2f);

			if (dust.alpha > 255)
				dust.active = false;
			return false;
		}
	}
}
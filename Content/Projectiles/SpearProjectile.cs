using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Projectiles
{
	public abstract class SpearProjectile : ModProjectile
	{
		/// <summary>
		/// The base duration in frames the spear should be out. Final duration will be scaled with melee speed.
		/// </summary>
		public int duration;

		/// <summary>
		/// The minimum distance this spear starts from the player. Measured by the spear tip.
		/// </summary>
		public float minDistance;

		/// <summary>
		/// The max distance this spear should reach away from the player. Measured by the spear tip.
		/// </summary>
		public float maxDistance;

		/// <summary>
		/// The motion this spear follows along its progress. If IsSymetrical is true it will move forward then back along this, else it will follow it forward only.
		/// </summary>
		public Func<float, float> motionFunction = (n) => n;

		/// <summary>
		/// If MotionFunction should be followed forwards and backwards or only forwards.
		/// </summary>
		public bool isSymetrical = true;

		/// <summary>
		/// How the rendering should offset rotation. Change this if its not in the same orientation as a vanilla spear sprite
		/// </summary>
		public float rotationOffset = MathHelper.Pi * (3 / 4f);

		/// <summary>
		/// Origin from which the spear is drawn at. Usually will need to be changed in tandem with rotationOffset.
		/// </summary>
		public Vector2 origin = Vector2.Zero;

		/// <summary>
		/// How many frames at the start and end that the spear should fade out/in.
		/// </summary>
		public int fadeDuration = 3;

		/// <summary>
		/// The actual duration this spear will be out based on owner melee speed.
		/// </summary>
		public int RealDuration
		{
			get
			{
				Player player = Main.player[Projectile.owner];
				float speed = 1f / player.GetTotalAttackSpeed(DamageClass.Melee);
				return (int)(duration * speed);
			}
		}

		protected SpearProjectile(int duration, float minOff, float maxOff)
		{
			this.duration = duration;
			minDistance = minOff;
			maxDistance = maxOff;
		}

		public virtual void SafeSetDefaults() { }

		public virtual void SafeAI() { }

		public sealed override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.penetrate = -1;
			Projectile.aiStyle = ProjAIStyleID.Spear;
			Projectile.friendly = true;
			Projectile.timeLeft = duration;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
			SafeSetDefaults();
		}

		public override void AI()
		{
			SafeAI();

			Player player = Main.player[Projectile.owner];

			player.heldProj = Projectile.whoAmI;
			player.itemTime = player.itemAnimation;

			if (Projectile.timeLeft == duration)
				Projectile.timeLeft = RealDuration;

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			Projectile.rotation = rotationOffset + Projectile.velocity.ToRotation();

			float progress;

			if (!isSymetrical)
				progress = 1f - Projectile.timeLeft / (float)RealDuration;
			else
				progress = Projectile.timeLeft > RealDuration / 2f ? (RealDuration - Projectile.timeLeft) / (RealDuration / 2f) : Projectile.timeLeft / (RealDuration / 2f);

			Projectile.Center = player.MountedCenter + Vector2.Lerp(Projectile.velocity * minDistance, Projectile.velocity * maxDistance, motionFunction(progress));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float opacity = 1f;

			if (Projectile.timeLeft < fadeDuration)
				opacity = Projectile.timeLeft / (float)fadeDuration;

			if (Projectile.timeLeft > RealDuration - fadeDuration)
				opacity = (RealDuration - Projectile.timeLeft) / (float)fadeDuration;

			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY), tex.Frame(), lightColor * opacity, Projectile.rotation, origin, Projectile.scale, 0, 0);
			return false;
		}
	}
}
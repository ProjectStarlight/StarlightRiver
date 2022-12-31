using StarlightRiver.Content.Buffs;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	internal class BurningGround : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public ref float Timer => ref Projectile.ai[0];

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.damage = 1;
			Projectile.height = 32;
			Projectile.width = 1;
			Projectile.timeLeft = 2;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Timer++;
			Projectile.timeLeft = 2;

			if (Timer < 60)
			{
				Projectile.width = (int)Timer * 6;
				Projectile.position.X -= 3;
			}

			if (Timer > 360)
				Projectile.timeLeft = 0;
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;
			var color = new Color(255, 200, 50)
			{
				A = 0
			};

			var target = new Rectangle((int)Projectile.position.X - (int)Main.screenPosition.X, (int)Projectile.position.Y - (int)Main.screenPosition.Y, Projectile.width, Projectile.height);
			Main.spriteBatch.Draw(tex, target, null, color, 0, Vector2.Zero, 0, 0);
		}

		public override bool CanHitPlayer(Player target)
		{
			if (target.Hitbox.Intersects(Projectile.Hitbox))
				target.AddBuff(ModContent.BuffType<GlassweaverDot>(), Main.masterMode ? 120 : 60);

			return false;
		}
	}

	internal class GlassweaverDot : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + "GlassweaverDot";

		public GlassweaverDot() : base("Incineration", "Burning incredibly fast!", true) { }

		public override void Update(Player player, ref int buffIndex)
		{
			player.lifeRegen -= Main.masterMode ? 200 : Main.expertMode ? 80 : 40;
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.lifeRegen -= 40;
		}
	}
}

using StarlightRiver.Content.Buffs;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	internal class BurningGround : ModProjectile, IDrawOverTiles
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
				Projectile.width = (int)Timer * 8;
				Projectile.position.X -= 4;
			}

			if (Timer > 360)
				Projectile.timeLeft = 0;

			for (int k = 0; k < Main.rand.Next(1, 3); k++)
			{
				Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX * Main.rand.NextFloat(-Projectile.width / 2, Projectile.width / 2),
					ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * Main.rand.NextFloat(-2, 0), 0, new Color(255, Main.rand.Next(150, 250), 50), Main.rand.NextFloat(2));
			}

			//Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(7), 0, new Color(255, 150, 50), 1);

			Lighting.AddLight(Projectile.Center, new Vector3(255, 100, 20) * 0.025f);

			for (int k = 0; k < 40; k++)
			{
				Vector2 pos = Projectile.Center + (Vector2.UnitX * Projectile.width * 0.5f).RotatedBy(k / 40f * 6.28f);
				Tile tile = Framing.GetTileSafely((int)pos.X / 16, (int)pos.Y / 16);

				if (tile.HasTile)
					Lighting.AddLight(pos, new Vector3(255, 100, 20) * 0.005f);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LongGlow").Value;
			var color = new Color(255, 200, 50, 0);

			var target = new Rectangle((int)Projectile.position.X - (int)Main.screenPosition.X - 16, (int)Projectile.position.Y - (int)Main.screenPosition.Y - 64, Projectile.width + 32, Projectile.height + 64);
			Main.spriteBatch.Draw(tex, target, null, color, 0, Vector2.Zero, 0, 0);
		}

		public override bool CanHitPlayer(Player target)
		{
			if (target.Hitbox.Intersects(Projectile.Hitbox))
				target.AddBuff(ModContent.BuffType<GlassweaverDot>(), Main.masterMode ? 120 : 60);

			return false;
		}

		public void DrawOverTiles(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/GlassMiniboss/FireAura").Value;
			var color = new Color(255, 200, 50, 0);
			//color.A = 0;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, 0, tex.Size() / 2, Projectile.width / (float)tex.Width, 0, 0);
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

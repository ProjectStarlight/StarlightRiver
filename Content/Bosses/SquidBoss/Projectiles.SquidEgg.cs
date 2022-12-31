using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class SquidEgg : ModProjectile
	{
		public override string Texture => AssetDirectory.SquidBoss + Name;
		public override void SetDefaults()
		{
			Projectile.width = 62;
			Projectile.height = 34;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 300;
			Projectile.hostile = true;
			Projectile.damage = 15;
		}

		public override void AI()
		{
			Projectile.ai[0]++;

			if (Projectile.ai[0] % 100 == 0)
			{
				NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, NPCType<Auroraling>());
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item86, Projectile.Center);
			}
		}

		public override void Kill(int timeLeft)
		{
			NPC.NewNPC(Projectile.GetSource_FromThis(), (int)Projectile.Center.X, (int)Projectile.Center.Y, Main.expertMode ? NPCType<Auroraborn>() : NPCType<Auroraling>());
		}
	}
}

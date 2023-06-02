using Terraria.ID;

namespace StarlightRiver.Content.Buffs
{
	class Illuminant : SmartBuff
	{
		public Illuminant() : base("Illuminant", "Glowing brightly!", true) { }

		public override string Texture => AssetDirectory.Buffs + "Illuminant";

		public override void Update(NPC NPC, ref int buffIndex)
		{
			/*if (Main.rand.NextBool(4))
			{
				int i = Dust.NewDust(NPC.position, NPC.width, NPC.height, 264, 0, 0, 0, new Color(255, 255, 200));
				Main.dust[i].noGravity = true;
			}*/
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Main.rand.NextBool(4))
			{
				int i = Dust.NewDust(Player.position, Player.width, Player.height, DustID.PortalBoltTrail, 0, 0, 0, new Color(255, 255, 200));
				Main.dust[i].noGravity = true;
			}

			Player.aggro += 999;
		}
	}
}
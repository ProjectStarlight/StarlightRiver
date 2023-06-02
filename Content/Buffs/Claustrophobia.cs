using System;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Buffs
{
	public class Claustrophobia : SmartBuff
	{
		public Claustrophobia() : base("Claustrophobia", "Stuck in wisp form!", true) { }

		public override string Texture => AssetDirectory.Buffs + "Claustrophobia";

		private int timer;

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.whoAmI == Main.myPlayer && timer++ % 15 == 0)
			{
				int dps = Player.statLifeMax2 / 10;
				int dmg = dps / 4;
				Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " couldn't maintain their form."), dmg, 0);
				Player.immune = false;
			}

			Player.lifeRegen = Math.Min(Player.lifeRegen, 0);
			Player.lifeRegenTime = 0;
		}
	}
}
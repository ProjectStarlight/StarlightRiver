using StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	public class ShrinePlayer : ModPlayer
	{
		public bool CombatShrineActive;
		public bool EvasionShrineActive;
		//public bool WitShrineActive;

		public int InNoBuildZone;//this is an int to get around update order issues, decreases once per tick

		public override void PreUpdateBuffs()
		{
			if (InNoBuildZone > 0)
			{
				Player.noBuilding = true;
				Player.AddBuff(BuffID.NoBuilding, 2);
			}

			//if (InNoBuildZone > 0)
			//	Main.NewText(InNoBuildZone);
		}

		public override void ResetEffects()
		{
			CombatShrineActive = false;
			EvasionShrineActive = false;
			//WitShrineActive = false;

			if (InNoBuildZone > 0)
				InNoBuildZone--;
		}
	}
}
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

		public override void ResetEffects()
		{
			CombatShrineActive = false;
			EvasionShrineActive = false;
			//WitShrineActive = false;
		}
	}
}
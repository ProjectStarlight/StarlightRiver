using StarlightRiver.Core.Systems.PlayableCharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StarlightRiver.Content.PlayableCharacters
{
	internal class Alican : PlayableCharacter
	{
		public override void PreUpdate()
		{
			// Adjust health/mana correctly while still forbidding vanilla upgrades
			player.statLifeMax = 500;
			player.statManaMax = 200;

			player.statLifeMax2 = 300;
			player.statManaMax2 = 0;
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/PlayableCharacters/Alican").Value;
			var effects = player.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			drawInfo.DrawDataCache.Add(
				new DrawData(tex, player.Center - Main.screenPosition + new Vector2(0, -8 + player.gfxOffY), null, Lighting.GetColor((player.Center / 16).ToPoint()), player.fullRotation, tex.Size() / 2f, 1, effects, 0)
				);
		}
	}
}

﻿using System;
using Terraria.DataStructures;

namespace StarlightRiver.Content.CustomHooks
{
	class PlayerLayerManipulation : HookGroup
	{
		//Swaps out some variables on PlayerLayer drawData for certain effects like the pancake and spinning.
		//This might look weird if other mods do strange PlayerLayer stuff but it shouldnt have consquences outside of visuals. The actual patch is a bit iffy rn tho, come fix this later.
		public override void Load()
		{
			if (Main.dedServ)
				return;

			Terraria.DataStructures.On_PlayerDrawLayers.DrawPlayer_TransformDrawData += CustomTransformations;
		}

		private void CustomTransformations(Terraria.DataStructures.On_PlayerDrawLayers.orig_DrawPlayer_TransformDrawData orig, ref PlayerDrawSet drawinfo)
		{
			orig(ref drawinfo);
			for (int k = 0; k < drawinfo.DrawDataCache.Count; k++)
			{
				drawinfo.DrawDataCache[k] = ManipulateDrawInfo(drawinfo.DrawDataCache[k], drawinfo.drawPlayer);
			}
		}

		public override void Unload() { }

		private DrawData ManipulateDrawInfo(DrawData input, Player Player)
		{
			float rotation = Player.GetModPlayer<StarlightPlayer>().rotation;

			if (rotation != 0) //paper mario-style rotation
			{
				float sin = (float)Math.Sin(rotation + 1.57f * Player.direction);
				int off = Math.Abs((int)((input.useDestinationRectangle ? input.destinationRectangle.Width : input.sourceRect?.Width ?? input.texture.Width) * sin));

				SpriteEffects effect = sin > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				if (input.effect == SpriteEffects.FlipHorizontally)
					effect = effect == SpriteEffects.FlipHorizontally ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

				var newRect = new Rectangle((int)input.position.X, (int)input.position.Y, off, input.useDestinationRectangle ? input.destinationRectangle.Height : input.sourceRect?.Height ?? input.texture.Height);
				var newData = new DrawData(input.texture, newRect, input.sourceRect, input.color, input.rotation, input.origin, effect, 0)
				{
					shader = input.shader
				};

				return newData;
			}

			//the pancake debuff
			else if (Player.HasBuff(ModContent.BuffType<Buffs.Squash>()))
			{
				var newData = new DrawData(input.texture, new Rectangle((int)Player.position.X - 20 - (int)Main.screenPosition.X, (int)Player.position.Y + 20 - (int)Main.screenPosition.Y + 1, Player.width + 40, Player.height - 20), input.sourceRect, input.color, input.rotation, default, input.effect, 0)
				{
					shader = input.shader
				};

				return newData;
			}
			else
			{
				return input;
			}
		}
	}
}
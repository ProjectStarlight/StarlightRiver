using System;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Vanity
{
	public class WardenVanityDrawLayer : PlayerDrawLayer
	{
		public int yFrame = 0;

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		{
			return drawInfo.drawPlayer.GetModPlayer<WardenVanityPlayer>().robeEquipped && !drawInfo.drawPlayer.dead;
		}

		public override Position GetDefaultPosition()
		{
			return new AfterParent(PlayerDrawLayers.Torso);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			Player armorOwner = drawInfo.drawPlayer;
			WardenVanityPlayer modPlayer = armorOwner.GetModPlayer<WardenVanityPlayer>();

			drawInfo.armorHidesArms = true;
			Texture2D tex = Assets.Items.Vanity.WardenRobeRealBody.Value;

			Vector2 drawPos = armorOwner.MountedCenter - Main.screenPosition - new Vector2(0, 3 - armorOwner.gfxOffY);

			int xFrame = 0;
			int divider = 8;

			if (Math.Abs(armorOwner.velocity.X) > 0.25f)
			{
				xFrame = 1;
				divider = 8;
			}

			if (armorOwner.velocity.Y != 0)
			{
				xFrame = 2;
				divider = 9;
			}

			if (xFrame == 2)
			{
				yFrame = 0;
			}
			else
			{

				if (modPlayer.frameCounter > 6)
				{
					modPlayer.frameCounter = 0;
					yFrame++;
				}

				yFrame %= divider;
			}

			var frame = new Rectangle(40 * xFrame, 56 * yFrame, 40, 56);

			int shader = drawInfo.cBody;

			var drawData = new DrawData(
				tex,
				new Vector2((int)drawPos.X, (int)drawPos.Y),
				frame,
				Lighting.GetColor((int)armorOwner.Center.X / 16, (int)armorOwner.Center.Y / 16),
				0f,
				frame.Size() / 2,
				1,
				armorOwner.direction != -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
				0
			);

			if (shader > 0)
				drawData.shader = shader;

			drawInfo.DrawDataCache.Add(drawData);
		}
	}
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	class ShieldOverlay : SmartUIState
	{
		public override bool Visible => true;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(n => n.Name == "Vanilla: Resource Bars") + 1;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var player = Main.LocalPlayer;
			var sp = player.GetModPlayer<ShieldPlayer>();

			if (sp.Shield > 0 || sp.MaxShield > 0)
			{
				int num4 = (int)((float)Main.player[Main.myPlayer].statLifeMax2 / 20);
				if (num4 >= 10)
					num4 = 10;

				string text = string.Concat(new string[]
				{
				Lang.inter[0].Value,
				" ",
				Main.player[Main.myPlayer].statLifeMax2.ToString(),
				"/",
				Main.player[Main.myPlayer].statLifeMax2.ToString()
				});

				Vector2 vector = Main.fontMouseText.MeasureString(text);

				var shieldText = $"  {sp.Shield}/{sp.MaxShield}";
				float textWidth = Main.fontMouseText.MeasureString(shieldText).X / 2;
				var pos2 = new Vector2(Main.screenWidth - 300 + 13 * num4 + vector.X * 0.5f - textWidth - 6, 6f);

				spriteBatch.DrawString(Main.fontMouseText, shieldText, pos2, Main.mouseTextColorReal.MultiplyRGB(new Color(120, 255, 255)) );
			}

			if (sp.Shield > 0)
			{
				int vanillaHearts = Math.Min(20, player.statLifeMax / 20);
				int fullHeartsToDraw = Math.Min(vanillaHearts, sp.Shield / 20);
				int partialHeartMax = fullHeartsToDraw < 20 ? sp.Shield % 20 : 0;
				float shieldPerHeart = sp.MaxShield > vanillaHearts * 20 ? sp.MaxShield / (float)vanillaHearts : 20;

				for (int k = 0; k <= fullHeartsToDraw; k++)
				{
					var pos = new Vector2(Main.screenWidth - 300 + k * 26, 32f + (Main.heartTexture.Height - Main.heartTexture.Height) / 2f);
					if (k >= 10)
						pos += new Vector2(-260, 26);

					var tex = ModContent.GetTexture(AssetDirectory.GUI + "ShieldHeart");
					var texOver = ModContent.GetTexture(AssetDirectory.GUI + "ShieldHeartOver");
					var texLine = ModContent.GetTexture(AssetDirectory.GUI + "ShieldHeartLine");
					int width = 0;

					if (sp.Shield >= (k + 1) * shieldPerHeart)
						width = tex.Width;
					else if (sp.Shield > k * shieldPerHeart)
						width = (int)((sp.Shield % shieldPerHeart) / shieldPerHeart * tex.Width);

					if (width > 0 && k < 20)
					{
						var source = new Rectangle(0, 0, width, tex.Height);
						var target = new Rectangle((int)pos.X, (int)pos.Y, width, tex.Height);
						var lineTarget = new Rectangle((int)pos.X + width - 2, (int)pos.Y, 2, tex.Height);
						var lineSource = new Rectangle(width - 2, 0, 2, tex.Height);

						spriteBatch.Draw(tex, target, source, Color.White * 0.25f);
						spriteBatch.Draw(texOver, target, source, Color.White);
						spriteBatch.Draw(texLine, lineTarget, lineSource, Color.White);
					}
				}
			}
		}
	}
}

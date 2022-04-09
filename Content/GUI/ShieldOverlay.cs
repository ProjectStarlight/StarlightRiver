using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.ResourceSets;
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
			var Player = Main.LocalPlayer;
			var sp = Player.GetModPlayer<BarrierPlayer>();

			if (sp.Barrier > 0 || sp.MaxBarrier > 0)
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

				Vector2 vector = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text);

				var shieldText = $"  {sp.Barrier}/{sp.MaxBarrier}";
				float textWidth = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(shieldText).X / 2;
				var pos2 = new Vector2(Main.screenWidth - 300 + 13 * num4 + vector.X * 0.5f - textWidth - 6, 6f);

				spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, shieldText, pos2, Main.MouseTextColorReal.MultiplyRGB(new Color(120, 255, 255)) );
			}

			if (sp.Barrier > 0)
			{
				int vanillaHearts = Math.Min(20, Player.statLifeMax / 20);
				int fullHeartsToDraw = Math.Min(vanillaHearts, sp.Barrier / 20);
				int partialHeartMax = fullHeartsToDraw < 20 ? sp.Barrier % 20 : 0;
				float shieldPerHeart = sp.MaxBarrier > vanillaHearts * 20 ? sp.MaxBarrier / (float)vanillaHearts : 20;

				for (int k = 0; k <= fullHeartsToDraw; k++)
				{
					Vector2 pos = Vector2.Zero;

					if (Main.ResourceSetsManager.ActiveSetKeyName == "Default")
					{
						pos = new Vector2(Main.screenWidth - 300 + k * 26, 32f);
						if (k >= 10)
							pos += new Vector2(-260, 26);
					}
					else if (Main.ResourceSetsManager.ActiveSetKeyName == "New")
					{
						pos = new Vector2(Main.screenWidth - 292 + k * 24, 19f);
						if (k >= 10)
							pos += new Vector2(-240, 28);
					}

					var tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldHeart").Value;
					var texOver = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldHeartOver").Value;
					var texLine = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ShieldHeartLine").Value;
					int width = 0;

					if (sp.Barrier >= (k + 1) * shieldPerHeart)
						width = tex.Width;
					else if (sp.Barrier > k * shieldPerHeart)
						width = (int)((sp.Barrier % shieldPerHeart) / shieldPerHeart * tex.Width);

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

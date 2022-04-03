/*using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.RandomExperiment
{
	class MoreMagicalMirror : GlobalItem, IOrderedLoadable
	{
		private static Texture2D savedScreen = null;
		private static int transitionTime = 0;

		public float Priority => 1;

		public void Load()
		{
			On.Terraria.Main.DrawInterface += DrawMirrorOver;
		}

		public void Unload() { }

		public override bool? UseItem(Item Item, Player Player)
		{
			if (Item.type == ItemID.MagicMirror)
			{
				savedScreen = new Texture2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

				Color[] data = new Color[Main.screenTarget.Width * Main.screenTarget.Height];
				Main.screenTarget.GetData<Color>(data);
				savedScreen.SetData(data);

				transitionTime = 160;
			}

			return base.UseItem(Item, Player);
		}

		private void DrawMirrorOver(On.Terraria.Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			int timer = 160 - transitionTime;

			if (transitionTime > 0)
			{
				transitionTime--;

				Main.spriteBatch.Begin();

				Rectangle target0 = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
				Main.spriteBatch.Draw(savedScreen, target0, null, Color.White);

				if (timer > 60)
				{
					Vector2 pos = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) + new Vector2(Main.screenWidth / 2 + 100, 0) * ((timer - 60) / 60f);
					Rectangle target1 = new Rectangle((int)(pos.X), (int)(pos.Y), 100, 160);
					Rectangle target2 = new Rectangle(Main.screenWidth / 2, Main.screenHeight / 2, 100, 160);
					Rectangle source1 = new Rectangle(Main.screenWidth / 2 - 50, Main.screenHeight / 2 - 80, 100, 160);

					Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target2, source1, Color.Black, 0, new Vector2(50, 80), 0, 0);
					Main.spriteBatch.Draw(savedScreen, target1, source1, Color.White, (timer - 60) * 0.05f, new Vector2(50, 80), 0, 0);
				}

				Main.spriteBatch.End();
			}

			orig(self, gameTime);
			return; 

			//original variant			

			if (transitionTime > 0)
			{
				transitionTime--;

				Main.spriteBatch.Begin();

				Rectangle target0 = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
				Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target0, null, Color.Black);

				if (timer <= 80)
				{
					Main.spriteBatch.Draw(savedScreen, target0, null, Color.White);
					Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target0, null, new Color(160, 220, 255) * 0.25f * (Math.Min(1, timer / 30f)));

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, BlendState.Additive);

					var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailUp").Value;

					Rectangle targetBar1 = new Rectangle((int)(Main.screenWidth * (timer - 30) / 40f), 0, 200, Main.screenHeight * 2);
					Main.spriteBatch.Draw(tex, targetBar1, null, Color.White * 0.65f, -0.1f, Vector2.Zero, 0, 0);

					Rectangle targetBar0 = new Rectangle((int)(Main.screenWidth * (timer - 10) / 60f), 0, 800, Main.screenHeight * 2);
					Main.spriteBatch.Draw(tex, targetBar0, null, Color.White * 0.4f, -0.1f, Vector2.Zero, 0, 0);

					Rectangle targetBar2 = new Rectangle((int)(Main.screenWidth * (timer - 32) / 50f), 0, 120, Main.screenHeight * 2);
					Main.spriteBatch.Draw(tex, targetBar2, null, Color.White * 0.6f, -0.1f, Vector2.Zero, 0, 0);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin();
				}

				if (timer > 80)
				{
					Rectangle target1 = new Rectangle(0, (int)(Main.screenHeight * ((timer - 80) / 30f) * 0.5f), Main.screenWidth, (int)(Main.screenHeight * (1 - (timer - 80) / 30f)));
					Main.spriteBatch.Draw(savedScreen, target1, null, Color.White);
					Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target1, null, new Color(160, 220, 255) * 0.25f);
				}

				if (timer > 110 && timer <= 130)
				{
					Rectangle target2 = new Rectangle(0, (int)(Main.screenHeight * (1 - (timer - 110) / 20f) * 0.5f), Main.screenWidth, (int)(Main.screenHeight * ((timer - 110) / 20f)));
					Main.spriteBatch.Draw(Main.screenTarget, target2, null, Color.White);
					Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target2, null, new Color(160, 220, 255) * 0.25f);
				}

				if(timer > 130)
				{
					Main.spriteBatch.Draw(Main.screenTarget, target0, null, Color.White);
					Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target0, null, new Color(160, 220, 255) * 0.25f * (1 - (timer - 130) / 30f));
				}

				Main.spriteBatch.End();
			}

			orig(self, gameTime);
		}
	}
}*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class CodexPopup : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        public override bool Visible => Timer > 0;

        private string Text;
        private Texture2D Texture;
        public int Timer;

        public override void Draw(SpriteBatch spriteBatch)
        {
            CodexHandler mp = Main.LocalPlayer.GetModPlayer<CodexHandler>();

            Texture2D tex = mp.CodexState == 1 ? Request<Texture2D>("StarlightRiver/Assets/GUI/Book1Closed").Value : Request<Texture2D>("StarlightRiver/Assets/GUI/Book2Closed").Value;

            string str = "New Entry: " + Text;
            float stringWidth = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(str).X;
            float xOff = stringWidth;

            Vector2 pos = Timer > 60 ? new Vector2(20, 140) : new Vector2(-xOff * 2 + 20 + (xOff * Timer / 60f) * 2, 140);
            float alpha = Timer > 60 ? 1 : (Timer / 60f);
            if (Timer > 350) alpha = ((360 - Timer) / 10f);

            Rectangle target = new Rectangle((int)(pos.X - 40 - 50), (int)(pos.Y - 25), (int)(stringWidth + 140), 40);
            var bgTex = TextureAssets.MagicPixel.Value;
            var edgeTex = Request<Texture2D>(AssetDirectory.GUI + "CodexPopupEdge").Value;

            spriteBatch.Draw(bgTex, target, null, new Color(20, 20, 35) * alpha * 0.5f);
            spriteBatch.Draw(edgeTex, target.TopRight(), null, new Color(20, 20, 35) * alpha * 0.5f);

            float alpha2 = 0;

            if (Texture != null)
            {
                if (Timer > 260 && Timer < 280)
                    alpha2 = 1 - (Timer - 260) / 20f;
                if (Timer <= 260 && Timer >= 160)
                    alpha2 = 1;
                if (Timer > 140 && Timer < 160)
                    alpha2 = (Timer - 140) / 20f;

                spriteBatch.Draw(Texture, pos + new Vector2(14, -6), null, Color.White * alpha * alpha2, 0, Texture.Size() / 2, 1, 0, 0);
            }

            float inverseAlpha2 = 1 - alpha2;

            spriteBatch.Draw(tex, pos + new Vector2(14, -6), null, Color.White * alpha * inverseAlpha2, 0, tex.Size() / 2, 1, 0, 0);

            Utils.DrawBorderString(spriteBatch, str, pos + new Vector2(40, -16), Color.White * alpha, 1);

            Timer--;
        }

        public void TripEntry(string text, Texture2D texture = null)
        {
            Text = text;
            Texture = texture;
            Timer = 360;
        }
    }
}
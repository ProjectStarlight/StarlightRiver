using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.GUI
{
    public class TextCard : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        private Ability Thisability;
        private string Title;
        private string Message;
        private int Timer = 0;
        private bool used = false;
        private float textScale = 1;
        private string texturePath = "StarlightRiver/Assets/GUI/DefaultCard";
        private bool reverse = false;

        private int tempTime = 0;
        private int tempTimeMax = 0;

        private Texture2D Texture { get => GetTexture(texturePath); }

        public void SetTexture(string path) => texturePath = path;

        public void Display(string title, string message, Ability ability = null, int time = 0, float scale = 1, bool titleFirst = false)
        {
            Thisability = ability;
            Title = title;
            Message = message;
            Visible = true;
            used = false;
            tempTimeMax = time;
            tempTime = 0;
            Timer = 1;
            textScale = scale;
            reverse = titleFirst;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            int TitleLength = (int)(Main.fontDeathText.MeasureString(Title).X * 0.65f * textScale) / 2;
            int MessageLength = (int)(Main.fontDeathText.MeasureString(Message).X * 0.4f * textScale) / 2;
            int Longest = MessageLength > TitleLength ? MessageLength : TitleLength;
            int startY = (int)(Main.screenHeight * Main.UIScale) / 5;
            int startX = (int)(Main.screenWidth * Main.UIScale) / 2;
            Color color = Color.White * (Timer / 120f);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.AlphaBlend);

            spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Keys/Glow"), new Rectangle(startX - Longest * 2, startY - (int)(25 * textScale), Longest * 4, (int)(150 * textScale)), Color.Black * 0.6f * (Timer / 120f));

            spriteBatch.End();
            spriteBatch.Begin();

            if (reverse)
            {
                spriteBatch.DrawString(Main.fontDeathText, Title, new Vector2(startX - TitleLength, startY + (int)(30 * textScale)), color, 0f, Vector2.Zero, 0.65f * textScale, 0, 0);
                spriteBatch.DrawString(Main.fontDeathText, Message, new Vector2(startX - MessageLength, startY + 10), color, 0f, Vector2.Zero, 0.4f * textScale, 0, 0);
            }
            else
            {
                spriteBatch.DrawString(Main.fontDeathText, Title, new Vector2(startX - TitleLength, startY + 10), color, 0f, Vector2.Zero, 0.65f * textScale, 0, 0);
                spriteBatch.DrawString(Main.fontDeathText, Message, new Vector2(startX - MessageLength, startY + (int)(50 * textScale)), color, 0f, Vector2.Zero, 0.4f * textScale, 0, 0);
            }

            spriteBatch.Draw(Texture, new Rectangle(startX - (int)(Longest * 1.2f), startY + (int)(75 * textScale), (int)(Longest * 2.4f), 6), new Rectangle(94, 0, 8, 6), color);

            spriteBatch.Draw(Texture, new Vector2(startX - (int)(Longest * 1.2f) - 46, startY + (int)(75 * textScale) - 34), new Rectangle(0, 0, 46, 46), color);
            spriteBatch.Draw(Texture, new Rectangle(startX + (int)(Longest * 1.2f), startY + (int)(75 * textScale) - 34, 46, 46), new Rectangle(46, 0, 46, 46), color);

            if (Thisability != null)
            {
                if (Thisability.Active) used = true;

                if (used) Timer--;
                else if (Timer < 120) Timer++;

                if (Timer == 0) Reset();
            }
            else
            {
                if (tempTime < tempTimeMax) tempTime++;
                if (tempTime >= tempTimeMax) Timer--;
                else if (Timer < 120) Timer++;

                if (Timer == 0) Reset();
            }
        }

        private void Reset()
        {
            Visible = false;
            textScale = 1;
            Thisability = null;
            SetTexture("StarlightRiver/Assets/GUI/DefaultCard");
        }
    }
}
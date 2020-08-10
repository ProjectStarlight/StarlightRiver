using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.GUI
{
    public class Stamina : UIState
    {
        public UIPanel abicon;
        public static bool visible = false;
        private readonly Stam Stam1 = new Stam();

        public override void OnInitialize()
        {
            Stam1.Left.Set(-303, 1);
            Stam1.Top.Set(110, 0);
            Stam1.Width.Set(30, 0f);
            Append(Stam1);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

            if (Main.mapStyle != 1)
            {
                if (Main.playerInventory)
                {
                    Stam1.Left.Set(-220, 1);
                    Stam1.Top.Set(90, 0);
                }
                else
                {
                    Stam1.Left.Set(-70, 1);
                    Stam1.Top.Set(90, 0);
                }
            }
            else
            {
                Stam1.Left.Set(-306, 1);
                Stam1.Top.Set(110, 0);
            }
            int height = 30 * mp.StatStaminaMax; if (height > 30 * 7) height = 30 * 7;

            Stam1.Height.Set(height, 0f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (Stam1.IsMouseHovering)
            {
                AbilityHandler mp = Main.LocalPlayer.GetModPlayer<AbilityHandler>();
                Utils.DrawBorderString(spriteBatch, "Stamina: " + mp.StatStamina + "/" + mp.StatStaminaMax, Main.MouseScreen + Vector2.One * 16, Main.mouseTextColorReal);
            }

            Recalculate();
        }
    }

    internal class Stam : UIElement
    {
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            Player player = Main.LocalPlayer;
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

            Texture2D emptyTex = GetTexture("StarlightRiver/GUI/Assets/StaminaEmpty");
            Texture2D fillTex = GetTexture("StarlightRiver/GUI/Assets/Stamina");

            int row = 0;
            for (int k = 0; k < mp.StatStaminaMax; k++)
            {
                if (k % 7 == 0 && k != 0) row++;

                Vector2 pos = row % 2 == 0 ? dimensions.ToRectangle().TopLeft() + new Vector2(row * -18, (k % 7) * 28) :
                    dimensions.ToRectangle().TopLeft() + new Vector2(row * -18, 14 + (k % 7) * 28);

                spriteBatch.Draw(emptyTex, pos, emptyTex.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                if (mp.StatStamina > k)
                {
                    spriteBatch.Draw(fillTex, pos + Vector2.One * 4, Color.White);
                }
                if (mp.StatStamina == k)
                {
                    float scale = 1 - (mp.StatStaminaRegen / (float)mp.StatStaminaRegenMax);
                    spriteBatch.Draw(fillTex, pos + Vector2.One * 4 + fillTex.Size() / 2, fillTex.Frame(), Color.White, 0, fillTex.Size() / 2, scale, 0, 0);
                }
            }
        }
    }
}
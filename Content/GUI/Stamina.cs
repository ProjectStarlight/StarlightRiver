using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Configs;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class Stamina : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => Main.LocalPlayer.GetHandler().StaminaMax > 1;

		public UIPanel abicon;
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
            AbilityHandler mp = player.GetHandler();

            if (Main.mapStyle != 1)
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
            else
            {
                Stam1.Left.Set(-306, 1);
                Stam1.Top.Set(110, 0);
            }

            float height = 30 * mp.StaminaMax; if (height > 30 * 7) height = 30 * 7;

            Stam1.Height.Set(height, 0f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (Stam1.IsMouseHovering)
            {
                AbilityHandler mp = Main.LocalPlayer.GetHandler();
                var stamina = Math.Round(mp.Stamina, 1);
                var staminaMax = Math.Round(mp.StaminaMax, 1);
                string text = $"Stamina: {stamina}/{staminaMax}";
                Vector2 pos = Main.MouseScreen + Vector2.One * 16;
                pos.X = Math.Min(Main.screenWidth - Main.fontMouseText.MeasureString(text).X - 6, pos.X);
                Utils.DrawBorderString(spriteBatch, text, pos, Main.mouseTextColorReal);
            }

            Recalculate();
        }
    }

    internal class Stam : UIElement
    {
        float fade;
        int time;
        public static Texture2D overrideTexture = null;
        public static List<string> specialVesselTextures = new List<string>();

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle dimensions = GetDimensions().ToRectangle();
            Player player = Main.LocalPlayer;
            AbilityHandler mp = player.GetHandler();

            Texture2D emptyTex = GetTexture("StarlightRiver/Assets/GUI/StaminaEmpty");
            Texture2D fillTex = overrideTexture is null ? GetTexture("StarlightRiver/Assets/GUI/Stamina") : overrideTexture;

            int row = 0;
            for (int k = 0; k <= mp.StaminaMax; k++)
            {
                if (k % 7 == 0 && k != 0) row++;

                Vector2 pos = row % 2 == 0 ? dimensions.TopLeft() + new Vector2(row * -18, k % 7 * 28) :
                    dimensions.TopLeft() + new Vector2(row * -18, 14 + k % 7 * 28);

                if (k >= mp.StaminaMax) //draws the incomplete vessel
                {
                    Texture2D shard1 = GetTexture("StarlightRiver/Assets/Abilities/Stamina1");
                    Texture2D shard2 = GetTexture("StarlightRiver/Assets/Abilities/Stamina2");

                    if (mp.ShardCount % 3 >= 1) spriteBatch.Draw(shard1, pos, shard1.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    if (mp.ShardCount % 3 >= 2) spriteBatch.Draw(shard2, pos, shard2.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    continue;
                }

                var slotTex = emptyTex;

                //if (k < specialVesselTextures.Count)
                //slotTex = GetTexture(specialVesselTextures[k]);

                if (k >= mp.StaminaMax - specialVesselTextures.Count)
                    slotTex = GetTexture(specialVesselTextures[(int)mp.StaminaMax - k - 1]);

                spriteBatch.Draw(slotTex, pos, emptyTex.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                // If on a filled stamina vessel
                if (k < mp.Stamina - 1)
                    spriteBatch.Draw(fillTex, pos + Vector2.One * 4, Color.White);
                // If on the last stamina vessel
                else if (k <= mp.Stamina)
                {
                    float scale = mp.Stamina - k;
                    spriteBatch.Draw(fillTex, pos + Vector2.One * 4 + fillTex.Size() / 2, fillTex.Frame(), Color.White, 0, fillTex.Size() / 2, scale, 0, 0);
                }
            }

            switch (GetInstance<GUIConfig>().OverheadStaminaState)
            {
                case OverlayState.AlwaysOn:
                    time = 120;
                    break;

                case OverlayState.WhileNotFull:
                    if (mp.Stamina < mp.StaminaMax)
                        time = 120;
                    break;

                case OverlayState.WhileUsing:
                    if (mp.ActiveAbility != null)
                        time = 120;
                    break;

                case OverlayState.Never:
                    time = 0;
                    break;
            }

            if (time > 0)
            {
                fade += 0.1f;
                time--;
            }
            else
                fade -= 0.1f;


            fade = MathHelper.Clamp(fade, 0, 1);

            DrawOverhead(spriteBatch);

            overrideTexture = null;
            specialVesselTextures.Clear();
        }

        private void DrawOverhead(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            AbilityHandler mp = player.GetHandler();
            Vector2 basepos = player.Center - Main.screenPosition - Vector2.UnitY * 48;

            var flagTex = GetTexture("StarlightRiver/Assets/GUI/StaminaFlag");
            var emptyTex = GetTexture("StarlightRiver/Assets/GUI/StaminaSmallEmpty");
            var fillTex = GetTexture("StarlightRiver/Assets/GUI/StaminaSmall");

            var width = mp.StaminaMax * (fillTex.Width / 2 + 1);

            spriteBatch.Draw(flagTex, basepos + new Vector2(-width / 2 - 9, -3), Color.White * fade);

            if (mp.StaminaMax % 2 == 1)
                spriteBatch.Draw(flagTex, basepos + new Vector2(width / 2 - 9, -3), Color.White * fade);
            else
                spriteBatch.Draw(flagTex, basepos + new Vector2(width / 2 - 9, -5), null, Color.White * fade, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);

            for (int k = 0; k < mp.StaminaMax; k++)
            {
                var x = k * (fillTex.Width / 2 + 1) - width / 2;
                var y = k % 2 == 0 ? -4 : 2;

                var pos = basepos + new Vector2(x, y);

                spriteBatch.Draw(emptyTex, pos, null, Color.White * fade, 0, fillTex.Size() / 2, 1, 0, 0);

                if (mp.Stamina >= k)
                {
                    var scale = MathHelper.Clamp(mp.Stamina - k, 0, 1);
                    spriteBatch.Draw(fillTex, pos, null, Color.White * scale * fade, 0, fillTex.Size() / 2, scale, 0, 0);
                }
            }
        }
    }
}
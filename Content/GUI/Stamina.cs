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
            Player Player = Main.LocalPlayer;
            AbilityHandler mp = Player.GetHandler();

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

            if (Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBars")
            {
                Stam1.Left.Set(-306 + 202, 1);
                Stam1.Top.Set(110 - 44, 0);
                Stam1.Height.Set(32, 0);
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
                pos.X = Math.Min(Main.screenWidth - Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text).X - 6, pos.X);
                Utils.DrawBorderString(spriteBatch, text, pos, Main.MouseTextColorReal);
            }

            Recalculate();
        }
    }

    internal class Stam : UIElement
    {       
        public static Texture2D overrideTexture = null;
        public static List<string> specialVesselTextures = new List<string>();

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle dimensions = GetDimensions().ToRectangle();
            Player Player = Main.LocalPlayer;
            AbilityHandler mp = Player.GetHandler();

            if(Main.ResourceSetsManager.ActiveSetKeyName == "HorizontalBars")
			{
                Texture2D ornament = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaBarOrnament").Value;
                Texture2D empty = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaBarEmpty").Value;
                Texture2D fill = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaBarFill").Value;
                Texture2D edge = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaBarEdge").Value;

                Vector2 pos = dimensions.TopLeft();

                pos.X += 12;

                for(int k = 0; k < mp.StaminaMax; k++)
				{                
                    spriteBatch.Draw(empty, pos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    if(mp.Stamina - 1 >= k)
                        spriteBatch.Draw(fill, pos + new Vector2(0, 6), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    else if (mp.Stamina > k)
					{
                        var width = (int)(fill.Width * (mp.Stamina % 1f));
                        var target = new Rectangle((int)pos.X + (fill.Width - width), (int)pos.Y + 6, width, fill.Height);
                        spriteBatch.Draw(fill, target, new Rectangle(fill.Width - width, 0, width, fill.Height), Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    }

                    pos.X -= 12;
                }

                pos.X += 6;
                spriteBatch.Draw(edge, pos, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                pos = dimensions.TopLeft();
                spriteBatch.Draw(ornament, pos + new Vector2(0, -2), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                return;
			}

            Texture2D emptyTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaEmpty").Value;
            Texture2D fillTex = overrideTexture is null ? Request<Texture2D>("StarlightRiver/Assets/GUI/Stamina").Value : overrideTexture;

            if (Main.ResourceSetsManager.ActiveSetKeyName == "New")
            {
                emptyTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaEmptyFancy").Value;
                fillTex = overrideTexture is null ? Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaFancy").Value : overrideTexture;
            }

            int row = 0;
            for (int k = 0; k <= mp.StaminaMax; k++)
            {
                if (k % 7 == 0 && k != 0) row++;

                Vector2 pos = row % 2 == 0 ? dimensions.TopLeft() + new Vector2(row * -18, k % 7 * 24) :
                    dimensions.TopLeft() + new Vector2(row * -18, 12 + k % 7 * 24);

                if (k >= mp.StaminaMax) //draws the incomplete vessel
                {
                    Texture2D shard1 = Request<Texture2D>("StarlightRiver/Assets/Abilities/Stamina1").Value;
                    Texture2D shard2 = Request<Texture2D>("StarlightRiver/Assets/Abilities/Stamina2").Value;

                    if (mp.ShardCount % 3 >= 1) spriteBatch.Draw(shard1, pos, shard1.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    if (mp.ShardCount % 3 >= 2) spriteBatch.Draw(shard2, pos, shard2.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    continue;
                }

                var slotTex = emptyTex;

                if(k == 0 && Main.ResourceSetsManager.ActiveSetKeyName == "New") //Maybe not the most elegant solution but a functional one
                    slotTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaEmptyFancyFirst").Value;

                if (k >= mp.StaminaMax - specialVesselTextures.Count)
                    slotTex = Request<Texture2D>(specialVesselTextures[(int)mp.StaminaMax - k - 1]).Value;

                spriteBatch.Draw(slotTex, pos, slotTex.Frame(), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

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

            overrideTexture = null;
            specialVesselTextures.Clear();
        }
    }
}
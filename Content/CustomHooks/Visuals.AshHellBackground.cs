using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.CustomHooks
{
    class AshHellBackground : HookGroup
    {
        //Creates a RenderTarget and then renders the banners in the vitric desert. Should be fairly safe, as its just drawing.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            On.Terraria.Main.DrawUnderworldBackground += DrawAltBackground;
        }

        //this is just gross stolen vanilla code. I'll replace it later. TODO: Replace this later
        private void DrawAltBackground(On.Terraria.Main.orig_DrawUnderworldBackground orig, Main self, bool flat)
        {
            orig(self, flat);
            if (Main.gameMenu) return; //safety net

            if (Main.LocalPlayer.GetModPlayer<BiomeHandler>().zoneAshhell)
            {
                //just a vanilla zoing for now to test 
                if (Main.screenPosition.Y + Main.screenHeight < (Main.maxTilesY - 220) * 16f) return;

                Vector2 vector = Main.screenPosition + new Vector2((Main.screenWidth >> 1), (Main.screenHeight >> 1));
                float num = (Main.GameViewMatrix.Zoom.Y - 1f) * 0.5f * 200f;

                for (int i = 3; i >= 0; i--)
                {
                    Texture2D tex = ModContent.GetTexture("StarlightRiver/Assets/Backgrounds/AshHell" + i);
                    Vector2 vector2 = new Vector2(tex.Width, tex.Height) * 0.5f;
                    float num2 = flat ? 1f : (i * 2 + 3f);
                    Vector2 vector3 = new Vector2(1f / num2);
                    Rectangle rectangle = new Rectangle(0, 0, tex.Width, tex.Height);
                    float num3 = 1.3f;
                    Vector2 zero = Vector2.Zero;

                    switch (i)
                    {
                        case 1:
                            {
                                int num4 = (int)(Main.GlobalTime * 8f) % 4;
                                rectangle = new Rectangle((num4 >> 1) * (tex.Width >> 1), num4 % 2 * (tex.Height >> 1), tex.Width >> 1, tex.Height >> 1);
                                vector2 *= 0.5f;
                                zero.Y += 75f;
                                break;
                            }

                        case 2:
                        case 3: zero.Y += 75f; break;

                        case 4:
                            num3 = 0.5f;
                            zero.Y -= 25f;
                            break;
                    }

                    if (flat) num3 *= 1.5f;

                    vector2 *= num3;
                    if (flat) zero.Y += (ModContent.GetTexture("StarlightRiver/Assets/Backgrounds/AshHell0").Height >> 1) * 1.3f - vector2.Y;

                    zero.Y -= num;
                    float num5 = num3 * rectangle.Width;
                    int num6 = (int)((vector.X * vector3.X - vector2.X + zero.X - (Main.screenWidth >> 1)) / num5);
                    for (int j = num6 - 2; j < num6 + 4 + (int)(Main.screenWidth / num5); j++)
                    {
                        Vector2 vector4 = (new Vector2(j * num3 * (rectangle.Width / vector3.X), (Main.maxTilesY - 200) * 16f) + vector2 - vector) * vector3 + vector - Main.screenPosition - vector2 + zero;
                        Main.spriteBatch.Draw(tex, vector4, new Rectangle?(rectangle), Color.White, 0f, Vector2.Zero, num3, SpriteEffects.None, 0f);

                        if (i == 0)
                        {
                            int num7 = (int)(vector4.Y + rectangle.Height * num3);
                            Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle((int)vector4.X, num7, (int)(rectangle.Width * num3), Math.Max(0, Main.screenHeight - num7)), new Color(11, 3, 7));
                        }
                    }
                }
            }
        }
    }
}
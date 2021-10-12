using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	class PlayerTarget : HookGroup
    {
        //Drawing Player to Target. Should be safe. Excuse me if im duplicating something that alr exists :p
        public override SafetyLevel Safety => SafetyLevel.Safe;

        private MethodInfo playerDrawMethod;

        public static RenderTarget2D Target;
        //Ill move it later :P
        public static RenderTarget2D ScaledTileTarget { get; set; }

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            ScaledTileTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);

            playerDrawMethod = typeof(Main).GetMethod("DrawPlayer_DrawAllLayers", BindingFlags.NonPublic|BindingFlags.Instance);

            Main.OnPreDraw += Main_OnPreDraw;
        }

        private void Main_OnPreDraw(GameTime obj)
        {
            if (Main.gameMenu)
                return;

            RenderTargetBinding[] oldtargets2 = Main.graphics.GraphicsDevice.GetRenderTargets();
            Main.graphics.GraphicsDevice.SetRenderTarget(Target);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Main.spriteBatch.Begin();

            if(Main.LocalPlayer.dye.Length > 0)
			    playerDrawMethod?.Invoke(Main.instance, new object[] { Main.LocalPlayer, -1, Main.LocalPlayer.dye[0].IsAir ? 0 : Main.LocalPlayer.dye[0].dye });

            /*
            for (int i = 0; i <= Main.playerDrawData.Count; i++)
            {
                int num = -1;
                if (num != 0)
                {
                    Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                    num = 0;
                }

                if (i != Main.playerDrawData.Count)
                {
                    DrawData value = Main.playerDrawData[i];
                    if (value.shader >= 0)
                    {
                        GameShaders.Hair.Apply(0, Main.LocalPlayer, value);
                        GameShaders.Armor.Apply(value.shader, Main.LocalPlayer, value);
                    }
                    else if (Main.LocalPlayer.head == 0)
                    {
                        GameShaders.Hair.Apply(0, Main.LocalPlayer, value);
                        GameShaders.Armor.Apply(Main.LocalPlayer.cHead, Main.LocalPlayer, value);
                    }
                    else
                    {
                        GameShaders.Armor.Apply(0, Main.LocalPlayer, value);
                        GameShaders.Hair.Apply((short)(-value.shader), Main.LocalPlayer, value);
                    }
                    if (!value.sourceRect.HasValue)
                    {
                        value.sourceRect = value.texture.Frame();
                    }
                    num = value.shader;
                    if (value.texture != null)
                    {
                        Main.spriteBatch.Draw(value.texture, value.position, value.sourceRect, value.color, value.rotation, value.origin, value.scale, value.effect, 0f);
                    }
                }
            }*/

            Main.spriteBatch.End();
            Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets2);

            if (Main.instance.tileTarget.IsDisposed) return;

            RenderTargetBinding[] oldtargets1 = Main.graphics.GraphicsDevice.GetRenderTargets();

            Matrix matrix = Main.GameViewMatrix.ZoomMatrix;

            GraphicsDevice GD = Main.graphics.GraphicsDevice;
            SpriteBatch sb = Main.spriteBatch;

            GD.SetRenderTarget(ScaledTileTarget);
            GD.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            Main.spriteBatch.Draw(Main.instance.tileTarget, Main.sceneTilePos - Main.screenPosition, Color.White);
            sb.End();

            Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets1);

        }
    }
}
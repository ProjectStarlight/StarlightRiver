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
	public class StaminaOverhead : IOrderedLoadable
	{
        float fade;
        int time;

        public float Priority => 1;

		public void Load()
		{
            StarlightPlayer.PreDrawEvent += DrawOverhead;
		}

		public void Unload()
		{
			throw new NotImplementedException();
		}

		private void DrawOverhead(Player player, SpriteBatch spriteBatch)
        {
            if (player != Main.LocalPlayer)
                return;

            AbilityHandler mp = player.GetHandler();
            Vector2 basepos = player.Center - Main.screenPosition - Vector2.UnitY * 48;

            var flagTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaFlag").Value;
            var emptyTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaSmallEmpty").Value;
            var fillTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaSmall").Value;

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

            if (time > 0)
            {
                fade += 0.1f;
                time--;
            }
            else
                fade -= 0.1f;


            fade = MathHelper.Clamp(fade, 0, 1);

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
        }
    }
}

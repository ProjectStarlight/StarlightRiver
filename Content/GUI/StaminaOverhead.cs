using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Configs;
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

		public void Unload() { }

		private void DrawOverhead(Player player, SpriteBatch spriteBatch)
		{
			if (player != Main.LocalPlayer)
				return;//a

			AbilityHandler mp = player.GetHandler();
			Vector2 basepos = player.Center - Main.screenPosition - Vector2.UnitY * 48 + Vector2.UnitX * 4;
			basepos.Y += player.gfxOffY;

			Texture2D flagTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaFlag").Value;
			Texture2D emptyTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaSmallEmpty").Value;
			Texture2D fillTex = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaSmall").Value;

			float width = mp.StaminaMax * (fillTex.Width / 2 + 1);

			spriteBatch.Draw(flagTex, basepos + new Vector2(-width / 2 - 9, -3), Color.White * fade);

			if (mp.StaminaMax % 2 == 1)
				spriteBatch.Draw(flagTex, basepos + new Vector2(width / 2 - 9, -3), Color.White * fade);
			else
				spriteBatch.Draw(flagTex, basepos + new Vector2(width / 2 - 9, -5), null, Color.White * fade, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);

			for (int k = 0; k < mp.StaminaMax; k++)
			{
				float x = k * (fillTex.Width / 2 + 1) - width / 2;
				int y = k % 2 == 0 ? -4 : 2;

				Vector2 pos = basepos + new Vector2(x, y);

				spriteBatch.Draw(emptyTex, pos, null, Color.White * fade, 0, fillTex.Size() / 2, 1, 0, 0);

				if (mp.Stamina >= k)
				{
					float scale = MathHelper.Clamp(mp.Stamina - k, 0, 1);
					spriteBatch.Draw(fillTex, pos, null, Color.White * scale * fade, 0, fillTex.Size() / 2, scale, 0, 0);
				}
			}

			if (time > 0)
			{
				fade += 0.1f;
				time--;
			}
			else
			{
				fade -= 0.1f;
			}

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

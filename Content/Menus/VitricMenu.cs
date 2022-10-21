using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.CustomHooks;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Menus
{
	internal class VitricMenu : ModMenu
	{
		public override string DisplayName => "Vitric";
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/GlassPassive");

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			VitricBackground.DrawTitleVitricBackground();
			return true;
		}
	}
}

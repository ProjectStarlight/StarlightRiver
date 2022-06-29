using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Backgrounds;
using StarlightRiver.Content.CustomHooks;

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

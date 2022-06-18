using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Systems
{
	internal class DebugSystem : ModSystem
	{
		public override void Load()
		{
			On.Terraria.Main.Update += DoUpdate;
		}

		private void DoUpdate(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
		{
			if (!StarlightRiver.DebugMode)
			{
				orig(self, gameTime);
				return;
			}
				
			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y)) //Boss Speed Up Key
			{
				for (int k = 0; k < 3; k++)
				{
					orig(self, gameTime);
				}
			}

			orig(self, gameTime);
		}
	}
}

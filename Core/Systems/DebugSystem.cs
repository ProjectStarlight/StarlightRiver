namespace StarlightRiver.Core.Systems
{
	internal class DebugSystem : ModSystem
	{
		int timer = 0;

		public override void Load()
		{
			On.Terraria.Main.Update += DoUpdate;
		}

		private void DoUpdate(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
		{
			if (Main.LocalPlayer.position == Vector2.Zero || float.IsNaN(Main.LocalPlayer.position.X) || float.IsNaN(Main.LocalPlayer.position.Y))
				Main.LocalPlayer.position = new Vector2(Main.spawnTileX * 16, Main.spawnTileY * 16);

			if (!StarlightRiver.debugMode)
			{
				orig(self, gameTime);
				return;
			}

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y)) //Boss Speed Up Key
			{
				for (int k = 0; k < 8; k++)
				{
					orig(self, gameTime);
				}
			}

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.U)) //Boss Slow Down Key
			{
				if (timer % 2 == 0)
					orig(self, gameTime);

				timer++;

				return;
			}

			orig(self, gameTime);
		}
	}
}

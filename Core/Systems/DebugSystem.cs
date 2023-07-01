using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Core.Systems
{
	internal class DebugSystem : ModSystem
	{
		int timer = 0;

		public override void Load()
		{
			On_Main.Update += DoUpdate;
			On_Main.DrawInterface += DrawDebugMenu;
		}

		private void DrawDebugMenu(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			orig(self, gameTime);

			if (!StarlightRiver.debugMode || Main.playerInventory)
				return;

			string menu = "Debug mode options:\n " +
				"Y: Hold to speed up game\n " +
				"U: Hold to slow down game\n " +
				"P: Press to change difficulty";

			Main.spriteBatch.Begin();
			Utils.DrawBorderString(Main.spriteBatch, menu, new Vector2(32, 120), new Color(230, 230, 255));
			Main.spriteBatch.End();
		}

		private void DoUpdate(On_Main.orig_Update orig, Main self, GameTime gameTime)
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

			if (Main.oldKeyState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.P) && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P)) //Difficulty toggle key
			{
				if (!Main.expertMode)
				{
					Main.GameMode = GameModeID.Expert;
					Main.NewText("The game is now in expert mode.", new Color(255, 150, 0));
				}
				else if (!Main.masterMode)
				{
					Main.GameMode = GameModeID.Master;
					Main.NewText("The game is now in master mode.", new Color(255, 0, 0));
				}
				else
				{
					Main.GameMode = GameModeID.Normal;
					Main.NewText("The game is now in normal mode.", new Color(180, 180, 255));
				}
			}

			orig(self, gameTime);
		}
	}

	/// <summary>
	/// Marks a ModItem as a debug item, which will be disabled in normal gameplay unless debug mode is turned on
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	internal class SLRDebugAttribute : Attribute
	{

	}

	/// <summary>
	/// Cancels various behaviors of debug items when debug mode is not enabled
	/// </summary>
	internal class DebugGlobalItem : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			if (entity.ModItem is null)
				return false;

			return Attribute.IsDefined(entity.ModItem.GetType(), typeof(SLRDebugAttribute));
		}

		public override void UpdateInventory(Item item, Player player)
		{
			if (!StarlightRiver.debugMode)
				item.TurnToAir();
		}

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			return StarlightRiver.debugMode;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (!StarlightRiver.debugMode)
			{
				tooltips.Clear();

				TooltipLine line = new(Mod, "SLRDebug", "[DISABLED]\ndebug only")
				{
					OverrideColor = Color.Red
				};

				tooltips.Add(line);
			}
		}

		public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
		{
			if (!StarlightRiver.debugMode)
				item.TurnToAir();
		}

		public override bool CanPickup(Item item, Player player)
		{
			return StarlightRiver.debugMode;
		}

		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			return StarlightRiver.debugMode;
		}
	}
}
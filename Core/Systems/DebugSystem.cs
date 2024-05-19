using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Core.Systems
{
	internal class DebugSystem : ModSystem
	{
		readonly int timer = 0;

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

			string menu = "Debug mode enabled!";

			Main.spriteBatch.Begin();
			Utils.DrawBorderString(Main.spriteBatch, menu, new Vector2(32, 120), new Color(230, 230, 255));
			Main.spriteBatch.End();
		}

		private void DoUpdate(On_Main.orig_Update orig, Main self, GameTime gameTime)
		{
			if (Main.LocalPlayer.position == Vector2.Zero || float.IsNaN(Main.LocalPlayer.position.X) || float.IsNaN(Main.LocalPlayer.position.Y))
				Main.LocalPlayer.position = new Vector2(Main.spawnTileX * 16, Main.spawnTileY * 16);

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

				TooltipLine line = new(Mod, "SLR{{Debug}}", "[DISABLED]\n{{Debug}} only")
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
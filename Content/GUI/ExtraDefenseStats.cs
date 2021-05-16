using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	class ExtraDefenseStats : SmartUIState
	{
		public int Timer = 0;
		public Vector2 basePos;

		public ExtraDefenseInfoPanel DoTResistPanel = new ExtraDefenseInfoPanel(ModContent.GetTexture(AssetDirectory.GUI + "DoTResistBG"), 1);

		public override bool Visible => Main.playerInventory;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(n => n.Name == "Vanilla: Inventory") + 1;
		}

		public override void OnInitialize()
		{
			Append(DoTResistPanel);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var player = Main.LocalPlayer;

			int mapHeight = 0;
			if (Main.mapEnabled)
			{
				if (!Main.mapFullscreen && Main.mapStyle == 1)
					mapHeight = 256;

				if (mapHeight + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight)
					mapHeight = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
			}

			int slotsOff = 10 + player.extraAccessorySlots;

			if (slotsOff == 10 && (player.armor[8].type > 0 || player.armor[18].type > 0 || player.dye[8].type > 0))
				slotsOff = 9;

			if (Main.screenHeight < 900 && slotsOff == 10)
				slotsOff--;

			int xOff = Main.screenWidth - 92;
			int yOff = (int)((174 + mapHeight) + (slotsOff * 56) * Main.inventoryScale);

			Vector2 vector = new Vector2(xOff - 118, yOff + Main.inventoryBackTexture.Height * 0.5f);
			var defenseRect = Utils.CenteredRectangle(vector, Main.extraTexture[58].Size());
			basePos = defenseRect.Center.ToVector2();

			if (defenseRect.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface) //zoinked form vanilla DrawInventory() at Main.cs
			{
				if(Timer < 25)
					Timer++;

				var ResistPlayer = player.GetModPlayer<DoTResistancePlayer>();

				int resistPercent = (int)Math.Round(ResistPlayer.DoTResist * 100, MidpointRounding.AwayFromZero);
				DoTResistPanel.value = $"{resistPercent}%";
				Main.hoverItemName += $"\n{resistPercent}% DoT Resistance";
			}
			else if (Timer > 0)
				Timer--;

			base.Draw(spriteBatch);
		}
	}

	class ExtraDefenseInfoPanel : UIElement
	{
		private readonly Texture2D texture;
		private readonly int offsetPosition;
		public string value = "";

		public ExtraDefenseStats ParentState => Parent as ExtraDefenseStats;
		private Vector2 endPos => ParentState.basePos + new Vector2(offsetPosition * -Main.extraTexture[58].Width - offsetPosition * 6, 0);

		public ExtraDefenseInfoPanel(Texture2D texture, int offsetPosition)
		{
			this.texture = texture;
			this.offsetPosition = offsetPosition;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (ParentState is null)
				return;

			float progress = ParentState.Timer / 25f;
			Vector2 pos = Vector2.SmoothStep(ParentState.basePos, endPos, progress);
			spriteBatch.Draw(texture, pos, null, Color.White * progress, 0, texture.Size() / 2, 0.8f * progress, 0, 0);
			Utils.DrawBorderString(spriteBatch, value, pos, Color.White * progress, 0.8f * progress, 0.5f, 0.5f);

			base.Draw(spriteBatch);
		}
	}
}

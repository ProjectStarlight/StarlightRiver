using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BarrierSystem;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	class ExtraDefenseStats : SmartUIState
	{
		public int Timer = 0;
		public Vector2 basePos;

		public bool Open;

		public ExtraDefenseInfoPanel DefensePanel = new(Assets.GUI.DefenseBG, 0);
		public ExtraDefenseInfoPanel EndurancePanel = new(Assets.GUI.EnduranceBG, 1);
		public ExtraDefenseInfoPanel BarrierPanel = new(Assets.GUI.BarrierBG, 2);
		public ExtraDefenseInfoPanel LifePanel = new(Assets.GUI.LifeBG, 3);
		public ExtraDefenseInfoPanel DoTResistPanel = new(Assets.GUI.DoTResistBG, 4);


		public override bool Visible => Main.playerInventory;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(n => n.Name == "Vanilla: Inventory") + 1;
		}

		public override void OnInitialize()
		{
			DefensePanel.title = "Defense";
			DefensePanel.color = new Color(210, 200, 220);
			Append(DefensePanel);

			EndurancePanel.title = "Endurance";
			EndurancePanel.color = new Color(255, 240, 150);
			Append(EndurancePanel);

			BarrierPanel.title = "Barrier";
			BarrierPanel.color = new Color(150, 255, 255);
			Append(BarrierPanel);

			LifePanel.title = "Life";
			LifePanel.color = new Color(255, 150, 150);
			Append(LifePanel);

			DoTResistPanel.title = "Inoculation";
			DoTResistPanel.color = new Color(170, 255, 150);
			Append(DoTResistPanel);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Player Player = Main.LocalPlayer;

			int mapHeight = 0;
			if (Main.mapEnabled)
			{
				if (!Main.mapFullscreen && Main.mapStyle == 1)
					mapHeight = 256;

				if (mapHeight + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight)
					mapHeight = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
			}

			int slotsOff = 10 + Player.extraAccessorySlots;

			if (slotsOff == 10 && (Player.armor[8].type > ItemID.None || Player.armor[18].type > ItemID.None || Player.dye[8].type > ItemID.None))
				slotsOff = 9;

			if (Main.screenHeight < 900 && slotsOff == 10)
				slotsOff--;

			int xOff = Main.screenWidth - 92;
			int yOff = (int)(174 + mapHeight + slotsOff * 56 * Main.inventoryScale);

			var vector = new Vector2(xOff - 118, yOff + TextureAssets.InventoryBack.Value.Height * 0.5f);
			Rectangle defenseRect = Utils.CenteredRectangle(vector, TextureAssets.Extra[58].Size());
			basePos = defenseRect.Center.ToVector2();

			if (defenseRect.Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface && Main.mouseLeft && Main.mouseLeftRelease)
			{
				Open = !Open;
			}

			if (Open) //zoinked form vanilla DrawInventory() at Main.cs
			{
				if (Timer < 41)
					Timer++;

				DefensePanel.value = $"{Player.statDefense}";
				DefensePanel.extraInfo = $"Absorbed: {(int)(Player.statDefense * Player.DefenseEffectiveness.Value)}";

				EndurancePanel.value = $"{(int)Math.Round(Player.endurance * 100, MidpointRounding.AwayFromZero)}%";

				var barrierPlayer = Player.GetModPlayer<BarrierPlayer>();
				BarrierPanel.value = $"{barrierPlayer.maxBarrier}";
				BarrierPanel.extraInfo = 
					$"Reduction: {(int)Math.Round(barrierPlayer.barrierDamageReduction * 100, MidpointRounding.AwayFromZero)}%\n" +
					$"Regen: {barrierPlayer.rechargeRate}/s\n" +
					$"Delay: {barrierPlayer.rechargeDelay / 60f} s";

				LifePanel.value = $"{Player.statLifeMax2}";
				LifePanel.extraInfo = $"Regen: {Player.lifeRegen / 2f}/s";

				DoTResistancePlayer ResistPlayer = Player.GetModPlayer<DoTResistancePlayer>();
				DoTResistPanel.value = $"{(int)Math.Round(ResistPlayer.DoTResist * 100, MidpointRounding.AwayFromZero)}%";
			}
			else if (Timer > 0)
			{
				Timer--;
			}

			base.Draw(spriteBatch);
		}
	}

	class ExtraDefenseInfoPanel : SmartUIElement
	{
		private readonly Asset<Texture2D> texture;
		private readonly int offsetPosition;
		public string value = "";
		public string title = "";
		public string extraInfo = "";
		public Color color;

		public ExtraDefenseStats ParentState => Parent as ExtraDefenseStats;
		private Vector2 endPos => ParentState.basePos + new Vector2(-20 + offsetPosition * -92, 0);

		public ExtraDefenseInfoPanel(Asset<Texture2D> texture, int offsetPosition)
		{
			this.texture = texture;
			this.offsetPosition = offsetPosition;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (ParentState is null)
				return;

			var timer = ParentState.Timer - offsetPosition * 4;

			if (timer > 25)
				timer = 25;

			float progress = Helpers.Helper.SwoopEase(timer / 25f);
			var pos = Vector2.Lerp(ParentState.basePos, endPos, progress);

			spriteBatch.Draw(Assets.Keys.GlowSoft.Value, pos, null, Color.Black * progress, 0, new Vector2(32, 32), 1.3f, 0, 0);

			spriteBatch.Draw(texture.Value, pos, null, Color.White * progress, 0, texture.Size() / 2, progress, 0, 0);
			Utils.DrawBorderString(spriteBatch, value, pos, color * progress, 0.8f * progress, 0.5f, 0.5f);

			pos.Y += 6;
			Utils.DrawBorderString(spriteBatch, title, pos, Color.White * progress, 0.8f * progress, 0.5f, 0f);

			pos.Y += 18;
			Utils.DrawBorderString(spriteBatch, extraInfo, pos, Color.LightGray * progress, 0.7f * progress, 0.5f, 0f);

			base.Draw(spriteBatch);
		}
	}
}
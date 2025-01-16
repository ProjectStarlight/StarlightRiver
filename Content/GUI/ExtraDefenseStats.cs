using Microsoft.Xna.Framework.Graphics;
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
		public static int Timer = 0;
		public Vector2 basePos;

		public static bool Open;

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
			DefensePanel.tooltip = "Defense reduces damage you take by a flat amount. The amount is equal to your defense multiplied by your defense effect and rounded up";
			DefensePanel.color = new Color(255, 255, 255);
			Append(DefensePanel);

			EndurancePanel.title = "Endurance";
			EndurancePanel.tooltip = "Endurance reduces the damage you take by a percent of the damage dealt. This is taken into account after defense.";
			EndurancePanel.color = new Color(255, 240, 150);
			Append(EndurancePanel);

			BarrierPanel.title = "Barrier";
			BarrierPanel.tooltip = "Barrier reduces the damage you take by a percent of the damage dealt, at the cost of the original damage to your barrier. Your barrier recharges after a short period of not taking damage. It is calculated after endurance and defense.";
			BarrierPanel.color = new Color(150, 255, 255);
			Append(BarrierPanel);

			LifePanel.title = "Life";
			LifePanel.tooltip = "Your maximum life represents how much damage you can take before dying. It regenerates slowly over time, or more quickly if standing still.";
			LifePanel.color = new Color(255, 150, 150);
			Append(LifePanel);

			DoTResistPanel.title = "Inoculation";
			DoTResistPanel.tooltip = "Inoculation reduces the damage over time you take, such as from debuffs like 'On Fire!'. Inoculation over 100% will cause these effects to heal you instead by a proportional amount.";
			DoTResistPanel.color = new Color(170, 255, 150);
			Append(DoTResistPanel);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Player player = Main.LocalPlayer;

			basePos = AccessorySlotLoader.DefenseIconPosition;
			basePos.X -= 118;
			var defenseRect = new Rectangle((int)basePos.X - 19, (int)basePos.Y, 38, 42);

			Recalculate();

			if (Open)
			{
				if (Timer < 41)
					Timer++;

				DefensePanel.value = $"{player.statDefense}";
				DefensePanel.magnitude = player.statDefense / 100f;
				DefensePanel.extraInfo =
					$"Effect: {(int)Math.Round(player.DefenseEffectiveness.Value * 100, MidpointRounding.AwayFromZero)}%\n" +
					$"Absorbed: {(int)(player.statDefense * player.DefenseEffectiveness.Value)}\n";

				EndurancePanel.value = $"{(int)Math.Round(player.endurance * 100, MidpointRounding.AwayFromZero)}%";
				EndurancePanel.magnitude = player.endurance * 2f;

				BarrierPlayer barrierPlayer = player.GetModPlayer<BarrierPlayer>();
				BarrierPanel.value = $"{barrierPlayer.maxBarrier}";
				BarrierPanel.magnitude = barrierPlayer.maxBarrier / 500f;
				BarrierPanel.extraInfo =
					$"Effect: {(int)Math.Round(barrierPlayer.barrierDamageReduction * 100, MidpointRounding.AwayFromZero)}%\n" +
					$"Regen: {barrierPlayer.rechargeRate}/s\n" +
					$"Delay: {barrierPlayer.rechargeDelay / 60f} s";

				LifePanel.value = $"{player.statLifeMax2}";
				LifePanel.magnitude = player.statLifeMax2 / 800f;
				LifePanel.extraInfo = $"Regen: {player.lifeRegen / 2f}/s";

				DoTResistancePlayer ResistPlayer = player.GetModPlayer<DoTResistancePlayer>();
				DoTResistPanel.value = $"{(int)Math.Round(ResistPlayer.DoTResist * 100, MidpointRounding.AwayFromZero)}%";
				DoTResistPanel.magnitude = ResistPlayer.DoTResist;
			}
			else if (Timer > 0)
			{
				Timer--;
			}

			base.Draw(spriteBatch);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			basePos = AccessorySlotLoader.DefenseIconPosition;
			basePos.X -= 118;
			var defenseRect = new Rectangle((int)basePos.X - 19, (int)basePos.Y, 38, 42);

			if (defenseRect.Contains(new Point(Main.mouseX, Main.mouseY)))
			{
				Open = true;
				Main.LocalPlayer.mouseInterface = true;
			}
		}
	}

	class ExtraDefenseInfoPanel : SmartUIElement
	{
		private readonly Asset<Texture2D> texture;
		private readonly int offsetPosition;
		public string value = "";
		public string title = "";
		public string extraInfo = "";
		public string tooltip = "";
		public float magnitude = 0f;
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
			if (ParentState is null || ExtraDefenseStats.Timer == 0)
				return;

			int timer = ExtraDefenseStats.Timer - offsetPosition * 4;

			if (timer > 25)
				timer = 25;

			float progress = Helpers.Helper.SwoopEase(timer / 25f);
			var pos = Vector2.Lerp(ParentState.basePos, endPos, progress);

			Left.Set(pos.X - 46, 0);
			Top.Set(pos.Y - 46, 0);
			Width.Set(92, 0);
			Height.Set(92, 0);

			Recalculate();

			float cappedMagnitude = Math.Min(magnitude, 1f);

			spriteBatch.Draw(Assets.Keys.GlowAlpha.Value, pos, null, new Color(color.R, color.G, color.B, 0) * progress * (0.5f + cappedMagnitude * 0.5f), 0, new Vector2(80, 80), 0.6f + magnitude * 0.25f, 0, 0);

			if (magnitude >= 0.75f)
			{
				spriteBatch.Draw(Assets.StarTexture.Value, pos, null, new Color(color.R, color.G, color.B, 0) * progress * (0.25f + (cappedMagnitude - 0.75f) / 0.25f * 0.5f), 0, Assets.StarTexture.Size() / 2f, 0.4f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.05f, 0, 0);
			}

			spriteBatch.Draw(texture.Value, pos, null, Color.White * progress, 0, texture.Size() / 2, progress, 0, 0);
			Utils.DrawBorderString(spriteBatch, value, pos, color * progress, 0.8f * progress, 0.5f, 0.5f);

			pos.Y += 6;
			Utils.DrawBorderString(spriteBatch, title, pos, Color.White * progress, 0.85f * progress, 0.5f, 0f);

			pos.Y += 24;
			Utils.DrawBorderString(spriteBatch, extraInfo, pos, Color.LightGray * progress, 0.7f * progress, 0.5f, 0f);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Tooltip.SetName($"{value} {title}");
				Tooltip.SetTooltip(tooltip);
				Tooltip.SetColor(color);
			}

			base.Draw(spriteBatch);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (ExtraDefenseStats.Timer >= 41)
				ExtraDefenseStats.Open = false;
		}
	}

	class DefenseHider : ModSystem
	{
		public override void Load()
		{
			On_Main.DrawDefenseCounter += HideDefense;
		}

		private void HideDefense(On_Main.orig_DrawDefenseCounter orig, int inventoryX, int inventoryY)
		{
			if (ExtraDefenseStats.Timer == 0)
			{
				orig(inventoryX, inventoryY);

				float flash = 0.5f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.5f;

				Vector2 vector = new Vector2(inventoryX - 10 - 47 - 47 - 14, inventoryY + TextureAssets.InventoryBack.Height() * 0.5f);
				Main.spriteBatch.Draw(Assets.GUI.DefenseFlash.Value, vector, null, Color.White * flash, 0f, TextureAssets.Extra[58].Value.Size() / 2f, Main.inventoryScale, SpriteEffects.None, 0f);

				if (Utils.CenteredRectangle(vector, TextureAssets.Extra[58].Value.Size()).Contains(new Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface)
					Main.hoverItemName += "\nClick for defensive stats";
			}
		}
	}
}
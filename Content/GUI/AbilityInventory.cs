﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Configs;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class AbilityInventory : SmartUIState
	{
		public static Ability activeAbility;
		public static bool shouldReset = false;
		public static Dictionary<Type, Vector2> abilityIconPositions = new(); //to easily communicate ability icon positions to other UI

		private Vector2 lastConfigPos;

		public override bool Visible => Main.playerInventory && Main.LocalPlayer.chest == -1 && Main.npcShop == 0;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		private void AddAbility(Ability ability, Vector2 off)
		{
			var element = new AbilityDisplay(ability);
			AddElement(element, (int)off.X - 16, (int)off.Y - 16, 32, 32);

			abilityIconPositions.Add(ability.GetType(), new Vector2(off.X, off.Y));
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			Vector2 configPos = GetInstance<GUIConfig>().AbilityIconPosition;

			if (!Main.gameMenu && Elements.Count == 0 && Main.LocalPlayer.GetHandler() != null || shouldReset || configPos != lastConfigPos)
			{
				RemoveAllChildren();
				abilityIconPositions.Clear();

				Ability[] abilities = Ability.GetAbilityInstances();

				for (int k = 0; k < abilities.Length; k++)
				{
					Ability ability = abilities[k];
					Vector2 pos = configPos + new Vector2(-60, 0).RotatedBy(-k / (float)(abilities.Length - 1) * 3.14f);
					AddAbility(ability, pos);
				}

				lastConfigPos = configPos;
				shouldReset = false;
			}
		}
	}

	public class AbilityDisplay : SmartUIElement
	{
		private readonly Ability ability;

		public AbilityDisplay(Ability ability)
		{
			this.ability = ability;
		}

		static bool ReturnConditions()
		{
			return Main.InReforgeMenu;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (ReturnConditions())
				return;

			if (ability != null)
				AbilityInventory.activeAbility = ability;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (ReturnConditions())
				return;

			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;

			bool unlocked = Main.LocalPlayer.GetHandler().Unlocked(ability.GetType());
			Vector2 pos = GetDimensions().Center() - Vector2.One;

			if (!unlocked)
			{
				Texture2D glow = Assets.Masks.GlowAlpha.Value;
				Texture2D glow2 = Assets.StarTexture.Value;

				var backgroundColor = Color.Lerp(new Color(100, 180, 255), new Color(130, 220, 255), 0.5f + 0.5f * (float)Math.Sin(Main.GameUpdateCount * 0.05f));
				backgroundColor *= 0.25f + 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.05f);
				backgroundColor.A = 0;

				spriteBatch.Draw(glow, pos, null, backgroundColor, 0, glow.Size() / 2, 0.3f, 0, 0);
				spriteBatch.Draw(glow2, pos, null, backgroundColor, 0, glow2.Size() / 2, 0.1f, 0, 0);
			}
			else
			{
				Texture2D tex = ability.Texture.Value;

				spriteBatch.Draw(tex, pos, tex.Frame(), Color.White, 0, tex.Size() / 2, 1, 0, 0);

				if (IsMouseHovering)
				{
					Tooltip.SetName($"{ability.Name}");
					Tooltip.SetTooltip($"[i:StarlightRiver/StarlightHover][c/AAF0FF:{ability.ActivationCostDefault}] Starlight\n\n" + ability.Tooltip);
				}
			}
		}
	}
}
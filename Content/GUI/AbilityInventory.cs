using StarlightRiver.Content.Abilities;
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
			if (!Main.gameMenu && Elements.Count == 0 && Main.LocalPlayer.GetHandler() != null || shouldReset)
			{
				RemoveAllChildren();
				abilityIconPositions.Clear();

				Ability[] abilities = Ability.GetAbilityInstances();

				for (int k = 0; k < abilities.Length; k++)
				{
					Ability ability = abilities[k];
					Vector2 pos = new Vector2(100, 300) + new Vector2(-60, 0).RotatedBy(-k / (float)(abilities.Length - 1) * 3.14f);
					AddAbility(ability, pos);
				}

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

			Vector2 pos = GetDimensions().Center() - Vector2.One;
			Texture2D tex = !Main.LocalPlayer.GetHandler().Unlocked(ability.GetType()) ? Request<Texture2D>("StarlightRiver/Assets/GUI/blank").Value : Request<Texture2D>(ability.Texture).Value;

			spriteBatch.Draw(tex, pos, tex.Frame(), Color.White, 0, tex.Size() / 2, 1, 0, 0);

			/*if (Collection.ActiveAbility == Ability) //extra VFX
            {
                if (Ability is Dash)
                {
                    Texture2D dustex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/Fire").Value;

                    for (int k = 0; k < 3; k++)
                    {
                        float timer = (float)Math.Sin(LegendWorld.rottime);
                        Vector2 duspos = pos + new Vector2(2, 0) + new Vector2((float)Math.Sin(LegendWorld.rottime * 2 + k * 2) * (10 - timer * 5), timer * 15);
                        Collection.dust.Add(new ExpertDust(dustex, duspos, Vector2.Zero, new Color(200, 240, 255), 1.8f, 30));
                    }
                }

                if (Ability is Wisp)
                {
                    Texture2D dustex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/Fire").Value;

                    Vector2 duspos = pos + new Vector2((float)Math.Cos(LegendWorld.rottime) * 2, (float)Math.Sin(LegendWorld.rottime)) * 8f;
                    Collection.dust.Add(new ExpertDust(dustex, duspos, Vector2.Zero, new Color(255, 255, 150), 1.8f, 30));

                    Vector2 duspos2 = pos + new Vector2((float)Math.Cos(LegendWorld.rottime + 1), (float)Math.Sin(LegendWorld.rottime + 1) * 2) * 8f;
                    Collection.dust.Add(new ExpertDust(dustex, duspos2, Vector2.Zero, new Color(255, 255, 150), 1.8f, 30));

                    Vector2 duspos3 = pos + new Vector2((float)Math.Cos(LegendWorld.rottime + 2), (float)Math.Sin(LegendWorld.rottime + 2)) * 16f;
                    Collection.dust.Add(new ExpertDust(dustex, duspos3, Vector2.Zero, new Color(255, 255, 150), 1.8f, 30));
                }
            }*/

			if (IsMouseHovering)
			{
				Tooltip.SetName(ability.Name);
				Tooltip.SetTooltip(ability.Tooltip);
			}
		}
	}
}
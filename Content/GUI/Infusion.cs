using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Loaders.UILoading;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class Infusion : SmartUIState
	{
		public static int animationProgress = 20;
		public static Color animationColor;

		private readonly InfusionSlot[] slots = new InfusionSlot[InfusionSlots];
		private readonly SmartUIElement infusionElement = new();
		public static ParticleSystem linkParticles = new("StarlightRiver/Assets/Keys/GlowSoft", UpdateLinkDelegate);

		public override bool Visible => Main.LocalPlayer.GetHandler().StaminaMax != 0 && Main.playerInventory && Main.LocalPlayer.chest == -1 && Main.npcShop == 0;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		/// <summary>
		/// Returns a copy of the internal slots array.
		/// </summary>
		public InfusionSlot[] GetInfusionSlots()
		{
			return slots.ToArray();
		}

		internal const int InfusionSlots = 3;

		public override void OnInitialize()
		{
			// "StarlightRiver/Assets/GUI/Infusions" is 64x58
			// The texture is centered at 100, 300 so its top-left is 68, 272
			// The top slot's top-left corner is at 20, 4 on the texture
			// So the top-left slot should be positioned at 88, 276 in screenspace. (Add top-left of texture and top-left corner of top slot)

			infusionElement.Width.Set(64, 0);
			infusionElement.Height.Set(58, 0);
			// Calculating these instead of using magic numbers.
			infusionElement.Left.Set(100 - infusionElement.Width.Pixels / 2, 0);
			infusionElement.Top.Set(300 - infusionElement.Height.Pixels / 2, 0);

			const float width = 20;
			const float height = 22;
			const float topSlotLeft = 90;
			const float topSlotTop = 276;

			int targetSlot = 0;
			void InitSlot(float left, float top)
			{
				var slot = new InfusionSlot(targetSlot);
				slots[targetSlot] = slot;
				slot.Width.Set(width, 0);
				slot.Height.Set(height, 0);
				slot.Left.Set(left, 0);
				slot.Top.Set(top, 0);
				Append(slot);
				targetSlot++;
			}

			InitSlot(topSlotLeft, topSlotTop);
			InitSlot(topSlotLeft - width / 2 - 4, topSlotTop + height);
			InitSlot(topSlotLeft + width / 2 + 4, topSlotTop + height);
		}

		internal static bool ReturnConditions()
		{
			return Main.InReforgeMenu;
		}

		private static void UpdateLinkDelegate(Particle particle)
		{
			particle.Position += Vector2.Normalize(particle.StoredPosition - particle.Position);
			particle.Alpha = (float)Math.Sin(particle.Timer / particle.Velocity.X * 3.14f);
			particle.Timer--;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (ReturnConditions())
				return;

			AbilityHandler mp = Main.LocalPlayer.GetHandler();

			if (animationProgress < 40)
				animationProgress++;

			if (mp.InfusionLimit > 0)
			{
				Texture2D texture = Request<Texture2D>("StarlightRiver/Assets/GUI/InfusionFrame").Value;
				var source = new Rectangle(60 * (mp.InfusionLimit - 1), 0, 60, 56);
				spriteBatch.Draw(texture, new Vector2(infusionElement.Left.Pixels + 2, infusionElement.Top.Pixels), source, Color.White);

				Texture2D textureGlow = Request<Texture2D>("StarlightRiver/Assets/GUI/InfusionFrameFlash").Value;
				var sourceGlow = new Rectangle(60 * (mp.InfusionLimit - 1), (int)(animationProgress / 4f) * 56, 60, 56);
				spriteBatch.Draw(textureGlow, new Vector2(infusionElement.Left.Pixels + 6, infusionElement.Top.Pixels), sourceGlow, animationColor * (1 - animationProgress / 40f));
			}

			base.Draw(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

			linkParticles.DrawParticles(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			RemoveAllChildren();
			Initialize();
			Recalculate();
		}
	}

	public class InfusionSlot : SmartUIElement
	{
		public int TargetSlot { get; }

		public bool Unlocked => Main.LocalPlayer.GetHandler().InfusionLimit > TargetSlot;

		public InfusionSlot(int slot)
		{
			TargetSlot = slot;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (Infusion.ReturnConditions())
				return;

			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;

			AbilityHandler mp = Main.LocalPlayer.GetHandler();
			InfusionItem equipped = mp.GetInfusion(TargetSlot);

			if (!Unlocked) //draw a lock instead for locked slots
			{
				Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GUI/InfusionLock").Value;
				//spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0f, tex.Size() / 2, 1, SpriteEffects.None, 0);
			}

			//Draws the slot
			else if (equipped != null)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

				Texture2D glowTex = Request<Texture2D>("StarlightRiver/Assets/Abilities/HexGlow").Value;
				float sin = 0.75f + (float)Math.Sin(Main.GameUpdateCount / 20f) * 0.25f;
				spriteBatch.Draw(glowTex, GetDimensions().Center(), null, equipped.color * sin, 0, glowTex.Size() / 2, 1.2f, 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

				if (Main.rand.NextBool(5))
				{
					Vector2 targetPos = Collection.abilityIconPositions.ContainsKey(equipped.AbilityType) ? Collection.abilityIconPositions[equipped.AbilityType] - Vector2.One * 8 : Vector2.Zero;
					Vector2 startPos = GetDimensions().Center() + new Vector2(-6, -6);
					float dist = Vector2.Distance(targetPos, startPos);
					Infusion.linkParticles.AddParticle(new Particle(startPos, Vector2.UnitX * dist, 0, Main.rand.NextFloat(0.2f, 0.25f), equipped.color, (int)dist, targetPos));
				}

				//Draws the Item itself
				equipped.Draw(spriteBatch, GetInnerDimensions().Center() + Vector2.UnitY, 1);

				if (IsMouseHovering && Main.mouseItem.IsAir)
				{
					//Grabs the Items tooltip
					var ToolTip = new System.Text.StringBuilder();
					for (int k = 0; k < equipped.Item.ToolTip.Lines; k++)
						ToolTip.AppendLine(equipped.Item.ToolTip.GetLine(k));

					//Draws the name and tooltip at the mouse
					Utils.DrawBorderStringBig(spriteBatch, equipped.Name, Main.MouseScreen + new Vector2(22, 22), ItemRarity.GetColor(equipped.Item.rare).MultiplyRGB(Main.MouseTextColorReal), 0.39f);
					Utils.DrawBorderStringBig(spriteBatch, ToolTip.ToString(), Main.MouseScreen + new Vector2(22, 48), Main.MouseTextColorReal, 0.39f);
				}
			}

			// Draws the transparent visual
			else if (Main.mouseItem?.ModItem is InfusionItem mouseItem && mp.CanSetInfusion(mouseItem))
			{
				float opacity = 0.33f + (float)Math.Sin(StarlightWorld.visualTimer) * 0.25f;
				mouseItem.Draw(spriteBatch, GetDimensions().Center() + Vector2.UnitY, opacity);
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (Infusion.ReturnConditions())
				return;

			if (Unlocked && IsMouseHovering && ItemSlot.ShiftInUse && Main.LocalPlayer.GetHandler().GetInfusion(TargetSlot) != null)
				// Set cursor to the little chest icon
				Main.cursorOverride = 9;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (Infusion.ReturnConditions())
				return;

			if (!Unlocked)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Unlock);
				return;
			}

			AbilityHandler handler = Main.LocalPlayer.GetHandler();

			InfusionItem occupant = handler.GetInfusion(TargetSlot);

			if (ItemSlot.ShiftInUse)
			{
				if (occupant == null)
					return;

				// Find an open slot and pop it in there
				int slot = Array.FindIndex(Main.LocalPlayer.inventory, i => i == null || i.IsAir);
				if (slot > -1)
				{
					Main.LocalPlayer.inventory[slot] = occupant.Item;
					handler.SetInfusion(null, TargetSlot);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
					return;
				}
			}

			//if the Player is holding an infusion
			if (Main.mouseItem.ModItem is InfusionItem Item && handler.SetInfusion(Item, TargetSlot))
			{
				if (occupant == null)
					Main.mouseItem.TurnToAir();  //if nothing is equipped, equip the held Item
				else
					Main.mouseItem = occupant.Item; //if something is equipped, swap that for the held Item

				Infusion.animationProgress = 0;
				Infusion.animationColor = Item.color;
				Helpers.Helper.PlayPitched("Magic/Shadow2", 1, 0);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}

			//if the Player isnt holding anything but something is equipped, unequip it
			else if (occupant != null && Main.mouseItem.IsAir)
			{
				handler.SetInfusion(null, TargetSlot);

				Main.mouseItem = occupant.Item;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}
		}
	}
}
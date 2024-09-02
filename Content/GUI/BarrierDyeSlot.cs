﻿using StarlightRiver.Content.Items.BarrierDye;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	class BarrierDyeSlot : SmartUIState
	{
		private const int leftPos = -186;
		private int topPos = 430;

		private readonly BarrierDyeSlotElement slot = new();

		public override bool Visible => Main.playerInventory && Main.EquipPageSelected == 2;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(n => n.Name == "Vanilla: Inventory") + 1;
		}

		public override void OnInitialize()
		{
			slot.Left.Set(leftPos, 1);
			slot.Top.Set(topPos, 0);
			Append(slot);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			recalculateSlotPosition();
			base.Draw(spriteBatch);
		}

		private void recalculateSlotPosition()
		{
			int mapHeight = Main.mapStyle == 1 ? 256 : 0;

			if (mapHeight + 600 > Main.screenHeight)
				mapHeight = Main.screenHeight - 600;

			if (leftPos != mapHeight + 174)
			{
				topPos = mapHeight + 174;

				slot.Left.Set(leftPos, 1);
				slot.Top.Set(topPos, 0);

				Recalculate();
			}
		}
	}

	public class BarrierDyeSlotElement : SmartUIElement
	{
		public BarrierDyeSlotElement()
		{
			OnLeftClick += (a, b) => ClickHandler();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Player Player = Main.LocalPlayer;
			BarrierPlayer mp = Player.GetModPlayer<BarrierPlayer>();
			Item Item = mp.barrierDyeItem;

			Texture2D tex = TextureAssets.InventoryBack8.Value;
			Texture2D texSlot = Assets.GUI.BarrierDyeSlot.Value;

			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White * 0.8f, 0, tex.Size() / 2, 0.85f, 0, 0);
			spriteBatch.Draw(texSlot, GetDimensions().Center(), null, Color.White * 0.4f, 0, texSlot.Size() / 2, 0.85f, 0, 0);

			if (!Item.IsAir)
			{
				Texture2D tex2 = ModContent.Request<Texture2D>(Item.ModItem.Texture).Value;
				spriteBatch.Draw(tex2, GetDimensions().Center(), null, Color.White, 0, tex2.Size() / 2, 0.85f, 0, 0);
			}

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;

				if (Item.type != ModContent.ItemType<BaseBarrierDye>())
				{
					Main.HoverItem = Item.Clone();
					Main.hoverItemName = Item.Name;

					if (Main.keyState.PressingShift() && Helper.getFreeInventorySlot(Main.LocalPlayer) != -1)
						Main.cursorOverride = 7;
				}
				else
				{
					Main.hoverItemName = "Barrier Tincture";
				}
			}
		}

		public void ClickHandler()
		{
			Main.isMouseLeftConsumedByUI = true;
			Player Player = Main.LocalPlayer;
			BarrierPlayer mp = Player.GetModPlayer<BarrierPlayer>();
			Item Item = mp.barrierDyeItem;

			if (Item.type == ModContent.ItemType<BaseBarrierDye>())
				Item.TurnToAir();

			//shift left click means they want to quick place into inventory
			if (PlayerInput.Triggers.Current.SmartSelect)
			{
				int invSlot = Helper.getFreeInventorySlot(Main.LocalPlayer);

				if (!Item.IsAir && invSlot != -1)
				{
					Main.LocalPlayer.GetItem(Main.myPlayer, Item.Clone(), GetItemSettings.InventoryUIToInventorySettings);
					Item.TurnToAir();
				}

				return;
			}

			if (Main.mouseItem.IsAir && !Item.IsAir) //if the cursor is empty and there is something in the slot, take the Item out
			{
				Main.mouseItem = Item.Clone();
				mp.barrierDyeItem.TurnToAir();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
				mp.rechargeAnimationTimer = 0;
			}

			if (Player.HeldItem.ModItem is BarrierDye && Item.IsAir) //if the slot is empty and the cursor has an Item, put it in the slot
			{
				mp.barrierDyeItem = Player.HeldItem.Clone();
				Player.HeldItem.TurnToAir();
				Main.mouseItem.TurnToAir();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
				mp.rechargeAnimationTimer = 0;
			}

			if (Player.HeldItem.ModItem is BarrierDye && !Item.IsAir) //swap or stack
			{
				Item temp = Item;
				mp.barrierDyeItem = Player.HeldItem;
				Main.mouseItem = temp;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
				mp.rechargeAnimationTimer = 0;
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			Width.Set(44, 0);
			Height.Set(44, 0);
		}
	}
}
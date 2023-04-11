using StarlightRiver.Content.Items.BaseTypes;
using Terraria.UI;

namespace StarlightRiver.Content.CustomHooks
{
	public class AccessorySlotControl : HookGroup
	{
		//Should be a fairly stable hook in theory, but some vanilla behavior is repeated/replaced here. Could be refactored in the future, this is old code.
		public override void Load()
		{
			Terraria.UI.On_ItemSlot.LeftClick_ItemArray_int_int += HandleSpecialItemInteractions;
			Terraria.UI.On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
			Terraria.UI.On_ItemSlot.RightClick_ItemArray_int_int += NoSwapCurse;
		}

		private void HandleSpecialItemInteractions(Terraria.UI.On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			if (inv[slot].ModItem is CursedAccessory && context == 10) //|| inv[slot].ModItem is Blocker)
			{
				ItemLoader.CanEquipAccessory(Main.mouseItem, slot, true);
				return;
			}

			//if (Main.mouseItem.ModItem is SoulboundItem && (context != 0 || inv != Main.LocalPlayer.inventory))
			//    return;

			//if (inv[slot].ModItem is SoulboundItem && Main.keyState.PressingShift())
			//    return;

			orig(inv, context, slot);
		}

		private void NoSwapCurse(Terraria.UI.On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			Player Player = Main.player[Main.myPlayer];

			for (int i = 0; i < Player.armor.Length; i++)
			{
				if (Player.armor[i].ModItem is CursedAccessory && ItemSlot.ShiftInUse && inv[slot].accessory) //Player.armor[i].ModItem is Blocker)
				{
					return;
				}
			}

			if (inv == Player.armor)
			{
				Item swaptarget = Player.armor[slot - 10];

				if (context == 11 && swaptarget.ModItem is CursedAccessory)// || swaptarget.ModItem is Blocker || swaptarget.ModItem is InfectedAccessory)
					return;
			}

			orig(inv, context, slot);
		}

		private void DrawSpecial(Terraria.UI.On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb, Item[] inv, int context, int slot, Vector2 position, Color color)
		{
			//TODO: Rewrite this later to be less... noob looking.
			if (inv[slot].ModItem is CursedAccessory && context == 10)
			{
				Texture2D back = ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/CursedBack").Value;
				Color backcolor = (!Main.expertMode && slot == 8) ? Color.White * 0.25f : Color.White * 0.75f;

				sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
				RedrawItem(sb, inv, position, slot, color);
			}
			//else if ((inv[slot].ModItem is InfectedAccessory || inv[slot].ModItem is Blocker) && context == 10)
			//{
			//    Texture2D back = ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/InfectedBack").Value;
			//    Color backcolor = (!Main.expertMode && slot == 8) ? Color.White * 0.25f : Color.White * 0.75f;

			//    sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
			//    RedrawItem(sb, inv, back, position, slot, color);
			//}
			//else if (inv[slot].ModItem is PrototypeWeapon && inv[slot] != Main.mouseItem)
			//{
			//    Texture2D back = ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/ProtoBack").Value;
			//    Color backcolor = Main.LocalPlayer.HeldItem != inv[slot] ? Color.White * 0.75f : Color.Yellow;

			//    sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
			//    RedrawItem(sb, inv, back, position, slot, color);
			//}
			else
			{
				orig(sb, inv, context, slot, position, color);
			}
		}

		//this is vanilla code. I cant be assed to try to change this. Only alternative I see is porting this all to IL.
		internal static void RedrawItem(SpriteBatch sb, Item[] inv, Vector2 position, int slot, Color color)
		{
			Item Item = inv[slot];
			Vector2 scaleVector = Vector2.One * 52 * Main.inventoryScale;
			Texture2D PopupTexture = ModContent.Request<Texture2D>(Item.ModItem.Texture).Value;
			Rectangle source = PopupTexture.Frame(1, 1, 0, 0);
			Color currentColor = color;
			float scaleFactor2 = 1f;
			ItemSlot.GetItemLight(ref currentColor, ref scaleFactor2, Item, false);
			float scaleFactor = 1f;

			if (source.Width > 32 || source.Height > 32)
				scaleFactor = (source.Width <= source.Height) ? (32f / source.Height) : (32f / source.Width);

			scaleFactor *= Main.inventoryScale;
			Vector2 drawPos = position + scaleVector / 2f - source.Size() * scaleFactor / 2f;
			Vector2 origin = source.Size() * (scaleFactor2 / 2f - 0.5f);
			if (ItemLoader.PreDrawInInventory(Item, sb, drawPos, source, Item.GetAlpha(currentColor), Item.GetColor(color), origin, scaleFactor * scaleFactor2))
			{
				sb.Draw(PopupTexture, drawPos, source, Color.White, 0f, origin, scaleFactor * scaleFactor2, SpriteEffects.None, 0f);
			}

			ItemLoader.PostDrawInInventory(Item, sb, drawPos, source, Item.GetAlpha(currentColor), Item.GetColor(color), origin, scaleFactor * scaleFactor2);
		}
	}
}
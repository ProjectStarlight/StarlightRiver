using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace StarlightRiver.Content.CustomHooks
{
	class MouseSlotUsableInator : HookGroup
	{
		public override void Load()
		{
			On_Player.dropItemCheck += DontDropCoolStuff;
			Terraria.UI.On_ItemSlot.LeftClick_ItemArray_int_int += LockMouseToSpecialItem;
			Terraria.UI.On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;

			IL_Player.ScrollHotbar += AllowBigScrolling;
			//IL_Player.Update += AllowBigHotkeying; PORTTODO: Fix IL, never mind this is unused for now. fix later if we actually use it!
		}

		private void DrawSpecial(Terraria.UI.On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb, Item[] inv, int context, int slot, Vector2 position, Color color)
		{
			if (inv[slot].ModItem is InworldItem && !(inv[slot].ModItem as InworldItem).VisibleInUI)
				return;

			if (inv[slot].ModItem is InworldItem && context == 13)
			{
				Texture2D back = ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/TempBack").Value;
				var source = new Rectangle(0, 52 * (int)(Main.GameUpdateCount / 4 % 4), 52, 52);

				sb.Draw(back, position, source, Color.White, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
				AccessorySlotControl.RedrawItem(sb, inv, position, slot, color);
			}

			else
			{
				orig(sb, inv, context, slot, position, color);
			}
		}

		private void AllowBigHotkeying(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchStloc(29), n => n.MatchLdcI4(0), n => n.MatchStloc(30), n => n.MatchLdsfld<Main>("drawingPlayerChat"));
			c.Index += 9;

			ILLabel label = il.DefineLabel(c.Next);

			c.Index -= 4;
			c.EmitDelegate<Func<bool>>(ShouldAllowHotkey);
			c.Emit(OpCodes.Brtrue, label);
		}

		private bool ShouldAllowHotkey()
		{
			return Main.mouseItem.ModItem is InworldItem;
		}

		private void AllowBigScrolling(ILContext il)
		{
			var c = new ILCursor(il);
			c.TryGotoNext(n => n.MatchLdcI4(10));
			c.Index++;

			c.Emit(OpCodes.Pop);
			c.Emit(OpCodes.Ldc_I4, 60);
		}

		private void LockMouseToSpecialItem(Terraria.UI.On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			if (!(Main.mouseItem.ModItem is Core.InworldItem))
				orig(inv, context, slot);
		}

		private void DontDropCoolStuff(On_Player.orig_dropItemCheck orig, Terraria.Player self)
		{
			if (!(Main.mouseItem.ModItem is Core.InworldItem))
				orig(self);
		}
	}
}
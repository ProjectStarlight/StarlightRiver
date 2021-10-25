using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
//using StarlightRiver.Items.Prototypes;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace StarlightRiver.Content.CustomHooks
{
	public class AccessorySlotControl : HookGroup
    {
        //Should be a fairly stable hook in theory, but some vanilla behavior is repeated/replaced here. Could be refactored in the future, this is old code.
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public override void Load()
        {
            On.Terraria.UI.ItemSlot.LeftClick_ItemArray_int_int += HandleSpecialItemInteractions;
            On.Terraria.UI.ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
            On.Terraria.UI.ItemSlot.RightClick_ItemArray_int_int += NoSwapCurse;
        }

        private void HandleSpecialItemInteractions(On.Terraria.UI.ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if ((inv[slot].modItem is CursedAccessory || inv[slot].modItem is Blocker) && context == 10)
            {
                ItemLoader.CanEquipAccessory(Main.mouseItem, slot);
                return;
            }

            if (Main.mouseItem.modItem is SoulboundItem && (context != 0 || inv != Main.LocalPlayer.inventory))
                return;

            if (inv[slot].modItem is SoulboundItem && Main.keyState.PressingShift())
                return;

            orig(inv, context, slot);
        }

        private void NoSwapCurse(On.Terraria.UI.ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            Player player = Main.player[Main.myPlayer];

            for (int i = 0; i < player.armor.Length; i++)
            {
                if ((player.armor[i].modItem is CursedAccessory || player.armor[i].modItem is Blocker) && ItemSlot.ShiftInUse && inv[slot].accessory)
				{              
                    return;
                }                  
            }

            if (inv == player.armor)
            {
                Item swaptarget = player.armor[slot - 10];

                if (context == 11 && (swaptarget.modItem is CursedAccessory || swaptarget.modItem is Blocker || swaptarget.modItem is InfectedAccessory))
                    return;
            }

            orig(inv, context, slot);
        }

        private void DrawSpecial(On.Terraria.UI.ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb, Item[] inv, int context, int slot, Vector2 position, Color color)
        {
            //TODO: Rewrite this later to be less... noob looking.
            if ((inv[slot].modItem is CursedAccessory) && context == 10)
            {
                Texture2D back = ModContent.GetTexture("StarlightRiver/Assets/GUI/CursedBack");
                Color backcolor = (!Main.expertMode && slot == 8) ? Color.White * 0.25f : Color.White * 0.75f;

                sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
                RedrawItem(sb, inv, back, position, slot, color);
            }
            else if ((inv[slot].modItem is InfectedAccessory || inv[slot].modItem is Blocker) && context == 10)
            {
                Texture2D back = ModContent.GetTexture("StarlightRiver/Assets/GUI/InfectedBack");
                Color backcolor = (!Main.expertMode && slot == 8) ? Color.White * 0.25f : Color.White * 0.75f;

                sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
                RedrawItem(sb, inv, back, position, slot, color);
            }
            /*else if (inv[slot].modItem is PrototypeWeapon && inv[slot] != Main.mouseItem)
            {
                Texture2D back = ModContent.GetTexture("StarlightRiver/Assets/GUI/ProtoBack");
                Color backcolor = Main.LocalPlayer.HeldItem != inv[slot] ? Color.White * 0.75f : Color.Yellow;

                sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
                RedrawItem(sb, inv, back, position, slot, color);
            }*/
            else
            {
                orig(sb, inv, context, slot, position, color);
            }
        }

        //this is vanilla code. I cant be assed to try to change this. Only alternative I see is porting this all to IL.
        internal static void RedrawItem(SpriteBatch sb, Item[] inv, Texture2D back, Vector2 position, int slot, Color color)
        {
            Item item = inv[slot];
            Vector2 scaleVector = Vector2.One * 52 * Main.inventoryScale;
            Texture2D itemTexture = ModContent.GetTexture(item.modItem.Texture);
            Rectangle source = (itemTexture.Frame(1, 1, 0, 0));
            Color currentColor = color;
            float scaleFactor2 = 1f;
            ItemSlot.GetItemLight(ref currentColor, ref scaleFactor2, item, false);
            float scaleFactor = 1f;

            if (source.Width > 32 || source.Height > 32)
                scaleFactor = ((source.Width <= source.Height) ? (32f / source.Height) : (32f / source.Width));

            scaleFactor *= Main.inventoryScale;
            Vector2 drawPos = position + scaleVector / 2f - source.Size() * scaleFactor / 2f;
            Vector2 origin = source.Size() * (scaleFactor2 / 2f - 0.5f);
            if (ItemLoader.PreDrawInInventory(item, sb, drawPos, source, item.GetAlpha(currentColor), item.GetColor(color), origin, scaleFactor * scaleFactor2))
            {
                sb.Draw(itemTexture, drawPos, source, Color.White, 0f, origin, scaleFactor * scaleFactor2, SpriteEffects.None, 0f);
            }
            ItemLoader.PostDrawInInventory(item, sb, drawPos, source, item.GetAlpha(currentColor), item.GetColor(color), origin, scaleFactor * scaleFactor2);
        }
    }

}
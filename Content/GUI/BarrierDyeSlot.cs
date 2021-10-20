using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using StarlightRiver.Content.Items.BarrierDye;

namespace StarlightRiver.Content.GUI
{
	class BarrierDyeSlot : SmartUIState
	{
		public int Timer = 0;
		public Vector2 basePos;

        private BarrierDyeSlotElement slot = new BarrierDyeSlotElement();

		public override bool Visible => Main.playerInventory && Main.EquipPageSelected == 2;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(n => n.Name == "Vanilla: Inventory") + 1;
		}

		public override void OnInitialize()
		{
            slot.Left.Set(-100, 1);
            slot.Top.Set(400, 0);
            Append(slot);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
            slot.Left.Set(-186, 1);
            slot.Top.Set(430, 0);

            Recalculate();

            var player = Main.LocalPlayer;
			var mp = player.GetModPlayer<ShieldPlayer>();

			base.Draw(spriteBatch);
		}
	}

    public class BarrierDyeSlotElement : UIElement
    {
        public override void Draw(SpriteBatch spriteBatch)
        {
            var player = Main.LocalPlayer;
            var mp = player.GetModPlayer<ShieldPlayer>();
            var Item = mp.barrierDyeItem;

            if (IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            Texture2D tex = Main.inventoryBack8Texture; 
            Texture2D texSlot = ModContent.GetTexture("StarlightRiver/Assets/GUI/BarrierDyeSlot");

            spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White * 0.8f, 0, tex.Size() / 2, 0.85f, 0, 0);
            spriteBatch.Draw(texSlot, GetDimensions().Center(), null, Color.White * 0.4f, 0, texSlot.Size() / 2, 0.85f, 0, 0);

            if (!Item.IsAir)
            {
                Texture2D tex2 = ModContent.GetTexture(Item.modItem.Texture);
                spriteBatch.Draw(tex2, GetDimensions().Center(), null, Color.White, 0, tex2.Size() / 2, 0.85f, 0, 0);
            }

            if (IsMouseHovering)
            {
                if (Item.type != ModContent.ItemType<BaseBarrierDye>())
                {
                    Main.HoverItem = Item.Clone();
                    Main.hoverItemName = Item.Name;
                }
                else
				{
                    Main.hoverItemName = "Barrier Tincture";
                }
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            var player = Main.LocalPlayer;
            var mp = player.GetModPlayer<ShieldPlayer>();
            var Item = mp.barrierDyeItem;

            if (Item.type == ModContent.ItemType<BaseBarrierDye>())
                Item.TurnToAir();

            if (Main.mouseItem.IsAir && !Item.IsAir) //if the cursor is empty and there is something in the slot, take the item out
            {
                Main.mouseItem = Item.Clone();
                mp.barrierDyeItem.TurnToAir();
                Main.PlaySound(SoundID.Grab);
                mp.rechargeAnimation = 0;
            }

            if (player.HeldItem.modItem is BarrierDye && Item.IsAir) //if the slot is empty and the cursor has an item, put it in the slot
            {
                mp.barrierDyeItem = player.HeldItem.Clone();
                player.HeldItem.TurnToAir();
                Main.mouseItem.TurnToAir();
                Main.PlaySound(SoundID.Grab);
                mp.rechargeAnimation = 0;
            }

            if (player.HeldItem.modItem is BarrierDye && !Item.IsAir) //swap or stack
            {
                var temp = Item;
                mp.barrierDyeItem = player.HeldItem;
                Main.mouseItem = temp;
                Main.PlaySound(SoundID.Grab);
                mp.rechargeAnimation = 0;
            }

            Main.isMouseLeftConsumedByUI = true;
        }

        public override void Update(GameTime gameTime)
        {
            Width.Set(44, 0);
            Height.Set(44, 0);
        }
    }
}

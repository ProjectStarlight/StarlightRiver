using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.GUI
{
    public class Infusion : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        private readonly InfusionSlot[] slots = new InfusionSlot[InfusionSlots];

        /// <summary>
        /// Returns a copy of the internal slots array.
        /// </summary>
        public InfusionSlot[] GetInfusionSlots() => slots.ToArray();

        internal const int InfusionSlots = 3;

        public override void OnInitialize()
        {
            // "StarlightRiver/Assets/GUI/Infusions" is 64x58
            // The texture is centered at 100, 300 so its top-left is 68, 272
            // The top slot's top-left corner is at 20, 4 on the texture
            // So the top-left slot should be positioned at 88, 276 in screenspace. (Add top-left of texture and top-left corner of top slot)

            const float width = 20;
            const float height = 22;
            const float topSlotLeft = 90;
            const float topSlotTop = 276;

            int targetSlot = 0;
            void InitSlot(float left, float top)
            {
                InfusionSlot slot = new InfusionSlot(targetSlot);
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D texture = GetTexture("StarlightRiver/Assets/GUI/Infusions");
            spriteBatch.Draw(texture, new Vector2(68, 272), Color.White);

            if(true) //TODO: Figure out some sort of cool condition for this
            {
                Texture2D charm = GetTexture(AssetDirectory.GUI + "charm");
                spriteBatch.Draw(charm, new Vector2(92, 318), Color.White);
            }

            base.Draw(spriteBatch);
            RemoveAllChildren();
            Initialize();
            Recalculate();
        }
    }

    public class InfusionSlot : UIElement
    {
        public InfusionSlot(int slot) => TargetSlot = slot;

        public int TargetSlot { get; }

        public bool Unlocked => Main.LocalPlayer.GetHandler().InfusionLimit > TargetSlot;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            var mp = Main.LocalPlayer.GetHandler();
            var equipped = mp.GetInfusion(TargetSlot);

            if (!Unlocked) //draw a lock instead for locked slots
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/GUI/InfusionLock");
                spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0f, tex.Size() / 2, 1, SpriteEffects.None, 0);
            }

            //Draws the slot
            else if (equipped != null)
            {
                //Draws the item itself
                equipped.Draw(spriteBatch, GetInnerDimensions().Center(), 1, false);

                if (IsMouseHovering && Main.mouseItem.IsAir)
                {
                    //Grabs the items tooltip
                    System.Text.StringBuilder ToolTip = new System.Text.StringBuilder();
                    for (int k = 0; k < equipped.item.ToolTip.Lines; k++)
                        ToolTip.AppendLine(equipped.item.ToolTip.GetLine(k));

                    //Draws the name and tooltip at the mouse
                    Utils.DrawBorderStringBig(spriteBatch, equipped.Name, Main.MouseScreen + new Vector2(22, 22), ItemRarity.GetColor(equipped.item.rare).MultiplyRGB(Main.mouseTextColorReal), 0.39f);
                    Utils.DrawBorderStringBig(spriteBatch, ToolTip.ToString(), Main.MouseScreen + new Vector2(22, 48), Main.mouseTextColorReal, 0.39f);
                }
            }

            // Draws the transparent visual
            else if (Main.mouseItem?.modItem is InfusionItem mouseItem && mp.CanSetInfusion(mouseItem))
            {
                float opacity = 0.33f + (float)Math.Sin(StarlightWorld.rottime) * 0.25f;
                mouseItem.Draw(spriteBatch, GetDimensions().Center(), opacity, false);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Unlocked && IsMouseHovering && ItemSlot.ShiftInUse && Main.LocalPlayer.GetHandler().GetInfusion(TargetSlot) != null)
                // Set cursor to the little chest icon
                Main.cursorOverride = 9;
        }

        public override void Click(UIMouseEvent evt)
        {
            if (!Unlocked)
            {
                Main.PlaySound(SoundID.Unlock);
                return;
            }

            var handler = Main.LocalPlayer.GetHandler();

            var occupant = handler.GetInfusion(TargetSlot);

            if (ItemSlot.ShiftInUse)
            {
                if (occupant == null)
                    return;

                // Find an open slot and pop it in there
                var slot = Array.FindIndex(Main.LocalPlayer.inventory, i => i == null || i.IsAir);
                if (slot > -1)
                {
                    Main.LocalPlayer.inventory[slot] = occupant.item;
                    handler.SetInfusion(null, TargetSlot);
                    Main.PlaySound(SoundID.Grab);
                    return;
                }
            }

            //if the player is holding an infusion
            if (Main.mouseItem.modItem is InfusionItem item && handler.SetInfusion(item, TargetSlot))
            {
                if (occupant == null) Main.mouseItem.TurnToAir();  //if nothing is equipped, equip the held item
                else Main.mouseItem = occupant.item; //if something is equipped, swap that for the held item

                Main.PlaySound(SoundID.Grab);
            }

            //if the player isnt holding anything but something is equipped, unequip it
            else if (occupant != null && Main.mouseItem.IsAir)
            {
                handler.SetInfusion(null, TargetSlot);

                Main.mouseItem = occupant.item;
                Main.PlaySound(SoundID.Grab);
            }
        }
    }
}
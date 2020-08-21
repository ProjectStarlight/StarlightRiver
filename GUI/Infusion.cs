using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Abilities;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.GUI
{
    public class Infusion : UIState
    {
        public static bool visible = false;
        private readonly InfusionSlot[] slots = new InfusionSlot[InfusionSlots];

        /// <summary>
        /// Returns a copy of the internal slots array.
        /// </summary>
        public InfusionSlot[] GetInfusionSlots() => slots.ToArray();

        internal const int InfusionSlots = 3;

        public override void OnInitialize()
        {
            InfusionSlot slot;

            slot = new InfusionSlot(0);
            slots[slot.TargetSlot] = slot;
            slot.Width.Set(20, 0);
            slot.Height.Set(22, 0);
            slot.Left.Set(90, 0);
            slot.Top.Set(276, 0);
            Append(slot);

            slot = new InfusionSlot(1);
            slots[slot.TargetSlot] = slot;
            slot.Width.Set(20, 0);
            slot.Height.Set(22, 0);
            slot.Left.Set(78, 0);
            slot.Top.Set(294, 0);
            Append(slot);

            slot = new InfusionSlot(2);
            slots[slot.TargetSlot] = slot;
            slot.Width.Set(20, 0);
            slot.Height.Set(22, 0);
            slot.Left.Set(102, 0);
            slot.Top.Set(294, 0);
            Append(slot);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GetTexture("StarlightRiver/GUI/Assets/Infusions"), new Vector2(100 - 26, 300 - 26), Color.White);

            base.Draw(spriteBatch);
            Recalculate();
        }
    }

    public class InfusionSlot : UIElement
    {
        public InfusionSlot(int slot) => TargetSlot = slot;

        public int TargetSlot { get; }

        public bool Unlocked => Main.LocalPlayer.GetHandler().InfusionLimit <= TargetSlot;

        public override void Draw(SpriteBatch spriteBatch)
        {
            var mp = Main.LocalPlayer.GetHandler();
            var hover = mp.GetInfusion(TargetSlot)?.item;


            if (Unlocked) //draw a lock instead for locked slots
            {
                Texture2D tex = GetTexture("StarlightRiver/GUI/Assets/InfusionLock");
                spriteBatch.Draw(tex, GetDimensions().Center(), tex.Frame(), Color.White, 0f, tex.Frame().Center(), 1, SpriteEffects.None, 0);
            }

            //Draws the slot
            else if (hover != null)
            {
                //Draws the item itself
                Texture2D tex2 = GetTexture(hover.modItem.Texture);
                spriteBatch.Draw(tex2, GetDimensions().Center(), tex2.Frame(), Color.White, 0f, tex2.Frame().Center(), 1, SpriteEffects.None, 0);

                if (IsMouseHovering && Main.mouseItem.IsAir)
                {
                    //Grabs the items tooltip
                    string ToolTip = "";
                    for (int k = 0; k < hover.ToolTip.Lines; k++)
                    {
                        ToolTip += hover.ToolTip.GetLine(k);
                        ToolTip += "\n";
                    }

                    //Draws the name and tooltip at the mouse
                    Utils.DrawBorderStringBig(spriteBatch, hover.Name, Main.MouseScreen + new Vector2(22, 22), ItemRarity.GetColor(hover.rare).MultiplyRGB(Main.mouseTextColorReal), 0.39f);
                    Utils.DrawBorderStringBig(spriteBatch, ToolTip, Main.MouseScreen + new Vector2(22, 48), Main.mouseTextColorReal, 0.39f);
                }
            }

            // Draws the transparent visual
            else if (Main.mouseItem?.modItem is InfusionItem mouseItem && mp.CanSetInfusion(mouseItem))
            {
                float opacity = 0.5f + (float)Math.Sin(StarlightWorld.rottime) * 0.25f;
                Texture2D tex2 = GetTexture(mouseItem.Texture);
                spriteBatch.Draw(tex2, GetDimensions().Center(), tex2.Frame(), Color.White * opacity, 0f, tex2.Frame().Center(), 1, SpriteEffects.None, 0);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Unlocked && IsMouseHovering && ItemSlot.ShiftInUse)
            {
                // Set cursor to the little chest icon
                Main.cursorOverride = 9;
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            if (Unlocked) 
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
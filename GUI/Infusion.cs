using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Abilities;
using System;
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
        private readonly InfusionSlot[] slot = new InfusionSlot[InfusionSlots] { new InfusionSlot(), new InfusionSlot(), new InfusionSlot() };

        internal const int InfusionSlots = 3;

        public override void OnInitialize()
        {
            slot[0].Width.Set(20, 0);
            slot[0].Height.Set(22, 0);
            slot[0].Left.Set(90, 0);
            slot[0].Top.Set(276, 0);
            slot[0].TargetSlot = 0;
            Append(slot[0]);

            slot[1].Width.Set(20, 0);
            slot[1].Height.Set(22, 0);
            slot[1].Left.Set(78, 0);
            slot[1].Top.Set(294, 0);
            slot[1].TargetSlot = 1;
            Append(slot[1]);

            slot[2].Width.Set(20, 0);
            slot[2].Height.Set(22, 0);
            slot[2].Left.Set(102, 0);
            slot[2].Top.Set(294, 0);
            slot[2].TargetSlot = 2;
            Append(slot[2]);
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
        public int TargetSlot;

        public override void Draw(SpriteBatch spriteBatch)
        {
            var mp = Main.LocalPlayer.GetHandler();
            var hover = mp.GetInfusion(TargetSlot)?.item;

            if (mp.InfusionLimit <= TargetSlot) //draw a lock instead for locked slots
            {
                Texture2D tex = GetTexture("StarlightRiver/GUI/Assets/InfusionLock");
                spriteBatch.Draw(tex, GetDimensions().Center(), tex.Frame(), Color.White, 0f, tex.Frame().Center(), 1, SpriteEffects.None, 0);
                return;
            }

            if (Main.mouseItem.modItem is InfusionItem)
            {
                Texture2D tex = GetTexture("StarlightRiver/GUI/Assets/InfusionGlow");
                spriteBatch.Draw(tex, GetDimensions().Center(), tex.Frame(), Color.White * (0.25f + (float)Math.Sin(StarlightWorld.rottime * 2) * 0.25f), 0f, tex.Frame().Center(), 1, SpriteEffects.None, 0);
            }

            //Draws the slot
            if (hover != null)
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
        }

        public override void Click(UIMouseEvent evt)
        {
            var player = Main.LocalPlayer;
            var handler = player.GetHandler();

            if(handler.InfusionLimit <= TargetSlot) //dont allow equipping infusions in locked slots
            {
                Main.PlaySound(SoundID.Unlock);
                return;
            }

            //if the player is holding an infusion
            var occupant = handler.GetInfusion(TargetSlot);
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
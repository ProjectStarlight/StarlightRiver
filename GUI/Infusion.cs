using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Abilities;
using StarlightRiver.Items.Infusions;
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
        private readonly InfusionSlot slot0 = new InfusionSlot();
        private readonly InfusionSlot slot1 = new InfusionSlot();

        public override void OnInitialize()
        {
            slot0.Width.Set(32, 0);
            slot0.Height.Set(32, 0);
            slot0.TargetSlot = 0;
            Append(slot0);

            slot1.Width.Set(32, 0);
            slot1.Height.Set(32, 0);
            slot1.TargetSlot = 1;
            Append(slot1);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            AbilityHandler mp = Main.LocalPlayer.GetModPlayer<AbilityHandler>();

            if (mp.HasSecondSlot) //draw both slots
            {
                slot0.Left.Set(496, 0);
                slot0.Top.Set(50, 0);

                slot1.Left.Set(534, 0);
                slot1.Top.Set(50, 0);
                slot1.Width.Set(32, 0);
                slot1.Height.Set(32, 0);
            }
            else //only 1 slot
            {
                slot0.Left.Set(515, 0);
                slot0.Top.Set(50, 0);

                slot1.Width.Set(0, 0);
                slot1.Height.Set(0, 0);
            }

            spriteBatch.DrawString(Main.fontItemStack, "Infusions", new Vector2(503, 30), Main.mouseTextColorReal, 0f, Vector2.Zero, 0.85f, 0, 0);

            base.Draw(spriteBatch);
            Recalculate();
        }
    }

    public class InfusionSlot : UIElement
    {
        private Item Equipped;
        private Item SwapCache;
        public int TargetSlot;

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = Main.inventoryBackTexture;
            AbilityHandler mp = Main.LocalPlayer.GetModPlayer<AbilityHandler>();
            Equipped = (TargetSlot == 0) ? mp.slot1 : mp.slot2;

            //Draws the slot
            spriteBatch.Draw(tex, GetDimensions().ToRectangle(), new Rectangle(0, 0, (int)tex.Size().X, (int)tex.Size().Y), Color.White * 0.75f);
            if (Equipped != null)
            {
                //Draws the item itself
                Texture2D tex2 = GetTexture(Equipped.modItem.Texture);
                spriteBatch.Draw(tex2, GetDimensions().Center(), tex2.Frame(), Color.White, 0f, tex2.Frame().Center(), 0.8f, SpriteEffects.None, 0);
                if (IsMouseHovering && Main.mouseItem.IsAir)
                {
                    //Grabs the items tooltip
                    string ToolTip = "";
                    for (int k = 0; k < Equipped.ToolTip.Lines; k++)
                    {
                        ToolTip += Equipped.ToolTip.GetLine(k);
                        ToolTip += "\n";
                    }

                    //Draws the name and tooltip at the mouse
                    Utils.DrawBorderStringBig(spriteBatch, Equipped.Name, Main.MouseScreen + new Vector2(22, 22), ItemRarity.GetColor(Equipped.rare).MultiplyRGB(Main.mouseTextColorReal), 0.39f);
                    Utils.DrawBorderStringBig(spriteBatch, ToolTip, Main.MouseScreen + new Vector2(22, 48), Main.mouseTextColorReal, 0.39f);
                }
            }
        }

        public static void SetSlot(Player player, Item item, int slot)
        {
            if (slot == 0) { player.GetModPlayer<AbilityHandler>().slot1 = item; }
            else { player.GetModPlayer<AbilityHandler>().slot2 = item; }
        }

        public override void Click(UIMouseEvent evt)
        {
            Player player = Main.LocalPlayer;

            //if the player is holding an infusion
            if (Main.mouseItem.modItem is InfusionItem && (Main.mouseItem.modItem as InfusionItem).CanEquipAccessory(player, 0))
            {
                //if nothing is equipped, equip the held item
                if (Equipped == null)
                {
                    Equipped = Main.mouseItem.Clone();
                    SetSlot(player, Equipped, TargetSlot);

                    Main.mouseItem.TurnToAir();
                }
                //if something is equipped, swap that for the held item
                else
                {
                    SwapCache = Equipped;
                    (Equipped.modItem as InfusionItem).Unequip(player);
                    SetSlot(player, null, TargetSlot);

                    Equipped = Main.mouseItem.Clone();
                    SetSlot(player, Equipped, TargetSlot);

                    Main.mouseItem = SwapCache.Clone();
                    SwapCache = null;
                }
                Main.PlaySound(SoundID.Grab);
            }
            //if the player isnt holding anything but something is equipped, unequip it
            else if (Equipped != null && Main.mouseItem.IsAir)
            {
                (Equipped.modItem as InfusionItem).Unequip(player);
                SetSlot(player, null, TargetSlot);

                Main.mouseItem = Equipped.Clone();
                Equipped = null;
                Main.PlaySound(SoundID.Grab);
            }
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Tiles;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.GUI
{
    public class LootUI : UIState
    {
        public static bool Visible = false;
        private Item BigItem = new Item();
        internal Item[] Selections = new Item[2];
        internal List<string> Quotes;
        private int QuoteID;

        public override void OnInitialize()
        {
            Quotes = new List<string>()
            {
                "Loot?",
                "Loot!",
                "Shiny treasures!",
                "Shinies!",
                "Treasure!",
                "For your troubles...",
                "This looks valuable...",
                "Not a mimmic!"
            };
        }
        public override void Update(GameTime gameTime)
        {
            if (Selections[1] != null)
            {
                Visible = false;
                Main.LocalPlayer.QuickSpawnItem(BigItem);
                Main.LocalPlayer.QuickSpawnItem(Selections[0], Selections[0].stack);
                Main.LocalPlayer.QuickSpawnItem(Selections[1], Selections[1].stack);
            }
            base.Update(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/GUI/Assets/LootSlotOn");

            Utils.DrawBorderStringBig(spriteBatch, Quotes[QuoteID], GetDimensions().Center() + new Vector2(0, -80) - 2.2f * Main.fontItemStack.MeasureString(Quotes[QuoteID]) / 2, Color.White, 0.75f);

            string str = "You get:";
            string str2 = "Pick two:";

            Utils.DrawBorderString(spriteBatch, str, GetDimensions().Center() + new Vector2(0, -40) - Main.fontItemStack.MeasureString(str) / 2, Color.White, 0.8f);
            Utils.DrawBorderString(spriteBatch, str2, GetDimensions().Center() + new Vector2(0, +50) - Main.fontItemStack.MeasureString(str2) / 2, Color.White, 0.8f);

            spriteBatch.Draw(tex, GetDimensions().Center(), tex.Frame(), Color.White * 0.75f, 0, tex.Size() / 2, 1, 0, 0);
            if (!BigItem.IsAir)
            {
                Texture2D tex2 = BigItem.type > ItemID.Count ? GetTexture(BigItem.modItem.Texture) : GetTexture("Terraria/Item_" + BigItem.type);
                float scale = tex2.Frame().Size().Length() < 52 ? 1 : 52f / tex2.Frame().Size().Length();

                spriteBatch.Draw(tex2, GetDimensions().Center(), tex2.Frame(), Color.White, 0, tex2.Frame().Size() / 2, scale, 0, 0);
                if (BigItem.stack > 1) spriteBatch.DrawString(Main.fontItemStack, BigItem.stack.ToString(), GetDimensions().Position() + Vector2.One * 28, Color.White);
            }

            Rectangle rect = new Rectangle(Main.screenWidth / 2 - 28, Main.screenHeight / 2 - 28, 56, 56);
            if (rect.Contains(Main.MouseScreen.ToPoint()))
            {
                float offY = 40 - BigItem.ToolTip.Lines * 14;
                Vector2 pos = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) + new Vector2(60, offY);
                for (int k = 0; k <= BigItem.ToolTip.Lines; k++)
                {
                    if (k == 0) Utils.DrawBorderString(spriteBatch, BigItem.Name, pos + new Vector2(0, k * 14), ItemRarity.GetColor(BigItem.rare), 0.75f);
                    else Utils.DrawBorderString(spriteBatch, BigItem.ToolTip.GetLine(k - 1), pos + new Vector2(0, k * 14), Color.White, 0.75f);
                }
            }

            base.Draw(spriteBatch);
            Recalculate();
        }
        public void SetItems(Loot bigItemID, Loot[] smallItemIDs)
        {
            Elements.Clear();
            Selections = new Item[2];

            Item item = new Item();
            item.SetDefaults(bigItemID.Type);
            item.stack = bigItemID.GetCount();
            BigItem = item;

            for (int k = 0; k < smallItemIDs.Length; k++)
            {
                Item item2 = new Item();
                item2.SetDefaults(smallItemIDs[k].Type);
                item2.stack = smallItemIDs[k].GetCount();
                AppendSlot(item2, (-2 + k) * 70);
            }
            QuoteID = Main.rand.Next(Quotes.Count);
        }
        private void AppendSlot(Item item, int offX)
        {
            LootSelection slot = new LootSelection(item);
            slot.Left.Set(offX - 30, 0.5f);
            slot.Top.Set(60, 0.5f);
            slot.Width.Set(60, 0);
            slot.Height.Set(60, 0);
            Append(slot);
        }
    }
    class LootSelection : UIElement
    {
        internal Item Item;
        public LootSelection(Item item) { Item = item; }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Parent is LootUI)
            {
                LootUI parent = Parent as LootUI;
                Texture2D tex = parent.Selections.Any(n => n == Item) ? GetTexture("StarlightRiver/GUI/Assets/LootSlotOn") : GetTexture("StarlightRiver/GUI/Assets/LootSlot");
                float opacity = IsMouseHovering ? 1 : 0.6f;

                spriteBatch.Draw(tex, GetDimensions().Position(), tex.Frame(), Color.White * opacity, 0, Vector2.Zero, 1, 0, 0);
                if (!Item.IsAir)
                {
                    Texture2D tex2 = Item.type > ItemID.Count ? GetTexture(Item.modItem.Texture) : GetTexture("Terraria/Item_" + Item.type);
                    float scale = tex2.Frame().Size().Length() < 52 ? 1 : 52f / tex2.Frame().Size().Length();

                    spriteBatch.Draw(tex2, GetDimensions().Center(), tex2.Frame(), Color.White, 0, tex2.Frame().Size() / 2, 1, 0, 0);
                    if (Item.stack > 1) Utils.DrawBorderString(spriteBatch, Item.stack.ToString(), GetDimensions().Position() + Vector2.One * 36, Color.White, 0.75f);
                }
                if (IsMouseHovering)
                {
                    float offY = 40 - Item.ToolTip.Lines * 14;
                    Vector2 pos = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) + new Vector2(60, offY);
                    for (int k = 0; k <= Item.ToolTip.Lines; k++)
                    {
                        if (k == 0) Utils.DrawBorderString(spriteBatch, Item.Name, pos + new Vector2(0, k * 14), ItemRarity.GetColor(Item.rare), 0.75f);
                        else Utils.DrawBorderString(spriteBatch, Item.ToolTip.GetLine(k - 1), pos + new Vector2(0, k * 14), Color.White, 0.75f);
                    }
                }
            }
        }
        public override void Click(UIMouseEvent evt)
        {
            if (Parent is LootUI)
            {
                LootUI parent = Parent as LootUI;
                if (parent.Selections[0] == null) parent.Selections[0] = Item;
                else if (parent.Selections[0] == Item) parent.Selections[0] = null;
                else parent.Selections[1] = Item;
            }
        }
    }
}

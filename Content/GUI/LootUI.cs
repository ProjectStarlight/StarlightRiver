using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Content.Tiles;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class LootUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        private Item BigItem = new Item();
        internal Item[] Selections = new Item[2];
        internal List<string> Quotes;
        private int QuoteID;

        public override void OnInitialize()
        {
            Quotes = new List<string>() //TODO somthing with localization
            {
                "Loot?",
                "Loot!",
                "Shiny treasures!",
                "Shinies!",
                "Treasure!",
                "For your troubles...",
                "This looks valuable...",
                "Not a mimic!",
                "Shiny!"
            };
        }

        public override void Update(GameTime gameTime)
        {
            if(Main.gameMenu)
                Visible = false;

            if (Selections[1] != null)
            {
                Visible = false;
                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), BigItem);
                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), Selections[0], Selections[0].stack);
                Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), Selections[1], Selections[1].stack);
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //additive stuff, shame I have to do this but terraria really do be terraria
            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

            var glowTex = Request<Texture2D>(AssetDirectory.GUI + "ItemGlow").Value;
            var sin = (float)Math.Sin(Main.GameUpdateCount / 20f);
            spriteBatch.Draw(glowTex, GetDimensions().Center(), null, Color.Gold * (0.4f + sin * 0.05f), Main.GameUpdateCount / 120f, glowTex.Size() / 2, 0.65f + sin * 0.03f, 0, 0);
            spriteBatch.Draw(glowTex, GetDimensions().Center(), null, Color.White * (0.4f + sin * 0.05f), -Main.GameUpdateCount / 60f, glowTex.Size() / 2, 0.5f + sin * 0.03f, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);


            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GUI/LootSlotOn").Value;

            Utils.DrawBorderStringBig(spriteBatch, Quotes[QuoteID], GetDimensions().Center() + new Vector2(0, -80) -  1.5f * Terraria.GameContent.FontAssets.ItemStack.Value.MeasureString(Quotes[QuoteID]) / 2, Color.White, 0.5f);

            string str = "You get:";
            string str2 = "Pick two:";

            Utils.DrawBorderString(spriteBatch, str, GetDimensions().Center() + new Vector2(0, -40) - Terraria.GameContent.FontAssets.ItemStack.Value.MeasureString(str) / 2, Color.White, 0.8f);
            Utils.DrawBorderString(spriteBatch, str2, GetDimensions().Center() + new Vector2(0, 40) - Terraria.GameContent.FontAssets.ItemStack.Value.MeasureString(str2) / 2, Color.White, 0.8f);

            spriteBatch.Draw(tex, GetDimensions().Center(), tex.Frame(), Color.White * 0.75f, 0, tex.Size() / 2, 1, 0, 0);

            if (!BigItem.IsAir)
            {
                Texture2D tex2 = BigItem.type > ItemID.Count ? Request<Texture2D>(BigItem.ModItem.Texture).Value : Request<Texture2D>("Terraria/Item_" + BigItem.type).Value;
                float scale = tex2.Frame().Size().Length() < 47 ? 1 : 47f / tex2.Frame().Size().Length();

                spriteBatch.Draw(tex2, GetDimensions().Center(), tex2.Frame(), Color.White, 0, tex2.Frame().Size() / 2, scale, 0, 0);

                if (BigItem.stack > 1)
                    spriteBatch.DrawString(Terraria.GameContent.FontAssets.ItemStack.Value, BigItem.stack.ToString(), GetDimensions().Position() + Vector2.One * 28, Color.White);
            }

            Rectangle rect = new Rectangle(Main.screenWidth / 2 - 28, Main.screenHeight / 2 - 28, 56, 56);

            if (rect.Contains(Main.MouseScreen.ToPoint()))
            {
                Main.HoverItem = BigItem.Clone();
                Main.hoverItemName = BigItem.Name + " (" + BigItem.stack + ")";
            }

            base.Draw(spriteBatch);
            Recalculate();
        }

        public void SetItems(Loot bigItemID, Loot[] smallItemIDs)
        {
            Elements.Clear();
            Selections = new Item[2];

            Item Item = new Item();
            Item.SetDefaults(bigItemID.Type);
            Item.stack = bigItemID.GetCount();
            BigItem = Item;

            for (int k = 0; k < smallItemIDs.Length; k++)
            {
                Item Item2 = new Item();
                Item2.SetDefaults(smallItemIDs[k].Type);
                Item2.stack = smallItemIDs[k].GetCount();
                AppendSlot(Item2, (-2 + k) * 60);
            }
            QuoteID = Main.rand.Next(Quotes.Count);
        }

        private void AppendSlot(Item Item, int offX)
        {
            LootSelection slot = new LootSelection(Item);
            slot.Left.Set(offX - 26, 0.5f);
            slot.Top.Set(50, 0.5f);
            slot.Width.Set(50, 0);
            slot.Height.Set(50, 0);
            Append(slot);
        }
    }

    class LootSelection : UIElement
    {
        internal Item Item;

        public LootSelection(Item Item) { this.Item = Item; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            if (Parent is LootUI)
            {
                LootUI parent = Parent as LootUI;
                Texture2D tex = parent.Selections.Any(n => n == Item) ? Request<Texture2D>("StarlightRiver/Assets/GUI/LootSlotOn").Value : Request<Texture2D>("StarlightRiver/Assets/GUI/LootSlot").Value;
                float opacity = IsMouseHovering ? 1 : 0.6f;

                spriteBatch.Draw(tex, GetDimensions().Position(), tex.Frame(), Color.White * opacity, 0, Vector2.Zero, 1, 0, 0);

                if (!Item.IsAir)
                {
                    Main.instance.LoadItem(Item.type);
                    Texture2D tex2 = Item.type > ItemID.Count ? Request<Texture2D>(Item.ModItem.Texture).Value : Terraria.GameContent.TextureAssets.Item[Item.type].Value;
                    float scale = tex2.Frame().Size().Length() < 47 ? 1 : 47f / tex2.Frame().Size().Length();

                    spriteBatch.Draw(tex2, GetDimensions().Center(), tex2.Frame(), Color.White, 0, tex2.Frame().Size() / 2, 1, 0, 0);

                    if (Item.stack > 1) 
                        Utils.DrawBorderString(spriteBatch, Item.stack.ToString(), GetDimensions().Position() + Vector2.One * 28, Color.White, 0.75f);
                }

                if (IsMouseHovering)
                {
                    Main.HoverItem = Item.Clone();
                    Main.hoverItemName = Item.Name + " (" + Item.stack + ")";
                }
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            if (Parent is LootUI)
            {
                LootUI parent = Parent as LootUI;

                if (parent.Selections[0] == null) 
                    parent.Selections[0] = Item;
                else if (parent.Selections[0] == Item) 
                    parent.Selections[0] = null;
                else
                    parent.Selections[1] = Item;
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Food;

namespace StarlightRiver.Content.GUI
{
    public class CookingUI : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        private static bool Moving = false;
        private static Vector2 MoveOffset = Vector2.Zero;
        private static int scrollStart = 0;
        private static int lineCount = 0;

        private readonly CookingSlot MainSlot = new CookingSlot(IngredientType.Main);
        private readonly CookingSlot SideSlot0 = new CookingSlot(IngredientType.Side);
        private readonly CookingSlot SideSlot1 = new CookingSlot(IngredientType.Side);
        private readonly CookingSlot SeasonSlot = new CookingSlot(IngredientType.Seasoning);
        private readonly UIImageButton CookButton = new UIImageButton(GetTexture("StarlightRiver/Assets/GUI/CookPrep"));
        private readonly UIImageButton ExitButton = new UIImageButton(GetTexture("StarlightRiver/Assets/GUI/CookExit"));
        private readonly UIImage StatBack = new UIImage(GetTexture("StarlightRiver/Assets/GUI/CookStatWindow"));
        private readonly UIImage TopBar = new UIImage(GetTexture("StarlightRiver/Assets/GUI/CookTop"));

        private Vector2 Basepos = new Vector2(Main.screenWidth / 2 - 173, Main.screenHeight / 2 - 122);

        public override void OnInitialize()
        {
            CookButton.OnClick += CookFood;
            CookButton.SetVisibility(1, 1);
            ExitButton.OnClick += Exit;
            ExitButton.SetVisibility(1, 1);

            OnScrollWheel += ScrollStats;
        }

        public override void Update(GameTime gameTime)
        {
            if (TopBar.IsMouseHovering && Main.mouseLeft)
            {
                if (!Moving)
                    MoveOffset = Main.MouseScreen - Basepos;

                Moving = true;
            }

            if (!Main.mouseLeft) Moving = false;

            if (Moving) Basepos = Main.MouseScreen - MoveOffset;
            if (Basepos.X < 20) Basepos.X = 20;
            if (Basepos.Y < 20) Basepos.Y = 20;
            if (Basepos.X > Main.screenWidth - 20 - 346) Basepos.X = Main.screenWidth - 20 - 346;
            if (Basepos.Y > Main.screenHeight - 20 - 244) Basepos.Y = Main.screenHeight - 20 - 244;

            Main.isMouseLeftConsumedByUI = true;
            SetPosition(MainSlot, 44, 44);
            SetPosition(SideSlot0, 10, 112);
            SetPosition(SideSlot1, 78, 112);
            SetPosition(SeasonSlot, 44, 180);
            SetPosition(StatBack, 170, 40);
            SetPosition(CookButton, 170, 202);
            SetPosition(ExitButton, 314, 0);
            SetPosition(TopBar, 0, 2);

            base.Update(gameTime);
        }

        private void ScrollStats(UIScrollWheelEvent evt, UIElement listeningElement)
        {
            scrollStart -= evt.ScrollWheelValue > 0 ? 1 : -1;
            scrollStart = (int)MathHelper.Clamp(scrollStart, 0, lineCount - 5);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Main.magicPixel, new Rectangle((int)Basepos.X - 10, (int)Basepos.Y - 10, 376, 266), new Color(25, 25, 25) * 0.5f);

            base.Draw(spriteBatch);
            Utils.DrawBorderString(spriteBatch, "Ingredients", Basepos + new Vector2(38, 8), Color.White, 0.8f);
            Utils.DrawBorderString(spriteBatch, "Info/Stats", Basepos + new Vector2(202, 8), Color.White, 0.8f);
            Utils.DrawBorderString(spriteBatch, "Prepare", Basepos + new Vector2(212, 210), Color.White, 1.1f);

            int drawY = 0;
            if (!Elements.Any(n => n is CookingSlot && !(n as CookingSlot).Item.IsAir && ((n as CookingSlot).Item.modItem as Ingredient).ThisType == IngredientType.Main))
                Utils.DrawBorderString(spriteBatch, "Place a Main Course in\nthe top slot to start\ncooking", Basepos + new Vector2(186, 54 + drawY), Color.White, 0.7f);

            else
            {
                int duration = 0;
                int cooldown = 0;
                List<(string, Color)> lines = new List<(string, Color)>();

                spriteBatch.Draw(Main.magicPixel, new Rectangle((int)Basepos.X + 182, (int)Basepos.Y + 52, 152, lineCount >= 5 ? 18 * 5 : lineCount * 18), new Color(40, 20, 10) * 0.5f);
                spriteBatch.Draw(Main.magicPixel, new Rectangle((int)Basepos.X + 182, (int)Basepos.Y + 148, 152, 28), new Color(40, 20, 10) * 0.5f);

                foreach (UIElement element in Elements.Where(n => n is CookingSlot && !(n as CookingSlot).Item.IsAir))
                {
                    Ingredient ingredient = (element as CookingSlot).Item.modItem as Ingredient;

                    var strings = ingredient.ItemTooltip.Split('\n');

                    for (int k = 0; k < strings.Count(); k++)
                    {
                        string text = "~" + Helper.WrapString(strings[k], 100, Main.fontItemStack, 0.65f);
                        string[] substrings = text.Split('\n');

                        for (int n = 0; n < substrings.Length; n++)
                            lines.Add((substrings[n], ingredient.GetColor()));
                    }

                    duration += ingredient.Fill;
                    cooldown += (int)(ingredient.Fill * 1.5f);
                }

                int max = (int)MathHelper.Clamp(scrollStart + 5, 0, lines.Count());
                lineCount = lines.Count();

                for (int k = scrollStart; k < max; k++)
                {
                    var line = lines[k];
                    Utils.DrawBorderString(spriteBatch, line.Item1, Basepos + new Vector2(186, 54 + drawY), line.Item2, 0.65f);
                    drawY += (int)(Main.fontItemStack.MeasureString(line.Item1).Y * 0.65f) + 2;
                }

                Utils.DrawBorderString(spriteBatch, duration / 60 + " seconds duration", Basepos + new Vector2(186, 150), new Color(110, 235, 255), 0.65f);
                Utils.DrawBorderString(spriteBatch, cooldown / 60 + " seconds fullness", Basepos + new Vector2(186, 164), new Color(255, 170, 120), 0.65f);

                if (lineCount > 5)
                {
                    var tex = GetTexture("StarlightRiver/Assets/GUI/Arrow");

                    spriteBatch.Draw(Main.magicPixel, new Rectangle((int)Basepos.X + 352, (int)Basepos.Y + 60, 4, 80), new Color(20, 20, 10) * 0.5f);
                    spriteBatch.Draw(tex, Basepos + new Vector2(354, 60 + scrollStart / (float)(lineCount - 5) * 80), null, Color.White, 0, tex.Size() / 2, 1, 0, 0);
                }
            }
        }

        private void SetPosition(UIElement element, int x, int y)
        {
            element.Left.Set(Basepos.X + x, 0);
            element.Top.Set(Basepos.Y + y, 0);
            Append(element);
        }

        private void CookFood(UIMouseEvent evt, UIElement listeningElement)
        {
            if (!MainSlot.Item.IsAir) //make sure were cooking SOMETHING!
            {
                Item item = new Item();
                item.SetDefaults(ItemType<Meal>()); //let TML hanlde making the item properly
                (item.modItem as Meal).Ingredients = new List<Item>();
                CookIngredient(item, MainSlot);
                CookIngredient(item, SideSlot0);
                CookIngredient(item, SideSlot1);
                CookIngredient(item, SeasonSlot);
                item.position = Main.LocalPlayer.Center;
                Main.LocalPlayer.QuickSpawnClonedItem(item);

                Main.PlaySound(SoundID.DD2_BetsyScream); //TODO: Change to custom chop chop sizzle sound
            }
        }

        private static void CookIngredient(Item target, CookingSlot source)
        {
            if (!source.Item.IsAir && source.Item.modItem is Ingredient)
            {
                (target.modItem as Meal).Ingredients.Add(source.Item.Clone());
                (target.modItem as Meal).Fullness += (source.Item.modItem as Ingredient).Fill;
                if (source.Item.stack == 1) source.Item.TurnToAir();
                else source.Item.stack--;
            }
        }

        private void Exit(UIMouseEvent evt, UIElement listeningElement)
        {
            Visible = false;
            Main.PlaySound(SoundID.MenuClose);
        }
    }

    public class CookingSlot : UIElement
    {
        public Item Item = new Item();
        private readonly IngredientType Type;

        public CookingSlot(IngredientType type)
        {
            Type = type;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;

            Texture2D tex = GetTexture("StarlightRiver/Assets/GUI/CookSlotY");
            switch (Type)
            {
                case IngredientType.Main: tex = GetTexture("StarlightRiver/Assets/GUI/CookSlotY"); break;
                case IngredientType.Side: tex = GetTexture("StarlightRiver/Assets/GUI/CookSlotG"); break;
                case IngredientType.Seasoning: tex = GetTexture("StarlightRiver/Assets/GUI/CookSlotB"); break;
            }

            spriteBatch.Draw(tex, GetDimensions().Position(), tex.Frame(), Color.White, 0, Vector2.Zero, 1, 0, 0);

            if (!Item.IsAir)
            {
                Texture2D tex2 = GetTexture(Item.modItem.Texture);
                spriteBatch.Draw(tex2, new Rectangle((int)GetDimensions().X + 30, (int)GetDimensions().Y + 30, (int)MathHelper.Min(tex2.Width, 28), (int)MathHelper.Min(tex2.Height, 28)), tex2.Frame(), Color.White, 0, tex2.Size() / 2, 0, 0);

                if (Item.stack > 1)
                    Utils.DrawBorderString(spriteBatch, Item.stack.ToString(), GetDimensions().Position() + Vector2.One * 32, Color.White, 0.75f);
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            Player player = Main.LocalPlayer;

            if (Main.mouseItem.IsAir && !Item.IsAir) //if the cursor is empty and there is something in the slot, take the item out
            {
                Main.mouseItem = Item.Clone();
                Item.TurnToAir();
                Main.PlaySound(SoundID.Grab);
            }

            if (Item.IsAir && player.HeldItem.type == Item.type) //if the cursor is the same type as the item already in the slot, add to the slot
            {
                Item.stack += player.HeldItem.stack;
                player.HeldItem.TurnToAir();
                Main.PlaySound(SoundID.Grab);
            }

            if (player.HeldItem.modItem is Ingredient && (player.HeldItem.modItem as Ingredient).ThisType == Type && Item.IsAir) //if the slot is empty and the cursor has an item, put it in the slot
            {
                Item = player.HeldItem.Clone();
                player.HeldItem.TurnToAir();
                Main.mouseItem.TurnToAir();
                Main.PlaySound(SoundID.Grab);
            }

            if(player.HeldItem.modItem is Ingredient && (player.HeldItem.modItem as Ingredient).ThisType == Type && !Item.IsAir) //swap or stack
			{
                if(player.HeldItem.type == Item.type) //stack
				{
                    if (Item.stack + player.HeldItem.stack > Item.maxStack)
                    {
                        Main.mouseItem.stack = Item.stack + player.HeldItem.stack - Item.maxStack;
                        Item.stack = Item.maxStack;                    
                        Main.PlaySound(SoundID.Grab);
                    }
                    else
                    {
                        Item.stack += player.HeldItem.stack;
                        player.HeldItem.TurnToAir();
                        Main.mouseItem.TurnToAir();
                        Main.PlaySound(SoundID.Grab);
                    }
                }
				else //swap
				{
                    var temp = Item;
                    Item = player.HeldItem;
                    Main.mouseItem = temp;
                    Main.PlaySound(SoundID.Grab);
                }
			}


            Main.isMouseLeftConsumedByUI = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (Item.type == ItemID.None || Item.stack <= 0) Item.TurnToAir();
            Width.Set(60, 0);
            Height.Set(60, 0);
        }
    }
}
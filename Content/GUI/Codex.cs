using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.GUI
{
    public class Codex : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        public override bool Visible => ButtonVisible;

        public static bool ButtonVisible = false;
        public static bool Open = false;
        private static bool Dragging = false;

        private readonly CodexBack Back = new CodexBack();
        private readonly UIImage DragButton = new UIImage(GetTexture("StarlightRiver/Assets/GUI/DragButton"));
        private readonly UIImage ExitButton = new UIImage(GetTexture("StarlightRiver/Assets/GUI/ExitButton"));
        private readonly UIImageButton BookButton = new UIImageButton(GetTexture("StarlightRiver/Assets/GUI/BookLocked"));
        private readonly UIElement EntryBack = new UIElement();
        internal UIList ClickableEntries = new UIList();
        private readonly UIScrollbar EntryScroll = new UIScrollbar();

        public override void OnInitialize()
        {
            AddElement(BookButton, 570, 240, 26, 32, this);
            BookButton.OnClick += OpenCodex;
            BookButton.SetVisibility(1, 1);

            AddElement(Back, Main.screenWidth / 2 - 250, Main.screenHeight / 2 - 225, 500, 450, this);
            Back.OnScrollWheel += ScrollEntry;

            AddElement(new CategoryButton(CodexEntry.Categories.Abilities, "Abilities"), 30, 10, 50, 28, Back);
            AddElement(new CategoryButton(CodexEntry.Categories.Biomes, "Biomes"), 90, 10, 50, 28, Back);
            AddElement(new CategoryButton(CodexEntry.Categories.Removed, "[PH]REMOVED"), 150, 10, 50, 28, Back);
            AddElement(new CategoryButton(CodexEntry.Categories.Relics, "Relics"), 210, 10, 50, 28, Back);
            AddElement(new CategoryButton(CodexEntry.Categories.Misc, "Misc"), 270, 10, 50, 28, Back);

            AddElement(EntryBack, 330, 52, 120, 360, Back);
            AddElement(ClickableEntries, 0, 0, 120, 340, EntryBack);
            ClickableEntries.ListPadding = 2;
            AddElement(EntryScroll, 460, 54, 18, 340, Back);
            EntryScroll.SetView(0, 360);
            ClickableEntries.SetScrollbar(EntryScroll);

            AddElement(DragButton, 410, 4, 38, 38, Back);
            AddElement(ExitButton, 454, 4, 38, 38, Back);
            ExitButton.OnClick += Exit;
        }

        private void ScrollEntry(UIScrollWheelEvent evt, UIElement listeningElement)
        {
            Vector2 pos = listeningElement.GetDimensions().ToRectangle().TopLeft();
            Rectangle entryWindow = new Rectangle((int)pos.X + 20, (int)pos.Y + 50, 310, 342);
            if (!entryWindow.Contains(Main.MouseScreen.ToPoint())) return; //makes sure were in the entry window to scroll. I shouldnt have hardcoded the entries to draw to the back element but oh well.

            CodexBack element = listeningElement as CodexBack;
            if (element.ActiveEntry != null)
                element.ActiveEntry.LinePos += evt.ScrollWheelValue > 0 ? -1 : 1;
        }

        private void OpenCodex(UIMouseEvent evt, UIElement listeningElement)
        {
            if (Main.LocalPlayer.GetModPlayer<CodexHandler>().CodexState != 0) Open = true;
        }

        private void Exit(UIMouseEvent evt, UIElement listeningElement)
        {
            Open = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (ButtonVisible)
            {
                CodexHandler player = Main.LocalPlayer.GetModPlayer<CodexHandler>();

                BookButton.Draw(spriteBatch);
                if (player.CodexState != 0 && player.Entries.Any(n => n.New))
                {
                    Texture2D tex = BookButton.IsMouseHovering ? GetTexture("StarlightRiver/Assets/GUI/BookGlowOpen") : GetTexture("StarlightRiver/Assets/GUI/BookGlowClosed");
                    spriteBatch.Draw(tex, BookButton.GetDimensions().Position() + new Vector2(-1, 0), Helper.IndicatorColor);
                }
                if (BookButton.IsMouseHovering)
                {
                    Utils.DrawBorderString(spriteBatch, player.CodexState == 0 ? "Found in the desert..." : "Starlight Codex", Main.MouseScreen + Vector2.One * 16, Main.mouseTextColorReal, 0.95f);
                    Main.LocalPlayer.mouseInterface = true;
                }
            }

            if (Open) base.Draw(spriteBatch);
        }
        public override void Update(GameTime gameTime)
        {
            CodexHandler player = Main.LocalPlayer.GetModPlayer<CodexHandler>();

            switch (player.CodexState)
            {
                case 0: //locked
                    BookButton.SetImage(GetTexture("StarlightRiver/Assets/GUI/BookLocked"));
                    break;

                case 1: //tier 1
                    if (BookButton.IsMouseHovering) BookButton.SetImage(GetTexture("StarlightRiver/Assets/GUI/Book1Open"));
                    else BookButton.SetImage(GetTexture("StarlightRiver/Assets/GUI/Book1Closed"));
                    break;

                case 2: //tier 2
                    if (BookButton.IsMouseHovering) BookButton.SetImage(GetTexture("StarlightRiver/Assets/GUI/Book2Open"));
                    else BookButton.SetImage(GetTexture("StarlightRiver/Assets/GUI/Book2Closed"));
                    break;
            }

            if (DragButton.IsMouseHovering && Main.mouseLeft) Dragging = true;
            if (!Main.mouseLeft) Dragging = false;

            if (Dragging) SetPos(Back, Main.MouseScreen + new Vector2(-429, -19));
            if (Back.Left.Pixels < 20) Back.Left.Set(20, 0);
            if (Back.Top.Pixels < 20) Back.Top.Set(20, 0);
            if (Back.Left.Pixels > Main.screenWidth - Back.Width.Pixels) Back.Left.Set(Main.screenWidth - Back.Width.Pixels, 0);
            if (Back.Top.Pixels > Main.screenHeight - Back.Height.Pixels) Back.Top.Set(Main.screenHeight - Back.Height.Pixels, 0);
            Recalculate();
            base.Update(gameTime);
        }

        internal void AddElement(UIElement element, int x, int y, int width, int height, UIElement appendTo)
        {
            element.Left.Set(x, 0);
            element.Top.Set(y, 0);
            element.Width.Set(width, 0);
            element.Height.Set(height, 0);
            appendTo.Append(element);
        }

        internal void AddEntryButton(UIElement element, float offY)
        {
            element.Left.Set(0, 0);
            element.Top.Set(offY, 0);
            element.Width.Set(120, 0);
            element.Height.Set(28, 0);
            ClickableEntries.Add(element);
        }

        internal static void SetPos(UIElement element, float x, float y)
        {
            element.Left.Set(x, 0);
            element.Top.Set(y, 0);
        }

        internal static void SetPos(UIElement element, Vector2 pos)
        {
            element.Left.Set(pos.X, 0);
            element.Top.Set(pos.Y, 0);
        }
    }

    internal class CodexBack : UIElement
    {
        internal CodexEntry ActiveEntry;
        internal CodexEntry.Categories ActiveCategory = CodexEntry.Categories.None;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;

            spriteBatch.Draw(Main.magicPixel, GetDimensions().ToRectangle(), Main.magicPixel.Frame(), Color.White * 0.1f);
            Vector2 pos = GetDimensions().ToRectangle().TopLeft() + new Vector2(20, 50);
            Texture2D backTex = GetTexture("StarlightRiver/Assets/GUI/CodexBack");
            if (ActiveEntry?.RequiresUpgradedBook == true) backTex = GetTexture("StarlightRiver/Assets/GUI/CodexBack2"); //use a purple back for rift entries
            spriteBatch.Draw(backTex, pos, Color.White * 0.8f);
            ActiveEntry?.Draw(pos + new Vector2(50, 16), spriteBatch); //draws the text of the active entry
            base.Draw(spriteBatch);

            foreach (EntryButton button in (Parent as Codex).ClickableEntries._items)
                if (button.IsMouseHovering && button.Entry.Locked && button.Entry.Hint != null)
                    Utils.DrawBorderString(spriteBatch, Helper.WrapString(button.Entry.Hint, 300, Main.fontDeathText, 0.8f), Main.MouseScreen + Vector2.One * 16, Main.mouseTextColorReal, 0.8f);
        }

        internal void ChangeCategory(CodexEntry.Categories category) //swaps out all of the entry buttons based on the category
        {
            if (!(Parent is Codex)) return;
            Codex parent = Parent as Codex;

            ActiveCategory = category;
            CodexHandler player = Main.LocalPlayer.GetModPlayer<CodexHandler>();

            parent.ClickableEntries.Clear();

            int offY = 0;
            foreach (CodexEntry entry in player.Entries.Where(n => n.Category == category && (!n.RequiresUpgradedBook || player.CodexState == 2)))
            {
                EntryButton button = new EntryButton(entry);
                parent.AddEntryButton(button, offY);
            }
        }
    }

    internal class CategoryButton : UIElement
    {
        private readonly CodexEntry.Categories Category;
        private readonly string Text = "";
        public CategoryButton(CodexEntry.Categories category, string text) { Category = category; Text = text; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!(Parent is CodexBack)) return;
            CodexBack parent = Parent as CodexBack;
            CodexHandler player = Main.LocalPlayer.GetModPlayer<CodexHandler>();

            Vector2 pos = GetDimensions().ToRectangle().TopLeft();

            Color backColor = player.Entries.Any(n => n.New && n.Category == Category) ?
                new Color(255, 255, 127 + (int)((float)Math.Sin(StarlightWorld.rottime * 2) * 127f))
                : Color.White; //yellow flashing background for new entries

            Texture2D backTex = GetTexture("StarlightRiver/Assets/GUI/CategoryButton");
            spriteBatch.Draw(backTex, pos, backColor * 0.8f);
            Vector2 textSize = Main.fontDeathText.MeasureString(Text) * 0.6f;
            Utils.DrawBorderString(spriteBatch, Text, GetDimensions().ToRectangle().Center(), parent.ActiveCategory == Category ? Color.Yellow : Color.White, 0.6f, 0.5f, 0.5f);
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            Main.PlaySound(SoundID.MenuTick);

            if (!(Parent is CodexBack)) return;
            CodexBack parent = Parent as CodexBack;
            parent.ChangeCategory(Category);
        }
    }

    internal class EntryButton : UIElement
    {
        public CodexEntry Entry;
        public EntryButton(CodexEntry entry) { Entry = entry; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!(Parent.Parent.Parent.Parent is CodexBack)) return; //way too many parents >.<
            CodexBack parent = Parent.Parent.Parent.Parent as CodexBack;

            Vector2 pos = GetDimensions().ToRectangle().TopLeft();

            Color backColor = Entry.New ?
                new Color(255, 255, 127 + (int)((float)Math.Sin(StarlightWorld.rottime * 2) * 127f))
                : Color.White; //yellow flashing background for new entries

            Texture2D backTex = Entry.RequiresUpgradedBook ? GetTexture("StarlightRiver/Assets/GUI/EntryButton2") : GetTexture("StarlightRiver/Assets/GUI/EntryButton");
            spriteBatch.Draw(backTex, pos, backColor * 0.8f);

            Vector2 iconPos = pos + new Vector2(10, 14);

            if (!Entry.Locked) //draws the icon and name if the entry is unlocked
            {
                spriteBatch.Draw(Entry.Icon, iconPos, Entry.Icon.Frame(), Color.White, 0, Entry.Icon.Size() / 2, 0.5f, 0, 0);
                Utils.DrawBorderString(spriteBatch, Entry.Title, iconPos + new Vector2(10, -6), parent.ActiveEntry != null && parent.ActiveEntry == Entry ? Color.Yellow : Color.White, 0.6f);
            }
            else //draws the locked icon if locked
            {
                Texture2D blankTex = GetTexture("StarlightRiver/Assets/GUI/blank");
                spriteBatch.Draw(blankTex, iconPos, blankTex.Frame(), Color.White, 0, blankTex.Size() / 2, 0.5f, 0, 0);
                Utils.DrawBorderString(spriteBatch, "???", iconPos + new Vector2(10, -6), Color.White, 0.6f);
            }
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            Main.PlaySound(SoundID.MenuTick);

            if (!(Parent.Parent.Parent.Parent is CodexBack) || Entry.Locked) return; //way too many parents >.<
            CodexBack parent = Parent.Parent.Parent.Parent as CodexBack;
            parent.ActiveEntry = Entry;
            Entry.New = false;
        }
    }
}

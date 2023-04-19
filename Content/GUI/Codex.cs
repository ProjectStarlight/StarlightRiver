﻿using StarlightRiver.Content.Codex;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class Codex : SmartUIState
	{
		public static bool Open = false;
		private static bool Dragging = false;

		private readonly CodexBack Back = new();
		private readonly UIImage DragButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/DragButton").Value);
		private readonly UIImage ExitButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/ExitButton").Value);
		private readonly UIImageButton BookButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/BookLocked"));
		private readonly SmartUIElement EntryBack = new();
		internal UIList ClickableEntries = new();
		private readonly UIScrollbar EntryScroll = new();

		public override bool Visible => Main.playerInventory && Main.LocalPlayer.chest == -1 && Main.npcShop == 0;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			AddElement(BookButton, 570, 240, 26, 32, this);
			BookButton.OnLeftClick += (a, b) => OpenCodex();
			BookButton.SetVisibility(1, 1);

			AddElement(Back, Main.screenWidth / 2 - 250, Main.screenHeight / 2 - 225, 500, 500, this);
			Back.OnScrollWheel += ScrollEntry;

			AddElement(new CategoryButton(CodexEntry.Categories.Abilities, "Abilities"), 30, 10, 50, 28, Back);
			AddElement(new CategoryButton(CodexEntry.Categories.Biomes, "Biomes"), 90, 10, 50, 28, Back);
			AddElement(new CategoryButton(CodexEntry.Categories.Crafting, "Crafting"), 150, 10, 50, 28, Back);
			AddElement(new CategoryButton(CodexEntry.Categories.Relics, "Relics"), 210, 10, 50, 28, Back);
			AddElement(new CategoryButton(CodexEntry.Categories.Misc, "Misc"), 270, 10, 50, 28, Back);

			AddElement(EntryBack, 330, 52, 120, 410, Back);
			AddElement(ClickableEntries, 0, 0, 120, 390, EntryBack);
			ClickableEntries.ListPadding = 2;
			AddElement(EntryScroll, 460, 54, 18, 390, Back);
			EntryScroll.SetView(0, 410);
			ClickableEntries.SetScrollbar(EntryScroll);

			AddElement(DragButton, 410, 4, 38, 38, Back);
			AddElement(ExitButton, 454, 4, 38, 38, Back);
			ExitButton.OnLeftClick += (a, b) => Exit();
		}

		private void ScrollEntry(UIScrollWheelEvent evt, UIElement listeningElement)
		{
			Vector2 pos = listeningElement.GetDimensions().ToRectangle().TopLeft();
			var entryWindow = new Rectangle((int)pos.X + 20, (int)pos.Y + 50, 360, 342);
			if (!entryWindow.Contains(Main.MouseScreen.ToPoint()))
				return; //makes sure were in the entry window to scroll. I shouldnt have hardcoded the entries to draw to the back element but oh well.

			var element = listeningElement as CodexBack;
			if (element.ActiveEntry != null)
				element.ActiveEntry.LinePos += evt.ScrollWheelValue > 0 ? -1 : 1;
		}

		private void OpenCodex()
		{
			if (Main.LocalPlayer.GetModPlayer<CodexHandler>().CodexState != 0)
				Open = true;
		}

		private void Exit()
		{
			Open = false;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (Visible)
			{
				CodexHandler Player = Main.LocalPlayer.GetModPlayer<CodexHandler>();

				BookButton.Draw(spriteBatch);
				if (Player.CodexState != 0 && Player.Entries.Any(n => n.New))
				{
					Texture2D tex = BookButton.IsMouseHovering ? Request<Texture2D>("StarlightRiver/Assets/GUI/BookGlowOpen").Value : Request<Texture2D>("StarlightRiver/Assets/GUI/BookGlowClosed").Value;
					spriteBatch.Draw(tex, BookButton.GetDimensions().Position() + new Vector2(-1, 0), Helper.IndicatorColor);
				}

				if (BookButton.IsMouseHovering)
				{
					Utils.DrawBorderString(spriteBatch, Player.CodexState == 0 ? "Found in the desert..." : "Starlight Codex", Main.MouseScreen + Vector2.One * 16, Main.MouseTextColorReal, 0.95f);
					Main.LocalPlayer.mouseInterface = true;
				}
			}

			if (Open)
				base.Draw(spriteBatch);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			CodexHandler Player = Main.LocalPlayer.GetModPlayer<CodexHandler>();

			switch (Player.CodexState)
			{
				case 0: //locked
					BookButton.SetImage(Request<Texture2D>("StarlightRiver/Assets/GUI/BookLocked"));
					break;

				case 1: //tier 1
					if (BookButton.IsMouseHovering)
						BookButton.SetImage(Request<Texture2D>("StarlightRiver/Assets/GUI/Book1Open"));
					else
						BookButton.SetImage(Request<Texture2D>("StarlightRiver/Assets/GUI/Book1Closed"));
					break;

				case 2: //tier 2
					if (BookButton.IsMouseHovering)
						BookButton.SetImage(Request<Texture2D>("StarlightRiver/Assets/GUI/Book2Open"));
					else
						BookButton.SetImage(Request<Texture2D>("StarlightRiver/Assets/GUI/Book2Closed"));
					break;
			}

			if (DragButton.IsMouseHovering && Main.mouseLeft)
				Dragging = true;

			if (!Main.mouseLeft)
				Dragging = false;

			if (Dragging)
				SetPos(Back, Main.MouseScreen + new Vector2(-429, -19));

			if (Back.Left.Pixels < 20)
				Back.Left.Set(20, 0);

			if (Back.Top.Pixels < 20)
				Back.Top.Set(20, 0);

			if (Back.Left.Pixels > Main.screenWidth - Back.Width.Pixels)
				Back.Left.Set(Main.screenWidth - Back.Width.Pixels, 0);

			if (Back.Top.Pixels > Main.screenHeight - Back.Height.Pixels)
				Back.Top.Set(Main.screenHeight - Back.Height.Pixels, 0);

			Recalculate();
		}

		internal void AddEntryButton(UIElement element, int offY)
		{
			AddElement(element, 0, offY, 120, 28, ClickableEntries);
		}

		internal static void SetPos(UIElement element, Vector2 pos)
		{
			element.Left.Set(pos.X, 0);
			element.Top.Set(pos.Y, 0);
		}
	}

	internal class CodexBack : SmartUIElement
	{
		internal CodexEntry ActiveEntry;
		internal CodexEntry.Categories ActiveCategory = CodexEntry.Categories.None;

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (ContainsPoint(Main.MouseScreen))
				Main.LocalPlayer.mouseInterface = true;

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, GetDimensions().ToRectangle(), TextureAssets.MagicPixel.Value.Frame(), Color.White * 0.1f);
			Vector2 pos = GetDimensions().ToRectangle().TopLeft() + new Vector2(20, 50);
			Texture2D backTex = Request<Texture2D>("StarlightRiver/Assets/GUI/CodexBack").Value;

			if (ActiveEntry?.RequiresUpgradedBook == true)
				backTex = Request<Texture2D>("StarlightRiver/Assets/GUI/CodexBack2").Value; //use a purple back for rift entries

			spriteBatch.Draw(backTex, pos, Color.White * 0.8f);
			ActiveEntry?.Draw(pos + new Vector2(50, 16), spriteBatch); //draws the text of the active entry
			base.Draw(spriteBatch);

			foreach (EntryButton button in (Parent as Codex).ClickableEntries._items)
			{
				if (button.IsMouseHovering && button.Entry.Locked && button.Entry.Hint != null)
					Utils.DrawBorderString(spriteBatch, Helper.WrapString(button.Entry.Hint, 300, FontAssets.DeathText.Value, 0.8f), Main.MouseScreen + Vector2.One * 16, Main.MouseTextColorReal, 0.8f);
			}
		}

		internal void ChangeCategory(CodexEntry.Categories category) //swaps out all of the entry buttons based on the category
		{
			if (!(Parent is Codex))
				return;

			var parent = Parent as Codex;

			ActiveCategory = category;
			CodexHandler Player = Main.LocalPlayer.GetModPlayer<CodexHandler>();

			parent.ClickableEntries.Clear();

			int offY = 0;
			foreach (CodexEntry entry in Player.Entries.Where(n => n.Category == category && (!n.RequiresUpgradedBook || Player.CodexState == 2)))
			{
				var button = new EntryButton(entry);
				parent.AddEntryButton(button, offY);
			}
		}
	}

	internal class CategoryButton : SmartUIElement
	{
		private readonly CodexEntry.Categories Category;

		private readonly string Text = "";

		public CategoryButton(CodexEntry.Categories category, string text)
		{
			Category = category;
			Text = text;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!(Parent is CodexBack))
				return;

			var parent = Parent as CodexBack;
			CodexHandler Player = Main.LocalPlayer.GetModPlayer<CodexHandler>();

			Vector2 pos = GetDimensions().ToRectangle().TopLeft();

			Color backColor = Player.Entries.Any(n => n.New && n.Category == Category) ?
				new Color(255, 255, 127 + (int)((float)Math.Sin(StarlightWorld.visualTimer * 2) * 127f))
				: Color.White; //yellow flashing background for new entries

			Texture2D backTex = Request<Texture2D>("StarlightRiver/Assets/GUI/CategoryButton").Value;
			spriteBatch.Draw(backTex, pos, backColor * 0.8f);
			Vector2 textSize = Terraria.GameContent.FontAssets.DeathText.Value.MeasureString(Text) * 0.6f;
			Utils.DrawBorderString(spriteBatch, Text, GetDimensions().ToRectangle().Center(), parent.ActiveCategory == Category ? Color.Yellow : Color.White, 0.6f, 0.5f, 0.5f);
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);

			if (!(Parent is CodexBack))
				return;

			var parent = Parent as CodexBack;
			parent.ChangeCategory(Category);
		}
	}

	internal class EntryButton : SmartUIElement
	{
		public CodexEntry Entry;
		public EntryButton(CodexEntry entry) { Entry = entry; }

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!(Parent.Parent.Parent.Parent is CodexBack))
				return; //way too many parents >.<

			var parent = Parent.Parent.Parent.Parent as CodexBack;

			Vector2 pos = GetDimensions().ToRectangle().TopLeft();

			Color backColor = Entry.New ?
				new Color(255, 255, 127 + (int)((float)Math.Sin(StarlightWorld.visualTimer * 2) * 127f))
				: Color.White; //yellow flashing background for new entries

			Texture2D backTex = Entry.RequiresUpgradedBook ? Request<Texture2D>("StarlightRiver/Assets/GUI/EntryButton2").Value : Request<Texture2D>("StarlightRiver/Assets/GUI/EntryButton").Value;
			spriteBatch.Draw(backTex, pos, backColor * 0.8f);

			Vector2 iconPos = pos + new Vector2(10, 14);

			if (!Entry.Locked) //draws the icon and name if the entry is unlocked
			{
				spriteBatch.Draw(Entry.Icon, iconPos, Entry.Icon.Frame(), Color.White, 0, Entry.Icon.Size() / 2, 0.5f, 0, 0);
				Utils.DrawBorderString(spriteBatch, Entry.Title, iconPos + new Vector2(10, -6), parent.ActiveEntry != null && parent.ActiveEntry == Entry ? Color.Yellow : Color.White, 0.6f);
			}
			else //draws the locked icon if locked
			{
				Texture2D blankTex = Request<Texture2D>("StarlightRiver/Assets/GUI/blank").Value;
				spriteBatch.Draw(blankTex, iconPos, blankTex.Frame(), Color.White, 0, blankTex.Size() / 2, 0.5f, 0, 0);
				Utils.DrawBorderString(spriteBatch, "???", iconPos + new Vector2(10, -6), Color.White, 0.6f);
			}
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);

			if (!(Parent.Parent.Parent.Parent is CodexBack) || Entry.Locked)
				return; //way too many parents >.<

			var parent = Parent.Parent.Parent.Parent as CodexBack;
			parent.ActiveEntry = Entry;
			Entry.New = false;
		}
	}
}
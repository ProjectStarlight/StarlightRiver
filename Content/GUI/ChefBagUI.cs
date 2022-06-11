using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Items.Food.Special;
using StarlightRiver.Content.Items.Utility;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	class ChefBagUI : SmartUIState
	{
		public static ChefBag openBag = null;
		public static bool visible;

		public static UIGrid grid = new UIGrid();
		public static UIImageButton SortButton = new UIImageButton(Request<Texture2D>("StarlightRiver/Assets/GUI/Book1Closed", ReLogic.Content.AssetRequestMode.ImmediateLoad));
		public static UIImageButton OwnedButton = new UIImageButton(Request<Texture2D>("StarlightRiver/Assets/GUI/Book1Closed", ReLogic.Content.AssetRequestMode.ImmediateLoad));

		public static UIImageButton IngredientTab = new UIImageButton(Request<Texture2D>("StarlightRiver/Assets/GUI/NPCButtonCustom", ReLogic.Content.AssetRequestMode.ImmediateLoad));
		public static UIImageButton RecipieTab = new UIImageButton(Request<Texture2D>("StarlightRiver/Assets/GUI/NPCButtonCustom", ReLogic.Content.AssetRequestMode.ImmediateLoad));

		public static string sortMode = "Rarity";
		public static bool hideUnowned = false;

		public override bool Visible => visible;

		public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

		public override void OnInitialize()
		{
			Elements.Clear();

			grid.Left.Set(-220, 0.5f);
			grid.Top.Set(-220, 0.5f);
			grid.Width.Set(440, 0);
			grid.Height.Set(440, 0);
			grid.ListPadding = 4;
			grid.MaxWidth.Set(440, 0);
			grid.MaxHeight.Set(440, 0);

			Append(grid);


			SortButton.Left.Set(-260, 0.5f);
			SortButton.Top.Set(-220, 0.5f);
			SortButton.Width.Set(26, 0);
			SortButton.Height.Set(32, 0);
			SortButton.OnClick += ChangeSortMode;

			Append(SortButton);


			OwnedButton.Left.Set(-260, 0.5f);
			OwnedButton.Top.Set(-180, 0.5f);
			OwnedButton.Width.Set(26, 0);
			OwnedButton.Height.Set(32, 0);
			OwnedButton.OnClick += ChangeOwnedMode;

			Append(OwnedButton);


			IngredientTab.Left.Set(-220, 0.5f);
			IngredientTab.Top.Set(-260, 0.5f);
			IngredientTab.Width.Set(86, 0);
			IngredientTab.Height.Set(28, 0);
			IngredientTab.OnClick += (a, b) => RebuildGrid();

			Append(IngredientTab);


			RecipieTab.Left.Set(-130, 0.5f);
			RecipieTab.Top.Set(-260, 0.5f);
			RecipieTab.Width.Set(86, 0);
			RecipieTab.Height.Set(28, 0);
			RecipieTab.OnClick += (a, b) => RebuildRecipies();

			Append(RecipieTab);
		}

		private void ChangeOwnedMode(UIMouseEvent evt, UIElement listeningElement)
		{
			hideUnowned = !hideUnowned;
			RebuildGrid();
		}

		private void ChangeSortMode(UIMouseEvent evt, UIElement listeningElement)
		{
			switch (sortMode)
			{
				case "Rarity": sortMode = "Type"; break;
				case "Type": sortMode = "Alphabetical"; break;
				case "Alphabetical": sortMode = "Rarity Reverse"; break;
				case "Rarity Reverse": sortMode = "Type Reverse"; break;
				case "Type Reverse": sortMode = "Alphabetical Reverse"; break;
				case "Alphabetical Reverse": sortMode = "Rarity"; break;
			}

			grid.UpdateOrder();
			Core.Loaders.UILoader.GetUIState<ChefBagUI>().Recalculate();
		}

		public static void Move(Vector2 moveTarget)
		{
			grid.Left.Set(moveTarget.X, 0);
			grid.Top.Set(moveTarget.Y, 0);

			SortButton.Left.Set(moveTarget.X - 40, 0);
			SortButton.Top.Set(moveTarget.Y, 0);

			OwnedButton.Left.Set(moveTarget.X - 40, 0);
			OwnedButton.Top.Set(moveTarget.Y + 40, 0);

			IngredientTab.Left.Set(moveTarget.X, 0);
			IngredientTab.Top.Set(moveTarget.Y - 40, 0);

			RecipieTab.Left.Set(moveTarget.X + 90, 0);
			RecipieTab.Top.Set(moveTarget.Y - 40, 0);

			Core.Loaders.UILoader.GetUIState<ChefBagUI>().Recalculate();
		}

		public static void RebuildGrid()
		{
			grid.Clear();

			if (hideUnowned)
			{
				for (int k = 0; k < openBag.Items.Count; k++)
				{
					var item = new Item();
					item.SetDefaults(openBag.Items[k].type);
					grid.Add(new IngredientStorageSlot(item, k));
				}
			}
			else
			{
				for (int k = 0; k < ChefBag.ingredientTypes.Count; k++)
				{
					var item = new Item();
					item.SetDefaults(ChefBag.ingredientTypes[k]);
					grid.Add(new IngredientStorageSlot(item, k));
				}
			}
		}

		public static void RebuildRecipies()
		{
			grid.Clear();

			for (int k = 0; k < ChefBag.specialTypes.Count; k++)
			{
				var item = new Item();
				item.SetDefaults(ChefBag.specialTypes[k]);
				grid.Add(new RecipieSlot(item, k));
			}
		}

		public override void Update(GameTime gameTime)
		{
			if (grid._items.Count() == 0)
				RebuildGrid();

			if(SortButton.IsMouseHovering)
				Main.hoverItemName = "Sort mode:\n" + sortMode;

			if (OwnedButton.IsMouseHovering)
				Main.hoverItemName = "Hide unowned:\n" + hideUnowned;

			if (!Main.playerInventory)
				visible = false;
		}
	}

	class IngredientStorageSlot : UIElement
	{
		public Item Item;
		public float scale;

		public IngredientStorageSlot(Item Item, int index, float scale = 1)
		{
			this.Item = Item;

			Width.Set(50 * scale, 0);
			Height.Set(50 * scale, 0);

			Left.Set(index % 8 * 54 * scale, 0);
			Top.Set(index / 8 * 54 * scale, 0);

			this.scale = scale;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Main.instance.LoadItem(Item.type);

			var tex = Request<Texture2D>(AssetDirectory.GUI + "FoodSlot").Value;
			var texOver = Request<Texture2D>(AssetDirectory.GUI + "FoodSlotOver").Value;
			var ItemTex = TextureAssets.Item[Item.type].Value;
			var pos = GetDimensions().Center();

			var items = ChefBagUI.openBag.Items;
			var heldItem = items.FirstOrDefault(n => n.type == Item.type);
			var count = heldItem is null ? 0 : heldItem.stack;

			var color = (count == 0 ? Color.LightGray * 0.5f : Color.White);

			var color2 = (Item.ModItem as Ingredient).GetColor().MultiplyRGB(color);
			color2.A = 0;

			spriteBatch.Draw(tex, pos, null, Terraria.GameContent.UI.ItemRarity.GetColor(Item.rare).MultiplyRGB(color), 0, tex.Size() / 2, scale, 0, 0);
			spriteBatch.Draw(texOver, pos, null, color2, 0, tex.Size() / 2, scale, 0, 0);
			spriteBatch.Draw(ItemTex, pos, null, color, 0, ItemTex.Size() / 2, scale, 0, 0);
			Utils.DrawBorderString(spriteBatch, count.ToString(), pos + Vector2.One * 14, color, 0.8f * scale, 1, 0.5f);

			if(IsMouseHovering && count > 0)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = Item.Clone();
				Main.hoverItemName = "a";
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			if (Main.mouseItem.IsAir)
			{
				Main.mouseItem = ChefBagUI.openBag.RemoveItem(Item.type) ?? Main.mouseItem;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}
			else if (ChefBagUI.openBag.InsertItem(Main.LocalPlayer.HeldItem))
			{
				Main.mouseItem.TurnToAir();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}		
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (Main.mouseItem.IsAir)
			{
				Main.mouseItem = ChefBagUI.openBag.RemoveItem(Item.type, 1) ?? Main.mouseItem;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
			}
			else if (Main.mouseItem.type == Item.type && Main.mouseItem.stack < Main.mouseItem.maxStack)
			{
				var removal = ChefBagUI.openBag.RemoveItem(Item.type, 1);

				if (removal != null)
					Main.mouseItem.stack += removal.stack;

				Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
			}		
		}

		public override int CompareTo(object obj)
		{
			var other = obj as IngredientStorageSlot;

			switch(ChefBagUI.sortMode)
			{
				case "Rarity": return CompareRarity(other);
				case "Type": return CompareType(other);
				case "Alphabetical": return CompareAlphabetical(other);
				case "Rarity Reverse": return CompareRarity(other) * -1;
				case "Type Reverse": return CompareType(other) * -1;
				case "Alphabetical Reverse": return CompareAlphabetical(other) * -1;
			}

			return CompareRarity(other); //use rarity sort as a default
		}

		private int CompareRarity(IngredientStorageSlot other)
		{
			int firstOrder = Item.type > other.Item.type ? 1 : 0;

			int x = Item.rare * 6 + (int)(Item.ModItem as Ingredient).ThisType * 2 + firstOrder;
			int y = other.Item.rare * 6 + (int)(other.Item.ModItem as Ingredient).ThisType * 2 + firstOrder;

			return x >= y ? 1 : -1;
		}

		private int CompareType(IngredientStorageSlot other)
		{
			int firstOrder = Item.type > other.Item.type ? 1 : 0;

			int x = (int)(Item.ModItem as Ingredient).ThisType * 24 + Item.rare * 2 + firstOrder;
			int y = (int)(other.Item.ModItem as Ingredient).ThisType * 24 + other.Item.rare * 2 + firstOrder;

			return x >= y ? 1 : -1;
		}

		private int CompareAlphabetical(IngredientStorageSlot other)
		{
			return Item.Name.CompareTo(other.Item.Name);
		}
	}

	class RecipieSlot : IngredientStorageSlot
	{
		public BonusIngredient Result => Item.ModItem as BonusIngredient;

		public RecipieSlot(Item Item, int index) : base(Item, index)
		{
			this.Item = Item;

			Width.Set(230, 0);
			Height.Set(50, 0);

			Left.Set(index % 2 * 240, 0);
			Top.Set(index / 2 * 54, 0);

			for(int k = 0; k < 4; k++)
			{
				var item = new Item();
				item.SetDefaults(Result.Recipie().AsList()[k]);
				IngredientStorageSlot slot = new IngredientStorageSlot(item, k + 1, 0.8f);
				slot.Left.Set(58 + k * 44, 0);
				slot.Top.Set(8, 0);
				Append(slot);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Main.instance.LoadItem(Item.type);

			var tex = Request<Texture2D>(AssetDirectory.GUI + "FoodSlot").Value;
			var texOver = Request<Texture2D>(AssetDirectory.GUI + "FoodSlotOver").Value;
			var ItemTex = TextureAssets.Item[Item.type].Value;
			var pos = GetDimensions().Center();
			pos.X = GetDimensions().X + 25;

			var items = ChefBagUI.openBag.Items;
			var heldItem = items.FirstOrDefault(n => n.type == Item.type);

			var color = Color.White;

			var color2 = (Item.ModItem as Ingredient).GetColor().MultiplyRGB(color);
			color2.A = 0;

			spriteBatch.Draw(tex, pos, null, Terraria.GameContent.UI.ItemRarity.GetColor(Item.rare).MultiplyRGB(color), 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(texOver, pos, null, color2, 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(ItemTex, pos, null, color, 0, ItemTex.Size() / 2, 1, 0, 0);

			var hitbox = new Rectangle((int)GetDimensions().X, (int)GetDimensions().Y, 50, 50);

			if (IsMouseHovering && hitbox.Contains(Main.MouseScreen.ToPoint()))
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = Item.Clone();
				Main.hoverItemName = "a";
			}

			foreach (var child in Children)
				child.Draw(spriteBatch);
		}
	}
}

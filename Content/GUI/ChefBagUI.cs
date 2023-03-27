using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Items.Food.Special;
using StarlightRiver.Content.Items.Utility;
using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using System.Linq;
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

		public static UIGrid grid = new();
		public static UIImageButton SortButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/SortButton", ReLogic.Content.AssetRequestMode.ImmediateLoad));
		public static UIImageButton OwnedButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/HideButtonOff", ReLogic.Content.AssetRequestMode.ImmediateLoad));

		public static UIImageButton IngredientTab = new(Request<Texture2D>("StarlightRiver/Assets/GUI/IngredientButton", ReLogic.Content.AssetRequestMode.ImmediateLoad));
		public static UIImageButton RecipieTab = new(Request<Texture2D>("StarlightRiver/Assets/GUI/MealButton", ReLogic.Content.AssetRequestMode.ImmediateLoad));

		public static string sortMode = "Rarity";
		public static bool hideUnowned = false;

		public override bool Visible => visible;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			grid.Left.Set(-220, 0.5f);
			grid.Top.Set(-220, 0.5f);
			grid.Width.Set(480, 0);
			grid.Height.Set(216, 0);
			grid.ListPadding = 4;
			grid.MaxWidth.Set(480, 0);
			grid.MaxHeight.Set(216, 0);

			Append(grid);

			AddElement(SortButton, -260, 0.5f, -220, 0.5f, 32, 0f, 32, 0f);
			SortButton.OnClick += ChangeSortMode;

			AddElement(OwnedButton, -260, 0.5f, -180, 0.5f, 32, 0f, 32, 0f);
			OwnedButton.OnClick += ChangeOwnedMode;

			AddElement(IngredientTab, -220, 0.5f, -256, 0.5f, 50, 0f, 28, 0f);
			IngredientTab.OnClick += (a, b) => RebuildGrid();

			AddElement(RecipieTab, -160, 0.5f, -256, 0.5f, 50, 0f, 28, 0f);
			RecipieTab.OnClick += (a, b) => RebuildRecipies();
		}

		private void ChangeOwnedMode(UIMouseEvent evt, UIElement listeningElement)
		{
			hideUnowned = !hideUnowned;
			OwnedButton.SetImage(Request<Texture2D>("StarlightRiver/Assets/GUI/HideButton" + (hideUnowned ? "On" : "Off"), ReLogic.Content.AssetRequestMode.ImmediateLoad));
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
			UILoader.GetUIState<ChefBagUI>().Recalculate();
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
			IngredientTab.Top.Set(moveTarget.Y - 36, 0);

			RecipieTab.Left.Set(moveTarget.X + 60, 0);
			RecipieTab.Top.Set(moveTarget.Y - 36, 0);

			UILoader.GetUIState<ChefBagUI>().Recalculate();
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

			grid.UpdateOrder();
			UILoader.GetUIState<ChefBagUI>().Recalculate();
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

		public override void SafeUpdate(GameTime gameTime)
		{
			if (grid._items.Count() == 0)
				RebuildGrid();

			if (SortButton.IsMouseHovering)
				Main.hoverItemName = "Sort mode:\n" + sortMode;

			if (OwnedButton.IsMouseHovering)
				Main.hoverItemName = "Hide unowned:\n" + hideUnowned;

			if (IngredientTab.IsMouseHovering)
				Main.hoverItemName = "Ingredients";

			if (RecipieTab.IsMouseHovering)
				Main.hoverItemName = "Cookbook";

			if (!Main.playerInventory)
				visible = false;
		}
	}

	class IngredientStorageSlot : SmartUIElement
	{
		public Item item;
		public float scale;

		public IngredientStorageSlot(Item item, int index, float scale = 1)
		{
			this.item = item;

			Width.Set(50 * scale, 0);
			Height.Set(50 * scale, 0);

			Left.Set(index % 8 * 54 * scale, 0);
			Top.Set(index / 8 * 54 * scale, 0);

			this.scale = scale;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Main.instance.LoadItem(item.type);

			Texture2D tex = Request<Texture2D>(AssetDirectory.GUI + "FoodSlot").Value;
			Texture2D texOver = Request<Texture2D>(AssetDirectory.GUI + "FoodSlotOver").Value;
			Texture2D ItemTex = TextureAssets.Item[item.type].Value;
			Vector2 pos = GetDimensions().Center();

			List<Item> items = ChefBagUI.openBag.Items;
			Item heldItem = items.FirstOrDefault(n => n.type == item.type);
			int count = heldItem is null ? 0 : heldItem.stack;

			Color color = count == 0 ? Color.LightGray * 0.5f : Color.White;

			Color color2 = (item.ModItem as Ingredient).GetColor().MultiplyRGB(color);
			color2.A = 0;

			spriteBatch.Draw(tex, pos, null, Terraria.GameContent.UI.ItemRarity.GetColor(item.rare).MultiplyRGB(color), 0, tex.Size() / 2, scale, 0, 0);
			spriteBatch.Draw(texOver, pos, null, color2, 0, tex.Size() / 2, scale, 0, 0);
			spriteBatch.Draw(ItemTex, pos, null, color, 0, ItemTex.Size() / 2, scale, 0, 0);
			Utils.DrawBorderString(spriteBatch, count.ToString(), pos + Vector2.One * 14, color, 0.8f * scale, 1, 0.5f);

			if (IsMouseHovering && count > 0)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = item.Clone();
				Main.hoverItemName = "a";
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (Main.mouseItem.IsAir)
			{
				Main.mouseItem = ChefBagUI.openBag.RemoveItem(item.type) ?? Main.mouseItem;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}
			else if (ChefBagUI.openBag.InsertItem(Main.LocalPlayer.HeldItem))
			{
				Main.mouseItem.TurnToAir();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			if (Main.mouseItem.IsAir)
			{
				Main.mouseItem = ChefBagUI.openBag.RemoveItem(item.type, 1) ?? Main.mouseItem;
				Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
			}
			else if (Main.mouseItem.type == item.type && Main.mouseItem.stack < Main.mouseItem.maxStack)
			{
				Item removal = ChefBagUI.openBag.RemoveItem(item.type, 1);

				if (removal != null)
					Main.mouseItem.stack += removal.stack;

				Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}

		public override int CompareTo(object obj)
		{
			var other = obj as IngredientStorageSlot;

			return ChefBagUI.sortMode switch
			{
				"Rarity" => CompareRarity(other),
				"Type" => CompareType(other),
				"Alphabetical" => CompareAlphabetical(other),
				"Rarity Reverse" => CompareRarity(other) * -1,
				"Type Reverse" => CompareType(other) * -1,
				"Alphabetical Reverse" => CompareAlphabetical(other) * -1,
				_ => CompareRarity(other),//use rarity sort as a default
			};
		}

		private int CompareRarity(IngredientStorageSlot other)
		{
			int firstOrder = item.type > other.item.type ? 1 : 0;

			int x = item.rare * 6 + (int)(item.ModItem as Ingredient).ThisType * 2 + firstOrder;
			int y = other.item.rare * 6 + (int)(other.item.ModItem as Ingredient).ThisType * 2 + firstOrder;

			return x >= y ? 1 : -1;
		}

		private int CompareType(IngredientStorageSlot other)
		{
			int firstOrder = item.type > other.item.type ? 1 : 0;

			int x = (int)(item.ModItem as Ingredient).ThisType * 24 + item.rare * 2 + firstOrder;
			int y = (int)(other.item.ModItem as Ingredient).ThisType * 24 + other.item.rare * 2 + firstOrder;

			return x >= y ? 1 : -1;
		}

		private int CompareAlphabetical(IngredientStorageSlot other)
		{
			return item.Name.CompareTo(other.item.Name);
		}
	}

	class RecipieSlot : IngredientStorageSlot
	{
		public BonusIngredient Result => item.ModItem as BonusIngredient;

		public RecipieSlot(Item item, int index) : base(item, index)
		{
			this.item = item;

			Width.Set(230, 0);
			Height.Set(50, 0);

			Left.Set(index % 2 * 240, 0);
			Top.Set(index / 2 * 54, 0);

			for (int k = 0; k < 4; k++)
			{
				var newItem = new Item();
				newItem.SetDefaults(Result.Recipie().AsList()[k]);
				var slot = new IngredientStorageSlot(newItem, k + 1, 0.8f);
				slot.Left.Set(50 + k * 42, 0);
				slot.Top.Set(8, 0);
				Append(slot);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Main.instance.LoadItem(item.type);

			Texture2D tex = Request<Texture2D>(AssetDirectory.GUI + "FoodSlot").Value;
			Texture2D texOver = Request<Texture2D>(AssetDirectory.GUI + "FoodSlotOver").Value;
			Texture2D ItemTex = TextureAssets.Item[item.type].Value;
			Vector2 pos = GetDimensions().Center();
			pos.X = GetDimensions().X + 25;

			List<Item> items = ChefBagUI.openBag.Items;
			Item heldItem = items.FirstOrDefault(n => n.type == item.type);

			Color color = Color.White;

			Color color2 = (item.ModItem as Ingredient).GetColor().MultiplyRGB(color);
			color2.A = 0;

			float scale = ItemTex.Width > ItemTex.Height ? 32f / ItemTex.Width : 32f / ItemTex.Height;

			spriteBatch.Draw(tex, pos, null, Terraria.GameContent.UI.ItemRarity.GetColor(item.rare).MultiplyRGB(color), 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(texOver, pos, null, color2, 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(ItemTex, pos, null, color, 0, ItemTex.Size() / 2, scale, 0, 0);

			var hitbox = new Rectangle((int)GetDimensions().X, (int)GetDimensions().Y, 50, 50);

			if (IsMouseHovering && hitbox.Contains(Main.MouseScreen.ToPoint()))
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = item.Clone();
				Main.hoverItemName = "a";
			}

			foreach (SmartUIElement child in Children)
				child.Draw(spriteBatch);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			var hitbox = new Rectangle((int)GetDimensions().X, (int)GetDimensions().Y, 50, 50);

			if (hitbox.Contains(Main.MouseScreen.ToPoint()) && CookingUI.visible)
			{
				CookingUI cooking = UILoader.GetUIState<CookingUI>();

				if (!cooking.MainSlot.Item.IsAir)
					ChefBagUI.openBag.InsertItem(cooking.MainSlot.Item.Clone());

				cooking.MainSlot.Item = ChefBagUI.openBag.RemoveItem(Result.Recipie().mainType, 1) ?? new Item();

				if (!cooking.SideSlot0.Item.IsAir)
					ChefBagUI.openBag.InsertItem(cooking.SideSlot0.Item.Clone());

				cooking.SideSlot0.Item = ChefBagUI.openBag.RemoveItem(Result.Recipie().sideType, 1) ?? new Item();

				if (!cooking.SideSlot1.Item.IsAir)
					ChefBagUI.openBag.InsertItem(cooking.SideSlot1.Item.Clone());

				cooking.SideSlot1.Item = ChefBagUI.openBag.RemoveItem(Result.Recipie().sideType2, 1) ?? new Item();

				if (!cooking.SeasonSlot.Item.IsAir)
					ChefBagUI.openBag.InsertItem(cooking.SeasonSlot.Item.Clone());

				cooking.SeasonSlot.Item = ChefBagUI.openBag.RemoveItem(Result.Recipie().seasoningType, 1) ?? new Item();

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}
		}
	}
}

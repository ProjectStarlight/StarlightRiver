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
		public static UIScrollbar scroll = new();

		public static UIImageButton SortButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/SortButton", ReLogic.Content.AssetRequestMode.ImmediateLoad));

		public static UIImageButton IngredientTab = new(Request<Texture2D>("StarlightRiver/Assets/GUI/IngredientButton", ReLogic.Content.AssetRequestMode.ImmediateLoad));
		public static UIImageButton RecipieTab = new(Request<Texture2D>("StarlightRiver/Assets/GUI/MealButton", ReLogic.Content.AssetRequestMode.ImmediateLoad));

		public static string sortMode = "Rarity";

		public override bool Visible => visible;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			scroll.Width.Set(32, 0);
			scroll.Height.Set(216, 0);
			scroll.Left.Set(490 - 220, 0.5f);
			scroll.Top.Set(-220, 0.5f);
			Append(scroll);

			grid.Left.Set(-220, 0.5f);
			grid.Top.Set(-220, 0.5f);
			grid.Width.Set(490, 0);
			grid.Height.Set(216, 0);
			grid.ListPadding = 4;
			grid.MaxWidth.Set(490, 0);
			grid.MaxHeight.Set(216, 0);
			grid.SetScrollbar(scroll);
			Append(grid);

			AddElement(SortButton, -260, 0.5f, -220, 0.5f, 32, 0f, 32, 0f);
			SortButton.OnLeftClick += (a, b) => ChangeSortMode();

			AddElement(IngredientTab, -220, 0.5f, -256, 0.5f, 50, 0f, 28, 0f);
			IngredientTab.OnLeftClick += (a, b) => RebuildGrid();

			AddElement(RecipieTab, -160, 0.5f, -256, 0.5f, 50, 0f, 28, 0f);
			RecipieTab.OnLeftClick += (a, b) => RebuildRecipies();
		}

		private void ChangeSortMode()
		{
			sortMode = sortMode switch
			{
				"Rarity" => "Type",
				"Type" => "Owned",
				"Owned" => "Rarity",
				_ => "Rarity",
			};
			grid.UpdateOrder();
			UILoader.GetUIState<ChefBagUI>().Recalculate();
		}

		public static void Move(Vector2 moveTarget)
		{
			scroll.Left.Set(moveTarget.X + 490, 0);
			scroll.Top.Set(moveTarget.Y, 0);

			grid.Left.Set(moveTarget.X, 0);
			grid.Top.Set(moveTarget.Y, 0);

			SortButton.Left.Set(moveTarget.X - 40, 0);
			SortButton.Top.Set(moveTarget.Y, 0);

			IngredientTab.Left.Set(moveTarget.X, 0);
			IngredientTab.Top.Set(moveTarget.Y - 36, 0);

			RecipieTab.Left.Set(moveTarget.X + 60, 0);
			RecipieTab.Top.Set(moveTarget.Y - 36, 0);

			UILoader.GetUIState<ChefBagUI>().Recalculate();
		}

		public static void RebuildGrid()
		{
			grid.Clear();

			for (int k = 0; k < ChefBag.ingredientTypes.Count; k++)
			{
				var item = new Item();
				item.SetDefaults(ChefBag.ingredientTypes[k]);
				grid.Add(new IngredientStorageSlot(item, k));
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

			if (!Main.playerInventory)
				visible = false;

			grid.Width.Set(600, 0);
			grid.MaxWidth.Set(490, 0);

			Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (SortButton.IsMouseHovering)
				Main.hoverItemName = "Sort mode:\n" + sortMode;

			if (IngredientTab.IsMouseHovering)
				Main.hoverItemName = "Ingredients";

			if (RecipieTab.IsMouseHovering)
				Main.hoverItemName = "Cookbook";

			base.Draw(spriteBatch);
		}
	}

	class IngredientStorageSlot : SmartUIElement
	{
		public Item item;
		public float scale;

		public int IngredientType => (int)(item.ModItem as Ingredient).ThisType;

		public int Count
		{
			get
			{
				List<Item> items = ChefBagUI.openBag.Items;
				Item heldItem = items.FirstOrDefault(n => n.type == item.type);
				return heldItem?.stack ?? 0;
			}
		}

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
				"Owned" => CompareOwned(other),
				_ => CompareRarity(other),//use rarity sort as a default
			};
		}

		private int CompareRarity(IngredientStorageSlot other)
		{
			if (item.rare != other.item.rare)
				return item.rare.CompareTo(other.item.rare);

			if (IngredientType != other.IngredientType)
				return IngredientType.CompareTo(other.IngredientType);

			return item.type.CompareTo(other.item.type);
		}

		private int CompareType(IngredientStorageSlot other)
		{
			if (IngredientType != other.IngredientType)
				return IngredientType.CompareTo(other.IngredientType);

			if (item.rare != other.item.rare)
				return item.rare.CompareTo(other.item.rare);

			return item.type.CompareTo(other.item.type);
		}

		private int CompareOwned(IngredientStorageSlot other)
		{
			if (Count != other.Count)
				return other.Count.CompareTo(Count);

			if (item.rare != other.item.rare)
				return item.rare.CompareTo(other.item.rare);

			if (IngredientType != other.IngredientType)
				return IngredientType.CompareTo(other.IngredientType);

			return item.type.CompareTo(other.item.type);
		}
	}

	class RecipieSlot : IngredientStorageSlot
	{
		public BonusIngredient Result => item.ModItem as BonusIngredient;

		public RecipieSlot(Item item, int index) : base(item, index)
		{
			this.item = item;

			Width.Set(240, 0);
			Height.Set(50, 0);

			Left.Set(index % 2 * 240, 0);
			Top.Set(index / 2 * 54, 0);

			for (int k = 0; k < 4; k++)
			{
				var newItem = new Item();
				newItem.SetDefaults(Result.Recipie().AsList()[k]);
				var slot = new IngredientStorageSlot(newItem, k + 1, 0.8f);
				slot.Left.Set(54 + k * 44, 0);
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

			bool craftable = true;
			for(int k = 0; k < 4; k++)
			{
				craftable &= ChefBagUI.openBag.Items.Any(n => n != null && n.type == Result.Recipie().AsList()[k] && n.stack > 0); 
			}

			Color color = !craftable ? Color.LightGray * 0.5f : Color.White;

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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Items.Utility;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
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

		public override bool Visible => visible;

		public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

		public override void OnInitialize()
		{
			Elements.Clear();
			grid.Clear();

			grid.Left.Set(-220, 0.5f);
			grid.Top.Set(-220, 0.5f);
			grid.Width.Set(440, 0);
			grid.Height.Set(440, 0);
			grid.ListPadding = 4;
			grid.MaxWidth.Set(440, 0);
			grid.MaxHeight.Set(440, 0);

			Append(grid);

			if (openBag is null)
				return;

			for(int k = 0; k < ChefBag.ingredientTypes.Count; k++)
			{
				var item = new Item();
				item.SetDefaults(ChefBag.ingredientTypes[k]);
				grid.Add(new IngredientStorageSlot(item, k));
			}		
		}

		public static void Move(Vector2 moveTarget)
		{
			grid.Left.Set(moveTarget.X, 0);
			grid.Top.Set(moveTarget.Y, 0);

			Core.Loaders.UILoader.GetUIState<ChefBagUI>().Recalculate();
		}

		public override void Update(GameTime gameTime)
		{
			Recalculate();

			if (!Main.playerInventory)
				visible = false;
		}
	}

	class IngredientStorageSlot : UIElement
	{
		public Item Item;

		public IngredientStorageSlot(Item Item, int index)
		{
			this.Item = Item;

			Width.Set(50, 0);
			Height.Set(50, 0);

			Left.Set(index % 8 * 54, 0);
			Top.Set(index / 8 * 54, 0);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
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

			spriteBatch.Draw(tex, pos, null, Terraria.GameContent.UI.ItemRarity.GetColor(Item.rare).MultiplyRGB(color), 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(texOver, pos, null, color2, 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(ItemTex, pos, null, color, 0, ItemTex.Size() / 2, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch, count.ToString(), pos + Vector2.One * 14, color, 0.8f, 1, 0.5f);

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

			int firstOrder = Item.type > other.Item.type ? 1 : 0;

			int x = Item.rare * 6 + (int)(Item.ModItem as Ingredient).ThisType * 2 + firstOrder;
			int y = other.Item.rare * 6 + (int)(other.Item.ModItem as Ingredient).ThisType * 2 + firstOrder;

			return x >= y ? 1 : -1;
		}
	}
}

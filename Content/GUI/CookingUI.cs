using StarlightRiver.Content.Items.Food;
using StarlightRiver.Content.Items.Food.Special;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class CookingUI : SmartUIState
	{
		public static bool visible;

		private static bool Moving = false;
		private static Vector2 MoveOffset = Vector2.Zero;
		private static int scrollStart = 0;
		private static int lineCount = 0;

		public readonly CookingSlot MainSlot = new(IngredientType.Main);
		public readonly CookingSlot SideSlot0 = new(IngredientType.Side);
		public readonly CookingSlot SideSlot1 = new(IngredientType.Side);
		public readonly CookingSlot SeasonSlot = new(IngredientType.Seasoning);
		private readonly UIImageButton CookButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/CookPrep", ReLogic.Content.AssetRequestMode.ImmediateLoad));
		private readonly UIImageButton ExitButton = new(Request<Texture2D>("StarlightRiver/Assets/GUI/CookExit", ReLogic.Content.AssetRequestMode.ImmediateLoad));
		private readonly UIImage StatBack = new(Request<Texture2D>("StarlightRiver/Assets/GUI/CookStatWindow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
		private readonly UIImage TopBar = new(Request<Texture2D>("StarlightRiver/Assets/GUI/CookTop", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);

		public static Vector2 Basepos = new(Main.screenWidth / 2 - 173, Main.screenHeight / 2 - 122);

		public override bool Visible => visible;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Radial Hotbars"));
		}

		public override void OnInitialize()
		{
			CookButton.OnLeftClick += CookFood;
			CookButton.SetVisibility(1, 1);
			ExitButton.OnLeftClick += Exit;
			ExitButton.SetVisibility(1, 1);

			OnScrollWheel += ScrollStats;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (!Main.playerInventory)
				visible = false;

			if (TopBar.IsMouseHovering && Main.mouseLeft)
			{
				if (!Moving)
					MoveOffset = Main.MouseScreen - Basepos;

				Moving = true;
			}

			if (!Main.mouseLeft)
				Moving = false;

			if (Moving)
				Basepos = Main.MouseScreen - MoveOffset;

			if (Basepos.X < 520)
				Basepos.X = 520;

			if (Basepos.Y < 20)
				Basepos.Y = 20;

			if (Basepos.X > Main.screenWidth - 20 - 346)
				Basepos.X = Main.screenWidth - 20 - 346;

			if (Basepos.Y > Main.screenHeight - 20 - 244)
				Basepos.Y = Main.screenHeight - 20 - 244;

			ChefBagUI.Move(Basepos + new Vector2(-480, 0));

			Main.isMouseLeftConsumedByUI = true;
			SetPosition(MainSlot, 44, 44);
			SetPosition(SideSlot0, 10, 112);
			SetPosition(SideSlot1, 78, 112);
			SetPosition(SeasonSlot, 44, 180);
			SetPosition(StatBack, 170, 40);
			SetPosition(CookButton, 170, 202);
			SetPosition(ExitButton, 314, 0);
			SetPosition(TopBar, 0, 2);
		}

		private void ScrollStats(UIScrollWheelEvent evt, UIElement listeningElement)
		{
			scrollStart -= evt.ScrollWheelValue > 0 ? 1 : -1;
			scrollStart = (int)MathHelper.Clamp(scrollStart, 0, lineCount - 5);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var backDims = new Rectangle((int)Basepos.X - 10, (int)Basepos.Y - 10, 376, 266);

			if (ChefBagUI.visible)
			{
				backDims.X -= 520;
				backDims.Width += 520;
			}

			spriteBatch.Draw(TextureAssets.MagicPixel.Value, backDims, new Color(25, 25, 25) * 0.5f);

			base.Draw(spriteBatch);
			Utils.DrawBorderString(spriteBatch, "Ingredients", Basepos + new Vector2(38, 8), Color.White, 0.8f);
			Utils.DrawBorderString(spriteBatch, "Info/Stats", Basepos + new Vector2(202, 8), Color.White, 0.8f);
			Utils.DrawBorderString(spriteBatch, "Prepare", Basepos + new Vector2(212, 210), Color.White, 1.1f);

			int drawY = 0;
			if (!Elements.Any(n => n is CookingSlot && !(n as CookingSlot).Item.IsAir && ((n as CookingSlot).Item.ModItem as Ingredient).ThisType == IngredientType.Main))
			{
				Utils.DrawBorderString(spriteBatch, "Place a Main Course in\nthe top slot to start\ncooking", Basepos + new Vector2(186, 54 + drawY), Color.White, 0.7f);
			}
			else
			{
				int duration = 0;
				float durationMult = 1;
				int cooldown = 0;
				float cooldownMult = 1;
				List<(string, Color)> lines = new List<(string, Color)>();

				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)Basepos.X + 182, (int)Basepos.Y + 52, 152, lineCount >= 5 ? 18 * 5 : lineCount * 18), new Color(40, 20, 10) * 0.5f);
				spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)Basepos.X + 182, (int)Basepos.Y + 148, 152, 28), new Color(40, 20, 10) * 0.5f);

				foreach (SmartUIElement element in Elements.Where(n => n is CookingSlot && !(n as CookingSlot).Item.IsAir))
				{
					var ingredient = (element as CookingSlot).Item.ModItem as Ingredient;

					string[] strings = ingredient.ItemTooltip.Split('\n');

					for (int k = 0; k < strings.Count(); k++)
					{
						string text = "~" + Helper.WrapString(strings[k], 100, FontAssets.ItemStack.Value, 0.65f);
						string[] substrings = text.Split('\n');

						for (int n = 0; n < substrings.Length; n++)
							lines.Add((substrings[n], ingredient.GetColor()));
					}

					duration += ingredient.Fill;
					durationMult *= ingredient.FullnessMult;

					cooldown += (int)(ingredient.Fill * 1.5f);
					cooldownMult *= ingredient.WellFedMult;
				}

				duration = (int)(duration * durationMult);
				cooldown = (int)(cooldown * cooldownMult);

				int max = (int)MathHelper.Clamp(scrollStart + 5, 0, lines.Count());
				lineCount = lines.Count();

				for (int k = scrollStart; k < max; k++)
				{
					(string, Color) line = lines[k];
					Utils.DrawBorderString(spriteBatch, line.Item1, Basepos + new Vector2(186, 54 + drawY), line.Item2, 0.65f);
					drawY += (int)(FontAssets.ItemStack.Value.MeasureString(line.Item1).Y * 0.65f) + 2;
				}

				Utils.DrawBorderString(spriteBatch, duration / 60 + " seconds duration", Basepos + new Vector2(186, 150), new Color(110, 235, 255), 0.65f);
				Utils.DrawBorderString(spriteBatch, cooldown / 60 + " seconds fullness", Basepos + new Vector2(186, 164), new Color(255, 170, 120), 0.65f);

				if (lineCount > 5)
				{
					Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GUI/Arrow").Value;

					spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)Basepos.X + 352, (int)Basepos.Y + 60, 4, 80), new Color(20, 20, 10) * 0.5f);
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
				var Item = new Item();
				Item.SetDefaults(ItemType<Meal>()); //let TML hanlde making the Item properly
				(Item.ModItem as Meal).Ingredients = new List<Item>();
				CookIngredient(Item, MainSlot);
				CookIngredient(Item, SideSlot0);
				CookIngredient(Item, SideSlot1);
				CookIngredient(Item, SeasonSlot);

				FoodRecipie special = FoodRecipieHandler.Recipes.FirstOrDefault(n => n.Matches((Item.ModItem as Meal).Ingredients));

				if (special.result != 0) //Bad check. This entire addition is kind of a bandaid. That kinda sucks.
					(Item.ModItem as Meal).Ingredients.Add(FoodRecipieHandler.GetFromRecipie(special));

				Item.position = Main.LocalPlayer.Center;
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), Item);

				Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BetsyScream); //TODO: Change to custom chop chop sizzle sound
			}
		}

		private static void CookIngredient(Item target, CookingSlot source)
		{
			if (!source.Item.IsAir && source.Item.ModItem is Ingredient)
			{
				(target.ModItem as Meal).Ingredients.Add(source.Item.Clone());
				(target.ModItem as Meal).FullnessMult *= (source.Item.ModItem as Ingredient).FullnessMult;
				(target.ModItem as Meal).WellFedMult *= (source.Item.ModItem as Ingredient).WellFedMult;
				(target.ModItem as Meal).Fullness += (source.Item.ModItem as Ingredient).Fill;

				if (source.Item.stack == 1)
					source.Item.TurnToAir();
				else
					source.Item.stack--;
			}
		}

		private void Exit(UIMouseEvent evt, UIElement listeningElement)
		{
			visible = false;
			Main.playerInventory = false;
			Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuClose);
		}
	}

	public class CookingSlot : SmartUIElement
	{
		public Item Item = new();
		private readonly IngredientType Type;

		public CookingSlot(IngredientType type)
		{
			Type = type;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;

			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GUI/CookSlotY").Value;
			switch (Type)
			{
				case IngredientType.Main: tex = Request<Texture2D>("StarlightRiver/Assets/GUI/CookSlotY").Value; break;
				case IngredientType.Side: tex = Request<Texture2D>("StarlightRiver/Assets/GUI/CookSlotG").Value; break;
				case IngredientType.Seasoning: tex = Request<Texture2D>("StarlightRiver/Assets/GUI/CookSlotB").Value; break;
			}

			spriteBatch.Draw(tex, GetDimensions().Position(), tex.Frame(), Color.White, 0, Vector2.Zero, 1, 0, 0);

			if (!Item.IsAir)
			{
				Texture2D tex2 = Request<Texture2D>(Item.ModItem.Texture).Value;
				spriteBatch.Draw(tex2, new Rectangle((int)GetDimensions().X + 30, (int)GetDimensions().Y + 30, (int)MathHelper.Min(tex2.Width, 28), (int)MathHelper.Min(tex2.Height, 28)), tex2.Frame(), Color.White, 0, tex2.Size() / 2, 0, 0);

				if (Item.stack > 1)
					Utils.DrawBorderString(spriteBatch, Item.stack.ToString(), GetDimensions().Position() + Vector2.One * 32, Color.White, 0.75f);

				if (IsMouseHovering)
				{
					Main.LocalPlayer.mouseInterface = true;
					Main.HoverItem = Item.Clone();
					Main.hoverItemName = "a";
				}
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			Player Player = Main.LocalPlayer;

			if (Main.mouseItem.IsAir && !Item.IsAir) //if the cursor is empty and there is something in the slot, take the Item out
			{
				Main.mouseItem = Item.Clone();
				Item.TurnToAir();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}

			if (Item.IsAir && Player.HeldItem.type == Item.type) //if the cursor is the same type as the Item already in the slot, add to the slot
			{
				Item.stack += Player.HeldItem.stack;
				Player.HeldItem.TurnToAir();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}

			if (Player.HeldItem.ModItem is Ingredient && (Player.HeldItem.ModItem as Ingredient).ThisType == Type && Item.IsAir) //if the slot is empty and the cursor has an Item, put it in the slot
			{
				Item = Player.HeldItem.Clone();
				Player.HeldItem.TurnToAir();
				Main.mouseItem.TurnToAir();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
			}

			if (Player.HeldItem.ModItem is Ingredient && (Player.HeldItem.ModItem as Ingredient).ThisType == Type && !Item.IsAir) //swap or stack
			{
				if (Player.HeldItem.type == Item.type) //stack
				{
					if (Item.stack + Player.HeldItem.stack > Item.maxStack)
					{
						Main.mouseItem.stack = Item.stack + Player.HeldItem.stack - Item.maxStack;
						Item.stack = Item.maxStack;
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
					}
					else
					{
						Item.stack += Player.HeldItem.stack;
						Player.HeldItem.TurnToAir();
						Main.mouseItem.TurnToAir();
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
					}
				}
				else //swap
				{
					Item temp = Item;
					Item = Player.HeldItem;
					Main.mouseItem = temp;
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
				}
			}

			Main.isMouseLeftConsumedByUI = true;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (Item.type == ItemID.None || Item.stack <= 0)
				Item.TurnToAir();

			Width.Set(60, 0);
			Height.Set(60, 0);
		}
	}
}
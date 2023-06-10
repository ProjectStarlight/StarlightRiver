using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	class ForgeMap : ModItem
	{
		public bool isEpic = false;

		public override string Texture => AssetDirectory.DesertItem + "Sandscript";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glassweaver's Map");
			Tooltip.SetDefault("Quite a self-explanatory exquisite piece of art isn't it?\nRight click on the item to view the map.");
		}

		public override void SetDefaults()
		{
			Item.consumable = false;
			Item.rare = ItemRarityID.Quest;

			Item.width = 32;
			Item.height = 32;

			Item.maxStack = 1;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (isEpic)
			{
				tooltips.FirstOrDefault(n => n.Name == "ItemName" && n.Mod == "Terraria").Text = "Glassweaver's... Map?";
				tooltips.FirstOrDefault(n => n.Name == "Tooltip0" && n.Mod == "Terraria").Text = "Erm... This doesn't look quit right...";
			}
			else
			{
				tooltips.FirstOrDefault(n => n.Name == "ItemName" && n.Mod == "Terraria").Text = "Glassweaver's Great Map of the Grand Forge Temple";
			}
		}

		public override bool CanRightClick()
		{
			return true;
		}

		public override bool ConsumeItem(Player player)
		{
			return false;
		}

		public override void RightClick(Player player)
		{
			UILoader.GetUIState<GlassTempleMapUI>().Display(isEpic);
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("isEpic", isEpic);
		}

		public override void LoadData(TagCompound tag)
		{
			isEpic = tag.GetBool("isEpic");
		}
	}

	public class GlassTempleMapUI : SmartUIState
	{
		public UIImage exitButton = new(Request<Texture2D>(AssetDirectory.VitricItem + "GlassTempleMapClose", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);

		public Texture2D mapTexture = Request<Texture2D>(AssetDirectory.VitricItem + "GlassTempleMap", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		public Texture2D swmgTexture = Request<Texture2D>(AssetDirectory.VitricItem + "GlassweaverGauntletGang", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

		public Vector2 basePos;

		public bool dragging;
		public Vector2 dragOff;
		public bool visible;
		public bool isEpic;

		public int width;
		public int height;
		public float scale;

		public Rectangle BoundingBox => new((int)basePos.X, (int)basePos.Y, width, height);

		public Texture2D Texture => isEpic ? swmgTexture : mapTexture;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			exitButton.OnLeftClick += (a, b) => Visible = false;
			AddElement(exitButton, 200, 0f, 32, 0f, 38, 0f, 38, 0f);
			scale = 0.75f;
			basePos = (new Vector2(Main.screenWidth, Main.screenHeight) - Texture.Size()) / 2;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			width = (int)(Texture.Width * scale);
			height = (int)(Texture.Height * scale);

			Recalculate();
			exitButton.Recalculate();

			if (!Main.mouseLeft && dragging)
				dragging = false;

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft || dragging)
			{
				dragging = true;

				if (dragOff == Vector2.Zero)
					dragOff = Main.MouseScreen - basePos;

				basePos = Main.MouseScreen - dragOff;
			}
			else
			{
				dragOff = Vector2.Zero;
			}

			exitButton.Left.Set(width - 48, 0);
			exitButton.Top.Set(24, 0);

			if (exitButton.IsMouseHovering)
			{
				Tooltip.SetName("Close Map");
			}

			Recalculate();
			exitButton.Recalculate();

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				Main.LocalPlayer.mouseInterface = true;
		}

		public void Display(bool isEpic)
		{
			this.isEpic = isEpic;

			Visible = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Height.Set(Texture.Height * scale, 0);
			Width.Set(Texture.Width * scale, 0);
			Left.Set(basePos.X, 0);
			Top.Set(basePos.Y, 0);


			spriteBatch.Draw(Texture, GetDimensions().ToRectangle(), Color.White);

			exitButton.Draw(spriteBatch);

			Recalculate();
		}
	}
}
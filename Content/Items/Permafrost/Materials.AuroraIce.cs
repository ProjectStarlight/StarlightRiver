using System;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	class AuroraIceItem : QuickMaterial
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraIceItem";

		public AuroraIceItem() : base("Frozen Aurora Chunk", "A preserved piece of the night sky", 999, Item.sellPrice(0, 0, 5, 0), ItemRarityID.White) { }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(tex, position, frame, itemColor, 0, origin, scale, 0, 0);

			Color overColor = Color.White;
			overColor.A = 0;

			spriteBatch.Draw(tex, position, frame, overColor * 0.75f, 0, origin, scale, 0, 0);

			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, Item.color * 0.25f, rotation, tex.Size() / 2, scale, 0, 0);

			Color overColor = Color.White;
			overColor.A = 0;

			spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, overColor * 0.25f, rotation, tex.Size() / 2, scale, 0, 0);

			return false;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			float off = 0;
			float time = Main.GameUpdateCount / 200f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			Item.color = color;
		}

		public override void UpdateInventory(Player player)
		{
			float off = 0;
			float time = Main.GameUpdateCount / 200f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			Item.color = color;
		}
	}

	class AuroraIceBar : QuickMaterial
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraIceBar";

		public AuroraIceBar() : base("Frozen Aurora Bar", "A preserved selection of the night sky", 99, Item.sellPrice(0, 0, 25, 0), ItemRarityID.Blue) { }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(tex, position, frame, itemColor, 0, origin, scale, 0, 0);

			Color overColor = Color.White;
			overColor.A = 0;

			spriteBatch.Draw(tex, position, frame, overColor * 0.75f, 0, origin, scale, 0, 0);

			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, Item.color * 0.25f, rotation, tex.Size() / 2, scale, 0, 0);

			Color overColor = Color.White;
			overColor.A = 0;

			spriteBatch.Draw(tex, Item.position - Main.screenPosition, null, overColor * 0.25f, rotation, tex.Size() / 2, scale, 0, 0);

			return false;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			float off = 0;
			float time = Main.GameUpdateCount / 200f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			Item.color = color;
		}

		public override void UpdateInventory(Player player)
		{
			float off = 0;
			float time = Main.GameUpdateCount / 200f * 6.28f;

			float sin2 = (float)Math.Sin(time + off * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + off * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			Item.color = color;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<AuroraIceItem>(), 3);
			recipe.AddTile(TileID.Furnaces);
			recipe.Register();
		}
	}
}

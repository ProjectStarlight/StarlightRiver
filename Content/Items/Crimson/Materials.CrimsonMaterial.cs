using StarlightRiver.Content.Items.Moonstone;
using StarlightRiver.Content.Tiles.Crimson;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class DendriteBar : QuickMaterial
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public DendriteBar() : base("Dendrite Bar", "This bar might be smarter than you...", 9999, 4000, 2) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe()
			.AddIngredient(ModContent.ItemType<DendriteItem>(), 4)
			.AddTile(TileID.Furnaces)
			.Register();
		}
	}

	public class ImaginaryTissue : QuickMaterial
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public ImaginaryTissue() : base("Imaginary Tissue", "As long as you focus on it, its real", 9999, 10000, 4) { }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			var tex = Assets.Items.Crimson.ImaginaryTissue.Value;

			Effect effect = Filters.Scene["MirageItemFilter"].GetShader().Shader;

			effect.Parameters["u_color"].SetValue(Vector3.One);
			effect.Parameters["u_fade"].SetValue(Vector3.One);
			effect.Parameters["u_resolution"].SetValue(tex.Size());
			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.05f);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.LinearClamp, default, default, effect, Main.UIScaleMatrix);

			spriteBatch.Draw(tex, position, frame, drawColor, 0, origin, scale, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);

			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			var tex = Assets.Items.Crimson.ImaginaryTissue.Value;

			Effect effect = Filters.Scene["MirageItemFilter"].GetShader().Shader;

			effect.Parameters["u_color"].SetValue(Vector3.One);
			effect.Parameters["u_fade"].SetValue(Vector3.One);
			effect.Parameters["u_resolution"].SetValue(tex.Size());
			effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.05f);

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.LinearClamp, default, default, effect, Main.UIScaleMatrix);

			spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, Color.White, rotation, tex.Size() / 2f, scale, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);

			return false;
		}
	}
}
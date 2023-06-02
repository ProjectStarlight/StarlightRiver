using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
	internal class DullBladeIngredient : AlchemyIngredient
	{
		public DullBladeIngredient()
		{
			ingredientColor = new Color(200, 200, 205);
		}

		public override int GetItemID()
		{
			return ModContent.ItemType<DullBlade>();
		}

		public override void VisualUpdate(AlchemyWrapper wrapper)
		{
			base.VisualUpdate(wrapper);

			if (Main.rand.NextBool(20))
				Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(0, 20), wrapper.cauldronRect.Width, 0, ModContent.DustType<BuzzSpark>(), 0, -2, 0, new Color(200, 200, 205) * 0.8f, 0.75f);
		}

		public override bool AddToStack(Item Item)
		{
			return false; //unstackable equipment
		}
	}
}
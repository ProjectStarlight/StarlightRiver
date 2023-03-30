using StarlightRiver.Content.Dusts;
using Terraria.ID;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
	internal class DirtIngredient : AlchemyIngredient
	{
		//TODO: this is being treated as though it is the tarnished ring that does not exist yet, replace with tarnished ring
		public DirtIngredient()
		{
			ingredientColor = Color.DarkRed;
		}

		public override int GetItemID()
		{
			return ItemID.DirtBlock;
		}

		public override void VisualUpdate(AlchemyWrapper wrapper)
		{
			base.VisualUpdate(wrapper);

			if (Main.rand.NextBool(20))
			{
				Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 20), wrapper.cauldronRect.Width, 0, ModContent.DustType<NeedlerDustSlowFade>(), 0, -2, 120, Color.Black, 0.5f);
			}
		}

		public override bool AddToStack(Item Item)
		{
			return false; //unstackable equipment
		}
	}
}
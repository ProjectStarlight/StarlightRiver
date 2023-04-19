using Terraria.ID;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
	class LivingFireIngredient : AlchemyIngredient
	{

		//TODO: this is a placeholder for Blood replace with that once added
		public LivingFireIngredient()
		{
			ingredientColor = Color.Red;
		}

		public override int GetItemID()
		{
			return ItemID.LivingFireBlock;
		}
	}
}
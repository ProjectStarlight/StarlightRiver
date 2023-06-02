using StarlightRiver.Content.Dusts;

namespace StarlightRiver.Content.Alchemy.Ingredients
{
	class VitricOreIngredient : AlchemyIngredient
	{
		public VitricOreIngredient()
		{
			ingredientColor = Color.Aquamarine;
		}

		public override int GetItemID()
		{
			return ModContent.ItemType<Items.Vitric.VitricOre>();
		}

		public override void VisualUpdate(AlchemyWrapper wrapper)
		{
			base.VisualUpdate(wrapper);
			if (timeSinceAdded == 0)
			{
				for (int i = 0; i < 3; i++)
				{
					Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 25), wrapper.cauldronRect.Width, 25, ModContent.DustType<CrystalSparkle>(), 0, 0);
					Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 25), wrapper.cauldronRect.Width, 25, ModContent.DustType<CrystalSparkle2>(), 0, 0);
				}
			}

			if (Main.rand.NextBool(50))
			{
				if (Main.rand.NextBool())
					Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 25), wrapper.cauldronRect.Width, 25, ModContent.DustType<CrystalSparkle>(), 0, 0);
				else
					Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 25), wrapper.cauldronRect.Width, 25, ModContent.DustType<CrystalSparkle2>(), 0, 0);
			}
		}
	}
}
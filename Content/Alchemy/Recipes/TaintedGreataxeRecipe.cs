using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Alchemy.Recipes
{
	public class TaintedGreataxeRecipe : AlchemyRecipe, IPostLoadable
	{
		public void PostLoad()
		{
			AddIngredientById(ModContent.ItemType<DullBlade>());
			AddIngredientById(ItemID.LivingFireBlock); //placeholder for "blood"
			AddOutputById(ModContent.ItemType<TaintedGreataxe>());
			AddRecipe();
		}

		public void PostLoadUnload()
		{
		}

		public override bool UpdateCrafting(AlchemyWrapper wrapper, List<AlchemyIngredient> currentingredients, CauldronDummyAbstract cauldronDummy)
		{
			if (wrapper.timeSinceCraftStarted < 180)
			{
				if (wrapper.timeSinceCraftStarted % 3 == 0)
				{
					Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(5, 20), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<BuzzSpark>(), 0, -5, newColor: new Color(200, 200, 205) * 0.8f);
					if (Main.rand.NextBool(3))
						Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 20), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<GlowFastDecelerate>(), 0, -3.5f, newColor: new Color(200, 200, 205), Scale: 0.35f);

					if (Main.rand.NextBool(3))
						Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 20), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<GlowFastDecelerate>(), 0, -3.5f, newColor: new Color(85, 220, 55), Scale: 0.35f);
				}
			}

			wrapper.bubbleColor = new Color(200, 200, 205);

			if (wrapper.timeSinceCraftStarted >= 200)
			{
				foreach (Item eachOutputItem in outputItemList)
				{
					Item.NewItem(new EntitySource_WorldEvent(), wrapper.cauldronRect.Center(), Vector2.Zero, eachOutputItem.type, Stack: eachOutputItem.stack, prefixGiven: eachOutputItem.prefix);
				}

				foreach (AlchemyIngredient eachIngredient in currentingredients)
				{
					requiredIngredientsMap.TryGetValue(eachIngredient.storedItem.type, out Item requiredItem);

					eachIngredient.storedItem.stack -= requiredItem.stack * wrapper.currentBatchSize;
				}

				for (int i = 0; i < 13; i++)
				{
					Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(0, 5), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<GlowFastDecelerate>(), 0, -3.5f, newColor: new Color(200, 200, 205), Scale: 0.4f);

					Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(0, 5), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<Glow>(), 0, -3f, newColor: new Color(200, 200, 205), Scale: 0.45f);
				}

				SoundEngine.PlaySound(SoundID.Splash, wrapper.cauldronRect.Center());
				cauldronDummy.DumpIngredients();
			}

			return true;
		}
	}
}

using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.Vitric;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Alchemy.Recipes
{
	public class BloodCrystalRecipe : AlchemyRecipe, IPostLoadable
	{
		public void PostLoad()
		{
			//todo: update with real components blood crystal, tarnished ring, blood when they are implemented

			AddIngredientById(ItemID.DirtBlock); //placeholder for "tarnished ring"
			AddIngredientById(ItemID.LivingFireBlock); //placeholder for "blood"
			AddIngredientById(ModContent.ItemType<VitricOre>(), 16);

			AddOutputById(ModContent.ItemType<BloodlessAmulet>()); //placeholder for "blood crystal"

			AddRecipe();
		}

		public void PostLoadUnload()
		{
		}

		public override bool UpdateCrafting(AlchemyWrapper wrapper, List<AlchemyIngredient> currentingredients, CauldronDummyAbstract cauldronDummy)
		{
			if (wrapper.timeSinceCraftStarted < 60)
			{
				for (int i = 0; i < wrapper.timeSinceCraftStarted / 3; i++)
				{
					Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(5, -30), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<BloodCrystalRecipeDust>(), 0, -10, Scale: 0.3f);
				}
			}

			wrapper.bubbleColor = Color.Red;

			if (Main.rand.NextBool(10))
			{
				if (Main.rand.NextBool())
					Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, wrapper.timeSinceCraftStarted / 2), wrapper.cauldronRect.Width, 20, ModContent.DustType<CrystalSparkle>(), 0, 0, newColor: Color.Red);
				else
					Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, wrapper.timeSinceCraftStarted / 2), wrapper.cauldronRect.Width, 20, ModContent.DustType<CrystalSparkle2>(), 0, 0, newColor: Color.Red);
			}

			if (wrapper.timeSinceCraftStarted > 60)
			{
				wrapper.bubbleOpacity = 1f - (wrapper.timeSinceCraftStarted - 60) * 0.025f;
			}

			if (wrapper.timeSinceCraftStarted >= 200)
			{
				foreach (Item eachOutputItem in outputItemList)
				{
					Item.NewItem(new EntitySource_WorldEvent(), wrapper.cauldronRect.Center() - new Vector2(0, 100), Vector2.Zero, eachOutputItem.type, Stack: eachOutputItem.stack, prefixGiven: eachOutputItem.prefix);
				}

				foreach (AlchemyIngredient eachIngredient in currentingredients)
				{
					requiredIngredientsMap.TryGetValue(eachIngredient.storedItem.type, out Item requiredItem);

					eachIngredient.storedItem.stack -= requiredItem.stack * wrapper.currentBatchSize;
				}

				cauldronDummy.DumpIngredients();
			}

			return true;
		}
	}
	public class BloodCrystalRecipeDust : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var gray = new Color(25, 25, 25);
			return gray * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.85f;
			else
				dust.velocity *= 0.92f;

			if (dust.alpha > 130)
			{
				dust.scale *= 0.92f;
				dust.alpha += 8;
			}
			else
			{
				dust.alpha += 1;
			}

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
}

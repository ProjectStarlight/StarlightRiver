using Microsoft.Xna.Framework;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Alchemy.Recipes
{
    public class TaintedGreataxeRecipe : AlchemyRecipe, IPostLoadable
    {
        public void PostLoad()
        {
            addIngredientById(ModContent.ItemType<DullBlade>());
            addIngredientById(ItemID.LivingFireBlock); //placeholder for "blood"
            addOutputById(ModContent.ItemType<TaintedGreataxe>());
            addRecipe();
        }

        public void PostLoadUnload()
        {
        }

        public override bool UpdateCrafting(AlchemyWrapper wrapper, List<AlchemyIngredient> currentingredients, CauldronDummyAbstract cauldronDummy)
        {
            if (wrapper.timeSinceCraftStarted < 180)
                if (wrapper.timeSinceCraftStarted % 3 == 0)
                {
                    Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(5, 20), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<BuzzSpark>(), 0, -5, newColor: new Color(200, 200, 205) * 0.8f);
                    if (Main.rand.NextBool(3))
                        Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 20), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<GlowFastDecelerate>(), 0, -3.5f, newColor: new Color(200, 200, 205), Scale: 0.35f);

                    if (Main.rand.NextBool(3))
                        Dust.NewDust(wrapper.cauldronRect.TopLeft() - new Vector2(0, 20), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<GlowFastDecelerate>(), 0, -3.5f, newColor: new Color(85, 220, 55), Scale: 0.35f);
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
                    Item requiredItem;
                    requiredIngredientsMap.TryGetValue(eachIngredient.storedItem.type, out requiredItem);

                    eachIngredient.storedItem.stack -= requiredItem.stack * wrapper.currentBatchSize;
                }
                for (int i = 0; i < 13; i++)
                {
                    Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(0, 5), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<GlowFastDecelerate>(), 0, -3.5f, newColor: new Color(200, 200, 205), Scale: 0.4f);

                    Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(0, 5), wrapper.cauldronRect.Width - 10, 30, ModContent.DustType<Glow>(), 0, -3f, newColor: new Color(200, 200, 205), Scale: 0.45f);
                }
                SoundEngine.PlaySound(SoundID.Splash, wrapper.cauldronRect.Center());
                cauldronDummy.dumpIngredients();
            }

            return true;
        }
    }
}

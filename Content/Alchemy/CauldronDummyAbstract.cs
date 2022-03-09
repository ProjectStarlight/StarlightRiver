using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Alchemy
{
    public abstract class CauldronDummyAbstract : Dummy
    {
        //This serves as the core logic driver of the alchemy system
        //any inputs and outputs will be routed through here
        //and this will execute the calls to any ingredient logic and visuals
        //Abstract so this can be overridden by more specific cauldrons if later there are multiple cauldrons with similar logic but different visuals

        List<AlchemyIngredient> currentIngredients = new List<AlchemyIngredient>();

        List<AlchemyRecipe> possibleRecipes = AlchemyRecipeSystem.recipeList;

        AlchemyIngredient mostRecentIngredient = null;

        AlchemyWrapper wrapper = new AlchemyWrapper();

        public const int bubbleAnimationFrameTime = 8;
        public const int bubbleAnimationFrames = 10;
        public const int bubbleYOffset = 10; //bubble animation is centered in their frames so we need an offset to find bottom

        protected CauldronDummyAbstract(int validType, int width, int height) : base(validType, width, height)
        {
        }

        public sealed override void Update()
        {
            foreach (Item eachWorldItem in Main.item)
            {
                if (eachWorldItem.active && projectile.Hitbox.Contains(eachWorldItem.Center.ToPoint()))
                {
                    if (AttemptAddItem(eachWorldItem))
                    {
                        //todo: mp logic
                        eachWorldItem.active = false;
                        eachWorldItem.TurnToAir();
                    }
                }
            }

            if (mostRecentIngredient != null)
            {
                if (wrapper.bubbleAnimationTimer >= bubbleAnimationFrameTime)
                {
                    wrapper.bubbleAnimationTimer = 0;
                    wrapper.bubbleAnimationFrame++;
                    wrapper.bubbleAnimationFrame %= bubbleAnimationFrames;
                }
                wrapper.bubbleColor = new Color(127, 127, 127);
                wrapper.cauldronRect = projectile.Hitbox;
                wrapper.bubbleAnimationTimer++;

                bool ignoreRegularVisuals = mostRecentIngredient.mostRecentUpdate(wrapper);

                foreach (AlchemyIngredient ingredient in currentIngredients)
                {
                    ingredient.Update(wrapper);
                    if (!ignoreRegularVisuals)
                        ingredient.visualUpdate(wrapper);
                    ingredient.incrementTimer();
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            if (mostRecentIngredient != null)
            {
                Texture2D bubbleSheet = GetTexture(AssetDirectory.AlchemyTile + "BubbleSheet");
                Texture2D bubbleGlow = GetTexture(AssetDirectory.AlchemyTile + "BubbleSheetGlow");
                int frameHeight = bubbleSheet.Height / bubbleAnimationFrames;
                
                spriteBatch.Draw(bubbleSheet, projectile.position - Main.screenPosition - new Vector2(0, frameHeight - bubbleYOffset), new Rectangle(0, frameHeight * wrapper.bubbleAnimationFrame, bubbleSheet.Width, frameHeight), wrapper.bubbleColor);
                spriteBatch.Draw(bubbleGlow, projectile.position - Main.screenPosition - new Vector2(0, frameHeight - bubbleYOffset), new Rectangle(0, frameHeight * wrapper.bubbleAnimationFrame, bubbleSheet.Width, frameHeight), wrapper.bubbleColor);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            return false;
        }

        /// <summary>
        /// empties out cauldron and dumps items into the world and resets any data like possible recipes
        /// </summary>
        public void dumpIngredients()
        {
            possibleRecipes = AlchemyRecipeSystem.recipeList;
            mostRecentIngredient = null;
        }

        /// <summary>
        /// attempts to insert a specific item into the cauldron. if cannot be added returns false and performs no additional logic.
        /// if can be added will create and add ingredient to the current ingredients, and update possibleRecipes, returning true
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AttemptAddItem(Item item)
        {
            List<AlchemyRecipe> newPossibilities =  AlchemyRecipeSystem.getRemainingPossiblities(item, possibleRecipes);
            if (newPossibilities != null && newPossibilities.Count >= 1)
            {
                possibleRecipes = newPossibilities;

                AlchemyIngredient newIngredient = AlchemyRecipeSystem.instantiateIngredient(item);

                mostRecentIngredient = newIngredient;

                currentIngredients.Add(newIngredient);

                //TODO: mp logic here ?
                return true;
            }
            else
                return false;
        }
    }
}

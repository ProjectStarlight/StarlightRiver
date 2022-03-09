using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Alchemy
{
    public abstract class AlchemyIngredient
    {
        /// <summary>
        /// When instantiated by the cauldron dummy we may need to store exactly the item being used as an ingredient so we don't lose properties on the ingredient like modifiers etc
        /// </summary>
        public Item storedIngredient;

        /// <summary>
        /// Item type ID of the ingredient, use ItemType<ModItem>() to get mod item IDs and Terraria.ID.ItemID for vanilla items
        /// </summary>
        public readonly int itemType;

        /// <summary>
        /// if not overriding default visuals this will be used to lerp the overall color towards this value. defaults to a sort of light blue
        /// </summary>
        public Color ingredientColor = new Color(3, 127, 252); 

        protected int timeSinceAdded;

        public AlchemyIngredient() { }

        public abstract int getItemId();

        /// <summary>
        /// for instantiating the actual item into the ingredient
        /// </summary>
        /// <param name="ingredientItem"></param>
        public void putIngredient(Item ingredientItem)
        {
            this.storedIngredient = ingredientItem;
            timeSinceAdded = 0;
        }

        /// <summary>
        /// called at the end of every frame that this is in the cauldron to increment the timer
        /// 0 on first frame inserted
        /// </summary>
        public void incrementTimer()
        {
            timeSinceAdded++;
        }

        /// <summary>
        /// performs logic and visuals while this is the most recent item added to the cauldron. return true if visual updates should be skipped for other ingredents currently in cauldron
        /// </summary>
        public virtual bool mostRecentUpdate(AlchemyWrapper wrapper)
        {
            return false;
        }


        /// <summary>
        /// perform logic updates while this is added to the cauldron, executed by both client and server. runs even if mostRecentUpdate returned true.
        /// runs after the most recent ingredient executes mostRecentUpdate and executes in order of oldest ingredient to newest. also executes for most recent ingredient.
        /// designed for the idea of "dangerous" ingredients that may damage nearby players or have other tangible effects on the world and players outside of just visuals
        /// </summary>
        public virtual void Update(AlchemyWrapper wrapper)
        {

        }

        /// <summary>
        /// perform client-only updates while this is added to the cauldron runs after Update, skipped if mostRecentUpdate returned true
        /// runs in order of the oldest ingredient to newest immediately after running Update for this ingredient
        /// </summary>
        public virtual void visualUpdate(AlchemyWrapper wrapper)
        {
            wrapper.bubbleAnimationTimer += 0.1f; //slightly increase bubbling speed

            if (timeSinceAdded == 0)
            {
                
                
                for (int k = 0; k < 10; k++)
                    Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(wrapper.cauldronRect.Width / 4, 0), wrapper.cauldronRect.Width / 2, 0, DustID.Water, 0, -6, 0, default, 1f);
                
                Main.PlaySound(SoundID.Splash, wrapper.cauldronRect.Center.ToVector2());
            }
            wrapper.bubbleColor = Color.Lerp(wrapper.bubbleColor, ingredientColor, 0.7f);

            if (timeSinceAdded < 15)
            {
                wrapper.bubbleAnimationTimer += 2;
                //lerp towards a white flash when freshly added
                wrapper.bubbleColor = Color.Lerp(Color.White, wrapper.bubbleColor, timeSinceAdded / 15f);
            }
        }

    }
}

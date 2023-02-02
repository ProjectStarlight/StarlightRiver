using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Alchemy
{
	public abstract class AlchemyIngredient
	{
		/// <summary>
		/// When instantiated by the cauldron dummy we may need to store exactly the Item being used as an ingredient so we don't lose properties on the ingredient like modifiers etc
		/// </summary>
		public Item storedItem;

		/// <summary>
		/// Item type ID of the ingredient, use ItemType<ModItem>() to get Mod Item IDs and Terraria.ID.ItemID for vanilla Items
		/// </summary>
		public readonly int ItemType;

		/// <summary>
		/// if not overriding default visuals this will be used to lerp the overall color towards this value. defaults to a sort of light blue
		/// </summary>
		public Color ingredientColor = new(3, 127, 252);

		protected int timeSinceAdded;

		public AlchemyIngredient() { }

		public abstract int GetItemID();

		/// <summary>
		/// for instantiating the actual Item into the ingredient
		/// </summary>
		/// <param name="ingredientItem"></param>
		public void PutIngredient(Item ingredientItem)
		{
			this.storedItem = ingredientItem;
			timeSinceAdded = 0;
		}

		/// <summary>
		/// called at the end of every frame that this is in the cauldron to increment the timer
		/// 0 on first frame inserted
		/// </summary>
		public void IncrementTimer()
		{
			timeSinceAdded++;
		}

		/// <summary>
		/// performs logic and visuals while this is the most recent Item added to the cauldron. return true if visual updates should be skipped for other ingredents currently in cauldron
		/// </summary>
		public virtual bool MostRecentUpdate(AlchemyWrapper wrapper)
		{
			return false;
		}

		/// <summary>
		/// performs logic and visuals that occur AFTER ALL other ingredient visuals / logic are run while this is the most recent Item added to the cauldron
		/// executes even if mostRecentUpdate returns true
		/// by default just adds cauldron lighting for the resulting bubble color
		/// </summary>
		/// <param name="wrapper"></param>
		public virtual void MostRecentPostUpdate(AlchemyWrapper wrapper)
		{
			Lighting.AddLight(wrapper.cauldronRect.TopLeft() + new Vector2(wrapper.cauldronRect.Width / 2, 0), wrapper.bubbleColor.ToVector3());
		}

		/// <summary>
		/// perform logic updates while this is added to the cauldron, executed by both client and server. runs even if mostRecentUpdate returned true.
		/// runs after the most recent ingredient executes mostRecentUpdate and executes in order of oldest ingredient to newest. also executes for most recent ingredient.
		/// designed for the idea of "dangerous" ingredients that may damage nearby Players or have other tangible effects on the world and Players outside of just visuals
		/// </summary>
		public virtual void Update(AlchemyWrapper wrapper)
		{

		}

		/// <summary>
		/// Perform client-only updates while this is added to the cauldron runs after Update, skipped if mostRecentUpdate returned true.
		/// Executes in order of the oldest ingredient to newest immediately after running Update for this ingredient
		/// </summary>
		public virtual void VisualUpdate(AlchemyWrapper wrapper)
		{
			wrapper.bubbleAnimationTimer += 0.1f; //slightly increase bubbling speed

			if (timeSinceAdded == 0)
			{
				for (int k = 0; k < 10; k++)
					Dust.NewDust(wrapper.cauldronRect.TopLeft() + new Vector2(wrapper.cauldronRect.Width / 4, 0), wrapper.cauldronRect.Width / 2, 0, DustID.Water, 0, -6, 0, default, 1f);

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash, wrapper.cauldronRect.Center.ToVector2());
			}

			wrapper.bubbleColor = Color.Lerp(wrapper.bubbleColor, ingredientColor, 0.7f);

			if (timeSinceAdded < 15)
			{
				wrapper.bubbleAnimationTimer += 2;
				//lerp towards a white flash when freshly added
				wrapper.bubbleColor = Color.Lerp(Color.White, wrapper.bubbleColor, timeSinceAdded / 15f);
			}
		}

		/// <summary>
		/// spawns this Item into the world randomized in the cauldron's rectangle subtracting the consumedCount from the stack or doing nothing if 0 after subtracting
		/// </summary>
		/// <param name="cauldronRect"></param>
		/// <param name="consumedCount"></param>
		public virtual void Dump(Rectangle cauldronRect)
		{
			if (storedItem.stack > 0)
			{
				//TODO: rework this to handle exact clone dropping like keeping modifiers, maybe look into calling newItem to get an open Main.item index and then replace it with the clone and net send it
				Item.NewItem(new EntitySource_WorldEvent(), cauldronRect, storedItem.type, storedItem.stack);
			}

			storedItem = null;
		}

		/// <summary>
		/// PRECONDITION: Item type matches this ingredient's Item type
		/// Attempts to add the Item stack to the ingredient stack
		/// return false if this is an ingredient that should not stack and instead create another AlchemyIngredient instance
		/// by default adds the stack counts together, resets timeSinceAdded to 0 and returns true
		/// </summary>
		/// <param name="Item"></param>
		/// <returns></returns>
		public virtual bool AddToStack(Item Item)
		{
			timeSinceAdded = 0;
			storedItem.stack += Item.stack;
			return true;
		}
	}
}

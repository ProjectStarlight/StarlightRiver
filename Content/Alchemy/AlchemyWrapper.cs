namespace StarlightRiver.Content.Alchemy
{
	public class AlchemyWrapper
	{
		//used to pass around to all the alchemy components to maintain context about bubble color, position, modifiers etc
		//add variables to this for more data
		//cauldron should be resetting any relevant fields every frame otherwise keeps context between frames

		public Rectangle cauldronRect;

		public Color bubbleColor;
		public Color bloomColor;

		/// <summary>
		/// opacity of the bubbling fluid in the cauldron 1f is fully opaque, 0f is fully transparent
		/// </summary>
		public float bubbleOpacity = 1f;

		public float bubbleAnimationTimer; //float for % multipliers potentially

		public int bubbleAnimationFrame;

		public int timeSinceCraftStarted;
		public int timeSinceCraftReady;

		public int currentBatchSize = 0; //when this has a value greater than 0, the recipe is ready
	}
}

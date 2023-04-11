using StarlightRiver.Content.Alchemy;

namespace StarlightRiver.Core.Loaders
{
	public class AlchemySystemLoader : IOrderedLoadable
	{

		public float Priority => 2.0f;

		public void Load()
		{
			AlchemyRecipeSystem.Load();
		}

		public void Unload()
		{
			AlchemyRecipeSystem.Unload();
		}
	}
}
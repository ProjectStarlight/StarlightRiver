using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Core.Loaders
{
	class CursedAccessoryLoader : IOrderedLoadable
	{
		public float Priority => 1f;
		public void Load()
		{
			CursedAccessory.LoadSystem();
		}

		public void Unload()
		{
			CursedAccessory.UnloadSystem();
		}
	}
}

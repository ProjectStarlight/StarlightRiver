using System.Collections.Generic;
using System;
using System.Linq;

namespace StarlightRiver.Core
{
	public class RecipeGroupLoader : ModSystem
	{
		private List<IRecipeGroup> recipeGroupCache;

		public override void Load()
		{
			recipeGroupCache = new List<IRecipeGroup>();

			foreach (Type type in StarlightRiver.Instance.Code.GetTypes())
			{
				if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IRecipeGroup)))
				{
					object instance = Activator.CreateInstance(type);
					recipeGroupCache.Add(instance as IRecipeGroup);
				}

				recipeGroupCache.Sort((n, t) => n.Priority > t.Priority ? 1 : -1);
			}
		}

		public override void AddRecipeGroups()
		{
			foreach (IRecipeGroup group in recipeGroupCache)
			{
				group.AddRecipeGroups();
			}
		}
	}

	interface IRecipeGroup
	{
		void AddRecipeGroups();
		float Priority { get; }
	}
}
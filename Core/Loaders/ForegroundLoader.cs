using StarlightRiver.Content.Foregrounds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Core.Loaders
{
	class ForegroundLoader : IOrderedLoadable
	{
		public static List<Foreground> Foregrounds;

		public float Priority => 1.0f;

		public static Foreground GetForeground<T>()
		{
			return Foregrounds.First(n => n is T);
		}

		public void Load()
		{
			if (Main.dedServ)
				return;

			Foregrounds = new List<Foreground>();

			Mod Mod = StarlightRiver.Instance;

			foreach (Type t in Mod.Code.GetTypes())
			{
				if (t.IsSubclassOf(typeof(Foreground)) && !t.IsAbstract)
					Foregrounds.Add((Foreground)Activator.CreateInstance(t));
			}
		}

		public void Unload()
		{
			Foregrounds.ForEach(t => t.Unload());
			Foregrounds = null;
		}
	}
}

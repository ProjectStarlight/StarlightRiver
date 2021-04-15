using StarlightRiver.Content.Foregrounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Loaders
{
	class ForegroundLoader : ILoadable
	{
        public static List<Foreground> Foregrounds = new List<Foreground>();

        public float Priority => 1.0f;

        public static Foreground GetForeground<T>() => Foregrounds.First(n => n is T);

        public void Load()
        {
            Mod mod = StarlightRiver.Instance;

            foreach (Type t in mod.Code.GetTypes())
            {
                if (t.IsSubclassOf(typeof(Foreground)) && !t.IsAbstract)
                {
                    Foregrounds.Add((Foreground)Activator.CreateInstance(t));
                }
            }
        }

        public void Unload()
        {
            Foregrounds.Clear();
        }
    }
}

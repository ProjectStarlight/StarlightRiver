using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace StarlightRiver.Core.Loaders
{
	class ShaderLoader : IOrderedLoadable
	{
		private static readonly Dictionary<string, Lazy<Asset<Effect>>> shaders = new();

		public float Priority => 0.9f;

		public void Load()
		{
			if (Main.dedServ)
				return;

			MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
			var file = (TmodFile)info.Invoke(StarlightRiver.Instance, null);

			IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.EndsWith(".xnb"));

			foreach (FileEntry entry in shaders)
			{
				string name = entry.Name.Replace(".xnb", "").Replace("Effects/", "");
				string path = entry.Name.Replace(".xnb", "");
				LoadShader(name, path);
			}
		}

		public void Unload()
		{

		}

		public static Asset<Effect> GetShader(string key)
		{
			if (shaders.ContainsKey(key))
			{
				return shaders[key].Value;
			}
			else
			{
				LoadShader(key, $"Effects/{key}");
				return shaders[key].Value;
			}
		}

		public static void LoadShader(string name, string path)
		{
			if (!shaders.ContainsKey(name))
				shaders.Add(name, new(() => StarlightRiver.Instance.Assets.Request<Effect>(path)));
		}
	}
}
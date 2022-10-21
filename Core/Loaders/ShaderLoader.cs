using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace StarlightRiver.Core.Loaders
{
	class ShaderLoader : IOrderedLoadable
	{
		public float Priority => 0.9f;

		public void Load()
		{
			if (Main.dedServ)
				return;

			MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
			var file = (TmodFile)info.Invoke(StarlightRiver.Instance, null);

			System.Collections.Generic.IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.EndsWith(".xnb"));

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

		public static void LoadShader(string name, string path)
		{
			var screenRef = new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>(path, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value);
			Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, name + "Pass"), EffectPriority.High);
			Filters.Scene[name].Load();
		}
	}
}

using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

using StarlightRiver.Core;

namespace StarlightRiver.Core.Loaders
{
    class ShaderLoader : ILoadable
    {
        public float Priority { get => 0.9f; }

        public void Load() //TODO: Debug and fix this after we can build
        {
            MethodInfo info = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
            var file = (TmodFile)info.Invoke(StarlightRiver.Instance, null);

            var shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.EndsWith(".xnb"));

            foreach (FileEntry entry in shaders)
            {
                var name = entry.Name.Replace(".xnb", "").Replace("Effects/", "");
                var path = entry.Name.Replace(".xnb", "");
                LoadShader(name, path);
            }
        }

        public void Unload()
        {
            
        }

        public static void LoadShader(string name, string path)
        {
            var screenRef = new Ref<Effect>(StarlightRiver.Instance.GetEffect(path));
            Filters.Scene[name] = new Filter(new ScreenShaderData(screenRef, name + "Pass"), EffectPriority.High);
            Filters.Scene[name].Load();
        }
    }
}

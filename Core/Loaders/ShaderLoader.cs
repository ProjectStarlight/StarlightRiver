using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using System.IO;
using static Terraria.ModLoader.Core.TmodFile;

namespace StarlightRiver.Core.Loaders
{
    class ShaderLoader : ILoadable
    {
        public void Load() //TODO: Debug and fix this after we can build
        {
            MethodInfo info = typeof(Mod).GetProperty("File").GetGetMethod(true);
            var file = (TmodFile)info.Invoke(StarlightRiver.Instance, null);

            var shaders = file.Where(n => n.Name.EndsWith(".xnb"));

            foreach(FileEntry entry in shaders)
            {
                LoadShader(entry.Name.Replace(".xnb", ""), "Effects/" + entry.Name.Replace(".xnb", ""));
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

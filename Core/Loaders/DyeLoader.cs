using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using StarlightRiver.Content.Items.Geomancer;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria.Graphics.Shaders;

namespace StarlightRiver.Core.Loaders
{
	class DyeLoader : ILoadable
    {

        public void Load()
        {
            GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye>(), new ArmorShaderData(new Ref<Effect>(Filters.Scene["RainbowArmor"].GetShader().Shader), "BasicPass"));
        }
        public void Unload()
        {

        }
        public float Priority => 1.2f;
    }
}

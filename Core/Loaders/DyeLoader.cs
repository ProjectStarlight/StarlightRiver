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
	class DyeLoader : IOrderedLoadable
    {

        public void Load()
        {
            if (Main.dedServ)
                return;

            GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye>(), new ArmorShaderData(new Ref<Effect>(Filters.Scene["RainbowArmor"].GetShader().Shader), "BasicPass"));
            GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye2>(), new ArmorShaderData(new Ref<Effect>(Filters.Scene["RainbowArmor2"].GetShader().Shader), "BasicPass"));
        }
        public void Unload()
        {

        }
        public float Priority => 1.2f;
    }
}

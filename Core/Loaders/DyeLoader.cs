using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Geomancer;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Loaders
{
	class DyeLoader : IOrderedLoadable
	{

		public void Load()
		{
			GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye>(), new ArmorShaderData(new Ref<Effect>(Filters.Scene["RainbowArmor"].GetShader().Shader), "BasicPass"));
			GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye2>(), new ArmorShaderData(new Ref<Effect>(Filters.Scene["RainbowArmor2"].GetShader().Shader), "BasicPass"));
		}
		public void Unload()
		{

		}
		public float Priority => 1.2f;
	}
}

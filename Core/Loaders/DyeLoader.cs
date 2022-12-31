using StarlightRiver.Content.Items.Geomancer;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Core.Loaders
{
	class DyeLoader : IOrderedLoadable
	{
		public float Priority => 1.2f;

		public void Load()
		{
			GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye>(), new ArmorShaderData(new Ref<Effect>(Filters.Scene["RainbowArmor"].GetShader().Shader), "BasicPass"));
			GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye2>(), new ArmorShaderData(new Ref<Effect>(Filters.Scene["RainbowArmor2"].GetShader().Shader), "BasicPass"));
		}

		public void Unload()
		{

		}
	}
}

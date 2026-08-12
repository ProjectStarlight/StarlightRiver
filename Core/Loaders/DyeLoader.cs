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
			if (Main.dedServ)
				return;

			GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye>(), new ArmorShaderData(ShaderLoader.GetShader("RainbowArmor"), "BasicPass"));
			GameShaders.Armor.BindShader(ModContent.ItemType<RainbowCycleDye2>(), new ArmorShaderData(ShaderLoader.GetShader("RainbowArmor2"), "BasicPass"));
		}

		public void Unload() { }
	}
}
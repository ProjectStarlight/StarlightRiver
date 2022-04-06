using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Backgrounds
{
	public class BGLoader : IOrderedLoadable
	{
        float IOrderedLoadable.Priority => 1;

		void IOrderedLoadable.Load()
		{
            if (Main.dedServ)
                return;

            BackgroundTextureLoader.AddBackgroundTexture(StarlightRiver.Instance, "StarlightRiver/Assets/Backgrounds/Blank");

            BackgroundTextureLoader.AddBackgroundTexture(StarlightRiver.Instance, "StarlightRiver/Assets/Backgrounds/PermafrostBack");

            BackgroundTextureLoader.AddBackgroundTexture(StarlightRiver.Instance, "StarlightRiver/Assets/Backgrounds/corruptjunglebackground");
            BackgroundTextureLoader.AddBackgroundTexture(StarlightRiver.Instance, "StarlightRiver/Assets/Backgrounds/bloodyjunglebackground");
        }

        void IOrderedLoadable.Unload() { }
	}

	public class BlankBG : ModUndergroundBackgroundStyle
    {
        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = BackgroundTextureLoader.GetBackgroundSlot("Assets/Backgrounds/Blank");
        }
    }

    public class JungleCorruptBG : ModUndergroundBackgroundStyle
    {
        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = BackgroundTextureLoader.GetBackgroundSlot("Assets/Backgrounds/corruptjunglebackground");
        }
    }

    public class JungleBloodyBG : ModUndergroundBackgroundStyle
    {
        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = BackgroundTextureLoader.GetBackgroundSlot("Assets/Backgrounds/bloodyjunglebackground");
        }
    }

    public class JungleHolyBG : ModUndergroundBackgroundStyle
    {
        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = BackgroundTextureLoader.GetBackgroundSlot("Assets/Backgrounds/bloodyjunglebackground");
        }
    }
}
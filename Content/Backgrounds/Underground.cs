using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Backgrounds
{
	public class BGLoader : ILoadable
	{
        float ILoadable.Priority => 1;

		void ILoadable.Load()
		{
            StarlightRiver.Instance.AddBackgroundTexture("StarlightRiver/Assets/Backgrounds/Blank");

            StarlightRiver.Instance.AddBackgroundTexture("StarlightRiver/Assets/Backgrounds/PermafrostBack");

            StarlightRiver.Instance.AddBackgroundTexture("StarlightRiver/Assets/Backgrounds/corruptjunglebackground");
            StarlightRiver.Instance.AddBackgroundTexture("StarlightRiver/Assets/Backgrounds/bloodyjunglebackground");
        }

        void ILoadable.Unload() { }
	}

	public class BlankBG : ModUgBgStyle
    {
        public override bool ChooseBgStyle() => StarlightWorld.VitricBiome.Intersects(new Rectangle((int)Main.screenPosition.X / 16, (int)Main.screenPosition.Y / 16, Main.screenWidth / 16, Main.screenHeight / 16));

        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = mod.GetBackgroundSlot("Assets/Backgrounds/Blank");
        }
    }

    public class PermafrostBG : ModUgBgStyle
    {
        public override bool ChooseBgStyle() => Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZonePermafrost;

        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = mod.GetBackgroundSlot("Assets/Backgrounds/PermafrostBack");
        }
    }

    public class JungleCorruptBG : ModUgBgStyle
    {
        public override bool ChooseBgStyle() => Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneJungleCorrupt;

        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = mod.GetBackgroundSlot("Assets/Backgrounds/corruptjunglebackground");
        }
    }

    public class JungleBloodyBG : ModUgBgStyle
    {
        public override bool ChooseBgStyle() => Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneJungleBloody;

        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = mod.GetBackgroundSlot("Assets/Backgrounds/bloodyjunglebackground");
        }
    }

    public class JungleHolyBG : ModUgBgStyle
    {
        public override bool ChooseBgStyle() => Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneJungleHoly;

        public override void FillTextureArray(int[] textureSlots)
        {
            for (int k = 0; k < textureSlots.Length; k++) textureSlots[k] = mod.GetBackgroundSlot("Assets/Backgrounds/bloodyjunglebackground");
        }
    }
}
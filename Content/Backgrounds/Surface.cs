using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Backgrounds
{
    public class NullBgStyle : ModSurfaceBgStyle
    {
        public override bool ChooseBgStyle()
        {
            return !Main.gameMenu && Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneVoidPre;
        }

        // Use this to keep far Backgrounds like the mountains.
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            for (int i = 0; i < fades.Length; i++)
            {
                if (i == Slot)
                {
                    fades[i] += transitionSpeed;
                    if (fades[i] > 1f)
                    {
                        fades[i] = 1f;
                    }
                }
                else
                {
                    fades[i] -= transitionSpeed;
                    if (fades[i] < 0f)
                    {
                        fades[i] = 0f;
                    }
                }
            }
        }

        public override int ChooseFarTexture()
        {
            return mod.GetBackgroundSlot("Invisible");
        }

        public override int ChooseMiddleTexture()
        {
            return mod.GetBackgroundSlot("Invisible");
        }

        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
        {
            return mod.GetBackgroundSlot("Invisible");
        }
    }

    public class JungleCorruptBgStyle : ModSurfaceBgStyle
    {
        public override bool ChooseBgStyle()
        {
            return (!Main.gameMenu && Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneJungleCorrupt);
        }

        // Use this to keep far Backgrounds like the mountains.
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            for (int i = 0; i < fades.Length; i++)
            {
                if (i == Slot)
                {
                    fades[i] += transitionSpeed;
                    if (fades[i] > 1f)
                    {
                        fades[i] = 1f;
                    }
                }
                else
                {
                    fades[i] -= transitionSpeed;
                    if (fades[i] < 0f)
                    {
                        fades[i] = 0f;
                    }
                }
            }
        }

        public override int ChooseFarTexture()
        {
            return mod.GetBackgroundSlot("Assets/Backgrounds/CorruptJungleSurface1");
        }

        public override int ChooseMiddleTexture()
        {
            return mod.GetBackgroundSlot("Assets/Backgrounds/CorruptJungleSurface3");
        }

        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
        {
            return mod.GetBackgroundSlot("Assets/Backgrounds/CorruptJungleSurface2");
        }
    }
}
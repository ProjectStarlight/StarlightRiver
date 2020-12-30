using Terraria;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Codex.Entries
{
    internal class PermafrostEntry : CodexEntry
    {
        public PermafrostEntry()
        {
            Category = Categories.Biomes;
            Title = "The Permafrost";
            Body = Helper.WrapString("It's pretty fucking cold here, and there are also giant squids that will try to rip your head off.",
                500, Main.fontDeathText, 0.8f);
            Hint = "Found beneath the icy depths...";
            Image = GetTexture("StarlightRiver/Assets/Codex/BiomeImageAurora");
            Icon = GetTexture("StarlightRiver/Assets/Codex/BiomeIconAurora");
        }
    }

    internal class VitricEntry : CodexEntry
    {
        public VitricEntry()
        {
            Category = Categories.Biomes;
            Title = "Vitric Desert";
            Body = Helper.WrapString("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Tempor nec feugiat nisl pretium fusce id velit. Quam nulla porttitor massa id neque aliquam. Orci phasellus egestas tellus rutrum tellus. Ut placerat orci nulla pellentesque. Magnis dis parturient montes nascetur. Eu augue ut lectus arcu bibendum at. Donec ultrices tincidunt arcu non sodales. Pulvinar mattis nunc sed blandit libero volutpat sed. Lacus suspendisse faucibus interdum posuere lorem. Augue lacus viverra vitae congue eu consequat ac felis donec. Nisl condimentum id venenatis a condimentum vitae sapien pellentesque. Sit amet volutpat consequat mauris. Egestas tellus rutrum tellus pellentesque eu tincidunt tortor. In dictum non consectetur a erat. Lectus magna fringilla urna porttitor rhoncus dolor purus non enim. Facilisi cras fermentum odio eu feugiat. Elit sed vulputate mi sit. Integer malesuada nunc vel risus commodo viverra maecenas accumsan. Diam vulputate ut pharetra sit.",
                500, Main.fontDeathText, 0.8f);
            Hint = "Found beneath the underground desert...";
            Image = GetTexture("StarlightRiver/Assets/Codex/BiomeImageVitric");
            Icon = GetTexture("StarlightRiver/Assets/Codex/BiomeIconVitric");
        }
    }

    internal class OvergrowEntry : CodexEntry
    {
        public OvergrowEntry()
        {
            Category = Categories.Biomes;
            Title = "The Overgrowth";
            Body = Helper.WrapString("NO TEXT",
                500, Main.fontDeathText, 0.8f);
            Hint = "Found beyond a sealed door in the dungeon...";
            Image = GetTexture("StarlightRiver/Assets/Codex/BiomeImageOvergrow");
            Icon = GetTexture("StarlightRiver/Assets/Codex/BiomeIconOvergrow");
        }
    }
}
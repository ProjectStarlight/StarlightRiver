/*using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items;
using StarlightRiver.Content.NPCs.Permafrost;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class TutorialCultistMaker : ModTile
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/TutorialCultistMaker";

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 3, 1, DustType<Dusts.Stone>(), SoundID.Tink, false, new Color(100, 200, 200));

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if(closer && !NPC.AnyNPCs(NPCType<TutorialCultist>()))
            {
                NPC.NewNPC(i * 16, j * 16 - 42, NPCType<TutorialCultist>());
            }
        }
    }

    class TutorialCultistMakerItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public TutorialCultistMakerItem() : base("Tutorial Cultist Marker", "Debug Item", TileType<TutorialCultistMaker>(), 1) { }
    }
}*/

using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleHoly
{
    internal class TreeJungleHoly : ModTree
    {
        public override int CreateDust()
        {
            return StarlightRiver.Instance.DustType("Corrupt");
        }

        public override int GrowthFXGore()
        {
            return StarlightRiver.Instance.GetGoreSlot("Gores/Ward0");
        }

        public override int DropWood()
        {
            return ItemID.Wood;
        }

        public override Texture2D GetTexture()
        {
            return StarlightRiver.Instance.GetTexture("Assets/Tiles/JungleHoly/TreeJungleHoly");
        }

        public override Texture2D GetTopTextures(int i, int j, ref int frame, ref int frameWidth, ref int frameHeight, ref int xOffsetLeft, ref int yOffset)
        {
            frameWidth = 116;
            frameHeight = 98;
            xOffsetLeft = 48;
            yOffset = 2;
            return StarlightRiver.Instance.GetTexture("Assets/Tiles/JungleHoly/TreeJungleHoly_Tops");
        }

        public override Texture2D GetBranchTextures(int i, int j, int trunkOffset, ref int frame)
        {
            return StarlightRiver.Instance.GetTexture("Assets/Tiles/JungleHoly/TreeJungleHoly_Branches");
        }
    }
}
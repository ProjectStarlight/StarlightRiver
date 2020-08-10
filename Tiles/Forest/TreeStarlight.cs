using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Tiles.Forest
{
    internal class TreeStarlight : ModTree
    {
        public override int CreateDust() => DustID.t_LivingWood;

        public override int DropWood() => ItemID.Wood;

        public override Texture2D GetTexture() => ModContent.GetTexture("StarlightRiver/Tiles/Forest/TreeStarlight");

        public override Texture2D GetBranchTextures(int i, int j, int trunkOffset, ref int frame) => ModContent.GetTexture("StarlightRiver/Tiles/Forest/TreeStarlightBranches");

        public override Texture2D GetTopTextures(int i, int j, ref int frame, ref int frameWidth, ref int frameHeight, ref int xOffsetLeft, ref int yOffset)
        {
            frameHeight = 118;
            return ModContent.GetTexture("StarlightRiver/Tiles/Forest/TreeStarlightTops");
        }
    }
}
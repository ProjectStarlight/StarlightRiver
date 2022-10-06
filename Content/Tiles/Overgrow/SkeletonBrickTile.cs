using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    public class SkeletonBrickTile : ModTile
    {
        public override string Texture => AssetDirectory.OvergrowTile + Name;

        public override void SetStaticDefaults()
        {
			this.QuickSet(50, DustID.Bone, SoundID.Tink, new Color(55, 55, 35), ModContent.ItemType<SkeletonBrickItem>(), false, false, "Skeleton Brick");
			Main.tileBrick[Type] = true;
        }
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
		{
			int frame = 0;
			int xPosition = i % 4;
			int yPosition = j % 4;
			switch (xPosition)
			{
				case 0:
					switch (yPosition)
					{
						case 0: frame = 1; break;
						case 1:	frame = 2; break;
						case 2: frame = 0; break;
						case 3: frame = 2; break;
						default: frame = 0; break;
					}
					break;
				case 1:
					switch (yPosition)
					{
						case 0: frame = 2; break;
						case 1: frame = 0; break;
						case 2: frame = 2; break;
						case 3: frame = 1; break;
						default: frame = 0; break;
					}
					break;
				case 2:
					switch (yPosition)
					{
						case 0: frame = 2; break;
						case 1: frame = 1; break;
						case 2: frame = 0; break;
						case 3: frame = 2; break;
						default: frame = 2; break;
					}
					break;
				case 3:
					switch (yPosition)
					{
						case 0: frame = 2; break;
						case 1: frame = 0; break;
						case 2: frame = 0; break;
						case 3: frame = 2; break;
						default: frame = 1; break;
					}
					break;
			}

			frameXOffset = frame * 288; //width of texture divided by 3, the amount of "frames" for the tile
		}
    }

    public class SkeletonBrickItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.OvergrowTile + Name;
        public SkeletonBrickItem() : base("Skeletal Brick", "", "SkeletonBrickTile") { }
    }
}

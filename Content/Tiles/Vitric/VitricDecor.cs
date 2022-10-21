using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class VitricDecor1x1 : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.RandomStyleRange = 2;

			this.QuickSetFurniture(1, 1, 32, SoundID.Tink, false, new Color(217, 193, 154));
		}
	}

	class VitricDecor1x2 : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.RandomStyleRange = 1;

			this.QuickSetFurniture(1, 2, 32, SoundID.Tink, false, new Color(217, 193, 154));
		}
	}

	class VitricDecor2x1 : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
			TileObjectData.newTile.RandomStyleRange = 7;

			this.QuickSetFurniture(2, 1, 32, SoundID.Tink, false, new Color(217, 193, 154));
		}
	}

	class VitricDecor2x2 : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
			TileObjectData.newTile.RandomStyleRange = 4;

			this.QuickSetFurniture(2, 2, 32, SoundID.Tink, false, new Color(217, 193, 154));
		}
	}
}
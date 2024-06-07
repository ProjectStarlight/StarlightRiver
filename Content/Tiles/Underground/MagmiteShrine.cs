﻿using StarlightRiver.Content.Abilities;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Underground
{
	class MagmiteShrine : ModTile, ICustomHintable
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/" + Name;

		public override void SetStaticDefaults()
		{
			TileID.Sets.DrawsWalls[Type] = true;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);
			QuickBlock.QuickSetFurniture(this, 2, 3, DustType<Dusts.Stamina>(), SoundID.Tink, false, new Color(255, 150, 80), false, false, "The Boi");
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			(r, g, b) = (0.1f, 0.08f, 0.025f);
		}
		public string GetCustomKey()
		{
			return "A shrine - obviously, to Their Greatness the Magmite. The lil goober's eyes seem to follow you, and heart-shaped runes dance across its pedestal.";
		}
	}

	class MagmiteShrineItem : QuickTileItem
	{
		public MagmiteShrineItem() : base("The Boi", "It's him!", "MagmiteShrine", 1, "StarlightRiver/Assets/Tiles/Underground/") { }
	}
}
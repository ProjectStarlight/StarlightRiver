using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class GrayMatterItem : QuickTileItem
	{
		public GrayMatterItem() : base("Gray Matter", "You can see it thinking", "GrayMatter", 0, "StarlightRiver/Assets/Tiles/Crimson/") { }
	}

	internal class GrayMatter : ModTile
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/" + Name;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileMerge[Type][TileID.CrimsonGrass] = true;
			Main.tileMerge[TileID.CrimsonGrass][Type] = true;

			HitSound = Terraria.ID.SoundID.NPCHit1;
			DustType = Terraria.ID.DustID.Blood;

			MinPick = 101;

			RegisterItemDrop(ModContent.ItemType<GrayMatterItem>());

			AddMapEntry(new Color(167, 180, 191));
			GraymatterBiome.grayEmissionTypes.Add(Type);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			//(r, g, b) = (0.3f, 0.3f, 0.3f);
		}

		public override void FloorVisuals(Player player)
		{
			if (StarlightWorld.HasFlag(WorldFlags.ThinkerBossOpen))
				player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 10);
		}
	}
}
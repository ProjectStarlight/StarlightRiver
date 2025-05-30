﻿using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Bosses.VitricBoss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Crimson
{
	internal class BonemineItem : QuickTileItem
	{
		public BonemineItem() : base("Bone mine", "Doyoyouuyuuhuyhh *BRRPT* *Anyuerism*", "Bonemine", 0, "StarlightRiver/Assets/Tiles/Crimson/") { }
	}

	internal class Bonemine : ModTile, ICustomGraymatterDrawOver
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/Crimson/" + Name;

		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;

			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;

			Main.tileMerge[Type][TileID.CrimsonGrass] = true;
			Main.tileMerge[TileID.CrimsonGrass][Type] = true;

			Main.tileMerge[Type][TileID.Crimstone] = true;
			Main.tileMerge[TileID.Crimstone][Type] = true;

			HitSound = Terraria.ID.SoundID.Tink;

			DustType = Terraria.ID.DustID.Blood;
			RegisterItemDrop(ModContent.ItemType<BonemineItem>());
			GraymatterBiome.grayOverTypes.Add(Type);

			AddMapEntry(new Color(165, 180, 191));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			Vector2 origin = new Vector2(i, j) * 16 + Vector2.One * 8;
			Projectile.NewProjectile(null, origin, Vector2.Zero, ModContent.ProjectileType<FireRingHostile>(), 20, 1, Main.myPlayer);
		}

		public void DrawOverlay(SpriteBatch spriteBatch, int x, int y)
		{
			Texture2D tex = Assets.Symbol.Value;
			spriteBatch.Draw(tex, new Vector2(x, y) * 16 + Vector2.One * 8 - Main.screenPosition, null, Color.Red * 0.8f, 0, tex.Size() / 2f, 1, 0, 0);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;

			HitSound = Terraria.ID.SoundID.NPCHit1;

			DustType = Terraria.ID.DustID.Blood;
			RegisterItemDrop(ModContent.ItemType<GrayMatterItem>());

			AddMapEntry(new Color(167, 180, 191));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			(r, g, b) = (0.7f, 0.7f, 0.7f);
		}

		public override void FloorVisuals(Player player)
		{
			player.AddBuff(BuffID.Invisibility, 10);
		}
	}
}

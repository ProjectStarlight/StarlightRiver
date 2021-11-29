using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.Enums;
using StarlightRiver.Core.Loaders;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.MusicBox
{
	public class MusicBox : Core.Loaders.TileLoader
	{
		private static AnchorData anchor = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
		private static FurnitureLoadData boxData = new FurnitureLoadData(2, 2, 0, 0, true, new Color(255, 200, 100), false, false, "Music Box", anchor);

		public override string AssetRoot => "StarlightRiver/Assets/Tiles/MusicBox/";

		public override void Load()
		{
			LoadMusicBox("VitricBoss1", "Music Box (Ceiros P1)", "Sounds/Music/VitricBoss1");
			LoadMusicBox("VitricBoss2", "Music Box (Ceiros P2)", "Sounds/Music/VitricBoss2");
			LoadMusicBox("VitricPassive", "Music Box (Vitric Desert)", "Sounds/Music/GlassPassive");
			LoadMusicBox("VitricTemple", "Music Box (Vitric Temple)", "Sounds/Music/GlassTemple");
			//LoadMusicBox("Miniboss", "Music Box (Miniboss)", "Sounds/Music/Miniboss");
			//LoadMusicBox("Overgrow", "Music Box (Overgrow)", "Sounds/Music/Overgrow");
		}

		private void LoadMusicBox(string name, string displayName, string path)
		{
			LoadFurniture(name, displayName, boxData);
			StarlightRiver.Instance.AddMusicBox(StarlightRiver.Instance.GetSoundSlot(SoundType.Music, path), StarlightRiver.Instance.ItemType(name + "Item"), StarlightRiver.Instance.TileType(name));
		}
	}
}

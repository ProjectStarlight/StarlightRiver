using StarlightRiver.Core.Loaders;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.MusicBox
{
	public class MusicBox : SimpleTileLoader
	{
		private static AnchorData anchor = new(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
		private static FurnitureLoadData boxData = new(2, 2, 0, SoundID.Dig, true, new Color(255, 200, 100), false, false, "Music Box", anchor);

		public override string AssetRoot => "StarlightRiver/Assets/Tiles/MusicBox/";

		public override void Load()
		{
			LoadMusicBox("VitricBoss1", "Music Box (Ceiros P1)", "Sounds/Music/VitricBoss1");
			LoadMusicBox("VitricBoss2", "Music Box (Ceiros P2)", "Sounds/Music/VitricBoss2");
			LoadMusicBox("VitricPassive", "Music Box (Vitric Desert)", "Sounds/Music/GlassPassive");
			LoadMusicBox("VitricTemple", "Music Box (Vitric Temple)", "Sounds/Music/GlassTemple");
			LoadMusicBox("Miniboss", "Music Box (Miniboss)", "Sounds/Music/Miniboss");
			LoadMusicBox("Overgrow", "Music Box (Overgrow)", "Sounds/Music/Overgrow");
		}

		public override void Unload()
		{
			boxData = default;
			anchor = default;
		}

		private void LoadMusicBox(string name, string displayName, string path)
		{
			LoadFurniture(name, displayName, boxData);
			StarlightRiver mod = StarlightRiver.Instance;
			MusicLoader.AddMusicBox(mod, MusicLoader.GetMusicSlot(StarlightRiver.Instance, path), mod.Find<ModItem>(name + "Item").Type, mod.Find<ModTile>(name).Type);
		}
	}
}

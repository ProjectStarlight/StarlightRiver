using StarlightRiver.Core.Loaders.TileLoading;
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
			LoadMusicBox("Auroracle", "Music Box (Auroracle)", "Sounds/Music/SquidBoss");
		}

		public override void Unload()
		{
			boxData = default;
			anchor = default;
		}

		private void LoadMusicBox(string name, string displayName, string path)
		{
			Mod.AddContent(new MusicBoxItem(name + "Item", displayName, "", name, ItemRarityID.LightRed, AssetRoot + name + "Item", true));
			Mod.AddContent(new LoaderFurniture(name, boxData, Mod.Find<ModItem>(name + "Item").Type, AssetRoot + name));

			StarlightRiver mod = StarlightRiver.Instance;
			MusicLoader.AddMusicBox(mod, MusicLoader.GetMusicSlot(StarlightRiver.Instance, path), mod.Find<ModItem>(name + "Item").Type, mod.Find<ModTile>(name).Type);
		}
	}

	public class MusicBoxItem : LoaderTileItem
	{
		public MusicBoxItem(string internalName, string name, string tooltip, string placetype, int rare = 0, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
			: base(internalName, name, tooltip, placetype, rare, texturePath, pathHasName, ItemValue)
		{

		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.accessory = true;
			Item.hasVanityEffects = true;
		}
	}
}
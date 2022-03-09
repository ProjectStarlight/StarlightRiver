using Microsoft.Xna.Framework;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Alchemy
{
	public class CauldronItem : ModItem
	{

		public override string Texture => AssetDirectory.AlchemyTile + Name;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Alchemic Cauldron");
		}

		public override void SetDefaults()
		{
			item.width = 26;
			item.height = 22;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.value = 500;
			item.createTile = ModContent.TileType<CauldronTile>();
		}
	}

	internal class CauldronTile : DummyTile
	{
		public override int DummyType => ProjectileType<CauldronDummy>();

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.AlchemyTile + name;
			return base.Autoload(ref name, ref texture);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int item = Item.NewItem(new Vector2(i, j) * 16, ItemType<CauldronItem>(), 1);

			// Sync the drop for multiplayer
			if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
		}

		public override void SetDefaults()
		{
			(this).QuickSetFurniture(3, 2, DustType<Air>(), SoundID.Tink, false, new Color(50, 50, 50), false, false, "Alchemic Cauldron");
		}

        public override bool NewRightClick(int i, int j)
        {
			//TODO: could this work since dummies only exist at top left corner of tile but the coordinates are where click happened
			Main.NewText("right click at: " + i + ", " + j);
			if (DummyExists(i, j, DummyType))
            {
				CauldronDummyAbstract cauldronDummy = (CauldronDummyAbstract)Dummy(i, j).modProjectile;
				Main.NewText("dummy exists");
				if (cauldronDummy.AttemptAddItem(Main.LocalPlayer.HeldItem))
                {
					Main.NewText("added");
					Main.LocalPlayer.HeldItem.TurnToAir();
					return true;
				}
			}

			return false;
        }
    }

	public class CauldronDummy : CauldronDummyAbstract
	{
		public CauldronDummy() : base(TileType<CauldronTile>(), 48, 32) { }
	}
}

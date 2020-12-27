using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Items;

namespace StarlightRiver.Content.Tiles.AshHell
{
    class MagicAsh : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = Directory.AshHellTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            this.QuickSet(0, DustID.Stone, SoundID.Dig, Color.White, ItemType<MagicAshItem>());
        }
    }

    class MagicAshItem : QuickTileItem
    {
<<<<<<< HEAD
        public override string Texture => Directory.AshHellTile + Name;

        public MagicAshItem() : base("Magic Ash", "", TileType<MagicAsh>(), 0) { }
=======
        public MagicAshItem() : base("Magic Ash", "", TileType<MagicAsh>(), 0, Directory.AshHellTile) { }
>>>>>>> a93da633f917beb5bb3693af9f0324eb3572cfdd
    }
}

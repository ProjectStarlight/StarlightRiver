using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow
{
    internal class BossPit : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.Width = 11;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0);

            TileObjectData.addTile(Type);

            drop = mod.ItemType("Bounce");
            minPick = 9000;
            AddMapEntry(new Color(50, 50, 50));
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            int dummyType = ProjectileType<Projectiles.Dummies.OvergrowBossPitDummy>();
            Tile tile = Main.tile[i, j];
            if (tile.frameX == 0 && tile.frameY == 0)
            {
                if (!Main.projectile.Any(proj => proj.active && proj.type == dummyType && proj.Hitbox.Contains(i * 16, j * 16)))
                {
                    Projectile.NewProjectile(new Vector2(i, j) * 16 + Vector2.One * 8, Vector2.Zero, dummyType, 0, 0);
                }
            }
            Projectile dummy = Main.projectile.FirstOrDefault(proj => proj.active && proj.type == dummyType && proj.Hitbox.Contains(i * 16, j * 16));
            if (dummy == null) return;

            Main.tileSolid[Type] = dummy.ai[1] == 0;
        }
    }
}
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow
{
    internal class HatchOvergrow : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.addTile(Type);

            dustType = DustType<Dusts.Gold2>();
            disableSmartCursor = true;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!Main.projectile.Any(proj => proj.active && proj.type == ProjectileType<Projectiles.Dummies.HatchDummy>() &&
             proj.Hitbox.Intersects(new Rectangle(i * 16, j * 16, 16, 16))) && Main.tile[i, j].frameX == 18)
            {
                Projectile.NewProjectile(new Vector2(i, j) * 16, Vector2.Zero, ProjectileType<Projectiles.Dummies.HatchDummy>(), 0, 0);
            }

            Lighting.AddLight(new Vector2(i, j + 2) * 16, new Vector3(0.6f, 0.6f, 0.5f));
        }
    }

    internal class BigHatchOvergrow : DummyTile
    {
        public override int DummyType => ProjectileType<Projectiles.Dummies.BigHatchDummy>();

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = false;
            minPick = 210;
            AddMapEntry(new Color(255, 255, 220));
        }

        public override void SafeNearbyEffects(int i, int j, bool closer)
        {
            Lighting.AddLight(new Vector2(i - 8, j + 4) * 16, new Vector3(0.6f, 0.6f, 0.5f) * 2);
        }
    }
}
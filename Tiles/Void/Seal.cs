using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Void
{
    internal class Seal : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.Width = 11;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0);

            TileObjectData.addTile(Type);

            drop = mod.ItemType("Bounce");
            minPick = int.MaxValue;
            AddMapEntry(new Color(50, 50, 50));
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].frameX == 0 && Main.tile[i, j].frameY == 0 && !Main.projectile.Any
                (proj => proj.type == ProjectileType<Projectiles.Dummies.SealDummy>() && proj.Center == new Vector2(i + 5.5f, j) * 16 && proj.active))
            {
                Projectile.NewProjectile(new Vector2(i + 5.5f, j) * 16, Vector2.Zero, ProjectileType<Projectiles.Dummies.SealDummy>(), 0, 0);
            }
            if (StarlightWorld.SealOpen)
            {
                Main.tileSolid[Type] = false;
                Main.tileSolidTop[Type] = true;
            }
            else
            {
                Main.tileSolid[Type] = true;
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            if (Main.tile[i, j].frameX == 0 && Main.tile[i, j].frameY == 0)
            {
                Vector2 Seal = new Vector2((i + 12) * 16, (j + 12) * 16);
                if (!StarlightWorld.SealOpen)
                {
                    spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Void/SealClosed"), Seal - Main.screenPosition, drawColor);
                }
                else
                {
                    spriteBatch.Draw(GetTexture("StarlightRiver/Tiles/Void/SealOpen"), Seal - Main.screenPosition, drawColor);
                }
            }
        }
    }
}
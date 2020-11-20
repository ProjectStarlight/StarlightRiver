using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Overgrow
{
    // This class shows off a number of less common ModTile methods. These methods help our trap tile behave like vanilla traps.
    // In particular, hammer behavior is particularly tricky. The logic here is setup for multiple styles as well.
    public class DartTile : ModTile
    {
        public override void SetDefaults()
        {
            TileID.Sets.DrawsWalls[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileMerge[Type][mod.GetTile("GrassOvergrow").Type] = true;
            Main.tileMerge[Type][mod.GetTile("BrickOvergrow").Type] = true;

            dustType = mod.DustType("Gold2");
            AddMapEntry(new Color(81, 77, 71));
        }

        public override bool Dangersense(int i, int j, Player player)
        {
            return true;
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            Tile tile = Main.tile[i, j];
            if (Main.LocalPlayer.direction == 1)
            {
                tile.frameX += 18;
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
            }
        }

        private static readonly int[] frameXCycle = { 2, 3, 1, 0 };

        public override bool Slope(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int nextFrameX = frameXCycle[tile.frameX / 18];
            tile.frameX = (short)(nextFrameX * 18);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(-1, Player.tileTargetX, Player.tileTargetY, 1, TileChangeType.None);
            }
            return false;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].frameY == 0)
            {
                if (!(Main.projectile.Any(proj => proj.modProjectile is Projectiles.DartShooter && (proj.modProjectile as Projectiles.DartShooter).parent == Main.tile[i, j] && proj.active)))
                {
                    int proj = Projectile.NewProjectile(new Vector2(i + 0.5f, j + 0.5f) * 16, Vector2.Zero, ProjectileType<Projectiles.DartShooter>(), 0, 0);
                    (Main.projectile[proj].modProjectile as Projectiles.DartShooter).parent = Main.tile[i, j];
                    (Main.projectile[proj].modProjectile as Projectiles.DartShooter).direction = Main.tile[i, j].frameX;
                }
            }
        }

        /*public override void NearbyEffects(int i, int j, bool closer) //old method
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameY == 0)
            {
                //Main.NewText(lastCount);
                if (LegendWorld.darttime >= 55 && lastCount >= LegendWorld.darttime)
                {
                    //SHOOTY SHOOT
                    lastCount = LegendWorld.darttime;
                    switch (tile.frameX)
                    {
                        case 0:
                            Projectile.NewProjectile(new Vector2(i * 16 - 0, j * 16 + 8), new Vector2(-3, 0) * 6f, ProjectileID.PoisonDartTrap, 20, 2f, Main.myPlayer);
                            break;

                        case 18:
                            Projectile.NewProjectile(new Vector2(i * 16 + 8, j * 16 + 8), new Vector2(3, 0) * 6f, ProjectileID.PoisonDartTrap, 20, 2f, Main.myPlayer);
                            break;

                        case 36:
                            Projectile.NewProjectile(new Vector2(i * 16 + 6, j * 16 - 8), new Vector2(0, -3) * 6f, ProjectileID.PoisonDartTrap, 20, 2f, Main.myPlayer);
                            break;

                        case 54:
                            Projectile.NewProjectile(new Vector2(i * 16 + 6, j * 16 + 8), new Vector2(0, 3) * 6f, ProjectileID.PoisonDartTrap, 20, 2f, Main.myPlayer);
                            break;
                    }
                }

                if (lastCount != 100 && LegendWorld.darttime <= 6)
                {
                    lastCount = 100;
                }
            }
        }*/

        public override void HitWire(int i, int j)
        {
            Tile tile = Main.tile[i, j];

            if (tile.frameY == 0)
            {
                tile.frameY = 18;
            }
            else
            {
                tile.frameY = 0;
            }
        }
    }
}
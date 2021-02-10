using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;


using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
    class LightBeam : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.timeLeft = 6000;
            projectile.extraUpdates = 10;
        }

        public override void AI()
        {
            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Electric>(), Vector2.Zero, 0, default, 0.5f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            int x = (int)(projectile.Center.X + oldVelocity.X * 16) / 16;
            int y = (int)(projectile.Center.Y + oldVelocity.Y * 16) / 16;

            if (WorldGen.InWorld(x, y))
            {
                Tile tile = Main.tile[x, y];

                if (tile.type == TileType<Mirror>())
                {
                    Vector2 velocity = Vector2.Zero;

                    if (oldVelocity == new Vector2(0, 1))
                        switch (tile.frameX)
                        {
                            case 0: velocity = new Vector2(-1, 0); break;
                            case 1: velocity = new Vector2(1, 0); break;
                            default: return true;
                        }

                    if (oldVelocity == new Vector2(0, -1))
                        switch (tile.frameX)
                        {
                            case 2: velocity = new Vector2(1, 0); break;
                            case 3: velocity = new Vector2(-1, 0); break;
                            default: return true;
                        }

                    if (oldVelocity == new Vector2(1, 0))
                        switch (tile.frameX)
                        {
                            case 0: velocity = new Vector2(0, -1); break;
                            case 3: velocity = new Vector2(0, 1); break;
                            default: return true;
                        }

                    if (oldVelocity == new Vector2(-1, 0))
                        switch (tile.frameX)
                        {
                            case 1: velocity = new Vector2(0, -1); break;
                            case 2: velocity = new Vector2(0, 1); break;
                            default: return true;
                        }

                    projectile.velocity = velocity;
                    projectile.position = new Vector2(x, y) * 16;

                    for (int k = 0; k < 8; k++)
                        Dust.NewDustPerfect(Vector2.Lerp(projectile.position, projectile.oldPosition, k / 8f) + projectile.Size / 2, DustType<Dusts.Electric>(), Vector2.Zero);

                    Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);

                    return false;
                }

                //TODO: Make this not yandev

                if (tile.type == TileType<Splitter>())
                {
                    Vector2 vel = oldVelocity.RotatedBy(MathHelper.Pi / 2f).ToPoint16().ToVector2();
                    Projectile.NewProjectile(new Vector2(x + 0.5f, y + 0.5f) * 16 + vel * 8, vel, projectile.type, 0, 0);

                    Vector2 vel2 = oldVelocity.RotatedBy(-MathHelper.Pi / 2f).ToPoint16().ToVector2();
                    Projectile.NewProjectile(new Vector2(x + 0.5f, y + 0.5f) * 16 + vel2 * 8, vel2, projectile.type, 0, 0);

                    for (int k = 0; k < 8; k++)
                        Dust.NewDustPerfect(projectile.Center + oldVelocity * k, DustType<Dusts.Electric>(), Vector2.Zero);

                    Main.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, projectile.Center);
                }

                if (tile.type == TileType<RecieverPuzzle>())
                    Main.NewText("Nice cock.", new Color(255, 0, 255));

                if (tile.type == TileType<RecieverPlacable>())
                {
                    for (int k = 0; k < 50; k++)
                        Dust.NewDustPerfect(projectile.Center + oldVelocity * 16, DustType<Dusts.Starlight>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20));

                    Main.PlaySound(SoundID.DD2_BetsyFireballImpact, projectile.Center);

                    Wiring.TripWire(x, y, 1, 1);
                }

                if (tile.type == TileType<DoorVertical>() || tile.type == TileType<DoorHorizontal>())
                    WorldGen.KillTile(x, y);
            }
            return true;
        }
    }
}

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles
{
    internal class Clentam : ModProjectile
    {
        //public Vector2 start;
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 400;
            projectile.tileCollide = false;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gamer Gas");
        }

        public override void AI()
        {
            for (int y = -3; y <= 3; y++)
            {
                for (int x = -3; x <= 3; x++)
                {
                    Tile target = Main.tile[x + (int)projectile.Center.X / 16, y + (int)projectile.Center.Y / 16];

                    if (target.type == TileID.JungleGrass) { target.type = (ushort)mod.TileType("GrassJungleCorrupt"); }
                    if (target.wall == WallID.JungleUnsafe) { target.wall = (ushort)WallType<Tiles.JungleCorrupt.WallJungleCorrupt>(); }
                }
            }
        }
    }

    internal class Clentam2 : ModProjectile
    {
        //public Vector2 start;
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 400;
            projectile.tileCollide = false;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gamer Gas");
        }

        public override void AI()
        {
            for (int y = -3; y <= 3; y++)
            {
                for (int x = -3; x <= 3; x++)
                {
                    Tile target = Main.tile[x + (int)projectile.Center.X / 16, y + (int)projectile.Center.Y / 16];

                    if (target.type == TileID.JungleGrass) { target.type = (ushort)mod.TileType("GrassJungleBloody"); }
                    if (target.wall == WallID.JungleUnsafe) { target.wall = (ushort)WallType<Tiles.JungleBloody.WallJungleBloody>(); }
                }
            }
        }
    }

    internal class Clentam3 : ModProjectile
    {
        //public Vector2 start;
        public override void SetDefaults()
        {
            projectile.width = 32;
            projectile.height = 32;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 400;
            projectile.tileCollide = false;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gamer Gas");
        }

        public override void AI()
        {
            for (int y = -3; y <= 3; y++)
            {
                for (int x = -3; x <= 3; x++)
                {
                    Tile target = Main.tile[x + (int)projectile.Center.X / 16, y + (int)projectile.Center.Y / 16];

                    if (target.type == TileID.JungleGrass) { target.type = (ushort)mod.TileType("GrassJungleHoly"); }
                }
            }
        }
    }
}
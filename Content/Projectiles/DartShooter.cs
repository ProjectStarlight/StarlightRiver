using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Projectiles
{
    public class DartShooter : ModProjectile
    {
        public Tile parent;
        public int direction;

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.height = 8;
            projectile.width = 8;
            projectile.penetrate = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Zapper");
        }

        public override void AI()
        {
            //Main.NewText(direction);
            if (projectile.ai[1] >= 120)
            {
                switch (direction)
                {
                    case 0:
                        Projectile.NewProjectile(new Vector2(projectile.position.X, projectile.position.Y + 4), new Vector2(-3, 0) * 6f, ProjectileID.PoisonDartTrap, 20, 2f, Main.myPlayer);
                        break;

                    case 18:
                        Projectile.NewProjectile(new Vector2(projectile.position.X + 4, projectile.position.Y + 4), new Vector2(3, 0) * 6f, ProjectileID.PoisonDartTrap, 20, 2f, Main.myPlayer);
                        break;

                    case 36:
                        Projectile.NewProjectile(new Vector2(projectile.position.X + 2, projectile.position.Y + 2), new Vector2(0, -3) * 6f, ProjectileID.PoisonDartTrap, 20, 2f, Main.myPlayer);
                        break;

                    case 54:
                        Projectile.NewProjectile(new Vector2(projectile.position.X + 2, projectile.position.Y + 6), new Vector2(0, 3) * 6f, ProjectileID.PoisonDartTrap, 20, 2f, Main.myPlayer);
                        break;
                }
                projectile.ai[1] = 0;
            }
            projectile.ai[1]++;

            projectile.timeLeft = 2;
            if (!parent.active() || direction != parent.frameX || parent.frameY != 0)
            {
                projectile.timeLeft = 0;
            }
        }

        /*public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
        }*/
    }
}
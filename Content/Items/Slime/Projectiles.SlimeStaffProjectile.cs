using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Slime
{
    internal class SlimeStaffProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.SlimeItem + Name;

        public float maxProjSpeed;
        public int maxTimeleft;

        public SlimeStaff parentItem;
        public int globSize = 1;

        private const int maxSize = 3;
        public override void SetDefaults()
        {
            maxProjSpeed = 7.5f;
            maxTimeleft = 1800;

            projectile.width = 20;
            projectile.height = 20;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 3;
            projectile.aiStyle = -1;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.timeLeft = maxTimeleft;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime");
        }

        private static int projectileSize(int globSize)
        {
            switch (globSize)
            {
                case 1:
                    return 20;
                case 2:
                    return 28;
                default:
                    return 34;
            }
        }

        public override void AI()
        {
            //parentItem.projIndexArray
            Vector2 DirectionToCursor = new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center;
            projectile.velocity += Vector2.Normalize(DirectionToCursor) * 0.5f;//last float is how fast it turns


            projectile.velocity = projectile.velocity.Length() > maxProjSpeed ? //dont question it
                Vector2.Normalize(projectile.velocity) * maxProjSpeed : //Case 1A
                projectile.velocity.Length() < maxProjSpeed ?           //Case 1B
                    projectile.velocity * 1.01f :                       //Case 2A
                    projectile.velocity;                                //Case 2B

            int size = projectileSize(globSize);//variable so this only has to be called once in AI

            foreach (int index in parentItem.projIndexArray)//detecting collisions
            {
                Projectile curProj = Main.projectile[index];
                if (index != projectile.whoAmI && curProj.active && curProj.type == ProjectileType<SlimeStaffProjectile>())//if active, this type, and not this projectile
                    if (Vector2.Distance(projectile.position, curProj.position) < size && projectile.timeLeft < maxTimeleft - 60)//if 60 seconds have passed, and the distance is less than size
                    {
                        SlimeStaffProjectile curSlimeProj = curProj.modProjectile as SlimeStaffProjectile;
                        int nextSize = curSlimeProj.globSize + globSize;//this projectiles size added to the selected projectile's size
                        if (nextSize <= 3 && curSlimeProj.globSize > 0)//checks added sizes are lowerthan or equal to three
                        {
                            curSlimeProj.globSize = nextSize;
                            curProj.penetrate = 2;//resets bounces
                            curProj.Center = curProj.position;//changes position (like an explosive)
                            projectile.Kill();//kills this projectile
                            break;//stops iterating to prevent extra combinings with projectiles after this one should be already dead
                        }
                    }
            }

            projectile.Size = Vector2.One * size;//called after the above stuff, position in where this is called may not actually change anything

            if (Main.myPlayer == projectile.owner)
            {
                int currentTargetX = (int)(Main.MouseWorld.X * 10);//multiply or divide these to change precision, this seems to be enogh for multiplayer
                int oldTargetX = (int)(projectile.ai[0] * 10);//dividing by ten is the lowest you can go and avoid desyncs
                int currentTargetY = (int)(Main.MouseWorld.Y * 10);
                int oldTargetY = (int)(projectile.ai[1] * 10);

                projectile.ai[0] = Main.MouseWorld.X;
                projectile.ai[1] = Main.MouseWorld.Y;

                // This code checks if the precious velocity of the projectile is different enough from its new velocity, and if it is, syncs it with the server and the other clients in MP.
                // We previously multiplied the speed by 1000, then casted it to int, this is to reduce its precision and prevent the speed from being synced too much.
                if (currentTargetX != oldTargetX || currentTargetY != oldTargetY)
                    projectile.netUpdate = true;
            }

            //Main.NewText(projectile.velocity.Length());

            //vfx
            projectile.rotation += 0.15f;
            Dust.NewDustPerfect(projectile.Center, 264, Vector2.Zero, 0, new Color(globSize * 50, globSize * 50, globSize * 50), 0.4f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            //Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);

            spriteBatch.Draw(Main.projectileTexture[projectile.type],
                projectile.Center - Main.screenPosition,
                new Rectangle(0, Main.projectileTexture[projectile.type].Height / maxSize * (globSize - 1), Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height / maxSize),
                lightColor,
                projectile.rotation,
                new Vector2(Main.projectileTexture[projectile.type].Width / 2, Main.projectileTexture[projectile.type].Height / (maxSize * 2)),
                1f, default, default);

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            projectile.penetrate--;
            if (projectile.penetrate <= 0)
                projectile.Kill();
            else
            {
                if (projectile.velocity.X != oldVelocity.X)
                    projectile.velocity.X = -oldVelocity.X;
                if (projectile.velocity.Y != oldVelocity.Y)
                {
                    projectile.velocity.Y = -oldVelocity.Y;
                }
                projectile.velocity = projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 0.50f;
                Main.PlaySound(SoundID.Item10, projectile.position);
            }
            return false;
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write(globSize);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            globSize = reader.ReadInt32();//might be reader.readSingle() instead
        }

        public override void Kill(int timeLeft)
        {
            Main.NewText(globSize + " index " + projectile.whoAmI);
            for (int k = 0; k <= 30; k++)
                Dust.NewDustPerfect(projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f), 0, Color.BlueViolet, 0.8f);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffType<Buffs.Slimed>(), 600, false);
        }
    }
}
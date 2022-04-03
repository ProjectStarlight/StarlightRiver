using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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

            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ranged = true;
            Projectile.penetrate = 3;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = maxTimeleft;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Slime");
        }

        private static int ProjectileSize(int globSize)
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
            Vector2 DirectionToCursor = new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center;
            Projectile.velocity += Vector2.Normalize(DirectionToCursor) * 0.5f;//last float is how fast it turns


            Projectile.velocity = Projectile.velocity.Length() > maxProjSpeed ? //dont question it
                Vector2.Normalize(Projectile.velocity) * maxProjSpeed : //Case 1A
                Projectile.velocity.Length() < maxProjSpeed ?           //Case 1B
                    Projectile.velocity * 1.01f :                       //Case 2A
                    Projectile.velocity;                                //Case 2B

            int size = ProjectileSize(globSize);//variable so this only has to be called once in AI

            foreach (int index in parentItem.projIndexArray)//detecting collisions
            {
                Projectile curProj = Main.projectile[index];
                if (index != Projectile.whoAmI && curProj.active && curProj.type == ProjectileType<SlimeStaffProjectile>())//if active, this type, and not this Projectile
                    if (Vector2.Distance(Projectile.position, curProj.position) < size && Projectile.timeLeft < maxTimeleft - 60)//if 60 seconds have passed, and the distance is less than size
                    {
                        SlimeStaffProjectile curSlimeProj = curProj.ModProjectile as SlimeStaffProjectile;
                        int nextSize = curSlimeProj.globSize + globSize;//this Projectiles size added to the selected Projectile's size
                        if (nextSize <= 3 && curSlimeProj.globSize > 0)//checks added sizes are lowerthan or equal to three
                        {
                            curSlimeProj.globSize = nextSize;
                            curProj.penetrate = 2;//resets bounces
                            curProj.Center = curProj.position;//changes position (like an explosive)
                            Projectile.Kill();//kills this Projectile
                            break;//stops iterating to prevent extra combinings with Projectiles after this one should be already dead
                        }
                    }
            }

            Projectile.Size = Vector2.One * size;//called after the above stuff, position in where this is called may not actually change anything

            if (Main.myPlayer == Projectile.owner)
            {
                int currentTargetX = (int)(Main.MouseWorld.X * 10);//multiply or divide these to change precision, this seems to be enogh for multiPlayer
                int oldTargetX = (int)(Projectile.ai[0] * 10);//dividing by ten is the lowest you can go and avoid desyncs
                int currentTargetY = (int)(Main.MouseWorld.Y * 10);
                int oldTargetY = (int)(Projectile.ai[1] * 10);

                Projectile.ai[0] = Main.MouseWorld.X;
                Projectile.ai[1] = Main.MouseWorld.Y;

                // This code checks if the precious velocity of the Projectile is different enough from its new velocity, and if it is, syncs it with the server and the other clients in MP.
                // We previously multiplied the speed by 1000, then casted it to int, this is to reduce its precision and prevent the speed from being synced too much.
                if (currentTargetX != oldTargetX || currentTargetY != oldTargetY)
                    Projectile.netUpdate = true;
            }

            //Main.NewText(Projectile.velocity.Length());

            //vfx
            Projectile.rotation += 0.15f;
            Dust.NewDustPerfect(Projectile.Center, 264, Vector2.Zero, 0, new Color(globSize * 50, globSize * 50, globSize * 50), 0.4f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            //Vector2 drawOrigin = new Vector2(Main.projectileTexture[Projectile.type].Width * 0.5f, Projectile.height * 0.5f);

            spriteBatch.Draw(Main.projectileTexture[Projectile.type],
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, Main.projectileTexture[Projectile.type].Height / maxSize * (globSize - 1), Main.projectileTexture[Projectile.type].Width, Main.projectileTexture[Projectile.type].Height / maxSize),
                lightColor,
                Projectile.rotation,
                new Vector2(Main.projectileTexture[Projectile.type].Width / 2, Main.projectileTexture[Projectile.type].Height / (maxSize * 2)),
                1f, default, default);

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.penetrate--;
            if (Projectile.penetrate <= 0)
                Projectile.Kill();
            else
            {
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Projectile.velocity.Y != oldVelocity.Y)
                {
                    Projectile.velocity.Y = -oldVelocity.Y;
                }
                Projectile.velocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 0.50f;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
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
            //Main.NewText(globSize + " index " + Projectile.whoAmI);
            for (int k = 0; k <= 30; k++)
                Dust.NewDustPerfect(Projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f), 0, Color.BlueViolet, 0.8f);
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
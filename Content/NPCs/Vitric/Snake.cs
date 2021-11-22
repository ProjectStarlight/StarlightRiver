using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class Snake : ModNPC
    {
        public ref float ActionState => ref npc.ai[0];
        public ref float ActionTimer => ref npc.ai[1];

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/Snake";

        public Player target => Main.player[npc.target];

		public override bool Autoload(ref string name)
		{
            for(int k = 0; k <= 5; k++)
			    mod.AddGore(AssetDirectory.VitricNpc + "Gore/SnakeGore" + k);

            return base.Autoload(ref name);
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spittin' Snek");
        }

        public override void SetDefaults()
        {
            npc.width = 66;
            npc.height = 64;
            npc.damage = 30;
            npc.defense = 10;
            npc.lifeMax = 80;
            npc.aiStyle = -1;
            npc.knockBackResist = 0;
            npc.npcSlots = 1;

            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
        }

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return ActionState == 3 && base.CanHitPlayer(target, ref cooldownSlot);
		}

		public override void AI()
        {
            ActionTimer++;

            switch (ActionState)
            {
                case 0: // Default/spawningd
                    npc.frame.Width = npc.width;
                    npc.frame.Height = npc.height;

                    npc.TargetClosest();
                    if (Vector2.Distance(npc.Center, target.Center) < 300)
                        ChangeState(1);

                    break;

                case 1: // Emerging
                    npc.frame.X = 2 * npc.width;
                    npc.frame.Y = npc.height * (int)(ActionTimer / 60f * 17);

                    if (ActionTimer > 60)
                        ChangeState(2);

                    break;

                case 2: // Targeting

                    if (ActionTimer == 1)
                    {
                        npc.TargetClosest();

                        if (CastToTarget())
                            ChangeState(3);

                        npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;
                    }

                    else
                    {
                        if (ActionTimer <= 30)
                        {
                            npc.frame.X = 1 * npc.width;
                            npc.frame.Y = npc.height * (int)(ActionTimer / 30f * 12);
                        }

                        if (ActionTimer == 30 && (Main.netMode == Terraria.ID.NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer))
                            npc.Center = FindNewPosition();

                        if (ActionTimer > 60 && ActionTimer <= 105)
                        {
                            npc.frame.X = 2 * npc.width;
                            npc.frame.Y = npc.height * (int)((ActionTimer - 60) / 45f * 17);
                        }

                        if (ActionTimer >= 135)
                            ActionTimer = 0;
                    }

                    break;

                case 3: // Shooting
                    npc.frame.X = 0;

                    if (ActionTimer <= 30)
                        npc.frame.Y = npc.height * (int)(ActionTimer / 30f * 7);
                    else if (ActionTimer > 30 && ActionTimer < 50)
                        npc.frame.Y = npc.height * (7 - (int)(((ActionTimer - 30) / 20f) * 5));

                    if (ActionTimer == 20 && (Main.netMode == Terraria.ID.NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer))
                        Projectile.NewProjectile(npc.Center, Vector2.Normalize(target.Center - npc.Center) * 10, ProjectileType<SnakeSpit>(), 20, 0.2f, Main.myPlayer);

                    if (ActionTimer == 140)
                        ChangeState(2, 2);

                    break;
            }
        }

        private void ChangeState(int target, int time = 0)
        {
            ActionState = target;
            ActionTimer = time;
        }

        private bool CastToTarget()
        {
            var dist = Vector2.Distance(npc.Center, target.Center);
            var checks = dist / 4;

            for (int k = 0; k < checks; k++)
            {
                var toCheck = Vector2.Lerp(npc.Center, target.Center, k / checks);

                if (Helpers.Helper.PointInTile(toCheck))
                    return false;
            }

            return true;
        }

        private Vector2 FindNewPosition()
        {
            Point16 currentPos = (target.Center / 16).ToPoint16();

            for (int k = 0; k < 150; k++) //maximum attempts for finding a new spot
            {
                Point16 randPos = currentPos + new Point16(Main.rand.Next(-60, 60), Main.rand.Next(-60, 60));

                //Checking for a shape that looks like this:
                // ---    ---
                // ---[][]---
                if (
                    Vector2.Distance(randPos.ToVector2() * 16, target.Center) > 100 &&
                    Framing.GetTileSafely(randPos).collisionType == 1 &&
                    Framing.GetTileSafely(randPos + new Point16(1, 0)).collisionType == 1 &&
                    !Framing.GetTileSafely(randPos + new Point16(0, -1)).active() && Framing.GetTileSafely(randPos + new Point16(0, -1)).liquid == 0 &&
                    !Framing.GetTileSafely(randPos + new Point16(1, -1)).active() && Framing.GetTileSafely(randPos + new Point16(1, -1)).liquid == 0
                    )
                {
                    npc.netUpdate = true;
                    return randPos.ToVector2() * 16 + new Vector2(16, -36);
                }
            }

            //Main.NewText("Couldnt find a landing point!");
            return npc.Center + new Vector2(16, -36); //when it cant find a landing point, default to the current position
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneGlass ? 100 : 0;
        }

        public override void NPCLoot()
        {
            for (int k = 0; k <= 5; k++)
                Gore.NewGoreDirect(npc.position, Vector2.Zero, ModGore.GetGoreSlot(AssetDirectory.VitricNpc + "Gore/SnakeGore" + k));
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition + Vector2.UnitY * 2, npc.frame, drawColor, npc.rotation, new Vector2(33, 32), 1, npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(GetTexture(Texture + "Glow"), npc.Center - Main.screenPosition + Vector2.UnitY * 2, npc.frame, Color.White, npc.rotation, new Vector2(33, 32), 1, npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }

    public class SnakeSpit : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.VitricNpc + Name;

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.width = 22;
            projectile.height = 22;
            projectile.penetrate = 1;
            projectile.timeLeft = 180;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.damage = 5;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Molten glass");

        public override void AI()
        {
            Dust d = Dust.NewDustPerfect(projectile.Center + projectile.velocity, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), 0.4f);
            d.noGravity = false;

            projectile.rotation = projectile.velocity.ToRotation() - 1.57f;

            if (projectile.timeLeft < 90)
                projectile.velocity.Y += 0.1f;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k <= 10; k++)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center + projectile.velocity, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 150, 50), 0.5f);
                d.noGravity = false;
            }
        }

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
            var tex = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, tex.Size() / 2, 1, 0, 0);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall");
            float alpha = projectile.timeLeft > 160 ? 1 - (projectile.timeLeft - 160) / 20f : 1;
            Color color = new Color(255, 150, 50) * alpha;

            spriteBatch.Draw(tex, projectile.Center + Vector2.Normalize(projectile.velocity) * -40 - Main.screenPosition, tex.Frame(),
                color * (projectile.timeLeft / 140f), projectile.rotation, tex.Size() / 2, 1.8f, 0, 0);
        }
    }
}

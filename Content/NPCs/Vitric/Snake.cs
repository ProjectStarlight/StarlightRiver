using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using System.IO;
using StarlightRiver.Content.Biomes;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class Snake : ModNPC
    {
        public ref float ActionState => ref NPC.ai[0];
        public ref float ActionTimer => ref NPC.ai[1];

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/Snake";

        public Player target => Main.player[NPC.target];

		public override void Load()
		{
            for(int k = 0; k <= 5; k++)
			    GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.VitricNpc + "Gore/SnakeGore" + k);          
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Spittin' Snek");
        }

        public override void SetDefaults()
        {
            NPC.width = 66;
            NPC.height = 64;
            NPC.damage = 30;
            NPC.defense = 10;
            NPC.lifeMax = 80;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 0;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return ActionState == 3 && base.CanHitPlayer(target, ref cooldownSlot);
		}

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.target);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.target = reader.ReadInt32();
        }

        public override void AI()
        {
            ActionTimer++;

            NPC.frame.Width = NPC.width;
            NPC.frame.Height = NPC.height;

            switch (ActionState)
            {
                case 0: // Default/spawning
                    NPC.TargetClosest();
                    if (Vector2.Distance(NPC.Center, target.Center) < 300 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        ChangeState(1);
                        NPC.netUpdate = true;
                    }
                        

                    break;

                case 1: // Emerging
                    NPC.frame.X = 2 * NPC.width;
                    NPC.frame.Y = NPC.height * (int)(ActionTimer / 60f * 17);

                    if (ActionTimer > 60)
                        ChangeState(2);

                    break;

                case 2: // Targeting

                    if (ActionTimer == 1)
                    {
                        NPC.TargetClosest();

                        if (CastToTarget())
                            ChangeState(3);

                        NPC.spriteDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                    }

                    else
                    {
                        if (ActionTimer <= 30)
                        {
                            NPC.frame.X = 1 * NPC.width;
                            NPC.frame.Y = NPC.height * (int)(ActionTimer / 30f * 12);
                        }

                        if (ActionTimer == 30 && (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer))
                            NPC.Center = FindNewPosition();

                        if (ActionTimer > 60 && ActionTimer <= 105)
                        {
                            NPC.frame.X = 2 * NPC.width;
                            NPC.frame.Y = NPC.height * (int)((ActionTimer - 60) / 45f * 17);
                        }

                        if (ActionTimer >= 135)
                            ActionTimer = 0;
                    }

                    break;

                case 3: // Shooting
                    NPC.frame.X = 0;

                    if (ActionTimer <= 30)
                        NPC.frame.Y = NPC.height * (int)(ActionTimer / 30f * 7);
                    else if (ActionTimer > 30 && ActionTimer < 50)
                        NPC.frame.Y = NPC.height * (7 - (int)(((ActionTimer - 30) / 20f) * 5));

                    if (ActionTimer == 20 && (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer))
                        Projectile.NewProjectile(NPC.GetSpawnSourceForProjectileNPC(), NPC.Center, Vector2.Normalize(target.Center - NPC.Center) * 10, ProjectileType<SnakeSpit>(), 20, 0.2f, Main.myPlayer);

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
            var dist = Vector2.Distance(NPC.Center, target.Center);
            var checks = dist / 4;

            for (int k = 0; k < checks; k++)
            {
                var toCheck = Vector2.Lerp(NPC.Center, target.Center, k / checks);

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
                    Framing.GetTileSafely(randPos).BlockType == BlockType.Solid &&
                    Framing.GetTileSafely(randPos + new Point16(1, 0)).BlockType == BlockType.Solid &&
                    !Framing.GetTileSafely(randPos + new Point16(0, -1)).HasTile && Framing.GetTileSafely(randPos + new Point16(0, -1)) .LiquidAmount == 0 &&
                    !Framing.GetTileSafely(randPos + new Point16(1, -1)).HasTile && Framing.GetTileSafely(randPos + new Point16(1, -1)) .LiquidAmount == 0
                    )
                {
                    NPC.netUpdate = true;
                    return randPos.ToVector2() * 16 + new Vector2(16, -36);
                }
            }

            //Main.NewText("Couldnt find a landing point!");
            return NPC.Center + new Vector2(16, -36); //when it cant find a landing point, default to the current position
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.InModBiome(ModContent.GetInstance<VitricDesertBiome>()) ? 100 : 0;
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int k = 0; k <= 5; k++)
                    Gore.NewGoreDirect(NPC.position, Vector2.Zero, Mod.Find<ModGore>("SnakeGore" + k).Type);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos + Vector2.UnitY * 2, NPC.frame, drawColor, NPC.rotation, new Vector2(33, 32), 1, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(Request<Texture2D>(Texture + "Glow").Value, NPC.Center - screenPos + Vector2.UnitY * 2, NPC.frame, Color.White, NPC.rotation, new Vector2(33, 32), 1, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }

    public class SnakeSpit : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.VitricNpc + Name;

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.damage = 5;
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Molten glass");

        public override void AI()
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 50), 0.4f);
            d.noGravity = false;

            Projectile.rotation = Projectile.velocity.ToRotation() - 1.57f;

            if (Projectile.timeLeft < 90)
                Projectile.velocity.Y += 0.1f;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k <= 10; k++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 150, 50), 0.5f);
                d.noGravity = false;
            }
        }

		public override void PostDraw(Color lightColor)
		{
            var tex = Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, 1, 0, 0);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
        {
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall").Value;
            float alpha = Projectile.timeLeft > 160 ? 1 - (Projectile.timeLeft - 160) / 20f : 1;
            Color color = new Color(255, 150, 50) * alpha;

            spriteBatch.Draw(tex, Projectile.Center + Vector2.Normalize(Projectile.velocity) * -40 - Main.screenPosition, tex.Frame(),
                color * (Projectile.timeLeft / 140f), Projectile.rotation, tex.Size() / 2, 1.8f, 0, 0);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Core;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class BoomBug : ModNPC, IDrawAdditive
    {
        public ref float Timer => ref NPC.ai[0];
        public ref float State => ref NPC.ai[1];
        public ref float SavedRotation => ref NPC.ai[2];

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/BoomBug";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 3;
            DisplayName.SetDefault("Fire Fly");
        }

        public override void SetDefaults()
        {
            NPC.width = 34;
            NPC.height = 40;
            NPC.knockBackResist = 1.5f;
            NPC.lifeMax = 20;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.damage = 10;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath4;
        }

        public override void AI()
        {
            Timer++;

            switch (State)
            {
                case 0: //wander

                    if (Timer == 1) //dont spawn on the ground
                        NPC.position.Y -= 32;

                    NPC.spriteDirection = NPC.direction;

                    if (Timer % 30 == 0 && (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer))
                    {
                        NPC.direction = Main.rand.NextBool() ? 1 : -1;
                        SavedRotation = Main.rand.NextFloat(-0.7f, 0.3f);
                        NPC.netUpdate = true;
                    }

                    float maxSpeed = 4.0f;

                    NPC.velocity = ((Vector2.UnitX * NPC.spriteDirection).RotatedBy(SavedRotation) * 0.15f + NPC.velocity);
                    if (NPC.velocity.Length() > maxSpeed)
                    {
                        NPC.velocity.Normalize();
                        NPC.velocity = NPC.velocity * maxSpeed;
                    }


                    NPC.TargetClosest(false);
                    Player Player = Main.player[NPC.target];
                    if (Vector2.DistanceSquared(NPC.Center, Player.Center) <= Math.Pow(400, 2) && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, Player.position, Player.width, Player.height))
                    {
                        Timer = 0;
                        State = 1;
                        NPC.netUpdate = true;
                    }

                    break;

                case 1: //attack

                    if (Timer < 60)
                        NPC.velocity *= 0.95f;

                    if (Timer == 20)
                    {
                        SavedRotation = (Main.player[NPC.target].Center - NPC.Center).ToRotation();

                        NPC.direction = (Main.player[NPC.target].Center - NPC.Center).X > 0 ? 1 : -1;
                        NPC.spriteDirection = NPC.direction;
                    }

                    if (Timer == 60)
                        NPC.velocity = Vector2.UnitX.RotatedBy(SavedRotation) * 5;

                    if (Timer > 66)
                    {
                        if (NPC.velocity.Length() < 10)
                            NPC.velocity *= 1.05f;

                        if (NPC.velocity == Vector2.Zero)
                        {
                            Explode();
                            return;
                        }

                        for (int x = -2; x <= 2; x++)
                            for (int y = -2; y <= 2; y++)
                            {
                                Tile tile = Framing.GetTileSafely((int)(NPC.Center.X / 16) + x, (int)(NPC.Center.Y / 16) + y);

                                if (Main.tileSolid[tile.TileType])
                                {
                                    Explode();
                                    return;
                                }

                            }
                    }

                    break;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.direction == 1 ? true : false);
            writer.Write(SavedRotation);
            writer.Write(NPC.target);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            NPC.direction = reader.ReadBoolean() ? 1 : -1;
            SavedRotation = reader.ReadSingle();
            NPC.target = reader.ReadInt32();
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame = new Rectangle(0, frameHeight * (int)(Timer / 3 % 3), NPC.width, frameHeight);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            Explode();
        }



        private void Explode()
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode);
            NPC.active = false;

            for (int k = 0; k < 20; k++)
            {
                var d = Dust.NewDustPerfect(NPC.Center, DustType<Bosses.VitricBoss.LavaSpew>(), null, 0, default, 1.3f);
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            for (int k = 0; k < Main.maxPlayers; k++)
            {
                var Player = Main.player[k];

                if (!Player.active)
                    return;

                var mp = Player.GetModPlayer<StarlightPlayer>();

                if (mp.Shake < 60)
                    mp.Shake += (int)Math.Max(0, 20 - Vector2.Distance(NPC.Center, Player.Center) / 8f);
            }
        }

        public void DrawAdditive(SpriteBatch sb)
        {
            var glowTex = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
            float sin = (float)Math.Sin(Timer / 5f);
            float sin2 = (float)Math.Sin(Timer / 7f);

            sb.Draw(glowTex, NPC.Center + new Vector2(-12 * NPC.spriteDirection, 16) - Main.screenPosition, null, new Color(255, 200, 100), 0, glowTex.Size() / 2, 0.5f + 0.1f * sin, 0, 0);
            sb.Draw(glowTex, NPC.Center + new Vector2(-12 * NPC.spriteDirection, 16) - Main.screenPosition, null, new Color(255, 100, 20), 0, glowTex.Size() / 2, 0.7f + 0.1f * sin2, 0, 0);

            if (State == 1 && Timer > 20 && Timer <= 60)
            {
                var tex = Request<Texture2D>(AssetDirectory.MiscTextures + "DirectionalBeam").Value;
                Vector2 origin = new Vector2(0, tex.Height / 2);

                for (int k = 0; k < 10; k++)
                {
                    var pos = NPC.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(SavedRotation) * k * 32;
                    var color = new Color(255, (int)(185 * (float)Math.Sin(k / 10f * 3.14f)), 50);
                    var colorMult = (float)Math.Sin(k / 10f * 3.14f) * (float)(Math.Sin((Timer - 20) / 40f * 3.14f));
                    var source = new Rectangle((int)(((Timer - 20) / 10f) * -tex.Width), 0, tex.Width, tex.Height);

                    sb.Draw(tex, pos, source, color * colorMult, SavedRotation, origin, 1, 0, 0);
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.InModBiome(ModContent.GetInstance<VitricDesertBiome>()) ? 100 : 0;
        }
    }
}
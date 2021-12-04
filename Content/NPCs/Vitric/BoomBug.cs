using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public ref float Timer => ref npc.ai[0];
        public ref float State => ref npc.ai[1];
        public ref float SavedRotation => ref npc.ai[2];

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/BoomBug";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 3;
            DisplayName.SetDefault("Fire Fly");
        }

        public override void SetDefaults()
        {
            npc.width = 34;
            npc.height = 40;
            npc.knockBackResist = 1.5f;
            npc.lifeMax = 20;
            npc.noGravity = true;
            npc.noTileCollide = false;
            npc.damage = 10;
            npc.aiStyle = -1;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath4;
            npc.npcSlots = 1;
        }

        public override void AI()
        {
            Timer++;

            switch(State)
			{
                case 0: //wander

                    if (Timer == 1) //dont spawn on the ground
                        npc.position.Y -= 32;

                    npc.spriteDirection = npc.direction;

                    if (Timer % 30 == 0 && (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer))
                    {
                        npc.direction = Main.rand.NextBool() ? 1 : -1;
                        SavedRotation = Main.rand.NextFloat(-0.7f, 0.3f);
                        npc.netUpdate = true;
                    }

                    float maxSpeed = 4.0f;

                    npc.velocity = ((Vector2.UnitX * npc.spriteDirection).RotatedBy(SavedRotation) * 0.15f + npc.velocity);
                    if (npc.velocity.Length() > maxSpeed)
                    {
                        npc.velocity.Normalize();
                        npc.velocity = npc.velocity * maxSpeed;
                    }
                        

                    npc.TargetClosest(false);
                    Player player = Main.player[npc.target];
                    if (Vector2.DistanceSquared(npc.Center, player.Center) <= Math.Pow(400, 2) && Collision.CanHitLine(npc.position, npc.width, npc.height, player.position, player.width, player.height))
					{
                        Timer = 0;
                        State = 1;
                        npc.netUpdate = true;
                    }

                break;

                case 1: //attack

                    if (Timer < 60)
                        npc.velocity *= 0.95f;

                    if (Timer == 20)
                    {
                        SavedRotation = (Main.player[npc.target].Center - npc.Center).ToRotation();

                        npc.direction = (Main.player[npc.target].Center - npc.Center).X > 0 ? 1 : -1;
                        npc.spriteDirection = npc.direction;
                    }

                    if (Timer == 60)
                        npc.velocity = Vector2.UnitX.RotatedBy(SavedRotation) * 5;

                    if (Timer > 66)
                    {
                        if (npc.velocity.Length() < 10)
                            npc.velocity *= 1.05f;

                        if (npc.velocity == Vector2.Zero)
                        {
                            Explode();
                            return;
                        }

                        for(int x = -2; x <= 2; x++)
                            for (int y = -2; y <= 2; y++)
							{
                                Tile tile = Framing.GetTileSafely((int)(npc.Center.X / 16) + x, (int)(npc.Center.Y / 16) + y);

                                if(tile.collisionType != 0 && Main.tileSolid[tile.type])
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
            writer.Write(npc.direction == 1 ? true : false);
            writer.Write(SavedRotation);
            writer.Write(npc.target);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            npc.direction = reader.ReadBoolean() ? 1 : -1;
            SavedRotation = reader.ReadSingle();
            npc.target = reader.ReadInt32();
        }

        public override void FindFrame(int frameHeight)
		{
            npc.frame = new Rectangle(0, frameHeight * (int)(Timer / 3 % 3), npc.width, frameHeight);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
            Explode();
		}

        

        private void Explode()
		{
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode);
            npc.active = false;

            for (int k = 0; k < 20; k++)
            {
                var d = Dust.NewDustPerfect(npc.Center, DustType<Bosses.VitricBoss.LavaSpew>(), null, 0, default, 1.3f);
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            for(int k = 0; k < Main.maxPlayers; k++)
			{
                var player = Main.player[k];

                if (!player.active)
                    return;

                var mp = player.GetModPlayer<StarlightPlayer>();

                if(mp.Shake < 60)
                    mp.Shake += (int)Math.Max(0, 20 - Vector2.Distance(npc.Center, player.Center) / 8f);
			}
        }

        public void DrawAdditive(SpriteBatch sb)
		{
            var glowTex = GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
            float sin = (float)Math.Sin(Timer / 5f);
            float sin2 = (float)Math.Sin(Timer / 7f);

            sb.Draw(glowTex, npc.Center + new Vector2(-12 * npc.spriteDirection, 16) - Main.screenPosition, null, new Color(255, 200, 100), 0, glowTex.Size() / 2, 0.5f + 0.1f * sin, 0, 0);
            sb.Draw(glowTex, npc.Center + new Vector2(-12 * npc.spriteDirection, 16) - Main.screenPosition, null, new Color(255, 100, 20), 0, glowTex.Size() / 2, 0.7f + 0.1f * sin2, 0, 0);

            if (State == 1 && Timer > 20 && Timer <= 60)
            {
                var tex = GetTexture(AssetDirectory.MiscTextures + "DirectionalBeam");
                Vector2 origin = new Vector2(0, tex.Height / 2);

                for (int k = 0; k < 10; k++)
                {
                    var pos = npc.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(SavedRotation) * k * 32;
                    var color = new Color(255, (int)(185 * (float)Math.Sin(k / 10f * 3.14f)), 50);
                    var colorMult = (float)Math.Sin(k / 10f * 3.14f) * (float)(Math.Sin((Timer - 20) / 40f * 3.14f));
                    var source = new Rectangle((int)(((Timer - 20) / 10f) * -tex.Width), 0, tex.Width, tex.Height);

                    sb.Draw(tex, pos, source, color * colorMult, SavedRotation, origin, 1, 0, 0);
                }
            }
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneGlass ? 100 : 0;
        }

        public override void NPCLoot()
        {

        }
    }
}
using Microsoft.Xna.Framework;

using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Vitric;
using Microsoft.Xna.Framework.Graphics;

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
            Main.npcFrameCount[npc.type] = 6;
            DisplayName.SetDefault("[PH]BoomBug");
        }

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 32;
            npc.knockBackResist = 1.5f;
            npc.lifeMax = 20;
            npc.noGravity = true;
            npc.noTileCollide = false;
            npc.damage = 10;
            npc.aiStyle = -1;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath4;
        }

        public override void AI()
        {
            Timer++;

            switch(State)
			{
                case 0: //wander

                    if (Timer == 1) //dont spawn on the ground
                        npc.position.Y -= 32;

                    if (Timer % 30 == 0)
                    {
                        npc.direction = Main.rand.NextBool() ? 1 : -1;
                        npc.spriteDirection = npc.direction;
                        SavedRotation = Main.rand.NextFloat(-0.5f, 0.5f);
                        npc.netUpdate = true;
                    }

                    npc.velocity = (Vector2.UnitX * npc.spriteDirection).RotatedBy(SavedRotation) * (Timer % 30) * 0.1f;

                    npc.TargetClosest();
                    if (Vector2.DistanceSquared(npc.Center, Main.player[npc.target].Center) <= Math.Pow(200, 2))
					{
                        Timer = 0;
                        State = 1;
					}

                break;

                case 1: //attack

                    if (Timer < 60)
                        npc.velocity *= 0.95f;

                    if (Timer == 20)
                    {
                        SavedRotation = (Main.player[npc.target].Center - npc.Center).ToRotation();
                        npc.netUpdate = true;
                    }

                    if (Timer == 60)
                        npc.velocity = Vector2.UnitX.RotatedBy(SavedRotation) * 5;

                    if (Timer > 60)
                    {
                        if(npc.velocity.Length() < 10)
                            npc.velocity *= 1.05f;

                        for(int x = -1; x <= 2; x++)
                            for (int y = -1; y <= 2; y++)
							{
                                Tile tile = Framing.GetTileSafely((int)(npc.position.X / 16) + x, (int)(npc.position.Y / 16) + y);

                                if(tile.collisionType == 1)
                                    Explode();
							}
                    }

                    break;
			}
        }

		public override void FindFrame(int frameHeight)
		{
            npc.frame = new Rectangle(0, frameHeight * (int)(Timer / 10 % 6), npc.width, frameHeight);
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
                var d = Dust.NewDustPerfect(npc.Center, DustType<Bosses.GlassBoss.LavaSpew>(), null, 0, default, 1.3f);
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            for(int k = 0; k < Main.maxPlayers; k++)
			{
                var player = Main.player[k];

                if (!player.active)
                    return;

                var mp = player.GetModPlayer<StarlightPlayer>();
                mp.Shake += (int)Math.Max(0, 30 - Vector2.Distance(npc.Center, player.Center) / 8f);
			}
        }

        public void DrawAdditive(SpriteBatch sb)
		{
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
            return spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneGlass ? 1 : 0;
        }

        public override void NPCLoot()
        {

        }
    }
}
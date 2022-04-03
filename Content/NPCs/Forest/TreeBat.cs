using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Forest
{
	class TreeBat : ModNPC
	{
        private Vector2 savedPos;
        private Vector2 targetPos;

        public ref float State => ref NPC.ai[0];
        public ref float GlobalTimer => ref NPC.ai[1];

        public enum BehaviorStates
		{
            Waiting,
            Dashing,
            Fleeing
		}

        public override string Texture => AssetDirectory.ForestNPC + Name;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => State == (int)BehaviorStates.Dashing;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Tree Bat");

        public override void SetDefaults()
        {
            NPC.width = 26;
            NPC.height = 20;
            NPC.knockBackResist = 0;
            NPC.lifeMax = 40;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.damage = 10;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath4;
            NPC.chaseable = false;
        }

		public override void AI()
        {
            GlobalTimer++;

            switch(State)
			{
                case (int)BehaviorStates.Waiting:

                    NPC.frame = new Rectangle(0, 0, 26, 20);

                    if (Main.dayTime || (Main.time > 28000 && Main.rand.Next(1000) == 0)) //flee at day
                    {
                        State = (int)BehaviorStates.Fleeing;
                        NPC.velocity.X += Main.rand.NextBool() ? 5 : -5;
                        GlobalTimer = 0;
                    }

                    for (int k = 0; k < Main.maxPlayers; k++)
					{
                        var Player = Main.player[k];

                        if (Vector2.DistanceSquared(Player.Center, NPC.Center) < Math.Pow(200, 2))
                        {
                            if ((Player.velocity - Player.oldVelocity).Length() > 10 || Player.UsingAnyAbility()) //accelerate too much, or use an ability are all enough to get them to aggro
                            {
                                if (Main.hardMode) //you're too much for them at this point
                                {
                                    State = (int)BehaviorStates.Fleeing;
                                    NPC.velocity.X += NPC.Center.X > Player.Center.X ? -5 : 5;
                                    GlobalTimer = 0;
                                }
                                else //they dash to attack you!
                                {
                                    State = (int)BehaviorStates.Dashing;
                                    GlobalTimer = 0;
                                    savedPos = NPC.Center;
                                    targetPos = Player.Center;

                                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath4, NPC.Center);
                                    return;
                                }
                            }
                        }
					}

                break;

                case (int)BehaviorStates.Dashing:

                    NPC.frame = new Rectangle(0, 5 * 20 + 20 * (((int)Main.GameUpdateCount / 2) % 3), 26, 20);

                    if (GlobalTimer == 1)
                        NPC.rotation = (savedPos - targetPos).ToRotation();

                    if(GlobalTimer > 30)
                        NPC.Center = Vector2.SmoothStep(savedPos, targetPos, (GlobalTimer - 30) / 20f);

                    if (GlobalTimer == 50)
                    {
                        NPC.velocity = (targetPos - savedPos) * 0.025f;
                        State = (int)BehaviorStates.Fleeing;
                        GlobalTimer = 0;
                    }

                break;

                case (int)BehaviorStates.Fleeing:

                    NPC.knockBackResist = 0.8f;
                    NPC.noTileCollide = true;

                    NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;

                    NPC.frame = new Rectangle(0, 20 + 20 * (((int)Main.GameUpdateCount / 2) % 4), 26, 20);

                    if (GlobalTimer == 1)
					{
                        NPC.velocity.X *= 2;
					}

                    NPC.velocity.X *= 0.998f;
                    NPC.velocity.Y -= 0.07f;
                    NPC.velocity.Y = Math.Max(NPC.velocity.Y, -4);
                    NPC.rotation = NPC.velocity.ToRotation();

                    if (GlobalTimer >= 450) //timeout
                        NPC.active = false;

                break;
			}
        }

		public override void OnHitByProjectile(Projectile Projectile, int damage, float knockback, bool crit)
		{
            State = (int)BehaviorStates.Fleeing;
            NPC.velocity.X += Main.rand.NextBool() ? 5 : -5;
            GlobalTimer = 0;
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
            spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, new Vector2(13, 10), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(Request<Texture2D>(Texture + "Glow").Value, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, new Vector2(13, 10), NPC.scale, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.player.ZoneForest() || Main.dayTime || Main.time > 24000) //they should only spawn late at night in the forest
                return 0;

            for (int x = -30; x < 30; x++)
                for(int y = -30; y < 30; y++)
				{
                    var realX = spawnInfo.spawnTileX + x;
                    var realY = spawnInfo.spawnTileY + y;

                    var tile = Framing.GetTileSafely(realX, realY);
                    var tileUnder = Framing.GetTileSafely(realX, realY + 1);
                    var tileWayUnder = Framing.GetTileSafely(realX, realY + 4);

                    if (tile.TileType == TileID.Trees && !tileUnder.HasTile && !tileWayUnder.HasTile)
					{
                        if (Main.npc.Any(n => n.type == NPC.type && n.position == new Vector2(realX * 16 + 8, realY * 16 + 24)))
                            continue;

                        NPC.NewNPC(NPC.GetSpawnSourceForNaturalSpawn(), realX * 16 + 8, realY * 16 + 28, NPC.type);
                        return 0;
					}
				}

            return 0;
        }
    }
}

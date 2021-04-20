using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Helpers;

using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Items.Herbology.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Content.NPCs.Forest
{
	class TreeBat : ModNPC
	{
        private Vector2 savedPos;
        private Vector2 targetPos;

        public ref float State => ref npc.ai[0];
        public ref float GlobalTimer => ref npc.ai[1];

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
            npc.width = 26;
            npc.height = 20;
            npc.knockBackResist = 0;
            npc.lifeMax = 40;
            npc.noGravity = true;
            npc.noTileCollide = false;
            npc.damage = 10;
            npc.aiStyle = -1;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath4;
            npc.chaseable = false;
        }

		public override void AI()
        {
            GlobalTimer++;

            switch(State)
			{
                case (int)BehaviorStates.Waiting:

                    npc.frame = new Rectangle(0, 0, 26, 20);

                    if (Main.dayTime || (Main.time > 28000 && Main.rand.Next(1000) == 0)) //flee at day
                    {
                        State = (int)BehaviorStates.Fleeing;
                        npc.velocity.X += Main.rand.NextBool() ? 5 : -5;
                        GlobalTimer = 0;
                    }

                    for (int k = 0; k < Main.maxPlayers; k++)
					{
                        var player = Main.player[k];

                        if (Vector2.DistanceSquared(player.Center, npc.Center) < Math.Pow(200, 2))
                        {
                            if (player.itemAnimation > 0 || (player.velocity - player.oldVelocity).Length() > 10 || player.UsingAnyAbility()) //use an item, accelerate too much, or use an ability are all enough to get them to aggro
                            {
                                if (Main.hardMode) //you're too much for them at this point
                                {
                                    State = (int)BehaviorStates.Fleeing;
                                    npc.velocity.X += npc.Center.X > player.Center.X ? -5 : 5;
                                    GlobalTimer = 0;
                                }
                                else //they dash to attack you!
                                {
                                    State = (int)BehaviorStates.Dashing;
                                    GlobalTimer = 0;
                                    savedPos = npc.Center;
                                    targetPos = player.Center;

                                    Main.PlaySound(SoundID.NPCDeath4, npc.Center);
                                    return;
                                }
                            }
                        }
					}

                break;

                case (int)BehaviorStates.Dashing:

                    npc.frame = new Rectangle(0, 5 * 20 + 20 * (((int)Main.GameUpdateCount / 2) % 3), 26, 20);

                    if (GlobalTimer == 1)
                        npc.rotation = (savedPos - targetPos).ToRotation();

                    if(GlobalTimer > 30)
                        npc.Center = Vector2.SmoothStep(savedPos, targetPos, (GlobalTimer - 30) / 20f);

                    if (GlobalTimer == 50)
                    {
                        npc.velocity = (targetPos - savedPos) * 0.025f;
                        State = (int)BehaviorStates.Fleeing;
                        GlobalTimer = 0;
                    }

                break;

                case (int)BehaviorStates.Fleeing:

                    npc.knockBackResist = 0.8f;
                    npc.noTileCollide = true;

                    npc.spriteDirection = npc.velocity.X > 0 ? 1 : -1;

                    npc.frame = new Rectangle(0, 20 + 20 * (((int)Main.GameUpdateCount / 2) % 4), 26, 20);

                    if (GlobalTimer == 1)
					{
                        npc.velocity.X *= 2;
					}

                    npc.velocity.X *= 0.998f;
                    npc.velocity.Y -= 0.07f;
                    npc.velocity.Y = Math.Max(npc.velocity.Y, -4);
                    npc.rotation = npc.velocity.ToRotation();

                    if (GlobalTimer >= 450) //timeout
                        npc.active = false;

                break;
			}
        }

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
		{
            State = (int)BehaviorStates.Fleeing;
            npc.velocity.X += Main.rand.NextBool() ? 5 : -5;
            GlobalTimer = 0;
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
            spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition, npc.frame, drawColor, npc.rotation, new Vector2(13, 10), npc.scale, npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            spriteBatch.Draw(GetTexture(Texture + "Glow"), npc.Center - Main.screenPosition, npc.frame, Color.White, npc.rotation, new Vector2(13, 10), npc.scale, npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
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

                    if (tile.type == TileID.Trees && !tileUnder.active() && !tileWayUnder.active())
					{
                        if (Main.npc.Any(n => n.type == npc.type && n.position == new Vector2(realX * 16 + 8, realY * 16 + 24)))
                            continue;

                        NPC.NewNPC(realX * 16 + 8, realY * 16 + 28, npc.type);
                        return 0;
					}
				}

            return 0;
        }

        public override void NPCLoot()
        {
            if(Main.rand.Next(4) == 0)
                Item.NewItem(npc.Hitbox, ItemType<ForestBerries>(), Main.rand.Next(3));
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Bosses.GlassBoss;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	internal class Snake : ModNPC
    {
        public ref float ActionState => ref npc.ai[0];
        public ref float ActionTimer => ref npc.ai[1];

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/Snake";

        public Player target => Main.player[npc.target];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Snek");
        }

        public override void SetDefaults()
        {
            npc.width = 66;
            npc.height = 64;
            npc.damage = 40;
            npc.defense = 10;
            npc.lifeMax = 100;
            npc.aiStyle = -1;
        }

        public override void AI()
        {
            ActionTimer++;

            switch(ActionState)
			{
                case 0: // Default/spawningd
                    npc.frame.Width = npc.width;
                    npc.frame.Height = npc.height;

                    npc.TargetClosest();
                    if (Vector2.Distance(npc.Center, target.Center) < 500)
                        ChangeState(1);

                    break;

                case 1: // Emerging
                    npc.frame.X = 2 * npc.width;
                    npc.frame.Y = npc.height * (int)(ActionTimer / 45f * 17);

                    if (ActionTimer > 60)
                        ChangeState(2);

                    break;

                case 2: // Targeting

                    if(ActionTimer == 1)
					{
                        npc.TargetClosest();

                        if (CastToTarget())
                            ChangeState(3);
                    }

					else
					{
                        npc.frame.X = 1 * npc.width;
                        npc.frame.Y = npc.height * (int)(ActionTimer / 30f * 12);

                        if(ActionTimer == 30)
                            npc.Center = FindNewPosition();

                        if(ActionTimer > 30)
						{
                            npc.frame.X = 2 * npc.width;
                            npc.frame.Y = npc.height * (int)((ActionTimer - 30) / 45f * 17);
                        }

                        if (ActionTimer >= 75)
                            ActionTimer = 0;
                    }

                    break;

                case 3: // Shooting
                    npc.frame.X = 0;

                    if(ActionTimer <= 30)
                        npc.frame.Y = npc.height * (int)(ActionTimer / 30f * 7);
                    else
                        npc.frame.Y = npc.height * (int)((1 - (ActionTimer - 30) / 20f) * 7);

                    if (ActionTimer == 20)
                        Projectile.NewProjectile(npc.Center, Vector2.Normalize(target.Center - npc.Center) * 10, ProjectileType<GlassSpike>(), 20, 0.2f, Main.myPlayer);

                    if (ActionTimer == 50)
                        ChangeState(2, 2);

                    break;
            }
        }

        private void ChangeState(int target, int time = 0)
		{
            ActionState = target;
            ActionTimer = time;

            Main.NewText("State changed to " + target);
		}

        private bool CastToTarget()
		{
            var dist = Vector2.Distance(npc.Center, target.Center);
            var checks = dist / 4;

            for(int k = 0; k < checks; k++)
			{
                var toCheck = Vector2.Lerp(npc.Center, target.Center, k / checks);

                if (Helpers.Helper.PointInTile(toCheck))
                    return false;
			}

            return true;
		}

        private Vector2 FindNewPosition()
		{
            Point16 currentPos = (npc.Center / 16).ToPoint16();

            for(int k = 0; k < 50; k++) //maximum attempts for finding a new spot
			{
                Point16 randPos = currentPos + new Point16(Main.rand.Next(-30, 30), Main.rand.Next(-30, 30));

                //Checking for a shape that looks like this:
                // ---    ---
                // ---[][]---
                if(
                    Framing.GetTileSafely(randPos).collisionType == 1 &&
                    Framing.GetTileSafely(randPos + new Point16(1, 0)).collisionType == 1 &&
                    !Framing.GetTileSafely(randPos + new Point16(0, -1)).active() &&
                    !Framing.GetTileSafely(randPos + new Point16(1, -1)).active() 
                    )
				{
                    return randPos.ToVector2() * 16 + new Vector2(16, -40);
				}
			}

            Main.NewText("Couldnt find a landing point!");
            return currentPos.ToVector2() * 16 + new Vector2(16, -40); //when it cant find a landing point, default to the current position
        }

        public override void NPCLoot()
        {

        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition + Vector2.UnitY * 2, npc.frame, drawColor, npc.rotation, new Vector2(33, 32), 1, 0, 0);
            return false;
        }
    }
}
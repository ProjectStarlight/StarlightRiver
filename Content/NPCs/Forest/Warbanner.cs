using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Forest
{
	class Warbanner : ModNPC, IDrawAdditive
    {
        public const float MAX_BUFF_RADIUS = 500;

        public List<NPC> targets = new List<NPC>();

        public ref float State => ref NPC.ai[0];
        public ref float GlobalTimer => ref NPC.ai[1];
        public ref float BuffRadius => ref NPC.ai[2];

        public float VFXAlpha => BuffRadius / MAX_BUFF_RADIUS;

        public enum BehaviorStates
        {
            Wandering,
            Locked,
            Fleeing
        }

        public override string Texture => AssetDirectory.ForestNPC + Name;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false; //harmless

        public override void SetStaticDefaults() => DisplayName.SetDefault("Haunted Warbanner");

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 32;
            NPC.knockBackResist = 0.1f;
            NPC.lifeMax = 100;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.damage = 1;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.chaseable = true;
        }

        public override void AI()
        {
            GlobalTimer++;

            if (Main.dayTime)
            {
                State = (int)BehaviorStates.Fleeing; //flee by day nomatter what
                GlobalTimer = 0;
            }

            switch (State)
            {
                case (int)BehaviorStates.Wandering: //find the nearest valid target

                    if (BuffRadius > 0)
                        BuffRadius--;

                    var firstTarget = NPC.FindNearestNPC(n => n.active && !n.friendly && !n.noGravity && Vector2.Distance(NPC.Center, n.Center) < 2500);

                    if (firstTarget != null)
                    {
                        targets.Add(firstTarget);
                        State = (int)BehaviorStates.Locked;
                        GlobalTimer = 0;
                    }
					else 
					{
                        NPC.velocity *= 0.9f;
                        if (GlobalTimer > 600)
                        {
                            State = (int)BehaviorStates.Fleeing; //after waiting for 10 seconds for a target they instead flee
                            GlobalTimer = 0;
                        }
					}

                    break;

                case (int)BehaviorStates.Locked:

                    if (BuffRadius < MAX_BUFF_RADIUS)
                        BuffRadius++;

                    for (int k = 0; k < targets.Count; k++) 
                    {
                        var toCheck = targets[k];
                        if (toCheck is null || !toCheck.active || Vector2.Distance(NPC.Center, toCheck.Center) > (targets.Count  > 1 ? MAX_BUFF_RADIUS : 2500) ) //remove invalid targets
                        {
                            targets.Remove(toCheck);
                            //k--;
                        }
                        else if (Helper.CheckCircularCollision(NPC.Center, (int)BuffRadius, toCheck.Hitbox))
						{
                            toCheck.AddBuff(BuffType<Rage>(), 2);
						}
                    }

                    if(targets.Count == 0) //if we've lost all targets go back to wandering
					{
                        State = (int)BehaviorStates.Wandering;
                        GlobalTimer = 0;
                        return;
					}

                    var target = Helper.Centeroid(targets) + new Vector2(0, -100);

                    NPC.velocity += Vector2.Normalize(NPC.Center - target) * -0.2f; //accelerate towards the centeroid of it's supported NPCs

                    if (NPC.velocity.Length() > 2) //speed cap
                        NPC.velocity = Vector2.Normalize(NPC.velocity) * 1.9f;

                    if (GlobalTimer % 60 == 0) //periodically check for more targets
					{
                        var potentialTarget = NPC.FindNearestNPC(n => !n.noGravity && !targets.Contains(n) && Vector2.Distance(NPC.Center, n.Center) < 500);

                        if(potentialTarget != null)
                            targets.Add(potentialTarget);
					}

                    break;

                case (int)BehaviorStates.Fleeing: //flee at daytime or when timing out

                    if (BuffRadius > 0)
                        BuffRadius -= 5;

                    NPC.velocity.Y -= 0.2f;

                    if (GlobalTimer > 300)
                        NPC.active = false;

                    break;
            }
        }

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
            var auraTex = Request<Texture2D>(AssetDirectory.VitricBoss + "BombTell").Value;
            var maxScale = MAX_BUFF_RADIUS / auraTex.Width;

            spriteBatch.Draw(auraTex, NPC.Center - Main.screenPosition, null, Color.Red * VFXAlpha * 0.8f, 0, auraTex.Size() / 2, VFXAlpha * maxScale, 0, 0);
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.player.ZoneForest() && !Main.dayTime && NPC.downedBoss1 && !Main.npc.Any(n => n.active && n.type == NPC.type)) //they should only spawn at night in the forest after EoC is dead, and one max
                return 0.25f;

            return 0;
        }
    }
}

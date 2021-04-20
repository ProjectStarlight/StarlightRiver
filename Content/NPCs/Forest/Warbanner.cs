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
using StarlightRiver.Content.Buffs;

namespace StarlightRiver.Content.NPCs.Forest
{
    class Warbanner : ModNPC, IDrawAdditive
    {
        public const float MAX_BUFF_RADIUS = 500;

        public List<NPC> targets = new List<NPC>();

        public ref float State => ref npc.ai[0];
        public ref float GlobalTimer => ref npc.ai[1];
        public ref float BuffRadius => ref npc.ai[2];

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
            npc.width = 32;
            npc.height = 32;
            npc.knockBackResist = 0.1f;
            npc.lifeMax = 100;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.damage = 1;
            npc.aiStyle = -1;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath2;
            npc.chaseable = true;
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

                    var firstTarget = npc.FindNearestNPC(n => n.active && !n.friendly && !n.noGravity && Vector2.Distance(npc.Center, n.Center) < 2500);

                    if (firstTarget != null)
                    {
                        targets.Add(firstTarget);
                        State = (int)BehaviorStates.Locked;
                        GlobalTimer = 0;
                    }
					else 
					{
                        npc.velocity *= 0.9f;
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
                        if (toCheck is null || !toCheck.active || Vector2.Distance(npc.Center, toCheck.Center) > (targets.Count  > 1 ? MAX_BUFF_RADIUS : 2500) ) //remove invalid targets
                        {
                            targets.Remove(toCheck);
                            //k--;
                        }
                        else if (Helper.CheckCircularCollision(npc.Center, (int)BuffRadius, toCheck.Hitbox))
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

                    npc.velocity += Vector2.Normalize(npc.Center - target) * -0.2f; //accelerate towards the centeroid of it's supported NPCs

                    if (npc.velocity.Length() > 2) //speed cap
                        npc.velocity = Vector2.Normalize(npc.velocity) * 1.9f;

                    if (GlobalTimer % 60 == 0) //periodically check for more targets
					{
                        var potentialTarget = npc.FindNearestNPC(n => !n.noGravity && !targets.Contains(n) && Vector2.Distance(npc.Center, n.Center) < 500);

                        if(potentialTarget != null)
                            targets.Add(potentialTarget);
					}

                    break;

                case (int)BehaviorStates.Fleeing: //flee at daytime or when timing out

                    if (BuffRadius > 0)
                        BuffRadius -= 5;

                    npc.velocity.Y -= 0.2f;

                    if (GlobalTimer > 300)
                        npc.active = false;

                    break;
            }
        }

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
            var auraTex = GetTexture(AssetDirectory.GlassBoss + "BombTell");
            var maxScale = MAX_BUFF_RADIUS / auraTex.Width;

            spriteBatch.Draw(auraTex, npc.Center - Main.screenPosition, null, Color.Red * VFXAlpha * 0.8f, 0, auraTex.Size() / 2, VFXAlpha * maxScale, 0, 0);
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.player.ZoneForest() && !Main.dayTime && NPC.downedBoss1 && !Main.npc.Any(n => n.active && n.type == npc.type)) //they should only spawn at night in the forest after EoC is dead, and one max
                return 0.25f;

            return 0;
        }

        public override void NPCLoot()
        {
            if (Main.rand.Next(4) == 0)
                Item.NewItem(npc.Hitbox, ItemType<ForestBerries>(), Main.rand.Next(3));
        }

    }
}

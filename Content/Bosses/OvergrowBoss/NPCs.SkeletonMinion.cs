using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.NPCs.Boss.OvergrowBoss
{
    class SkeletonMinion : ModNPC
    {
        public override string Texture => AssetDirectory.OvergrowBoss + Name;

        public override void SetDefaults()
        {
            npc.lifeMax = 120;
            npc.damage = 35;
            npc.width = 32;
            npc.height = 48;
            npc.defense = -20;
            npc.aiStyle = -1;
            npc.knockBackResist = 1.8f;
            npc.HitSound = SoundID.NPCHit2;
            npc.DeathSound = SoundID.NPCDeath2;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => npc.ai[2] > 0;

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) => damage *= 3;

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit) => damage *= 3;

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (npc.ai[0] < 60)
            {
                Texture2D tex = GetTexture(Texture);
                int progress = (int)(npc.ai[0] / 60f * npc.height);
                Rectangle target = new Rectangle((int)(npc.position.X - Main.screenPosition.X), (int)(npc.position.Y + npc.height - progress - Main.screenPosition.Y), npc.width, progress);
                Rectangle frame = new Rectangle(0, 0, npc.width, progress);

                spriteBatch.Draw(tex, target, frame, drawColor);
                return false;
            }
            return true;
        }

        public override void AI()
        {
            npc.ai[0]++;

            if (npc.ai[0] == 1) npc.ai[1] = Main.rand.NextFloat(2, 4);

            if (npc.ai[0] < 60)
            {
                Dust.NewDustPerfect(npc.position + new Vector2(Main.rand.Next(npc.width), npc.height), DustID.Stone);
                npc.noGravity = true;
                npc.velocity.Y = 10;
            }

            if (npc.ai[0] > 60)
            {
                npc.noGravity = false;
                npc.TargetClosest();
                Player player = Main.player[npc.target];

                if (player != null)
                {
                    //pathing
                    npc.velocity.X += player.Center.X > npc.Center.X ? 0.4f : -0.4f;
                    if (npc.velocity.Length() > npc.ai[1]) npc.velocity = Vector2.Normalize(npc.velocity) * npc.ai[1];

                    //jump
                    Tile obstacleTile = Framing.GetTileSafely((npc.position + new Vector2(npc.direction == 1 ? npc.width : 0, 48)));
                    if (obstacleTile.collisionType == 1 && npc.velocity.Y == 0) npc.velocity.Y -= 5;

                    //melee attack activation
                    if (npc.ai[2] == 0 && Vector2.Distance(player.Center, npc.Center) <= 32)
                    {
                        npc.ai[2] += 60;
                    }

                    //melee attack
                    if (npc.ai[2] > 0)
                    {
                        npc.ai[2]--;
                        npc.velocity.X = 0;
                        npc.velocity.Y = 0;
                    }
                }
            }
        }
    }
}

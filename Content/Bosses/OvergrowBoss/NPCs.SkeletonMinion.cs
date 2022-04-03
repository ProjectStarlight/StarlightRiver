using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Boss.OvergrowBoss
{
	class SkeletonMinion : ModNPC
    {
        public override string Texture => AssetDirectory.OvergrowBoss + Name;

        public override void SetDefaults()
        {
            NPC.lifeMax = 120;
            NPC.damage = 35;
            NPC.width = 32;
            NPC.height = 48;
            NPC.defense = -20;
            NPC.aiStyle = -1;
            NPC.knockBackResist = 1.8f;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => NPC.ai[2] > 0;

        public override void ModifyHitByProjectile(Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) => damage *= 3;

        public override void ModifyHitByItem(Player Player, Item Item, ref int damage, ref float knockback, ref bool crit) => damage *= 3;

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (NPC.ai[0] < 60)
            {
                Texture2D tex = Request<Texture2D>(Texture).Value;
                int progress = (int)(NPC.ai[0] / 60f * NPC.height);
                Rectangle target = new Rectangle((int)(NPC.position.X - Main.screenPosition.X), (int)(NPC.position.Y + NPC.height - progress - Main.screenPosition.Y), NPC.width, progress);
                Rectangle frame = new Rectangle(0, 0, NPC.width, progress);

                spriteBatch.Draw(tex, target, frame, drawColor);
                return false;
            }
            return true;
        }

        public override void AI()
        {
            NPC.ai[0]++;

            if (NPC.ai[0] == 1) NPC.ai[1] = Main.rand.NextFloat(2, 4);

            if (NPC.ai[0] < 60)
            {
                Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), NPC.height), DustID.Stone);
                NPC.noGravity = true;
                NPC.velocity.Y = 10;
            }

            if (NPC.ai[0] > 60)
            {
                NPC.noGravity = false;
                NPC.TargetClosest();
                Player Player = Main.player[NPC.target];

                if (Player != null)
                {
                    //pathing
                    NPC.velocity.X += Player.Center.X > NPC.Center.X ? 0.4f : -0.4f;
                    if (NPC.velocity.Length() > NPC.ai[1]) NPC.velocity = Vector2.Normalize(NPC.velocity) * NPC.ai[1];

                    //jump
                    Tile obstacleTile = Framing.GetTileSafely((NPC.position + new Vector2(NPC.direction == 1 ? NPC.width : 0, 48)));
                    if (obstacleTile.collisionType == 1 && NPC.velocity.Y == 0) NPC.velocity.Y -= 5;

                    //melee attack activation
                    if (NPC.ai[2] == 0 && Vector2.Distance(Player.Center, NPC.Center) <= 32)
                    {
                        NPC.ai[2] += 60;
                    }

                    //melee attack
                    if (NPC.ai[2] > 0)
                    {
                        NPC.ai[2]--;
                        NPC.velocity.X = 0;
                        NPC.velocity.Y = 0;
                    }
                }
            }
        }
    }
}

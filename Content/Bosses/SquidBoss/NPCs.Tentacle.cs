using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.NPCs.BaseTypes;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    public class Tentacle : ModNPC, IUnderwater
    {
        public override string Texture => AssetDirectory.Invisible;
        public SquidBoss Parent { get; set; }
        public Vector2 MovePoint;
        public Vector2 SavedPoint;
        public int OffBody;

        public enum TentacleStates
        {
            SpawnAnimation = 0,
            FirstPhase = 1
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) => false;

        public override bool CheckActive() => false;

        public override void SetDefaults()
        {
            npc.width = 60;
            npc.height = 80;
            npc.lifeMax = 225;
            npc.damage = 20;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.knockBackResist = 0f;
            npc.HitSound = Terraria.ID.SoundID.NPCHit1;
        }

        public void DrawUnderWater(SpriteBatch spriteBatch)
        {
            if (Parent != null)
            {
                Texture2D top = GetTexture(AssetDirectory.SquidBoss + "TentacleTop");
                Texture2D glow = GetTexture(AssetDirectory.SquidBoss + "TentacleGlow");
                Texture2D glow2 = GetTexture(AssetDirectory.SquidBoss + "TentacleGlow2");
                Texture2D body = GetTexture(AssetDirectory.SquidBoss + "TentacleBody");
                Texture2D ring = GetTexture(AssetDirectory.SquidBoss + "TentacleRing");

                float dist = npc.Center.X - Parent.npc.Center.X;
                int underMax = 0;
                underMax = (int)(npc.ai[1] / 60 * 40);

                if (underMax > 40) underMax = 40;

                if (Parent.npc.ai[0] != (int)SquidBoss.AIStates.SpawnAnimation)
                {
                    NPC actor = Main.npc.FirstOrDefault(n => n.active && n.modNPC is ArenaActor);
                    underMax = (int)((actor.Center.Y - Parent.npc.Center.Y) / 10f) + 40;
                }

                if (Parent.npc.ai[0] == (int)SquidBoss.AIStates.ThirdPhase && Parent.npc.ai[1] > 240) underMax = 0;

                for (int k = 0; k < underMax; k++)
                {
                    Vector2 pos = Parent.npc.Center + new Vector2(OffBody - 9 + (float)Math.Sin(npc.ai[1] / 20f + k) * 2, 100 + k * 10);
                    spriteBatch.Draw(body, pos - Main.screenPosition, Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) * 1.6f);
                }

                if (npc.ai[1] > 60 && Vector2.Distance(npc.Center, SavedPoint) > 8)
                {
                    Color color;

                    switch (npc.ai[0])
                    {
                        case 0: color = new Color(100, 255, 50); break;
                        case 1: color = new Color(255, 120, 140); break;

                        case 2:

                            float sin = 1 + (float)Math.Sin(npc.ai[1] / 10f);
                            float cos = 1 + (float)Math.Cos(npc.ai[1] / 10f);
                            color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

                            if (Parent.Phase == (int)SquidBoss.AIStates.ThirdPhase) color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.8f;

                            break;

                        default: color = Color.Black; break;
                    }

                    float rot = (SavedPoint - npc.Center).ToRotation() - 1.57f;

                    spriteBatch.Draw(top, npc.Center - Main.screenPosition, top.Frame(), Lighting.GetColor((int)npc.Center.X / 16, (int)npc.Center.Y / 16) * 2f, rot, top.Size() / 2, 1, 0, 0);
                    spriteBatch.Draw(glow, npc.Center - Main.screenPosition, glow.Frame(), color * 0.6f, rot, top.Size() / 2, 1, 0, 0);
                    spriteBatch.Draw(glow2, npc.Center - Main.screenPosition, glow.Frame(), color, rot, top.Size() / 2, 1, 0, 0);

                    Lighting.AddLight(npc.Center, color.ToVector3() * 0.2f);

                    for (int k = 0; k < Vector2.Distance(npc.Center + new Vector2(0, npc.height / 2), SavedPoint) / 10f; k++)
                    {
                        Vector2 pos = new Vector2((float)Math.Sin(npc.ai[1] / 20f + k) * 4, 0) + Vector2.Lerp(npc.Center + new Vector2(0, npc.height / 2).RotatedBy(rot),
                            SavedPoint, k / Vector2.Distance(npc.Center + new Vector2(0, npc.height / 2), SavedPoint) * 10f);

                        spriteBatch.Draw(body, pos - Main.screenPosition, body.Frame(), Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) * 1.6f, rot, body.Size() / 2, 1, 0, 0);
                    }

                    // Draw the ring around the tentacle
                    int squish = (int)(Math.Sin(npc.ai[1] * 0.1f) * 5);
                    Rectangle rect = new Rectangle((int)(npc.Center.X - Main.screenPosition.X), (int)(npc.Center.Y - Main.screenPosition.Y) + 40, 34 - squish, 16 + (int)(squish * 0.4f));

                    if (npc.ai[0] != 2) spriteBatch.Draw(ring, rect, ring.Frame(), color * 0.6f, 0, ring.Size() / 2, 0, 0);
                }
            }
        }

        public override bool CheckDead()
        {
            npc.life = 1;
            npc.ai[0] = 2;

            Main.PlaySound(Terraria.ID.SoundID.NPCDeath1, npc.Center);

            for (int k = 0; k < 40; k++)
                Dust.NewDust(npc.position + new Vector2(0, 30), npc.width, 16, 131, 0, 0, 0, default, 0.5f);

            return false;
        }

        //TODO: Give this another look/find a better way to deplete main HP

        public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (npc.life - damage <= 0) damage = npc.life;
            Parent.npc.life -= damage;
        }

        public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (npc.life - damage <= 0) damage = npc.life;
            Parent.npc.life -= damage;
        }

        public override void AI()
        {
            /* AI fields:
             * 0: state
             * 1: timer
             */
            if (Parent == null || !Parent.npc.active) npc.active = false;

            npc.dontTakeDamage = npc.ai[0] != 0;

            if (Parent.npc.ai[0] == (int)SquidBoss.AIStates.SpawnAnimation) npc.dontTakeDamage = true;

            if ((npc.ai[0] == 0 || npc.ai[0] == 1) && npc.ai[1] == 0) SavedPoint = npc.Center;

            npc.ai[1]++;

            if (npc.ai[1] >= 60 && npc.ai[1] < 120) npc.Center = Vector2.SmoothStep(SavedPoint, MovePoint, (npc.ai[1] - 60) / 60f); //Spawn animation
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write(SavedPoint.X);
            writer.Write(SavedPoint.Y);

            writer.Write(MovePoint.X);
            writer.Write(MovePoint.Y);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            SavedPoint = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            MovePoint = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
    }
}

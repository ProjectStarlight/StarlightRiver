using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            NPC.width = 60;
            NPC.height = 80;
            NPC.lifeMax = 225;
            NPC.damage = 20;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.HitSound = Terraria.ID.SoundID.NPCHit1;
        }

        public void DrawUnderWater(SpriteBatch spriteBatch)
        {
            if (Parent != null)
            {
                Texture2D top = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleTop").Value;
                Texture2D glow = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleGlow").Value;
                Texture2D glow2 = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleGlow2").Value;
                Texture2D body = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleBody").Value;
                Texture2D ring = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleRing").Value;

                float dist = NPC.Center.X - Parent.NPC.Center.X;
                int underMax = 0;
                underMax = (int)(NPC.ai[1] / 60 * 40);

                if (underMax > 40) underMax = 40;

                if (Parent.NPC.ai[0] != (int)SquidBoss.AIStates.SpawnAnimation)
                {
                    NPC actor = Main.npc.FirstOrDefault(n => n.active && n.ModNPC is ArenaActor);
                    underMax = (int)((actor.Center.Y - Parent.NPC.Center.Y) / 10f) + 40;
                }

                if (Parent.NPC.ai[0] == (int)SquidBoss.AIStates.ThirdPhase && Parent.NPC.ai[1] > 240) underMax = 0;

                for (int k = 0; k < underMax; k++)
                {
                    Vector2 pos = Parent.NPC.Center + new Vector2(OffBody - 9 + (float)Math.Sin(NPC.ai[1] / 20f + k) * 2, 100 + k * 10);
                    spriteBatch.Draw(body, pos - Main.screenPosition, Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) * 1.6f);
                }

                if (NPC.ai[1] > 60 && Vector2.Distance(NPC.Center, SavedPoint) > 8)
                {
                    Color color;

                    switch (NPC.ai[0])
                    {
                        case 0: color = new Color(100, 255, 50); break;
                        case 1: color = new Color(255, 120, 140); break;

                        case 2:

                            float sin = 1 + (float)Math.Sin(NPC.ai[1] / 10f);
                            float cos = 1 + (float)Math.Cos(NPC.ai[1] / 10f);
                            color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

                            if (Parent.Phase == (int)SquidBoss.AIStates.ThirdPhase) color = new Color(1.2f + sin * 0.1f, 0.7f + sin * -0.25f, 0.25f) * 0.8f;

                            break;

                        default: color = Color.Black; break;
                    }

                    float rot = (SavedPoint - NPC.Center).ToRotation() - 1.57f;

                    spriteBatch.Draw(top, NPC.Center - Main.screenPosition, top.Frame(), Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) * 2f, rot, top.Size() / 2, 1, 0, 0);
                    spriteBatch.Draw(glow, NPC.Center - Main.screenPosition, glow.Frame(), color * 0.8f, rot, top.Size() / 2, 1, 0, 0);

                    spriteBatch.End();
                    spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                    spriteBatch.Draw(glow2, NPC.Center - Main.screenPosition, glow.Frame(), color * 0.6f, rot, top.Size() / 2, 1, 0, 0);

                    spriteBatch.End();
                    spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                    Lighting.AddLight(NPC.Center, color.ToVector3() * 0.2f);

                    for (int k = 0; k < Vector2.Distance(NPC.Center + new Vector2(0, NPC.height / 2), SavedPoint) / 10f; k++)
                    {
                        Vector2 pos = new Vector2((float)Math.Sin(NPC.ai[1] / 20f + k) * 4, 0) + Vector2.Lerp(NPC.Center + new Vector2(0, NPC.height / 2).RotatedBy(rot),
                            SavedPoint, k / Vector2.Distance(NPC.Center + new Vector2(0, NPC.height / 2), SavedPoint) * 10f);

                        spriteBatch.Draw(body, pos - Main.screenPosition, body.Frame(), Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) * 1.6f, rot, body.Size() / 2, 1, 0, 0);
                    }

                    // Draw the ring around the tentacle
                    int squish = (int)(Math.Sin(NPC.ai[1] * 0.1f) * 5);
                    Rectangle rect = new Rectangle((int)(NPC.Center.X - Main.screenPosition.X), (int)(NPC.Center.Y - Main.screenPosition.Y) + 40, 34 - squish, 16 + (int)(squish * 0.4f));

                    if (NPC.ai[0] != 2) spriteBatch.Draw(ring, rect, ring.Frame(), color * 0.6f, 0, ring.Size() / 2, 0, 0);
                }
            }
        }

        public override bool CheckDead()
        {
            NPC.life = 1;
            NPC.ai[0] = 2;

            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.NPCDeath1, NPC.Center);

            for (int k = 0; k < 40; k++)
                Dust.NewDust(NPC.position + new Vector2(0, 30), NPC.width, 16, 131, 0, 0, 0, default, 0.5f);

            return false;
        }

        //TODO: Give this another look/find a better way to deplete main HP

        public override void ModifyHitByProjectile(Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (NPC.life - damage <= 0) damage = NPC.life;
            Parent.NPC.life -= damage;
        }

        public override void ModifyHitByItem(Player Player, Item Item, ref int damage, ref float knockback, ref bool crit)
        {
            if (NPC.life - damage <= 0) damage = NPC.life;
            Parent.NPC.life -= damage;
        }

        public override void AI()
        {
            /* AI fields:
             * 0: state
             * 1: timer
             */
            if (Parent == null || !Parent.NPC.active) NPC.active = false;

            NPC.dontTakeDamage = NPC.ai[0] != 0;

            if (Parent.NPC.ai[0] == (int)SquidBoss.AIStates.SpawnAnimation) NPC.dontTakeDamage = true;

            if ((NPC.ai[0] == 0 || NPC.ai[0] == 1) && NPC.ai[1] == 0) SavedPoint = NPC.Center;

            NPC.ai[1]++;

            if (NPC.ai[1] >= 60 && NPC.ai[1] < 120) NPC.Center = Vector2.SmoothStep(SavedPoint, MovePoint, (NPC.ai[1] - 60) / 60f); //Spawn animation
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

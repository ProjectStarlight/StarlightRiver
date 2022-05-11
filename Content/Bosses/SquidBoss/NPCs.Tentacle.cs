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
        public Vector2 MovementTarget;
        public Vector2 BasePoint;
        public int OffsetFromParentBody;
        public bool DrawPortal;

        public float StalkWaviness = 1;
        public float ZSpin = 0;
        public int DownwardDrawDistance = 28;

        public ref float State => ref NPC.ai[0];
        public ref float Timer => ref NPC.ai[1];

        public enum TentacleStates
        {
            SpawnAnimation = 0,
            FirstPhase = 1
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

        public override bool CheckActive() => false;

        public override void SetDefaults()
        {
            NPC.width = 80;
            NPC.height = 100;
            NPC.lifeMax = 500;
            NPC.damage = 20;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.HitSound = Terraria.ID.SoundID.NPCHit1;
        }

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
            NPC.lifeMax = Main.masterMode ? (int)(1000 * bossLifeScale) : (int)(750 * bossLifeScale);
        }

		public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
        {
            if (Parent != null)
            {
                Texture2D top = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleTop").Value;
                Texture2D glow = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleGlow").Value;
                Texture2D glow2 = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleGlow2").Value;
                Texture2D body = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleBody").Value;
                Texture2D ring = Request<Texture2D>(AssetDirectory.SquidBoss + "TentacleRing").Value;

                Color glowColor;
                Color auroraColor;

                float sin1 = 1 + (float)Math.Sin(Timer / 10f);
                float cos1 = 1 + (float)Math.Cos(Timer / 10f);
                auroraColor = new Color(0.5f + cos1 * 0.2f, 0.8f, 0.5f + sin1 * 0.2f);

                switch (State) //Select the color of this tentacle's glow
                {
                    case 0: //vulnerable
                        float sin0 = 1 + (float)Math.Sin(Timer / 10f);
                        glowColor = new Color(255, 100 + (int)(sin0 * 50), 40);

                        break;
                    
                    case 1: //invulnerable
                        glowColor = auroraColor;

                        if (Parent.Phase == (int)SquidBoss.AIStates.ThirdPhase) 
                            glowColor = new Color(1.2f + sin1 * 0.1f, 0.7f + sin1 * -0.25f, 0.25f) * 0.8f;

                        break;

                    case 2: //dead
                        glowColor = new Color(100, 100, 150) * 0.5f;
                        
                        break;

                    default: glowColor = Color.Black; break;
                }

                if (NPCLayer == 0)
                {
                    var extraLength = (int)(Math.Abs(OffsetFromParentBody) * 0.15f);
                    var maxSegments = DownwardDrawDistance + extraLength;

                    if (DrawPortal) 
                        maxSegments = Math.Min(maxSegments, 40 + extraLength);

                    for (int k = 0; k <= maxSegments; k++)
                    {
                        var pos = Parent.NPC.Center + new Vector2(OffsetFromParentBody, 100 + k * 10) - Main.screenPosition;
                        pos.X += (float)Math.Sin(Timer / 20f + k * 0.1f) * k * 0.5f;
                        pos.X += OffsetFromParentBody * k * 0.03f;

                        var scale = 1 + Math.Max(0, (k - maxSegments + 10) / 20f);
                        var lightColor = Lighting.GetColor((int)(pos.X + Main.screenPosition.X) / 16, (int)(pos.Y + Main.screenPosition.Y) / 16) * 1.1f * Parent.Opacity;
                        lightColor.A = 255;

                        spriteBatch.Draw(body, pos, null, lightColor, 0, body.Size() / 2, scale, 0, 0);

                        if (k == maxSegments)
                        {
                            if (DrawPortal && maxSegments >= 40 + extraLength)
                            {
                                var portal = Request<Texture2D>(AssetDirectory.SquidBoss + "Portal").Value;
                                var target = new Rectangle((int)pos.X, (int)pos.Y, (int)((DownwardDrawDistance - 28) / 12f * portal.Width), portal.Height / 2);
                                spriteBatch.Draw(portal, target, top.Frame(), auroraColor, 0, portal.Size() / 2, 0, 0);
                            }
                            else
                            {
                                var topOriginBody = new Vector2(top.Width / 2, top.Height);
                                var bodyRotation = 3.14f + ((float)Math.Sin(Timer * 0.1f) * 0.25f) + Parent.NPC.rotation;

                                spriteBatch.Draw(top, pos, top.Frame(), lightColor, bodyRotation, topOriginBody, 1, 0, 0);
                                spriteBatch.Draw(glow, pos, glow.Frame(), glowColor * 0.325f * Parent.Opacity, bodyRotation, topOriginBody, 1, 0, 0);

                                var glow2Color = glowColor;
                                glow2Color.A = 0;
                                spriteBatch.Draw(glow2, pos, glow.Frame(), glow2Color * 0.3f * Parent.Opacity, bodyRotation, topOriginBody, 1, 0, 0);
                            }
                        }
                    }
                }

                if (NPCLayer == 1 && Timer > 60)
                {
                    if (DrawPortal)
                    {
                        var portal = Request<Texture2D>(AssetDirectory.SquidBoss + "Portal").Value;
                        var target = new Rectangle((int)(BasePoint.X - Main.screenPosition.X), (int)(BasePoint.Y - Main.screenPosition.Y), (int)((DownwardDrawDistance - 28) / 12f * portal.Width), portal.Height / 2);
                        var rotation = (MovementTarget - BasePoint).ToRotation() - 1.57f;

                        spriteBatch.Draw(portal, target, top.Frame(), auroraColor, rotation, portal.Size() / 2, 0, 0);
                    }

                    if (Vector2.Distance(NPC.Center, BasePoint) > 8)
                    {
                        float rot = (BasePoint - NPC.Center).ToRotation() - 1.57f;
                        float tentacleSin = (float)Math.Sin(Timer / 20f) * StalkWaviness;

                        rot += tentacleSin * 0.5f;

                        var topOrigin = new Vector2(top.Width / 2, top.Height + 10);
                        var litColor = Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16) * 2.6f;
                        var topPos = NPC.Center + new Vector2(tentacleSin * 30, 0).RotatedBy(rot) + Vector2.UnitY * 36;
                        var topTarget = new Rectangle((int)(topPos.X - Main.screenPosition.X), (int)(topPos.Y - Main.screenPosition.Y), (int)(top.Width * Math.Abs(Math.Sin(ZSpin + 1.57f))), top.Height);

                        spriteBatch.Draw(top, topTarget, top.Frame(), litColor, rot, topOrigin, 0, 0);
                        spriteBatch.Draw(glow, topTarget, glow.Frame(), glowColor * 0.65f, rot, topOrigin, 0, 0);

                        var glow2Color = glowColor;
                        glow2Color.A = 0;
                        spriteBatch.Draw(glow2, topTarget, glow.Frame(), glow2Color * 0.6f, rot, topOrigin, 0, 0);

                        Lighting.AddLight(NPC.Center, glowColor.ToVector3() * 0.2f);

                        for (float k = 0; k < Vector2.Distance(NPC.Center, BasePoint);)
                        {
                            float segmentSin = (float)Math.Sin(Timer / 20f + k * 0.02f);
                            float magnitude = Math.Max(0, 30 - k * 0.05f) * StalkWaviness;
                            float size = Math.Max(1, 2 - k * 0.005f);
                            Vector2 pos = new Vector2(segmentSin * magnitude, 0).RotatedBy(rot) + Vector2.Lerp(NPC.Center + new Vector2(0, 36), BasePoint, k / Vector2.Distance(NPC.Center, BasePoint));

                            if (k == 0 && State != 2)
                                spriteBatch.Draw(ring, pos - Main.screenPosition, ring.Frame(), glowColor, rot + segmentSin * 0.25f, ring.Size() / 2, 1, 0, 0);
                            else
                                spriteBatch.Draw(body, pos - Main.screenPosition, body.Frame(), Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16) * 2.6f, rot + segmentSin * 0.25f, body.Size() / 2, size, 0, 0);

                            k += 10 * size;
                        }
                    }
                }
            }
        }

        public override bool CheckDead()
        {
            NPC.life = 1;
            State = 2;

            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.NPCDeath1, NPC.Center);

            for (int k = 0; k < 40; k++)
                Dust.NewDust(NPC.position + new Vector2(0, 30), NPC.width, 16, 131, 0, 0, 0, default, 0.5f);

            return false;
        }

		public override void AI()
        {
            /* AI fields:
             * 0: state
             * 1: timer
             */
            if (Parent == null || !Parent.NPC.active) 
                NPC.active = false;

            NPC.dontTakeDamage = State != 0;

            if (Parent.NPC.ai[0] == (int)SquidBoss.AIStates.SpawnAnimation) 
                NPC.dontTakeDamage = true;

            if ((State == 0 || State == 1) && Timer == 0) 
                BasePoint = NPC.Center;

            Timer++;

            if (Timer >= 60 && Timer < 120) 
                NPC.Center = Vector2.SmoothStep(BasePoint, MovementTarget, (Timer - 60) / 60f); //Spawn animation

            //Colission for the stalks since tmod... dosent have a hook for this?
            if (Vector2.Distance(NPC.Center, BasePoint) > 32)
            {
                foreach (Player player in Main.player.Where(n => n.active))
                {
                    if (Helpers.Helper.CheckLinearCollision(NPC.Center, BasePoint, player.Hitbox, out Vector2 intersect))
                    {
                        if(intersect.X < player.Center.X)
                            player.velocity.X = Math.Max(6.5f, player.velocity.X * -1.05f);
                        else
                            player.velocity.X = Math.Min(-6.5f, player.velocity.X * -1.05f);
                        

                        player.Hurt(Terraria.DataStructures.PlayerDeathReason.ByNPC(NPC.whoAmI), NPC.damage, NPC.Center.X > player.Center.X ? -1 : 1);
                    }
                }
            }
        }

        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {
            writer.Write(BasePoint.X);
            writer.Write(BasePoint.Y);

            writer.Write(MovementTarget.X);
            writer.Write(MovementTarget.Y);
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {
            BasePoint = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            MovementTarget = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
    }
}

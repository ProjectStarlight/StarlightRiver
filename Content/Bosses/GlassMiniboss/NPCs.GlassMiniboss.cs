using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	public partial class GlassMiniboss : ModNPC
    {
        public bool attackVariant = false;
        //bool attackLowHPVariant => NPC.life <= NPC.lifeMax * 0.5f;

        internal ref float Phase => ref NPC.ai[0];
        internal ref float GlobalTimer => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
        internal ref float AttackTimer => ref NPC.ai[3];
        internal ref float AttackType => ref NPC.localAI[0];

        public Vector2 arenaPos;

        //Phase tracking utils
        public enum PhaseEnum
        {
            SpawnEffects,
            DespawnEffects,
            JumpToBackground,
            GauntletPhase,
            ReturnToForeground,
            DirectPhase,
            DeathEffects
        }

        public enum AttackEnum
        {
            None,
            Jump,
            SpinJump,
            Slash,
            SpinSlash,
            Spears,
            Hammer,
            BigBrightBubble
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glassweaver"); 
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
        }

        public override string Texture => AssetDirectory.GlassMiniboss + "Glassweaver";

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false; //no contact damage!

        public override void SetDefaults()
        {
            NPC.width = 82;
            NPC.height = 75;
            NPC.lifeMax = 1500;
            NPC.damage = 20;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.knockBackResist = 0;
            NPC.boss = true;
            NPC.defense = 14;
            //Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Miniboss");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(2000 * bossLifeScale);
        }

        public override bool CheckDead()
        {
            StarlightWorld.Flag(WorldFlags.DesertOpen);

            NPC.life = 1;
            NPC.dontTakeDamage = true;

            return false;
        }

        private void SetPhase(PhaseEnum phase)
        {
            Phase = (float)phase;
        }

        public override void AI()
        {
            AttackTimer++;

            switch (Phase)
            {
                case (int)PhaseEnum.SpawnEffects:

                    arenaPos = StarlightWorld.VitricBiome.TopLeft() * 16 + new Vector2(1 * 16, 76 * 16) + new Vector2(0, 256);

                    //UILoader.GetUIState<TextCard>().Display("Glassweaver", "the", null, 240, 1, true);

                    SetPhase(PhaseEnum.JumpToBackground);
                    ResetAttack();

                    break;

                case (int)PhaseEnum.JumpToBackground:

                    //if (AttackTimer <= 90) 
                    //    SpawnAnimation();

                    //else
                    //{
                    SetPhase(PhaseEnum.GauntletPhase);
                    ResetAttack();
                    //    NPC.noGravity = false;
                    //}

                    break;

                case (int)PhaseEnum.GauntletPhase:

                    SetPhase(PhaseEnum.ReturnToForeground);
                    ResetAttack();

                    break;                
                
                case (int)PhaseEnum.ReturnToForeground:

                    SetPhase(PhaseEnum.DirectPhase);
                    ResetAttack();

                    break;

                case (int)PhaseEnum.DirectPhase:

                    NPC.noGravity = false;
                    NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.33f);

                    const int maxAttacks = 7;

                    if (AttackTimer == 1)
                    {
                        AttackPhase++;

                        if (AttackPhase > maxAttacks) 
                            AttackPhase = 0;

                        attackVariant = Main.rand.NextBool();
                        NPC.netUpdate = true;
                    }

                    switch (AttackPhase)
                    {
                        //case 0: Spears(); break;//slash
                        //case 1: Spears(); break;//thrust
                        //case 2: if (attackVariant) Hammer(); else HammerVariant(); break;
                        //case 3: Spears(); break;
                        //case 4: Spears(); break;//slash or bigger variant
                        //case 5: BigBrightBubble(); break;//spin
                        //case 6: if (attackVariant) Hammer(); else HammerVariant(); break;
                        //case 7: Spears(); break;
                        default: if (attackVariant) Hammer(); else HammerVariant(); break;
                    }

                    break;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(attackVariant);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            attackVariant = reader.ReadBoolean();
        }

        //i hate this specific thing right here
        public override ModNPC Clone(NPC npc)
        {
            var newNPC = base.Clone(npc) as GlassMiniboss;
            newNPC.moveTarget = new Vector2();
            newNPC.moveStart = new Vector2();
            newNPC.attackVariant = false;
            newNPC.hammerIndex = -1;
            newNPC.bubbleIndex = -1;
            return newNPC;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> weaver = Request<Texture2D>(AssetDirectory.GlassMiniboss + "Glassweaver");
            Asset<Texture2D> weaverGlow = Request<Texture2D>(AssetDirectory.GlassMiniboss + "GlassweaverGlow");

            Rectangle frame = weaver.Frame(1, 5, 0, 0);
            frame.X = 0;
            frame.Width = 144;

            const int frameHeight = 152;

            Vector2 origin = frame.Size() * new Vector2(0.5f, 0.5f);
            Vector2 drawPos = new Vector2(0, -35) - Main.screenPosition;

            switch (Phase)
            {
                case (int)PhaseEnum.DirectPhase:

                    switch (AttackType)
                    {
                        case (int)AttackEnum.Jump:

                            float jumpProgress = Utils.GetLerpValue(jumpStart, jumpEnd, AttackTimer, true);
                            if (jumpProgress < 0.66f || Math.Abs(NPC.velocity.Y) > 1f)
                                frame.Y = frameHeight;

                            break;

                        case (int)AttackEnum.SpinJump:

                            frame.Y = frameHeight * 4;

                            break;

                        case (int)AttackEnum.Spears:

                            if (AttackTimer < spearTime - spearSpawn + 10)
                                frame.Y = frameHeight * 2;
                            break;

                        case (int)AttackEnum.Hammer:

                            float hammerTimer = AttackTimer - hammerSpawn + 5;

                            if (hammerTimer <= hammerTime + 55 && AttackTimer > 50)
                            {
                                frame.X = 144;
                                frame.Width = 180;

                                if (hammerTimer <= hammerTime * 0.87f)
                                {
                                    frame.Y = 0;
                                    bool secFrame = (hammerTimer >= hammerTime * 0.33f) && (hammerTimer < hammerTime * 0.66f);
                                    if (secFrame)
                                        frame.Y = frameHeight;
                                }
                                else
                                {
                                    float swingTime = Utils.GetLerpValue(hammerTime * 0.87f, hammerTime * 0.98f, hammerTimer, true);
                                    frame.Y = frameHeight + (frameHeight * (int)(1f + (swingTime * 2f)));
                                }
                            }
                            break;

                        case (int)AttackEnum.BigBrightBubble:

                            if (AttackTimer > 50)
                            {
                                if (AttackTimer < 330)
                                    frame.Y = frameHeight * 3;
                                else if (AttackTimer < bubbleRecoil - 60)
                                    frame.Y = frameHeight;
                                else if (AttackTimer < bubbleRecoil + 10)
                                    frame.Y = frameHeight * 4;
                            }

                            break;
                    }

                    break;
            }

            Main.EntitySpriteDraw(weaver.Value, NPC.Center + drawPos, frame, drawColor, NPC.rotation, origin, NPC.scale, GetSpriteEffects(), 0);
            Main.EntitySpriteDraw(weaverGlow.Value, NPC.Center + drawPos, frame, new Color(255, 255, 255, 128), NPC.rotation, origin, NPC.scale, GetSpriteEffects(), 0);


            return false;
        }

        private SpriteEffects GetSpriteEffects() => NPC.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None; 
    }
}

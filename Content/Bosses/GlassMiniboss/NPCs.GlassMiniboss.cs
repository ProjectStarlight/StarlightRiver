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
        bool attackVariant = false;
        bool attackLowHPVariant => NPC.life <= NPC.lifeMax * 0.5f;

        internal ref float Phase => ref NPC.ai[0];
        internal ref float GlobalTimer => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
        internal ref float AttackTimer => ref NPC.ai[3];
        internal ref float AttackType => ref NPC.localAI[0];

        public Vector2 arenaPos;

        //Phase tracking utils
        public enum PhaseEnum
        {
            SpawnEffects = 0,
            JumpToBackground = 1,
            GauntletPhase = 2,
            ReturnToForeground = 3,
            DirectPhase = 4,
            DeathAnimation = 5
        }

        public enum AttackEnum
        {
            None = 0,
            Slash = 1,
            Spinjitzu = 2,
            Spears = 3,
            Hammer = 4,
            BigBrightBubble = 5,
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glassweaver"); 
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
        }

        public override string Texture => AssetDirectory.GlassMiniboss + Name;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false; //no contact damage!

        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 92;
            NPC.lifeMax = 1500;
            NPC.damage = 20;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.knockBackResist = 0;
            NPC.boss = true;
            NPC.defense = 14;
            Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Miniboss");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(2000 * bossLifeScale);
        }

        public override bool CheckDead()
        {
            StarlightWorld.Flag(WorldFlags.DesertOpen);
            return true;
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

                    if (AttackTimer == 1)
                    {
                        AttackPhase++;

                        if (AttackPhase > 3) 
                            AttackPhase = 0;

                        attackVariant = Main.rand.NextBool();
                        NPC.netUpdate = true;
                    }

                    switch (AttackPhase)
                    {
                        case 0: BigBrightBubble(); break;
                        //case 1: if (attackVariant) Hammer(); else HammerVariant(); break;
                        case 1: BigBrightBubble(); break;
                        case 2: BigBrightBubble(); break;
                        case 3: HammerVariant(); break;
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

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> weaver = Request<Texture2D>(AssetDirectory.GlassMiniboss + "GlassMiniboss");
            Asset<Texture2D> weaverGlow = Request<Texture2D>(AssetDirectory.GlassMiniboss + "GlassMinibossGlow");

            Rectangle frame = weaver.Frame(1, 5, 0, 0);
            frame.Width = 136;

            switch (Phase)
            {
                case (int)PhaseEnum.DirectPhase:


                    switch (AttackType)
                    {
                        case (int)AttackEnum.Spears:

                            if (AttackTimer > 10 && AttackTimer < spearTime - spearSpawn)
                                frame.Y = 300;
                            break;

                        case (int)AttackEnum.Hammer:

                            float hammerTimer = AttackTimer - hammerSpawn + 5;
                            if (AttackTimer < 61)
                                frame.Y = 150;
                            else if (hammerTimer <= hammerTime + 60)
                            {
                                frame.X = 138;
                                frame.Width = 180;

                                if (hammerTimer <= hammerTime * 0.87f)
                                {
                                    frame.Y = 0;
                                    bool secFrame = (hammerTimer >= hammerTime * 0.33f) && (hammerTimer < hammerTime * 0.66f);
                                    if (secFrame)
                                        frame.Y = 150;
                                }
                                else
                                {
                                    float swingTime = Utils.GetLerpValue(hammerTime * 0.87f, hammerTime * 0.98f, hammerTimer, true);
                                    frame.Y = 150 + (150 * (int)(1f + (swingTime * 2f)));
                                }
                            }
                            break;
                    }

                    break;
            }

            Vector2 origin = frame.Size() * new Vector2(0.5f, 1f);
            Vector2 drawPos = new Vector2(0, 48) - Main.screenPosition;

            Main.EntitySpriteDraw(weaver.Value, NPC.Center + drawPos, frame, drawColor, NPC.rotation, origin, NPC.scale, GetSpriteEffects(), 0);
            Main.EntitySpriteDraw(weaverGlow.Value, NPC.Center + drawPos, frame, new Color(255, 255, 255, 128), NPC.rotation, origin, NPC.scale, GetSpriteEffects(), 0);


            return false;
        }

        private SpriteEffects GetSpriteEffects() => NPC.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None; 
    }
}

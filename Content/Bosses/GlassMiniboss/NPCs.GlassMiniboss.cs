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

        internal ref float Phase => ref NPC.ai[0];
        internal ref float GlobalTimer => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
        internal ref float AttackTimer => ref NPC.ai[3];
        internal ref float AttackType => ref NPC.localAI[0];

        public static Vector2 spawnPos => StarlightWorld.VitricBiome.TopLeft() * 16 + new Vector2(1 * 16, 76 * 16);

        //Phase tracking utils
        public enum PhaseEnum
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            GauntletPhase = 2,
            ComeBack = 3,
            DirectPhase = 4,
            DeathAnimation = 5
        }

        public enum AttackEnum
        {
            None = 0,
            Spears = 1,
            Hammer = 2,
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
            Music = SoundLoader.GetSoundSlot(Mod, "Sounds/Music/Miniboss");
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

                    ResetAttack();

                    //UILoader.GetUIState<TextCard>().Display("Glassweaver", "the", null, 240, 1, true);

                    SetPhase(PhaseEnum.SpawnAnimation);

                    break;

                case (int)PhaseEnum.SpawnAnimation:

                    //if (AttackTimer <= 90) 
                    //    SpawnAnimation();

                    //else
                    //{
                    SetPhase(PhaseEnum.DirectPhase);
                    ResetAttack();
                    //    NPC.noGravity = false;
                    //}

                    break;

                case (int)PhaseEnum.GauntletPhase:



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
                        case 0: Spears(); break;
                        case 1: Hammer(); break;
                        case 2: Spears(); break;
                        case 3: Hammer(); break;
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
            Asset<Texture2D> weaverSolid = Request<Texture2D>(AssetDirectory.GlassMiniboss + "GlassMinibossSolid");
            Asset<Texture2D> weaverGlow = Request<Texture2D>(AssetDirectory.GlassMiniboss + "GlassMinibossGlow");

            Rectangle frame = weaver.Frame(1, 4, 0, 0);
            frame.Width = 136;
            float trailOpacity = 0f;

            switch (Phase)
            {
                case (int)PhaseEnum.DirectPhase:

                    switch (AttackType)
                    {
                        case (int)AttackEnum.Spears:

                            if (AttackTimer > 10)
                                frame.Y = 300;
                            trailOpacity = 0.4f;
                            break;
                        case (int)AttackEnum.Hammer:
                            if (AttackTimer < 40)
                            {
                                frame.Y = 150;
                            }
                            else
                            {
                                frame.X = 138;
                                frame.Width = 180;

                                int swing = 0;
                                frame.Y = 150 * swing;
                            }
                            break;
                    }

                    break;
            }

            Vector2 origin = frame.Size() * new Vector2(0.5f, 1f);
            Vector2 drawPos = new Vector2(0, 48) - Main.screenPosition;

            for (int i = 0; i < 10; i++)
            {
                Color trailColor = new Color(230, 60, 16, 0) * trailOpacity * ((10 - i) / 10f);
                float scale = 0.8f + (((10 - i) / 10f) * 0.2f);
                Main.EntitySpriteDraw(weaverSolid.Value, NPC.oldPos[i] + (NPC.Size * 0.5f) + drawPos, frame, trailColor, NPC.oldRot[i], origin, NPC.scale * scale, GetSpriteEffects(), 0);
            }
            Main.EntitySpriteDraw(weaver.Value, NPC.Center + drawPos, frame, drawColor, NPC.rotation, origin, NPC.scale, GetSpriteEffects(), 0);
            Main.EntitySpriteDraw(weaverGlow.Value, NPC.Center + drawPos, frame, new Color(255, 255, 255, 128), NPC.rotation, origin, NPC.scale, GetSpriteEffects(), 0);
            
            //switch (Phase)
            //{
            //    case (int)PhaseEnum.DirectPhase:

            //        switch (AttackType)
            //        {
            //        }

            //        break;
            //}

            return false;
        }

        private SpriteEffects GetSpriteEffects() => NPC.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None; 
    }
}

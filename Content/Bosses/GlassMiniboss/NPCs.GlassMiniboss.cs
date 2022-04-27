using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	internal partial class GlassMiniboss : ModNPC
    {
        bool attackVariant = false;

        internal ref float Phase => ref NPC.ai[0];
        internal ref float GlobalTimer => ref NPC.ai[1];
        internal ref float AttackPhase => ref NPC.ai[2];
        internal ref float AttackTimer => ref NPC.ai[3];

        public static Vector2 spawnPos => StarlightWorld.VitricBiome.TopLeft() * 16 + new Vector2(1 * 16, 76 * 16);

        //Animation tracking utils
        private int Frame
        {
            set => NPC.frame.Y = value * 128;
        }

        //Phase tracking utils
        public enum PhaseEnum
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            GauntletPhase = 2,
            DirectPhase = 3,
            DeathAnimation = 4
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glassweaver");

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

                    if (AttackTimer <= 90) 
                        SpawnAnimation();

                    else
                    {
                        SetPhase(PhaseEnum.DirectPhase);
                        ResetAttack();
                        NPC.noGravity = false;
                    }

                    break;

                case (int)PhaseEnum.GauntletPhase:



                    break;

                case (int)PhaseEnum.DirectPhase:

                    if (AttackTimer == 1)
                    {
                        AttackPhase++;

                        if (AttackPhase > 2) 
                            AttackPhase = 0;

                        attackVariant = Main.rand.NextBool();
                        NPC.netUpdate = true;
                    }

                    switch (AttackPhase)
                    {
                        case 0: Spears(); break;
                        case 1: Spears(); break;
                        case 2: Spears(); break;
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

            return false;
        }
    }
}

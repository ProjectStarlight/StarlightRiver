using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using static Terraria.ModLoader.ModContent;
using System.IO;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    internal partial class GlassMiniboss : ModNPC
    {
        bool attackVariant = false;

        internal ref float GlobalTimer => ref npc.ai[0];
        internal ref float Phase => ref npc.ai[1];
        internal ref float AttackPhase => ref npc.ai[2];
        internal ref float AttackTimer => ref npc.ai[3];

        Vector2 spawnPos => StarlightWorld.VitricBiome.TopLeft() * 16 + new Vector2( -9.5f * 16, 76 * 16);

        public enum PhaseEnum
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            FirstPhase = 2,
            DeathAnimation = 3
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glassweaver");

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false; //no contact damage! this is strictly a GOOD GAME DESIGN ONLY ZONE!!!

        public override void SetDefaults()
        {
            npc.width = 64;
            npc.height = 64;
            npc.lifeMax = 1500;
            npc.damage = 20;
            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.knockBackResist = 0;
            npc.boss = true;
            npc.defense = 14;
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/Miniboss");
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(2000 * bossLifeScale);
        }

        public override bool CheckDead()
        {
            NPC.NewNPC((StarlightWorld.VitricBiome.X - 10) * 16, (StarlightWorld.VitricBiome.Center.Y + 12) * 16, NPCType<GlassweaverTown>());
            StarlightWorld.DesertOpen = true;
            Main.NewText("The temple doors slide open...", new Color(200, 170, 80));
            return true;
        }

        private void SetPhase(PhaseEnum phase) => Phase = (float)phase;

        public override void AI()
        {
            AttackTimer++;

            switch (Phase)
            {
                case (int)PhaseEnum.SpawnEffects:

                    ResetAttack();

                    StarlightRiver.Instance.textcard.Display("Glassweaver", "the", null, 240, 1, true);

                    SetPhase(PhaseEnum.SpawnAnimation);

                    break;

                case (int)PhaseEnum.SpawnAnimation:

                    if (AttackTimer < 300) SpawnAnimation();
                    else
                    {
                        SetPhase(PhaseEnum.FirstPhase);
                        ResetAttack();
                        npc.noGravity = false;
                    }

                    break;

                case (int)PhaseEnum.FirstPhase:

                    npc.spriteDirection = npc.Center.X > Target.Center.X ? 1 : -1;

                    if (AttackTimer == 1)
                    {
                        AttackPhase++;
                        if (AttackPhase > 8) AttackPhase = 0;

                        attackVariant = Main.rand.NextBool();
                        npc.netUpdate = true;
                    }

                    switch (AttackPhase)
                    {
                        case 0: Spears(); break;
                        case 1: Knives(); break;
                        case 2: UppercutGlide(); break;
                        case 3: Idle(60); break;
                        case 4: Hammer(); break;
                        case 5: Knives(); break;
                        case 6: SlashUpSlash(); break;
                        case 7: Idle(90); break;
                        case 8: if (attackVariant) SlashUpSlash(); else UppercutGlide(); break;

                        //case 12: Greatsword(); break;
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
    }
}

using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Miniboss.Glassweaver
{
    internal partial class GlassMiniboss : ModNPC
    {
        internal ref float PathingTimer => ref npc.ai[0];
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
            npc.lifeMax = 2500;
            npc.damage = 20;
            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.knockBackResist = 0;
            npc.boss = true;
            npc.defense = 14;
        }

        public override bool CheckDead()
        {
            NPC.NewNPC((StarlightWorld.VitricBiome.X - 10) * 16, (StarlightWorld.VitricBiome.Center.Y + 12) * 16, NPCType<GlassweaverTown>());
            StarlightWorld.DesertOpen = true;
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
                    targetRectangle = RegionCenter;
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

                    if(npc.velocity.X != 0) npc.spriteDirection = npc.velocity.X < 0 ? 1 : -1;

                    if (AttackTimer == 1)
                    {
                        AttackPhase++;
                        if (AttackPhase > 1) AttackPhase = 0;
                    }

                    switch (AttackPhase)
                    {
                        case 0: if (GetRegion(npc) == RegionCenter) HammerSlam(); else SummonKnives(); break;
                        case 1: PathToTarget(); break;

                        case 2: SummonKnives(); break;
                        case 3: PathToTarget(); break;

                        case 4: SlashCombo(); break;
                        case 5: PathToTarget(); break;

                        case 6: SummonKnives(); break;
                        case 7: PathToTarget(); break;
                    }

                    //pathing updates
                    for (int k = 0; k < pads.Length; k++) pads[k].Update();

                    Main.NewText(npc.noTileCollide);

                    break;
            }
        }
    }
}

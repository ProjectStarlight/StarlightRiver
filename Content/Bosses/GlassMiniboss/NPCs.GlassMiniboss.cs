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

        internal ref float GlobalTimer => ref npc.ai[0];
        internal ref float Phase => ref npc.ai[1];
        internal ref float AttackPhase => ref npc.ai[2];
        internal ref float AttackTimer => ref npc.ai[3];

        private float spinAngle = 0;
        private float glowStrength = 0.25f;

        public static Vector2 spawnPos => StarlightWorld.VitricBiome.TopLeft() * 16 + new Vector2(1 * 16, 76 * 16);

        //Animation tracking utils
        private int Frame
        {
            set => npc.frame.Y = value * 128;
        }

        //Phase tracking utils
        public enum PhaseEnum
        {
            SpawnEffects = 0,
            SpawnAnimation = 1,
            FirstPhase = 2,
            DeathAnimation = 3
        }

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glassweaver");

        public override string Texture => AssetDirectory.GlassMiniboss + Name;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false; //no contact damage!

        public override void SetDefaults()
        {
            npc.width = 110;
            npc.height = 92;
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
                        SetPhase(PhaseEnum.FirstPhase);
                        ResetAttack();
                        npc.noGravity = false;
                    }

                    break;

                case (int)PhaseEnum.FirstPhase:

                    if (AttackTimer == 1)
                    {
                        AttackPhase++;

                        if (AttackPhase > 2) 
                            AttackPhase = 0;

                        attackVariant = Main.rand.NextBool();
                        npc.netUpdate = true;
                    }

                    switch (AttackPhase)
                    {
                        case 0: Knives(); break;
                        case 1: Hammer(); break;
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

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            npc.frame.Width = 110;
            npc.frame.Height = 92;

            Vector2 offset = new Vector2(0, 16);

            if (spinAngle != 0)
            {
                float sin = (float)Math.Sin(spinAngle + 1.57f * npc.direction);
                int off = Math.Abs((int)(npc.frame.Width * sin));

                SpriteEffects effect = sin > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                spriteBatch.Draw(GetTexture(Texture), 
                    new Rectangle((int)(npc.position.X - Main.screenPosition.X - off / 2 + npc.width / 2), 
                    (int)(npc.position.Y - Main.screenPosition.Y - 64), off, npc.frame.Height),
                    npc.frame, drawColor, 0, Vector2.Zero, effect, 0);
            }

            else
            {
                spriteBatch.Draw(GetTexture(Texture), npc.Center - Main.screenPosition - offset, npc.frame, drawColor, 0, npc.frame.Size() / 2, npc.scale, npc.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
                spriteBatch.Draw(GetTexture(Texture + "Glow"), npc.Center - Main.screenPosition - offset, npc.frame, Color.White * glowStrength, 0, npc.frame.Size() / 2, npc.scale, npc.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }

            if (glowStrength > 0.2f)
                Lighting.AddLight(npc.Center, new Vector3(1, 0.75f, 0.2f) * (glowStrength - 0.2f));

            return false;
        }
    }
}

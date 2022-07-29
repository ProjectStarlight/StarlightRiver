using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.Collections.Generic;

using Terraria.DataStructures;
using Terraria.GameContent;

using Terraria.Audio;

using System;
using System.Linq;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
    internal class JuggernautConstruct : ModNPC, IHealableByHealerConstruct 
    {
        public override string Texture => AssetDirectory.GauntletNpc + "JuggernautConstruct";

        private Player target => Main.player[NPC.target];

        private const int XFRAMES = 3; //TODO: Swap to using NPC.Frame
        private readonly float ACCELERATION = 0.2f;
        private readonly float MAXSPEED = 2;

        public bool ableToDoCombo = true;

        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        private bool attacking = false;
        private int attackCooldown = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Juggernaut Construct");
            Main.npcFrameCount[NPC.type] = 23;
        }

        public override void SetDefaults()
        {
            NPC.width = 104;
            NPC.height = 82;
            NPC.damage = 30;
            NPC.defense = 5;
            NPC.lifeMax = 250;
            NPC.value = 10f;
            NPC.knockBackResist = 0.6f;
            NPC.HitSound = SoundID.Item27 with
            {
                Pitch = -0.3f
            };           
            NPC.DeathSound = SoundID.Shatter;
        }

        public override void AI()
        {
            NPC.TargetClosest(false);
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            attackCooldown--;

            WalkingBehavior();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            SpriteEffects effects = SpriteEffects.None;

            Texture2D mainTex = Request<Texture2D>(Texture).Value;
            Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

            int frameHeight = mainTex.Height / Main.npcFrameCount[NPC.type];
            int frameWidth = mainTex.Width / XFRAMES;

            Rectangle frameBox = new Rectangle(xFrame * frameWidth, (yFrame * frameHeight), frameWidth, frameHeight);

            Vector2 origin = new Vector2(frameWidth * 0.25f, (frameHeight * 0.5f) + 3);

            if (NPC.spriteDirection != 1)
            {
                effects = SpriteEffects.FlipHorizontally;
                origin.X = frameWidth - origin.X;
            }

            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);
            Main.spriteBatch.Draw(mainTex, slopeOffset + NPC.Center - screenPos, frameBox, drawColor, NPC.rotation, origin, NPC.scale * 2, effects, 0f);
            Main.spriteBatch.Draw(glowTex, slopeOffset + NPC.Center - screenPos, frameBox, Color.White, NPC.rotation, origin, NPC.scale * 2, effects, 0f);

            return false;
        }

        public override void OnKill()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 9; i++)
                    Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(3, 3), 0, new Color(255, 150, 50), Main.rand.NextFloat(0.75f, 1.25f)).noGravity = false;

                for (int k = 1; k <= 12; k++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3,3), Mod.Find<ModGore>("ConstructGore" + k).Type);
                for (int j = 1; j <= 3; j++)
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("GruntSwordGore" + j).Type);
            }
        }

        private void WalkingBehavior()
        {
            float xDir = target.Center.X - NPC.Center.X;
            int xSign = Math.Sign(xDir);

            xFrame = 2;
            frameCounter++;

            if (frameCounter > 3)
            {
                frameCounter = 0;
                yFrame++;
                yFrame %= 12;
            }

            NPC.velocity.X += ACCELERATION * xSign;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAXSPEED, MAXSPEED);
            NPC.spriteDirection = xSign;
        }

        public void DrawHealingGlow(SpriteBatch spriteBatch)
        {

        }
    }
}
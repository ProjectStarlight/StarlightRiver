using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
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
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader.Utilities;

namespace StarlightRiver.Content.NPCs.Forest
{
    internal class Blover : ModNPC
    {
        public override string Texture => AssetDirectory.ForestNPC + "Blover";

        private const int XFRAMES = 2;

        private int xFrame = 0;
        private int yFrame = 0;
        private int frameCounter = 0;

        private bool blowing = false;

        private Player target => Main.player[NPC.target];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Blover");
            Main.npcFrameCount[NPC.type] = 6;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        }

        public override void Load()
        {
            for (int j = 1; j <= 4; j++)
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.ForestNPC + "BloverGore" + j);
        }

        public override void SetDefaults()
        {
            NPC.width = 38;
            NPC.height = 44;
            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 60;
            NPC.value = 100f;
            NPC.knockBackResist = 0f;
            NPC.HitSound = SoundID.Grass;
            NPC.DeathSound = SoundID.Grass;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!Main._shouldUseWindyDayMusic)
                return 0;
            return SpawnCondition.OverworldDay.Chance * 0.2f;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("[PH] It's a blover! CUUUUUUUUUTE")
            });
        }

        public override void AI()
        {
            NPC.TargetClosest(true);

            if (Math.Abs(target.Center.X - NPC.Center.X) < 300 && Math.Abs(target.Center.Y - NPC.Center.Y) < 30)
            {
                if (!blowing)
                {
                    blowing = true;
                    yFrame = 0;
                    xFrame = 1;
                    frameCounter = 0;
                }
            }
            else if (blowing)
            {
                blowing = false;
                yFrame = 0;
                xFrame = 0;
                frameCounter = 0;
            }

            if (blowing)
                BlowingBehavior();
            else
                IdleBehavior();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = Request<Texture2D>(Texture).Value;

            SpriteEffects effects = SpriteEffects.None;
            Vector2 origin = new Vector2(NPC.width / 2, (NPC.height / 2) - 2);

            if (NPC.spriteDirection != 1)
                effects = SpriteEffects.FlipHorizontally;

            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);
            Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameWidth = NPC.width;
            NPC.frame = new Rectangle(frameWidth * xFrame, frameHeight * yFrame, frameWidth, frameHeight);
        }

        public override void OnKill()
        {
            if(Main.netMode != NetmodeID.Server)
            {
                for (int j = 1; j <= 4; j++)
                {
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("BloverGore" + j).Type);
                    Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), GoreID.TreeLeaf_Normal);
                }
            }
        }

        private void BlowingBehavior()
        {
            xFrame = 1;
            frameCounter++;

            if (frameCounter % 3 == 0)
            {
                yFrame++;
                yFrame %= 6;
            }

            float targetAcceleration = Math.Sign(target.Center.X - NPC.Center.X) * (float)((300 - Math.Abs(target.Center.X - NPC.Center.X)) / 300f) * 0.55f;
            
            if (Math.Abs(target.velocity.X) < 10 || Math.Sign(target.velocity.X) != Math.Sign(targetAcceleration))
                target.velocity.X += targetAcceleration;

            Vector2 dustPos = NPC.Center + new Vector2(60 * Math.Sign(target.Center.X - NPC.Center.X), Main.rand.Next(-15, 15));
            Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.GlowLine>(), 7 * new Vector2(Math.Sign(target.Center.X - NPC.Center.X), 0), 0, Color.White * 0.3f, 1.25f);
        }

        private void IdleBehavior()
        {
            xFrame = 0;
            frameCounter++;

            if (frameCounter % 5 == 0)
            {
                yFrame++;
                yFrame %= 5;
            }
        }
    }
}
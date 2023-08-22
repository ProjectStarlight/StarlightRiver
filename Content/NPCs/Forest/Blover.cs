﻿using System;
using System.Linq;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.Utilities;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Forest
{
	internal class Blover : ModNPC
	{
		private int xFrame = 0;
		private int yFrame = 0;
		private int frameCounter = 0;

		private bool blowing = false;

		private ref float BlowCounter => ref NPC.ai[0];

		private Player Target => Main.player[NPC.target];

		public override string Texture => AssetDirectory.ForestNPC + "Blover";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blover");
			Main.npcFrameCount[NPC.type] = 6;

			var drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0);
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
				new FlavorTextBestiaryInfoElement("Blovers are clover-based plants that harness the ambient mana during particularly windy days to spin their fan-shaped leaves at approaching creatures. They are immobile, and would serve as a good form of defense if one were to discover a way to move or perhaps plant one.")
			});
		}

		public override void AI()
		{
			NPC.TargetClosest(true);

			if (Math.Abs(Target.Center.X - NPC.Center.X) < 300 && Math.Abs(Target.Center.Y - NPC.Center.Y) < 30)
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
				BlowCounter++;
			else
				BlowCounter = 0;

			if (BlowCounter > 15)
				BlowingBehavior();
			else
				IdleBehavior();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = Request<Texture2D>(Texture).Value;

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(NPC.width / 2, NPC.height / 2 - 2);

			if (NPC.spriteDirection != 1)
				effects = SpriteEffects.FlipHorizontally;

			var slopeOffset = new Vector2(0, NPC.gfxOffY);
			Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

			return false;
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = NPC.width;
			NPC.frame = new Rectangle(frameWidth * xFrame, frameHeight * yFrame, frameWidth, frameHeight);
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
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

			float targetAcceleration = Math.Sign(Target.Center.X - NPC.Center.X) * (float)((300 - Math.Abs(Target.Center.X - NPC.Center.X)) / 300f) * 0.55f;

			if (!Target.noKnockback && (Math.Abs(Target.velocity.X) < 10 || Math.Sign(Target.velocity.X) != Math.Sign(targetAcceleration)))
				Target.velocity.X += targetAcceleration;

			Vector2 dustPos = NPC.Center + new Vector2(60 * Math.Sign(Target.Center.X - NPC.Center.X), Main.rand.Next(-15, 15));
			Dust.NewDustPerfect(dustPos, ModContent.DustType<Dusts.GlowLine>(), 7 * new Vector2(Math.Sign(Target.Center.X - NPC.Center.X), 0), 0, Color.White * 0.3f, 1.25f);

			NPC npcTarget = Main.npc.Where(n => n.active && n.knockBackResist > 0 && Math.Abs(n.Center.X - NPC.Center.X) < 300 && Math.Abs(n.Center.Y - NPC.Center.Y) < 30 && Math.Sign(n.Center.X - NPC.Center.X) == NPC.direction).OrderBy(n => n.Distance(NPC.Center)).FirstOrDefault();
			if (npcTarget != default)
			{
				float npcTargetAcceleration = Math.Sign(npcTarget.Center.X - NPC.Center.X) * (float)((300 - Math.Abs(npcTarget.Center.X - NPC.Center.X)) / 300f) * 0.55f * npcTarget.knockBackResist;

				if (Math.Abs(npcTarget.velocity.X) < 10 || Math.Sign(npcTarget.velocity.X) != Math.Sign(npcTargetAcceleration))
					npcTarget.velocity.X += npcTargetAcceleration;
			}
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
//TODO on firebug:
//Bestiary
//Sound effects
//Balance
//Money dropping
//Magma charging
//Spiraling on death
//Drops

//TODO on lesser firebug
//Bestiary
//Sound effects
//Balance
//No mney dropping
//Explosion effect
using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.IO;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	internal class BoomBug : ModNPC
	{
		private int yFrame = 0;

		private int bugTimer = 0;

		private bool chargingMagma = false;

		private Player Target => Main.player[NPC.target];

		public override string Texture => AssetDirectory.VitricNpc + Name;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 3;
			DisplayName.SetDefault("Firebug");
		}

		public override void SetDefaults()
		{
			NPC.width = 34;
			NPC.height = 40;
			NPC.knockBackResist = 1.5f;
			NPC.lifeMax = 150;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath4;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("[PH] Entry")
			});
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			NPC.spriteDirection = NPC.direction;
			if (chargingMagma)
			{

			}
			else
			{
				if (bugTimer++ % 190 == 0)
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<LesserFirebug>(), 0, NPC.whoAmI);
				if (TileGapDown() < 15 && TileGapUp() > 5)
					NPC.velocity.Y -= 0.1f;
				else
					NPC.velocity.Y += 0.1f;
				NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -4, 4);

				NPC.velocity.X += Math.Sign(Target.Center.X - NPC.Center.X) * 0.1f;
				NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -6, 6);
			}
		}
		public override void FindFrame(int frameHeight)
		{
			if (!chargingMagma)
				NPC.frameCounter++;

			if (NPC.frameCounter % 4 == 0)
				yFrame++;

			yFrame %= Main.npcFrameCount[NPC.type];
			NPC.frame = new Rectangle(0, frameHeight * yFrame, NPC.width, frameHeight);
		}


		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return spawnInfo.Player.InModBiome(GetInstance<VitricDesertBiome>()) ? 100 : 0;
		}

		private int TileGapDown()
		{
			int i = 0;
			for (; i < 50; i++)
			{
				int x = (int)(NPC.Center.X / 16);
				int y = (int)(NPC.Center.Y / 16);
				Tile tile = Framing.GetTileSafely(x, y + i);
				if (tile.HasTile && Main.tileSolid[tile.TileType])
					break;
			}
			return i;
		}

		private int TileGapUp()
		{
			int i = 0;
			for (; i < 50; i++)
			{
				int x = (int)(NPC.Center.X / 16);
				int y = (int)(NPC.Center.Y / 16);
				Tile tile = Framing.GetTileSafely(x, y - i);
				if (tile.HasTile && Main.tileSolid[tile.TileType])
					break;
			}
			return i;
		}
	}

	internal class LesserFirebug : ModNPC
	{
		private int yFrame = 0;

		private bool parentless = false;

		private Player Target => Main.player[NPC.target];

		private NPC Parent => Main.npc[(int)NPC.ai[0]];

		public override string Texture => AssetDirectory.VitricNpc + Name;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 2;
			DisplayName.SetDefault("Lesser Firebug");
		}

		public override void SetDefaults()
		{
			NPC.width = 16;
			NPC.height = 16;
			NPC.knockBackResist = 1.5f;
			NPC.lifeMax = 5;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath4;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("[PH] Entry")
			});
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			NPC.spriteDirection = NPC.direction;

			if (NPC.Distance(Target.Center) < 600)
				NPC.velocity = NPC.DirectionTo(Target.Center) * 8;
			else if (Parent.active && !parentless)
				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Parent.Center) * 6, 0.1f);
			else
				parentless = true;

			if (NPC.collideX || NPC.collideY)
				NPC.Kill();
		}
		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;

			if (NPC.frameCounter % 4 == 0)
				yFrame++;

			yFrame %= Main.npcFrameCount[NPC.type];
			NPC.frame = new Rectangle(0, frameHeight * yFrame, NPC.width, frameHeight);
		}
	}
}
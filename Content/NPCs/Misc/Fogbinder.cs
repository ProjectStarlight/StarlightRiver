using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.Utilities;
using Terraria.GameContent.Bestiary;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Misc
{
	class Fogbinder : ModNPC
	{
		private int frameCounter = 0;
		private int yFrame = 0;

		private float deathTimer = 0;

		private float bobTimer = 0f;

		public List<NPC> targets = new();

		public Player player => Main.player[NPC.target];

		public override string Texture => AssetDirectory.MiscNPC + Name;

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscNPC + "Fogbinder_Chain");
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Fogbinder");
			Main.npcFrameCount[NPC.type] = 4;
		}

		public override void SetDefaults()
		{
			NPC.width = 56;
			NPC.height = 114;
			NPC.knockBackResist = 0.1f;
			NPC.lifeMax = 100;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 1;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("They call it... The fogbinder. Appearing during thunderstorms, this being is very Spooky, Demented, Demonic, Hellish, and Evil.")
			});
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false; //harmless
		}

		public override void AI()
		{
			Dust.NewDustPerfect(NPC.Center + new Vector2(0,50), ModContent.DustType<Dusts.Mist>(), new Vector2(0, -0.88f).RotatedByRandom(0.15f), 0, Color.White, 0.35f);

			foreach (NPC target in targets)
			{
				float distanceToTarget = (target.Center - NPC.Center).Length();

				Vector2 directionToTarget = Vector2.Normalize(target.Center - NPC.Center);

				for (float i = 0; i < distanceToTarget; i += 10)
				{
					if (Main.rand.NextBool(60))
					{
						Vector2 pos = Vector2.Lerp(NPC.Center, target.Center, i / distanceToTarget);
						Dust.NewDustPerfect(pos + new Vector2(0, 20), ModContent.DustType<Dusts.Mist>(), new Vector2(0, -0.28f).RotatedByRandom(0.3f), 0, Color.White, 0.25f);
					}
				}
			}

			NPC.TargetClosest(true);
			NPC.spriteDirection = NPC.direction;

			bobTimer += 0.05f;
			NPC.velocity = new Vector2(0, 0.2f * (float)System.MathF.Sin(bobTimer));

			targets = Main.npc.Where(n => n.active && n.knockBackResist > 0 && n.Distance(NPC.Center) < 500 && n.type != NPC.type && !n.townNPC).ToList();
			targets.ForEach(n => n.GetGlobalNPC<FogbinderGNPC>().fogbinder = NPC);

			if (Main.player.Any(n => n.active && !n.dead && n.Hitbox.Intersects(NPC.Hitbox)))
			{
				deathTimer += 0.01f;
				if (deathTimer > 1)
				{
					SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);
					NPC.active = false;
					foreach (NPC target in targets)
					{
						float distanceToTarget = (target.Center - NPC.Center).Length();

						Vector2 directionToTarget = Vector2.Normalize(target.Center - NPC.Center);

						for (float i = 0; i < distanceToTarget; i += 14)
						{
							Vector2 pos = Vector2.Lerp(NPC.Center, target.Center, i / distanceToTarget);
							Gore.NewGoreDirect(NPC.GetSource_Death(), pos, Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("Fogbinder_Chain").Type);
						}
					}
				}
			}
			else
			{
				deathTimer = 0;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			frameCounter++;
			if (frameCounter % 5 == 0)
				yFrame++;
			yFrame %= Main.npcFrameCount[NPC.type];

			int frameWidth = NPC.width;
			NPC.frame = new Rectangle(0, frameHeight * yFrame, frameWidth, frameHeight);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = Request<Texture2D>(Texture).Value;
			Texture2D glowTexture = Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D chainTex = Request<Texture2D>(Texture + "_Chain").Value;

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(NPC.width / 2, NPC.height / 2);

			if (NPC.spriteDirection != 1)
				effects = SpriteEffects.FlipHorizontally;

			var slopeOffset = new Vector2(0, NPC.gfxOffY);

			foreach (NPC target in targets)
			{
				float distanceToTarget = (target.Center - NPC.Center).Length();

				Vector2 directionToTarget = Vector2.Normalize(target.Center - NPC.Center);

				for (float i = 0; i < distanceToTarget; i+= chainTex.Width + 4)
				{
					Vector2 pos = Vector2.Lerp(NPC.Center, target.Center, i / distanceToTarget);
					Color lightColor = Lighting.GetColor((int)(pos.X / 16), (int)(pos.Y / 16)) * target.GetGlobalNPC<FogbinderGNPC>().chainOpacity;
					Main.spriteBatch.Draw(chainTex, pos - screenPos, null, lightColor, directionToTarget.ToRotation(), chainTex.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
				}
			}

			Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor * (1 - deathTimer), NPC.rotation, origin, NPC.scale, effects, 0f);
			Main.spriteBatch.Draw(glowTexture, slopeOffset + NPC.Center - screenPos, NPC.frame, Color.White * (1 - deathTimer), NPC.rotation, origin, NPC.scale, effects, 0f);

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (!Main.IsItStorming)
				return 0;

			return SpawnCondition.Overworld.Chance * 0.05f;
		}
	}

	public class FogbinderGNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public NPC fogbinder;

		public float chainOpacity = 0;

		private float pull = -1;

		public override void AI(NPC npc)
		{
			if (fogbinder == default || fogbinder is null)
			{
				chainOpacity = 0;
				pull = -1;
				return;
			}

			if (fogbinder.active)
			{
				if (chainOpacity < 1)
					chainOpacity += 0.02f;

				if (pull == -1)
				{
					npc.damage *= 2;
					npc.defense *= 2;
					pull = Main.rand.Next(1500, 2500);
				}
				npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionTo(fogbinder.Center), npc.Distance(fogbinder.Center) / pull);

				if (Main.rand.NextBool(2))
					Dust.NewDustPerfect(npc.Center + new Vector2(0, 20), ModContent.DustType<Dusts.Mist>(), new Vector2(0, -0.28f).RotatedByRandom(0.3f), 0, Color.White, 0.35f);

				if (npc.Distance(fogbinder.Center) > 500)
				{
					float distanceToBinder = (fogbinder.Center - npc.Center).Length();

					Vector2 directionToBinder = Vector2.Normalize(fogbinder.Center - npc.Center);

					for (float i = 0; i < distanceToBinder; i += 14)
					{
						Vector2 pos = Vector2.Lerp(npc.Center, fogbinder.Center, i / distanceToBinder);
						Gore.NewGoreDirect(npc.GetSource_FromAI(), pos, Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("Fogbinder_Chain").Type);
					}
					fogbinder = default;
					pull = -1;
					npc.damage = (int)(npc.damage * 0.5f);
					npc.defense = (int)(npc.defense * 0.5f);
				}
			}
			else
			{
				fogbinder = default;
				npc.damage = (int)(npc.damage * 0.5f);
				npc.defense = (int)(npc.defense * 0.5f);
			}
		}
	}
}

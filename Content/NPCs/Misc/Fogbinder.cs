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
		private class BindedNPC //I made it a class instead of a struct so that I can consistantly pass by reference
		{
			public NPC npc;

			public float chainTimer;

			public int distanceDividend;

			public bool active = true;

			public BindedNPC(NPC npc, float chainTimer, int distanceDividend)
			{
				this.npc = npc;
				this.chainTimer = chainTimer;
				this.distanceDividend = distanceDividend;
			}
		}

		private const int XFRAMES = 2;
		private int frameCounter = 0;
		public int yFrame = 0;
		public int xFrame = 1;

		private float deathTimer = 0;

		private float bobTimer = 0f;

		private List<BindedNPC> targets = new();

		public Player player => Main.player[NPC.target];

		private bool laughing => xFrame == 0;

		public override string Texture => AssetDirectory.MiscNPC + Name;

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscNPC + "Fogbinder_Chain");
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Fogbinder");
			Main.npcFrameCount[NPC.type] = 15;
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

			NPC.TargetClosest(true);
			NPC.spriteDirection = NPC.direction;

			bobTimer += 0.05f;
			NPC.velocity = new Vector2(0, 0.2f * (float)System.MathF.Sin(bobTimer));
			var newTargets = Main.npc.Where(n => n.active && n.knockBackResist > 0 && n.Distance(NPC.Center) < 500 && n.type != NPC.type && !n.townNPC).ToList();
			newTargets.ForEach(n => ApplyBuff(n));

			targets.ForEach(n => UpdateTarget(n, !newTargets.Contains(n.npc)));
			foreach (BindedNPC target in targets.ToArray())
			{
				if (!target.npc.active)
					targets.Remove(target);
			}

			if (Main.player.Any(n => n.active && !n.dead && n.Hitbox.Intersects(NPC.Hitbox)))
			{
				deathTimer += 0.01f;
				if (deathTimer > 1)
				{
					SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);
					NPC.active = false;
					Main.BestiaryTracker.Kills.RegisterKill(NPC);
					targets.ForEach(n => UpdateTarget(n, true));
				}
			}
			else
			{
				deathTimer = 0;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			int divider = laughing ? Main.npcFrameCount[NPC.type] : 4;
			frameCounter++;
			if (frameCounter % 5 == 0)
				yFrame++;
			if (yFrame >= divider && laughing)
			{
				xFrame = 1;
				divider = 4;
			}
			yFrame %= divider;

			int frameWidth = NPC.width;
			NPC.frame = new Rectangle(frameWidth * xFrame, frameHeight * yFrame, frameWidth, frameHeight);
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

			foreach (BindedNPC bindedTarget in targets)
			{
				NPC target = bindedTarget.npc;
				float distanceToTarget = (target.Center - NPC.Center).Length();

				Vector2 directionToTarget = Vector2.Normalize(target.Center - NPC.Center);

				for (float i = 0; i < distanceToTarget; i+= chainTex.Width + 4)
				{
					Vector2 pos = Vector2.Lerp(NPC.Center, target.Center, i / distanceToTarget);
					Color lightColor = Lighting.GetColor((int)(pos.X / 16), (int)(pos.Y / 16)) * bindedTarget.chainTimer;
					Main.spriteBatch.Draw(chainTex, pos - screenPos, null, lightColor, directionToTarget.ToRotation(), chainTex.Size() / 2, NPC.scale, SpriteEffects.None, 0f);
				}
			}

			Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor * (1 - deathTimer), NPC.rotation, origin, NPC.scale, effects, 0f);
			Main.spriteBatch.Draw(glowTexture, slopeOffset + NPC.Center - screenPos, NPC.frame, Color.White * (1 - deathTimer), NPC.rotation, origin, NPC.scale, effects, 0f);

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (!Main.IsItStorming || Main.npc.Any(n => n.active && n.type == ModContent.NPCType<Fogbinder>()))
				return 0;

			return SpawnCondition.Overworld.Chance * 0.05f;
		}

		private void ApplyBuff(NPC target)
		{
			if (!target.HasBuff(ModContent.BuffType<Fogbinded>()))
			{
				target.defense *= 2;
				target.damage *= 2;
				targets.Add(new BindedNPC(target, 0, Main.rand.Next(1500, 2500)));

				if (!laughing)
				{
					xFrame = 0;
					yFrame = 0;
				}
			}

			target.AddBuff(ModContent.BuffType<Fogbinded>(), 99999);
		}

		private void UpdateTarget(BindedNPC boundTarget, bool leaving)
		{
			NPC target = boundTarget.npc;
			float distanceToTarget = (target.Center - NPC.Center).Length();
			Vector2 directionToTarget = Vector2.Normalize(target.Center - NPC.Center);
			if (leaving)
			{
				target.damage = (int)(target.damage * 0.5f);
				target.defense = (int)(target.defense * 0.5f);

				for (float i = 0; i < distanceToTarget; i += 14)
				{
					Vector2 pos = Vector2.Lerp(NPC.Center, target.Center, i / distanceToTarget);
					Gore.NewGoreDirect(NPC.GetSource_Death(), pos, Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("Fogbinder_Chain").Type);
				}
				int buffIndex = target.FindBuffIndex(ModContent.BuffType<Fogbinded>());
				target.DelBuff(buffIndex);
				boundTarget.active = false;
			}

			boundTarget.chainTimer += 0.02f;
			target.velocity = Vector2.Lerp(target.velocity, target.DirectionTo(NPC.Center), distanceToTarget / boundTarget.distanceDividend);

			for (float i = 0; i < distanceToTarget; i += 10)
			{
				if (Main.rand.NextBool(60))
				{
					Vector2 pos = Vector2.Lerp(NPC.Center, target.Center, i / distanceToTarget);
					Dust.NewDustPerfect(pos + new Vector2(0, 20), ModContent.DustType<Dusts.Mist>(), new Vector2(0, -0.28f).RotatedByRandom(0.3f), 0, Color.White, 0.25f);
				}
			}
		}
	}

	public class Fogbinded : SmartBuff
	{
		public override string Texture => AssetDirectory.Invisible;
		public Fogbinded() : base("Fogbinded", "Bound to the fogbinder", true) { }
	}
}

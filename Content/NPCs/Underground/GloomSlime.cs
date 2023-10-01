using StarlightRiver.Content.NPCs.BaseTypes;
using System;
using System.Linq;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Underground
{
	internal class GloomSlime : Swarmer
	{
		public ref float Timer => ref NPC.ai[0];

		public override string Texture => "StarlightRiver/Assets/NPCs/Underground/" + Name;

		public override float IdealDistance => 15;

		public override float PushPower => 1.5f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gloom Slime");
		}

		public override void SetDefaults()
		{
			NPC.width = 20;
			NPC.height = 20;
			NPC.damage = 4;
			NPC.lifeMax = 25;
			NPC.knockBackResist = 1.1f;
			NPC.noGravity = true;
			NPC.aiStyle = -1;

			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
		{
			NPC.lifeMax = 25 + (NPC.lifeMax - 25) / 2;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("These drab dribbling droplets descend devilishly after dusk, deep in the depths of doom.")
			});
		}

		public override void AI()
		{
			Timer++;

			ShoveSwarm();
			NPC.TargetClosest();

			if (NPC.target >= 0)
			{
				Player target = Main.player[NPC.target];

				NPC.velocity += NPC.DirectionTo(target.Center) * 0.05f;

				if (NPC.velocity.LengthSquared() > 36)
					NPC.velocity = Vector2.Normalize(NPC.velocity) * 5.99f;
			}

			foreach (Player player in Main.player.Where(n => n.active))
			{
				float dist = Vector2.DistanceSquared(NPC.Center, player.Center);

				if (dist < Math.Pow(128, 2))
					player.AddBuff(BuffID.Darkness, 5);

				if (dist < Math.Pow(32, 2))
					player.AddBuff(BuffID.Blackout, 5);
			}

			if (Main.rand.NextBool(5))
				Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(7, 7), DustID.t_Slime, Vector2.UnitY, 180, new Color(95, 85, 80), Main.rand.NextFloat(0.5f, 1.0f));
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			NPC.velocity *= -3;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
			var frame = new Rectangle(0, (Timer % 20) < 10 ? 14 : 0, 14, 14);

			spriteBatch.Draw(texGlow, NPC.Center - Main.screenPosition, null, Color.Black * 0.5f, NPC.rotation, texGlow.Size() / 2f, NPC.scale, 0, 0);
			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, frame, drawColor * 0.8f, NPC.rotation, Vector2.One * 7, NPC.scale, 0, 0);

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			Tile tile = Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);

			return (spawnInfo.Player.ZoneRockLayerHeight && !Main.dayTime) ? 0.4f : 0;
		}
	}
}
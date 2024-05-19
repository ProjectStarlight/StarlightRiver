using StarlightRiver.Content.NPCs.BaseTypes;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Corruption
{
	internal class SoulNibbler : Swarmer
	{
		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];

		public override string Texture => "StarlightRiver/Assets/NPCs/Corruption/" + Name;

		public override float IdealDistance => 24;

		public override float PushPower => 0.7f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soul Nibbler");
		}

		public override void SetDefaults()
		{
			NPC.width = 32;
			NPC.height = 34;
			NPC.damage = 8;
			NPC.lifeMax = 34;
			NPC.knockBackResist = 1.1f;
			NPC.noGravity = true;
			NPC.aiStyle = -1;

			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("These tiny pests take after their larger bretheren, floating eerily and swarming any foolish enough to come near their tiny, but awfully sharp, fangs.")
			});
		}

		public override void AI()
		{
			Timer++;

			if (Main.dayTime)
			{
				NPC.noTileCollide = true;
				NPC.velocity.Y += 0.25f;

				return;
			}

			if (NPC.target >= 0)
			{
				Player target = Main.player[NPC.target];
				NPC.rotation = NPC.DirectionTo(target.Center).ToRotation() - 1.57f;
			}

			if (State == 0)
			{
				ShoveSwarm();
				NPC.TargetClosest();

				if (NPC.target >= 0)
				{
					Player target = Main.player[NPC.target];

					NPC.velocity += NPC.DirectionTo(target.Center) * 0.05f;

					if (NPC.velocity.LengthSquared() > 36)
						NPC.velocity = Vector2.Normalize(NPC.velocity) * 5.99f;

					if (Timer > 90)
						Lighting.AddLight(NPC.Center, new Vector3(0.3f, 0.4f, 0f) * (Timer - 90) / 30f);

					if (Timer > 120)
					{
						State = 1;
						Timer = 0;
					}
				}
			}

			if (State == 1)
			{
				if (Timer == 10 && NPC.target >= 0)
				{
					Player target = Main.player[NPC.target];
					NPC.velocity += NPC.DirectionTo(target.Center) * 8f;

					for (int k = 0; k < 10; k++)
						Dust.NewDustPerfect(NPC.Center, DustID.GreenBlood, Main.rand.NextVector2Circular(2, 2));
				}

				Dust.NewDustPerfect(NPC.Center, DustID.GreenBlood, Main.rand.NextVector2Circular(1, 1), 0, default, 0.25f);
				Lighting.AddLight(NPC.Center, new Vector3(0.3f, 0.4f, 0f) * (1f - Timer / 60f));

				if (Timer >= 60)
				{
					State = 0;
					Timer = 0;
				}
			}
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return State == 1 && Timer > 10;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			NPC.velocity *= -1;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = Assets.Keys.GlowAlpha.Value;
			var frame = new Rectangle(0, (int)(Timer % 30 / 10) * 34, 32, 34);

			var color = new Color(50, 80, 0)
			{
				A = 0
			};

			if (State == 0 && Timer > 90)
				color *= (Timer - 90) / 30f;
			else if (State == 1 && Timer < 40)
				color *= 1;
			else if (State == 1 && Timer >= 40)
				color *= 1f - (Timer - 40) / 20f;
			else
				color *= 0;

			spriteBatch.Draw(texGlow, NPC.Center - Main.screenPosition, null, color, NPC.rotation, texGlow.Size() / 2f, NPC.scale * 0.6f, 0, 0);
			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, frame, drawColor * 0.8f, NPC.rotation, new Vector2(16, 17), NPC.scale, 0, 0);

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			Tile tile = Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);

			return (spawnInfo.Player.ZoneCorrupt && !Main.dayTime) ? 0.1f : 0;
		}
	}
}
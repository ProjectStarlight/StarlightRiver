using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Physics;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Core.VerletGenerators;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Forest
{
	class Warbanner : ModNPC
	{
		public enum BehaviorStates
		{
			Wandering,
			Locked,
			Fleeing
		}

		public const float MAX_BUFF_RADIUS = 255;

		public List<NPC> targets = new();

		private VerletChain chain;

		private VerletChain miniChain0;
		private VerletChain miniChain1;

		public ref float State => ref NPC.ai[0];
		public ref float GlobalTimer => ref NPC.ai[1];
		public ref float BuffRadius => ref NPC.ai[2];
		public ref float VisualTimer => ref NPC.ai[3];

		public float VFXAlpha => BuffRadius / MAX_BUFF_RADIUS;

		public override string Texture => AssetDirectory.ForestNPC + Name;

		public override void Load()
		{
			for (int k = 0; k <= 6; k++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(StarlightRiver.Instance, AssetDirectory.ForestNPC + "Gore/WarbannerGore" + k);
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunted Warbanner");
		}

		public override void SetDefaults()
		{
			NPC.width = 60;
			NPC.height = 100;
			NPC.knockBackResist = 0.1f;
			NPC.lifeMax = 250;
			NPC.defense = 11;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 1;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.chaseable = true;
			NPC.value = 100;

			if (Main.netMode != NetmodeID.Server)
			{
				chain = new RectangularBanner(8, true, NPC.Center + Vector2.UnitY * -40, 8)
				{
					constraintRepetitions = 2,
					drag = 1f,
					forceGravity = new Vector2(0f, 0.55f),
					scale = 8f,
					parent = NPC,
				};

				miniChain0 = new RectangularBanner(7, true, NPC.Center + new Vector2(22, -48), 8)
				{
					constraintRepetitions = 2,
					drag = 1f,
					forceGravity = new Vector2(0f, 0.55f),
					scale = 6f,
					parent = NPC
				};

				miniChain1 = new RectangularBanner(7, true, NPC.Center + new Vector2(-22, -48), 8)
				{
					constraintRepetitions = 2,
					drag = 1f,
					forceGravity = new Vector2(0f, 0.55f),
					scale = 6f,
					parent = NPC
				};
			}
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement("An artifact from battles past, this cursed sign still inspires others with the enraging spirit of battle to this day.")
			});
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false; //harmless
		}

		public override void AI()
		{
			GlobalTimer++;
			VisualTimer++;

			NPC.rotation = NPC.velocity.X * -0.1f;
			NPC.rotation += MathF.Sin(VisualTimer * 0.025f) * 0.05f;

			if (Main.netMode != NetmodeID.Server)
			{
				Vector2 center = NPC.Center + new Vector2(0, 42);
				chain.UpdateChain(center + Vector2.UnitY.RotatedBy(NPC.rotation) * -68);
				miniChain0.UpdateChain(center + new Vector2(8, -68).RotatedBy(NPC.rotation));
				miniChain1.UpdateChain(center + new Vector2(-8, -68).RotatedBy(NPC.rotation));

				chain.IterateRope(UpdateBanner);
				miniChain0.IterateRope(UpdateBannerSmall);
				miniChain1.IterateRope(UpdateBannerSmall);
			}

			Lighting.AddLight(NPC.Center, new Vector3(1.25f, 0.4f, 0.2f) * 0.7f);

			for (int k = 0; k < 32; k++)
			{
				Vector2 pos = NPC.Center + Vector2.One.RotatedBy(k / 32f * 6.28f) * 332 * VFXAlpha * 0.5f;
				Lighting.AddLight(pos, new Vector3(0.9f, 0.4f, 0.2f) * VFXAlpha * 0.1f);
			}

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc != NPC && CollisionHelper.CheckCircularCollision(NPC.Center, (int)BuffRadius, npc.Hitbox))
					npc.AddBuff(BuffType<Rage>(), 2);
			}

			if (Main.dayTime)
			{
				State = (int)BehaviorStates.Fleeing; //flee by day nomatter what
				GlobalTimer = 0;
			}

			switch (State)
			{
				case (int)BehaviorStates.Wandering: //find the nearest valid target

					if (BuffRadius > 0)
						BuffRadius--;

					NPC firstTarget = NPC.FindNearestNPC(n => n.active && !n.friendly && !n.noGravity && n.CanBeChasedBy() && Vector2.Distance(NPC.Center, n.Center) < 2500);

					if (firstTarget != null)
					{
						targets.Add(firstTarget);
						State = (int)BehaviorStates.Locked;
						GlobalTimer = 0;
					}
					else
					{
						NPC.velocity *= 0.9f;

						if (GlobalTimer > 600)
						{
							State = (int)BehaviorStates.Fleeing; //after waiting for 10 seconds for a target they instead flee
							GlobalTimer = 0;
						}
					}

					break;

				case (int)BehaviorStates.Locked:

					if (BuffRadius < MAX_BUFF_RADIUS)
						BuffRadius++;

					if (Main.rand.NextBool(2))
					{
						float dist = Main.rand.NextFloat();

						var d = Dust.NewDustPerfect(NPC.Center, DustType<BannerBuffDust>(), Vector2.UnitY * Main.rand.NextFloat(-4, -1), 0, new Color(1f, (1f - dist) * 0.6f, 0, 0), Main.rand.NextFloat(0.3f, 0.5f));
						d.customData = (NPC, Vector2.UnitY * 48 + Vector2.One.RotatedByRandom(6.28f) * VFXAlpha * dist * 170);
					}

					for (int k = 0; k < targets.Count; k++)
					{
						NPC toCheck = targets[k];

						if (toCheck is null || !toCheck.active || toCheck.friendly || !toCheck.CanBeChasedBy() || Vector2.Distance(NPC.Center, toCheck.Center) > (targets.Count > 1 ? MAX_BUFF_RADIUS : 2500)) //remove invalid targets
							targets.Remove(toCheck);
					}

					if (targets.Count == 0) //if we've lost all targets go back to wandering
					{
						State = (int)BehaviorStates.Wandering;
						GlobalTimer = 0;
						return;
					}

					Vector2 target = GeometryHelper.Centeroid(targets) + new Vector2(0, -140);

					if (Vector2.Distance(NPC.Center, target) > 32)
						NPC.velocity = (NPC.Center - target) * -0.02f; //accelerate towards the centeroid of it's supported NPCs

					if (NPC.velocity.Length() > 3) //speed cap
						NPC.velocity = Vector2.Normalize(NPC.velocity) * 2.99f;

					NPC.velocity *= 0.95f;

					if (GlobalTimer % 60 == 0) //periodically check for more targets
					{
						NPC potentialTarget = NPC.FindNearestNPC(n => n.active && !n.friendly && !n.noGravity && !targets.Contains(n) && Vector2.Distance(NPC.Center, n.Center) < 500);

						if (potentialTarget != null)
							targets.Add(potentialTarget);
					}

					break;

				case (int)BehaviorStates.Fleeing: //flee at daytime or when timing out

					if (BuffRadius > 0)
						BuffRadius -= 5;

					NPC.velocity.Y -= 0.2f;

					if (GlobalTimer > 300)
						NPC.active = false;

					break;
			}
		}

		private void UpdateBanner(VerletChain rope, int index)
		{
			int x = (int)rope.ropeSegments[index].posNow.X / 16;
			int y = (int)rope.ropeSegments[index].posNow.Y / 16;
			Color light = Lighting.GetColor(x, y);

			rope.ropeSegments[index].color = new Color(255, index * 30, 40 - index).MultiplyRGB(light);

			rope.ropeSegments[index].posNow += Vector2.UnitX * (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + index * 0.4f) * 0.15f;

			if (index == 1)
				rope.ropeSegments[index].posNow = rope.ropeSegments[0].posNow + Vector2.UnitY;
		}

		private void UpdateBannerSmall(VerletChain rope, int index)
		{
			int x = (int)rope.ropeSegments[index].posNow.X / 16;
			int y = (int)rope.ropeSegments[index].posNow.Y / 16;
			Color light = Lighting.GetColor(x, y);

			rope.ropeSegments[index].color = new Color(160, 20, 10).MultiplyRGB(light);

			rope.ropeSegments[index].posNow += Vector2.UnitX * (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + index * 0.4f) * 0.15f;

			if (index == 1)
				rope.ropeSegments[index].posNow = rope.ropeSegments[0].posNow + Vector2.UnitY;
		}

		private void DrawBestiary(SpriteBatch spriteBatch, Vector2 screenPos)
		{
			var target = new Rectangle((int)NPC.Center.X - 8, (int)NPC.Center.Y - 24, 16, 70);
			spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target, new Color(55, 0, 0));
			target.Inflate(-2, -2);
			spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target, new Color(200, 60, 30));

			target = new Rectangle((int)NPC.Center.X - 10 - 5, (int)NPC.Center.Y - 24, 10, 60);
			spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target, new Color(55, 0, 0));
			target.Inflate(-2, -2);
			spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target, new Color(120, 20, 20));

			target = new Rectangle((int)NPC.Center.X + 10 - 5, (int)NPC.Center.Y - 24, 10, 60);
			spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target, new Color(55, 0, 0));
			target.Inflate(-2, -2);
			spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, target, new Color(120, 20, 20));

			Texture2D tex = Request<Texture2D>(Texture).Value;
			spriteBatch.Draw(tex, NPC.Center - screenPos, null, Color.White, NPC.rotation, tex.Size() / 2f, NPC.scale, 0, 0);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				DrawBestiary(spriteBatch, screenPos);
				return false;
			}

			Texture2D tex = Assets.NPCs.Forest.Warbanner.Value;
			spriteBatch.Draw(tex, NPC.Center + new Vector2(0, 42) - screenPos, null, drawColor, NPC.rotation, new Vector2(tex.Width / 2f, tex.Height), NPC.scale, 0, 0);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderNPCs", () =>
			{
				chain.DrawStrip(chain.scale);
				miniChain0.DrawStrip(miniChain0.scale);
				miniChain1.DrawStrip(miniChain1.scale);
			});

			Texture2D auraTex = Assets.Misc.GlowRing.Value;
			Texture2D ballTex = Assets.Masks.GlowHarshAlpha.Value;
			float maxScale = auraTex.Width / MAX_BUFF_RADIUS * 1.1f;
			Color glowColor = new(255, 255, 255, 0);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
			{
				spriteBatch.Draw(auraTex, NPC.Center - Main.screenPosition, null, new Color(255, 60, 0, 0) * VFXAlpha * 0.2f, 0, auraTex.Size() / 2, BuffRadius * 2 / auraTex.Width, 0, 0);

				for (int k = 0; k < 2; k++)
				{
					for (int i = 0; i < 40; i++)
					{
						Vector2 pos = NPC.Center + Vector2.One.RotatedBy(VisualTimer * 0.012f + k * 3.14f + i * 0.04f) * 170 * VFXAlpha;
						spriteBatch.Draw(ballTex, pos - Main.screenPosition, null, new Color(255, 120, 0, 0) * VFXAlpha * (i / 40f) * 0.2f, pos.DirectionTo(NPC.Center).ToRotation(), ballTex.Size() / 2f, new Vector2(0.2f, 2f), 0, 0);

						Vector2 pos2 = NPC.Center + Vector2.One.RotatedBy(VisualTimer * 0.023f + k * 3.14f + i * 0.04f) * 155 * VFXAlpha;
						spriteBatch.Draw(ballTex, pos2 - Main.screenPosition, null, new Color(255, 80, 0, 0) * VFXAlpha * (i / 40f) * 0.2f, pos2.DirectionTo(NPC.Center).ToRotation(), ballTex.Size() / 2f, new Vector2(0.2f, 2f), 0, 0);

						Vector2 pos3 = NPC.Center + Vector2.One.RotatedBy(VisualTimer * 0.043f + k * 3.14f + i * 0.04f) * 140 * VFXAlpha;
						spriteBatch.Draw(ballTex, pos3 - Main.screenPosition, null, new Color(255, 50, 0, 0) * VFXAlpha * (i / 40f) * 0.2f, pos3.DirectionTo(NPC.Center).ToRotation(), ballTex.Size() / 2f, new Vector2(0.2f, 2f), 0, 0);
					}
				}
			});

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			//they should only spawn at night in the forest after EoC is dead, and one max
			if (spawnInfo.Player.ZoneForest() && Main.bloodMoon && NPC.downedBoss1 && !Main.npc.Any(n => n.active && n.type == NPC.type))
				return 0.05f;

			return 0;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int k = 0; k <= 12; k++)
				{
					int goreType = StarlightRiver.Instance.Find<ModGore>("WarbannerGore" + Main.rand.Next(7)).Type;
					Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Vector2.Zero, goreType);
				}
			}
		}
	}

	internal class BannerBuffDust : ModDust
	{
		public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

		public override void OnSpawn(Dust dust)
		{
			dust.fadeIn = 0;
			dust.noLight = false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = MathF.Sin(dust.alpha / 255f * 6.28f);

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Texture2D tex = Assets.Masks.GlowHarshAlpha.Value;
				Vector2 pos = dust.position - Main.screenPosition;
				//pos.X -= pos.X % 2;

				Main.spriteBatch.Draw(tex, pos, null, dust.color * lerper * 0.5f, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);

				Main.spriteBatch.Draw(tex, pos, null, Color.White with { A = 0 } * 0.1f * lerper, dust.rotation, tex.Size() / 2f, new Vector2(dust.scale * lerper, dust.scale), 0f, 0f);
			});

			return false;
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is (NPC, Vector2))
			{
				var data = ((NPC, Vector2))dust.customData;
				dust.position = data.Item1.Center + data.Item2;

				dust.customData = (data.Item1, data.Item2 + dust.velocity);
			}

			dust.rotation = dust.velocity.ToRotation() - 1.57f;

			dust.velocity.Y = -1f + -2f * (1f - MathF.Sin(dust.alpha / 255f * 6.28f));
			dust.alpha += 2;

			if (dust.alpha > 255)
				dust.active = false;

			return false;
		}
	}
}
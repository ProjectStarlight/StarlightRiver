using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Physics;
using StarlightRiver.Core.VerletGenerators;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Forest
{
	class Warbanner : ModNPC, IDrawAdditive
	{
		public enum BehaviorStates
		{
			Wandering,
			Locked,
			Fleeing
		}

		public const float MAX_BUFF_RADIUS = 332;

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

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunted Warbanner");
		}

		public override void SetDefaults()
		{
			NPC.width = 32;
			NPC.height = 32;
			NPC.knockBackResist = 0.1f;
			NPC.lifeMax = 100;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 1;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.chaseable = true;

			chain = new RectangularBanner(15, false, NPC.Center + Vector2.UnitY * -40, 8)
			{
				constraintRepetitions = 2,
				drag = 1.2f,
				forceGravity = new Vector2(0f, 0.55f),
				scale = 16f,
				parent = NPC
			};

			miniChain0 = new RectangularBanner(10, false, NPC.Center + new Vector2(22, -48), 8)
			{
				constraintRepetitions = 2,
				drag = 1.35f,
				forceGravity = new Vector2(0f, 0.55f),
				scale = 4f,
				parent = NPC
			};

			miniChain1 = new RectangularBanner(10, false, NPC.Center + new Vector2(-22, -48), 8)
			{
				constraintRepetitions = 2,
				drag = 1.35f,
				forceGravity = new Vector2(0f, 0.55f),
				scale = 4f,
				parent = NPC
			};
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false; //harmless
		}

		public override void AI()
		{
			GlobalTimer++;
			VisualTimer++;

			chain.UpdateChain(NPC.Center + Vector2.UnitY * -40);
			miniChain0.UpdateChain(NPC.Center + new Vector2(22, -48));
			miniChain1.UpdateChain(NPC.Center + new Vector2(-22, -48));

			chain.IterateRope(ColorBanner);
			miniChain0.IterateRope(ColorBannerSmall);
			miniChain1.IterateRope(ColorBannerSmall);

			Lighting.AddLight(NPC.Center, new Vector3(1.25f, 0.4f, 0.2f) * VFXAlpha * 0.7f);

			for (int k = 0; k < 2; k++)
			{
				Vector2 pos = NPC.Center + Vector2.One.RotatedBy(VisualTimer * 0.012f + k * 3.14f + 1.6f) * 332 * VFXAlpha * 0.5f;
				Lighting.AddLight(pos, new Vector3(0.9f, 0.4f, 0.2f) * VFXAlpha * 0.6f);
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

					NPC firstTarget = NPC.FindNearestNPC(n => n.active && !n.friendly && !n.noGravity && Vector2.Distance(NPC.Center, n.Center) < 2500);

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
						var d = Dust.NewDustPerfect(NPC.Center, DustType<BannerBuffDust>(), Vector2.UnitY * Main.rand.NextFloat(-4, -1), 0, new Color(255, Main.rand.Next(150), 0), Main.rand.NextFloat(0.3f));
						d.customData = (NPC, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(BuffRadius * 0.45f));
					}

					for (int k = 0; k < targets.Count; k++)
					{
						NPC toCheck = targets[k];

						if (toCheck is null || !toCheck.active || Vector2.Distance(NPC.Center, toCheck.Center) > (targets.Count > 1 ? MAX_BUFF_RADIUS : 2500)) //remove invalid targets
							targets.Remove(toCheck);
						else if (Helper.CheckCircularCollision(NPC.Center, (int)BuffRadius, toCheck.Hitbox))
							toCheck.AddBuff(BuffType<Rage>(), 2);
					}

					if (targets.Count == 0) //if we've lost all targets go back to wandering
					{
						State = (int)BehaviorStates.Wandering;
						GlobalTimer = 0;
						return;
					}

					Vector2 target = Helper.Centeroid(targets) + new Vector2(0, -100);

					if (Vector2.Distance(NPC.Center, target) > 32)
						NPC.velocity += Vector2.Normalize(NPC.Center - target) * -0.12f; //accelerate towards the centeroid of it's supported NPCs

					if (NPC.velocity.Length() > 3) //speed cap
						NPC.velocity = Vector2.Normalize(NPC.velocity) * 2.99f;

					NPC.velocity *= 0.95f;

					if (GlobalTimer % 60 == 0) //periodically check for more targets
					{
						NPC potentialTarget = NPC.FindNearestNPC(n => !n.noGravity && !targets.Contains(n) && Vector2.Distance(NPC.Center, n.Center) < 500);

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

		private void ColorBanner(VerletChain rope, int index)
		{
			int x = (int)rope.ropeSegments[index].posNow.X / 16;
			int y = (int)rope.ropeSegments[index].posNow.Y / 16;
			Color light = Lighting.GetColor(x, y);

			rope.ropeSegments[index].color = new Color(255, 80 - index * 2, 40 - index).MultiplyRGB(light);

			rope.ropeSegments[index].posNow += Vector2.UnitX * (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + index * 0.4f) * 0.15f;

			if (index == 1)
				rope.ropeSegments[index].posNow = rope.ropeSegments[0].posNow + Vector2.UnitY;
		}

		private void ColorBannerSmall(VerletChain rope, int index)
		{
			int x = (int)rope.ropeSegments[index].posNow.X / 16;
			int y = (int)rope.ropeSegments[index].posNow.Y / 16;
			Color light = Lighting.GetColor(x, y);

			rope.ropeSegments[index].color = new Color(180, 30, 10).MultiplyRGB(light);

			rope.ropeSegments[index].posNow += Vector2.UnitX * (float)System.Math.Sin(Main.GameUpdateCount * 0.05f + index * 0.4f) * 0.15f;

			if (index == 1)
				rope.ropeSegments[index].posNow = rope.ropeSegments[0].posNow + Vector2.UnitY;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			spriteBatch.Draw(tex, NPC.Center + new Vector2(0, -64) - screenPos, null, drawColor, NPC.rotation, tex.Size() / 2f, NPC.scale, 0, 0);
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D auraTex = Request<Texture2D>("StarlightRiver/Assets/Misc/GlowRingTransparent").Value;
			Texture2D ballTex = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
			float maxScale = auraTex.Width / MAX_BUFF_RADIUS;

			spriteBatch.Draw(auraTex, NPC.Center - Main.screenPosition, null, Color.Red * VFXAlpha * 0.4f, 0, auraTex.Size() / 2, VFXAlpha * maxScale, 0, 0);

			for (int k = 0; k < 2; k++)
			{
				for (int i = 0; i < 40; i++)
				{
					Vector2 pos = NPC.Center + Vector2.One.RotatedBy(VisualTimer * 0.012f + k * 3.14f + i * 0.04f) * 320 * VFXAlpha * 0.5f;
					spriteBatch.Draw(ballTex, pos - Main.screenPosition, null, new Color(255, 50, 0) * VFXAlpha * (i / 40f) * 0.5f, 0, ballTex.Size() / 2f, 0.9f - i * 0.01f, 0, 0);
					spriteBatch.Draw(ballTex, pos - Main.screenPosition, null, Color.White * VFXAlpha * (i / 40f) * 0.5f, 0, ballTex.Size() / 2f, 0.45f - i * 0.005f, 0, 0);

					Vector2 pos2 = NPC.Center + Vector2.One.RotatedBy(VisualTimer * 0.023f + k * 3.14f + i * 0.04f) * 294 * VFXAlpha * 0.5f;
					spriteBatch.Draw(ballTex, pos2 - Main.screenPosition, null, new Color(255, 80, 0) * VFXAlpha * (i / 40f) * 0.47f, 0, ballTex.Size() / 2f, 0.8f - i * 0.01f, 0, 0);
					spriteBatch.Draw(ballTex, pos2 - Main.screenPosition, null, Color.White * VFXAlpha * (i / 40f) * 0.47f, 0, ballTex.Size() / 2f, 0.4f - i * 0.005f, 0, 0);

					Vector2 pos3 = NPC.Center + Vector2.One.RotatedBy(VisualTimer * 0.043f + k * 3.14f + i * 0.04f) * 268 * VFXAlpha * 0.5f;
					spriteBatch.Draw(ballTex, pos3 - Main.screenPosition, null, new Color(255, 120, 0) * VFXAlpha * (i / 40f) * 0.4f, 0, ballTex.Size() / 2f, 0.7f - i * 0.01f, 0, 0);
					spriteBatch.Draw(ballTex, pos3 - Main.screenPosition, null, Color.White * VFXAlpha * (i / 40f) * 0.4f, 0, ballTex.Size() / 2f, 0.35f - i * 0.005f, 0, 0);
				}
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			//they should only spawn at night in the forest after EoC is dead, and one max
			if (spawnInfo.Player.ZoneForest() && !Main.dayTime && NPC.downedBoss1 && !Main.npc.Any(n => n.active && n.type == NPC.type))
				return 0.25f;

			return 0;
		}

		public override void OnKill()
		{
			VerletChainSystem.toDraw.Remove(chain);
			VerletChainSystem.toDraw.Remove(miniChain0);
			VerletChainSystem.toDraw.Remove(miniChain1);
		}
	}

	internal class BannerBuffDust : ModDust
	{
		public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

		public override void OnSpawn(Dust dust)
		{
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 8, 128);
			dust.scale = 1;

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is (NPC, Vector2))
			{
				var data = ((NPC, Vector2))dust.customData;
				dust.position = data.Item1.Center + data.Item2;

				dust.customData = (data.Item1, data.Item2 + dust.velocity);
			}

			dust.rotation = dust.velocity.ToRotation() + 1.57f;

			dust.velocity *= 0.98f;
			dust.color *= 0.97f;

			if (dust.fadeIn <= 2)
				dust.shader.UseColor(Color.Transparent);
			else
				dust.shader.UseColor(dust.color * 0.5f);

			dust.fadeIn++;

			if (dust.fadeIn > 60)
				dust.active = false;

			return false;
		}
	}
}

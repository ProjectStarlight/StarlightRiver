using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Crimson
{
	enum NeuronChomperState : int
	{
		Idle,
		Charging,
		Dashing
	}

	internal class NeuronChomper : ModNPC
	{
		public Vector2 homePos;
		public Vector2 targetPos;

		public ref float Timer => ref NPC.ai[0];

		public NeuronChomperState State
		{
			get => (NeuronChomperState)NPC.ai[1];
			set => NPC.ai[1] = (float)value;
		}

		public override string Texture => AssetDirectory.CrimsonNPCs + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Neuron Chomper");

			NPCID.Sets.TrailCacheLength[Type] = 10;
			NPCID.Sets.TrailingMode[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 96;
			NPC.height = 80;
			NPC.lifeMax = 200;
			NPC.damage = 30;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0.1f;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.DD2_OgreDeath.WithPitchOffset(1.5f);
			NPC.defense = 8;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return State == NeuronChomperState.Dashing;
		}

		public override void AI()
		{
			if (homePos == default)
				homePos = NPC.Center;

			Timer++;

			Player target = NPC.target != -1 ? Main.player[NPC.target] : null;

			NPC.rotation = NPC.Center.DirectionFrom(homePos).ToRotation() - 1.57f;

			NPC.frame = new Rectangle((int)(Main.GameUpdateCount / 4f) % 6 * NPC.width, 0, NPC.width, NPC.height);

			switch (State)
			{
				case NeuronChomperState.Idle:

					NPC.TargetClosest();

					if (target != null && target.active && !target.dead && Vector2.Distance(homePos, target.Center) <= 600)
					{
						State = NeuronChomperState.Charging;
						Timer = 0;
					}

					Vector2 idleTargetPos = homePos + new Vector2(0, 64);
					NPC.velocity = (idleTargetPos - NPC.Center) * 0.025f;

					break;

				case NeuronChomperState.Charging:

					if (target is null || !target.active || target.dead || Vector2.Distance(homePos, target.Center) > 600)
					{
						State = NeuronChomperState.Idle;
						Timer = 0;
					}

					// Spawn charge up dusts
					if (Timer > 60 && Timer < 140)
					{
						float rot = Main.rand.NextFloat(6.28f);
						float opacity = (Timer - 60) / 80f;
						Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * 60 * opacity, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.UnitX.RotatedBy(rot) * -Main.rand.NextFloat(3) * opacity, 0, new Color(255, 80, 80, 0) * opacity, Main.rand.NextFloat(0.1f, 0.2f) * opacity);
					}

					// Float into position
					if (Timer < 120)
					{
						Vector2 targetPos = homePos + homePos.DirectionTo(target.Center) * -(300 * Timer / 120f);
						NPC.velocity = (targetPos - NPC.Center) * 0.05f * (Timer > 100 ? 1f - (Timer - 100) / 20f : 1f);
					}

					if (Timer == 150)
					{
						NPC.velocity = NPC.Center.DirectionTo(target.Center) * -12;
						targetPos = target.Center;

						SoundEngine.PlaySound(SoundID.Roar.WithPitchOffset(-0.5f), NPC.Center);

						State = NeuronChomperState.Dashing;
						Timer = 0;
					}

					break;

				case NeuronChomperState.Dashing:

					if (Timer < 30)
					{
						NPC.velocity += NPC.Center.DirectionTo(targetPos) * 0.075f * Timer;
					}

					if (Timer < 70)
					{
						NPC.velocity *= 0.99f;
						Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<Dusts.GraymatterDust>(), 0, 0, 0, default, MathF.Sin(Timer / 70f * 3.14f));
					}

					if (Timer > 50 && Timer < 90)
						NPC.velocity *= 0.95f;

					if (Timer == 90)
					{
						NPC.velocity *= 0;
						State = NeuronChomperState.Idle;
						Timer = 0;
					}

					break;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = Assets.NPCs.Crimson.NeuronChomper.Value;
			Texture2D chain = Assets.NPCs.Crimson.NeuronChomperChain.Value;

			Vector2 chainStart = NPC.Center + Vector2.UnitX.RotatedBy(NPC.rotation - 1.57f) * 30;

			float dist = Vector2.Distance(chainStart, homePos);
			for (int k = 0; k < dist; k += 26)
			{
				Vector2 pos = Vector2.Lerp(chainStart, homePos, k / dist) - screenPos;
				pos.Y += MathF.Sin(k / dist * 3.14f) * 32;

				Vector2 next = Vector2.Lerp(chainStart, homePos, (k + 1) / dist) - screenPos;
				next.Y += MathF.Sin((k + 1) / dist * 3.14f) * 32;

				float opacity = 1f;

				if (k + 26 > dist)
					opacity *= dist % 26 / 26f;

				Color color = Color.Lerp(new Color(255, 255, 140), new Color(255, 100, 200), k / dist) * opacity;

				spriteBatch.Draw(chain, pos, null, color, pos.DirectionTo(next).ToRotation() + 1.57f, chain.Size() / 2f, 1f, 0, 0);
			}

			if (State == NeuronChomperState.Dashing)
			{
				for (int k = 0; k < NPC.oldPos.Length; k++)
				{
					float progress = (float)((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
					Vector2 drawPos = NPC.oldPos[k] + NPC.Size / 2f - screenPos + new Vector2(0f, NPC.gfxOffY);
					Color color = drawColor * Eases.EaseQuarticOut(progress);

					color *= 1f - Timer / 90f;

					Main.EntitySpriteDraw(texture, drawPos, NPC.frame, color, NPC.rotation, NPC.Size / 2f, NPC.scale * Eases.EaseQuadOut(progress), SpriteEffects.None, 0);
				}
			}

			spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, NPC.frame, drawColor, NPC.rotation, NPC.Size / 2f, NPC.scale, 0, 0);

			if (State == NeuronChomperState.Charging)
			{
				float opacity = Timer < 140 ? (Timer - 60) / 80f : 1f - (Timer - 140) / 10f;

				spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, NPC.frame, new Color(255, 100, 100, 0) * opacity, NPC.rotation, NPC.Size / 2f, NPC.scale, 0, 0);

				if (Timer > 90 && Timer <= 150)
				{
					ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
					{
						Texture2D tex = Assets.Misc.GlowRing.Value;

						Color color = new Color(255, 100, 100) * ((Timer - 90) / 60f);
						color.A = 0;

						if (Timer > 140)
							color *= 1f - (Timer - 140) / 10f;

						float scale = 50 + (1f - (Timer - 90) / 20f) * 60;
						spriteBatch.Draw(tex, NPC.Center - screenPos, null, color, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);

						scale = 50 + (1f - (Timer - 100) / 20f) * 60;
						spriteBatch.Draw(tex, NPC.Center - screenPos, null, color, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);

						scale = 50 + (1f - (Timer - 110) / 20f) * 60;
						spriteBatch.Draw(tex, NPC.Center - screenPos, null, color, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);
					});
				}
			}

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return spawnInfo.Player.ZoneCrimson && StarlightWorld.HasFlag(WorldFlags.ThinkerBossOpen) ? 0.3f : 0;
		}

		public override void OnSpawn(IEntitySource source)
		{
			for (int x = -40; x < 40; x++)
			{
				for (int y = -40; y < 40; y++)
				{
					int tX = x + (int)NPC.Center.X / 16;
					int tY = y + (int)NPC.Center.Y / 16;
					Tile tile = Main.tile[tX, tY];
					Tile up = Main.tile[tX, tY - 1];
					Tile left = Main.tile[tX - 1, tY];
					Tile right = Main.tile[tX + 1, tY];
					Tile down = Main.tile[tX, tY + 1];

					if (tile.HasTile && tile.TileType == TileID.Trees && !up.HasTile && !left.HasTile && !right.HasTile && down.HasTile && down.TileType == TileID.Trees)
					{
						Vector2 target = new Vector2(tX, tY) * 16 + Vector2.One * 8;

						if (Main.npc.Any(n => n.active && n.ModNPC is NeuronChomper chomper && Vector2.Distance(chomper.homePos, target) < 256))
							continue;

						NPC.Center = target;
						return;
					}
				}
			}

			NPC.active = false;
		}

		public override void OnKill()
		{
			for (int k = 0; k < 60; k++)
			{
				float rot = Main.rand.NextFloat(6.28f);
				Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * Main.rand.NextFloat(20), DustID.Blood, Vector2.UnitX.RotatedBy(rot) * Main.rand.NextFloat(6), 0, default, Main.rand.NextFloat(3));
			}

			for (int k = 0; k < 10; k++)
			{
				float rot = Main.rand.NextFloat(6.28f);
				Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * Main.rand.NextFloat(20), ModContent.DustType<Dusts.GraymatterDust>(), Vector2.UnitX.RotatedBy(rot) * Main.rand.NextFloat(3));
			}
		}
	}
}

using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Crimson
{
	enum ThoughtPoliceState : int
	{
		Idle,
		Aggroed,
		Attack,
		Recall
	}

	internal class ThoughtPolice : ModNPC
	{
		public readonly int MAX_FLASH_TIMER = 30;

		public static List<ThoughtPolice> toRender = [];

		public float localScanOpacity;
		public float aggroFlashTimer;

		public float homeX;

		public ref float Timer => ref NPC.ai[0];

		public ThoughtPoliceState State
		{
			get => (ThoughtPoliceState)NPC.ai[1];
			set => NPC.ai[1] = (float)value;
		}

		public ref float AimRotation => ref NPC.ai[2];
		public ref float Variant => ref NPC.ai[3];

		public Player Target => Main.player[NPC.target];

		public override string Texture => AssetDirectory.CrimsonNPCs + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawOverHallucinationMap += DrawThoughPoliceHallucination;
		}

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}

		public override void SetDefaults()
		{
			NPC.width = 52;
			NPC.height = 94;
			NPC.knockBackResist = 0.0f;
			NPC.lifeMax = 200;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 25;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			Variant = Main.rand.Next(6);

			NPC.netUpdate = true;

			toRender.Add(this);
		}

		public override void AI()
		{
			Timer++;

			// check for and set home X position if needed
			if (homeX == 0)
				homeX = NPC.Center.X;

			if (aggroFlashTimer > 0)
				aggroFlashTimer--;

			// spawn dust
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 0, 2);

			switch (State)
			{
				case ThoughtPoliceState.Idle:

					// calculate local scan opacity
					if (Main.LocalPlayer.HasBuff(ModContent.BuffType<CrimsonHallucination>()) && localScanOpacity < 1)
						localScanOpacity += 0.05f;
					if (!Main.LocalPlayer.HasBuff(ModContent.BuffType<CrimsonHallucination>()) && localScanOpacity > 0)
						localScanOpacity -= 0.05f;

					// Idle bobbing
					NPC.position.Y += MathF.Sin(Timer * 0.05f) * 0.5f;

					// Patrol
					NPC.position.X = homeX + MathF.Sin(Timer * 6.28f / 2400) * 500f;

					// Hover up if needed
					bool foundGround = false;

					for (int k = 0; k < 30; k++)
					{
						Tile tile = Framing.GetTileSafely((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16 + k);

						if (tile.HasTile && Main.tileSolid[tile.TileType])
						{
							NPC.velocity.Y -= 0.05f;
							foundGround = true;
							break;
						}
					}

					if (!foundGround)
						NPC.velocity.Y += 0.05f;

					if (Math.Abs(NPC.velocity.Y) > 6)
					{
						NPC.velocity.Y *= 6f / Math.Abs(NPC.velocity.Y);
					}

					NPC.velocity.Y *= 0.95f;

					// rotate cone
					AimRotation = 1.57f + MathF.Sin(Timer * 6.28f / 600);

					// Cone check
					foreach (Player player in Main.ActivePlayers)
					{
						if (player.HasBuff(ModContent.BuffType<CrimsonHallucination>()) && CollisionHelper.CheckConicalCollision(NPC.Center, 400, AimRotation, 1.57f / 2f, player.Hitbox))
						{
							NPC.target = player.whoAmI;
							Timer = 0;
							State = ThoughtPoliceState.Aggroed;
							aggroFlashTimer = MAX_FLASH_TIMER;
							NPC.netUpdate = true;
						}
					}

					break;

				case ThoughtPoliceState.Aggroed:

					if (localScanOpacity > 0)
						localScanOpacity -= 0.1f;

					if (localScanOpacity < 0)
						localScanOpacity = 0;

					if (Target != null && Vector2.Distance(NPC.Center, Target.Center) <= 2000)
					{
						Vector2 targetPos = Target.Center + new Vector2(MathF.Sin(Timer * 6.28f / 120) * 40, -120);
						NPC.Center += (targetPos - NPC.Center) * 0.05f;

						if (Timer == 180)
						{
							NPC.velocity = (targetPos - NPC.Center) * 0.05f;
							aggroFlashTimer = 30;

							Timer = 0;
							State = ThoughtPoliceState.Attack;
							NPC.netUpdate = true;
						}
					}
					else
					{
						Timer = 0;
						State = ThoughtPoliceState.Recall;
						NPC.netUpdate = true;
					}

					break;

				case ThoughtPoliceState.Attack:

					NPC.velocity *= 0.95f;

					if (Timer == 30)
					{
						NPC.velocity *= 0f;

						int count = Main.masterMode ? 10 : Main.expertMode ? 8 : 6;

						for (int k = 0; k < count; k++)
						{
							Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<ThoughtPoliceProjectile>(), NPC.damage, 0.5f, Main.myPlayer, 600, k / (float)count * 6.28f);
						}
					}

					if (Timer == 200)
					{
						Timer = 0;
						State = ThoughtPoliceState.Aggroed;
						NPC.netUpdate = true;
					}

					break;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.NPCs.Crimson.ThoughtPolice.Value;
			var frame = new Rectangle((int)Variant * 104, (int)(Timer / 10 % 6) * 148, 104, 148);

			if (State == ThoughtPoliceState.Aggroed || State == ThoughtPoliceState.Attack)
			{
				Texture2D aura = Assets.Bosses.TheThinkerBoss.HallucionationHazard.Value;
				float pulse = 1.1f + MathF.Sin(Timer * 0.1f) * 0.1f;
				float colorPulse = 1f + MathF.Sin(Timer * 0.1f) * 0.5f;
				spriteBatch.Draw(aura, NPC.Center - Main.screenPosition, null, new Color(255, 80, 80) * 0.35f * colorPulse, MathF.Sin(Timer / 120f * 6.28f) * 0.1f, aura.Size() / 2f, pulse, 0, 0);
			}

			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, frame, drawColor, NPC.rotation, frame.Size() / 2f, NPC.scale, 0, 0);

			if (aggroFlashTimer > 0)
			{
				Texture2D shape = Assets.NPCs.Crimson.ThoughtPoliceShape.Value;
				float prog = (MAX_FLASH_TIMER - aggroFlashTimer) / MAX_FLASH_TIMER;
				Color color = new Color(255, 100, 100, 0) * (prog < 0.25f ? Eases.EaseQuadIn(prog / 0.25f) : 1f - Eases.EaseCircularOut((prog - 0.25f) / 0.75f));

				spriteBatch.Draw(shape, NPC.Center - Main.screenPosition, frame, color, NPC.rotation, frame.Size() / 2f, NPC.scale + Eases.EaseCircularOut(prog) * 0.5f, 0, 0);
			}

			return false;
		}

		private void DrawThoughPoliceHallucination(SpriteBatch spriteBatch)
		{
			if (toRender != null && toRender.Count > 0)
			{
				Texture2D tex = Assets.Misc.TriTellTransparent.Value;

				foreach (ThoughtPolice tp in toRender)
				{
					Vector2 pos = tp.NPC.Center + new Vector2(0, -16) - Main.screenPosition;
					var origin = new Vector2(0, tex.Height);
					var color = new Color(255, 100, 100, (byte)(125 * tp.localScanOpacity));

					spriteBatch.Draw(tex, pos, null, color, tp.AimRotation + 1.57f / 2f, origin, 1f, 0, 0);
				}

				toRender.RemoveAll(n => n is null || !n.NPC.active);
			}
		}
	}
}

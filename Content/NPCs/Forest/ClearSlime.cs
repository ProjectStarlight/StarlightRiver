using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Forest
{
	enum ClearSlimeState
	{
		bouncing = 0,
		attacking = 1
	}

	internal class ClearSlime : ModNPC
	{
		public Vector2 squishScale = Vector2.One;
		public int attackCooldown;

		public override string Texture => AssetDirectory.ForestNPC + Name;

		public ref float Timer => ref NPC.ai[0];
		public ClearSlimeState State
		{
			get => (ClearSlimeState)NPC.ai[1];
			set => NPC.ai[1] = (float)value;
		}
		public ref float JumpHeight => ref NPC.ai[2];
		public ref float LastLandX => ref NPC.ai[3];

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 30;
			NPC.damage = 20;
			NPC.knockBackResist = 0.1f;
			NPC.value = 200;
			NPC.width = 32;
			NPC.height = 28;
			NPC.aiStyle = -1;
			NPC.defense = 3;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
				new FlavorTextBestiaryInfoElement("Clear slimes may look like your garden variety slime, but they pack a much bigger punch! Get too close to one and it will give you a personal demonstration of the explosive properties of gel.")
			});
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return false;
		}

		public override void AI()
		{
			Timer++;

			if (attackCooldown > 0)
				attackCooldown--;

			if (JumpHeight < 6)
				JumpHeight = 6;

			squishScale.X += (1 - squishScale.X) * 0.09f;
			squishScale.Y += (1 - squishScale.Y) * 0.09f;

			switch (State)
			{
				case ClearSlimeState.bouncing:

					NPC.velocity.X *= 0.98f;

					NPC.TargetClosest();

					NPC.frame = Timer < 10 ? new Rectangle(0, 0, 32, 28) : new Rectangle(0, 28, 32, 28);

					if (NPC.target < 0)
						return;

					Player target = Main.player[NPC.target];

					if (target is null || !target.active || target.dead)
						return;

					if (NPC.velocity.Y == 0) // On the ground
					{
						squishScale.X += 0.045f;
						squishScale.Y -= 0.035f;
						NPC.velocity.X *= 0.7f;

						// pick the direction to jump in then jump
						NPC.direction = NPC.Center.X < target.Center.X ? 1 : -1;

						// If we're close to our target, trigger the attack
						if (Vector2.Distance(NPC.Center, target.Center) < 80 && attackCooldown <= 0)
						{
							State = ClearSlimeState.attacking;
							Timer = 0;
							return;
						}

						// Otherwise keep jumping!

						if (Timer > 10)
						{
							// Check if this is our first bounce, or if we didnt move with the last bounce
							if (LastLandX == 0 || NPC.Center.X == LastLandX)
							{
								// bounce higher next time if so!
								JumpHeight += 1;
								NPC.direction *= -1;
							}
							else
							{
								// reset bounce height
								JumpHeight -= 0.5f;
							}

							// set last X pos 
							LastLandX = NPC.Center.X;

							squishScale.X -= 0.65f;
							squishScale.Y += 0.5f;

							NPC.velocity.Y = -JumpHeight;
							NPC.velocity.X = NPC.direction * JumpHeight / 2;
							Timer = 0;
						}
					}
					else
					{
						Timer = 0;
					}

					break;

				case ClearSlimeState.attacking:

					NPC.velocity *= 0.8f;

					if (Timer < 100)
					{
						float rot = Main.rand.NextFloat(6.28f);
						Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * 76, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.UnitX.RotatedBy(rot) * -Main.rand.NextFloat(4, 6), 0, new Color(255, 80, 0, 0), 0.1f);
					}

					if (Timer == 20)
					{
						SoundHelper.PlayPitched("Magic/FireCast", 1, -0.25f, NPC.Center);
					}

					if (Timer < 120)
					{
						Lighting.AddLight(NPC.Center, Color.Orange.ToVector3() * (Timer / 120f));

						if (Main.rand.NextBool(3))
						{
							float fuseRot = NPC.rotation - 1.57f / 2f;
							fuseRot += NPC.velocity.Y * 0.03f;

							Vector2 fusePos = NPC.Center + Vector2.UnitX.RotatedBy(fuseRot) * 28 * (1f - Timer / 120f);
							fusePos += Vector2.One * 2;

							Dust.NewDustPerfect(fusePos, DustID.Torch, Vector2.UnitY * -Main.rand.NextFloat(1), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
						}
					}

					if (Timer == 120)
					{
						for (int k = 0; k < 20; k++)
						{
							float rot = Main.rand.NextFloat(6.28f);
							Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * 16, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.UnitX.RotatedBy(rot) * Main.rand.NextFloat(12), 0, new Color(255, 80, 0, 0), 0.15f);
							Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedBy(rot) * 16, ModContent.DustType<Dusts.PixelatedEmber>(), Vector2.UnitX.RotatedBy(rot) * Main.rand.NextFloat(4) + Vector2.UnitY * -Main.rand.NextFloat(1, 3), 0, new Color(255, Main.rand.Next(100, 200), 0, 0), 0.3f);
						}

						for (int k = 0; k < 8; k++)
						{
							Dust.NewDustPerfect(NPC.Center + Vector2.UnitX.RotatedByRandom(6.28f) * 8, ModContent.DustType<Dusts.PixelSmokeColor>(), Vector2.UnitY.RotatedByRandom(1.5f) * -Main.rand.NextFloat(5), 0, new Color(255, Main.rand.Next(80, 150), 0), Main.rand.NextFloat(0.1f));
						}

						SoundHelper.PlayPitched("Magic/FireHit", 1, 0, NPC.Center);

						foreach (Player player in Main.ActivePlayers)
						{
							if (CollisionHelper.CheckCircularCollision(NPC.Center, 80, player.Hitbox))
							{
								player.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), Main.masterMode ? 80 : Main.expertMode ? 40 : 20, 0);
								player.AddBuff(BuffID.OnFire, 120);
								player.velocity += Vector2.Normalize(player.Center - NPC.Center) * 10;
							}
						}
					}

					if (Timer >= 135)
					{
						attackCooldown = 120;
						State = ClearSlimeState.bouncing;
						Timer = 0;
					}

					break;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.NPCs.Forest.ClearSlime.Value;
			Texture2D over = Assets.NPCs.Forest.ClearSlimeOver.Value;
			Texture2D fuse = Assets.NPCs.Forest.ClearSlimeFuse.Value;

			int fuseLen = fuse.Width;
			float fuseOpacity = 1f;

			if (State == ClearSlimeState.attacking)
			{
				fuseLen = (int)(fuse.Width * (1f - Timer / 120f));
			}
			else
			{
				if (attackCooldown > 30)
					fuseLen = 0;

				if (attackCooldown <= 30)
					fuseOpacity = 1f - attackCooldown / 30f;
			}

			Rectangle fuseSource = new(0, 0, fuseLen, fuse.Height);
			Rectangle fuseTarget = new((int)(NPC.Center.X - Main.screenPosition.X), (int)(NPC.Center.Y - Main.screenPosition.Y), fuseLen, fuse.Height);

			float fuseRot = NPC.rotation - 1.57f / 2f;
			fuseRot += NPC.velocity.Y * 0.03f;

			spriteBatch.Draw(fuse, fuseTarget, fuseSource, drawColor * fuseOpacity, fuseRot, Vector2.Zero, 0, 0);

			spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, NPC.frame, drawColor * 0.8f, NPC.rotation, NPC.frame.Size() / 2f, squishScale * NPC.scale, 0, 0);

			if (State == ClearSlimeState.attacking && Timer > 60)
			{
				var fade = (Timer - 60) / 60f;
				Color color = Color.Lerp(Color.Orange, new Color(255, 255, 200), fade) * fade;
				spriteBatch.Draw(over, NPC.Center - Main.screenPosition, NPC.frame, color, NPC.rotation, NPC.frame.Size() / 2f, squishScale * NPC.scale, 0, 0);
			}

			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (State == ClearSlimeState.attacking)
			{
				ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
				{
					if (Timer <= 120)
					{
						Texture2D tex = Assets.Masks.GlowWithRing.Value;

						Color color = Color.Lerp(Color.Red, new Color(255, 100, 0), Timer / 120f) * (Timer / 120f);
						color.A = 0;

						if (Timer > 100)
							color *= 1f - (Timer - 100) / 20f;

						float scale = 150 + (1f - Timer / 120f) * 30;

						spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, color, 0, tex.Size() / 2f, scale / tex.Width, 0, 0);
					}
					else if (Timer > 120 && Timer < 135)
					{
						Texture2D tex = Assets.Masks.GlowAlpha.Value;
						Texture2D tex2 = Assets.StarTexture.Value;
						float fade = (Timer - 120) / 15f;

						Color color = Color.Lerp(Color.Orange, Color.Red, fade) * (1f - fade);
						color.A = 0;

						spriteBatch.Draw(tex, NPC.Center - Main.screenPosition, null, color, 0, tex.Size() / 2f, 180f / tex.Width * (1 + fade), 0, 0);
						spriteBatch.Draw(tex2, NPC.Center - Main.screenPosition, null, color, 0, tex2.Size() / 2f, 320f / tex2.Width * (1 + fade), 0, 0);
						spriteBatch.Draw(tex2, NPC.Center - Main.screenPosition, null, color, 1.57f / 2f, tex2.Size() / 2f, 290f / tex2.Width * (1 + fade), 0, 0);
					}
				});
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return spawnInfo.Player.ZoneForest && Main.dayTime ? 0.08f : 0f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 4, 5));
			npcLoot.Add(ItemDropRule.Common(ItemID.Wood, 1, 1, 1));
			npcLoot.Add(ItemDropRule.Common(ItemID.WandofSparking, 10, 1, 1));
		}
	}
}

using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Items.Moonstone;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Helpers;
using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Moonstone
{
	//bestiary needs to be done but there isnt a moonstone bestiary template thingy
	public class AngryCrescent : ModNPC
	{
		public int eyeFrame;

		public int flashTimer;
		public int lerpTimer;

		public int pointOnChain;

		public int flip;

		public int animationTimer;

		public bool curving;
		public bool animating;
		public bool initialized;
		public bool initializeAnimation;
		public bool blinking;
		public bool blinked;
		public bool playedWhoosh;

		public Vector2[] curvePositions;

		public Vector2 offset;

		public ref float AIState => ref NPC.ai[0];
		public ref float Timer => ref NPC.ai[1];
		public ref float AttackDelay => ref NPC.ai[2];

		public override string Texture => AssetDirectory.MoonstoneNPC + Name;

		public override void Load()
		{
			for (int i = 1; i < 4; i++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + i);
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Angry Crescent");
			NPCID.Sets.TrailCacheLength[Type] = 10;
			NPCID.Sets.TrailingMode[Type] = 0;
		}

		public override void SetDefaults()
		{
			NPC.width = 34;
			NPC.height = 34;
			NPC.knockBackResist = 1f;
			NPC.lifeMax = 25;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 35;
			NPC.aiStyle = -1;
			NPC.noTileCollide = true;
			NPC.friendly = false;

			NPC.HitSound = new Terraria.Audio.SoundStyle($"{nameof(StarlightRiver)}/Sounds/VitricBoss/ceramicimpact") with { Volume = 0.8f, Pitch = 0.15f };
			NPC.DeathSound = new Terraria.Audio.SoundStyle($"{nameof(StarlightRiver)}/Sounds/VitricBoss/ceramicimpact") with { Volume = 1.1f, Pitch = -0.2f };

			NPC.value = Item.buyPrice(silver: 1, copper: 15);
		}

		public override void AI()
		{
			if (flashTimer > 0)
				flashTimer--;

			if (AttackDelay > 0)
				AttackDelay--;

			if (animationTimer > 0)
				animationTimer--;

			switch (AIState)
			{
				case 0:

					if (!initialized)
					{
						flip = Main.rand.NextBool() ? -1 : 1;
						initialized = true;
					}

					NPC.TargetClosest(false);
					Player player = Main.player[NPC.target];
					NPC.rotation += NPC.velocity.Length() * 0.025f;

					if (AttackDelay <= 0 && !player.dead)
					{
						float speed = NPC.Distance(player.Center + new Vector2(150 * flip, -150)) * 0.015f;
						speed = Utils.Clamp(speed, 4.5f, 15f);
						NPC.velocity = NPC.DirectionTo(player.Center + new Vector2(150 * flip, -150)) * speed;
					}
					else if (player.dead && Main.rand.NextBool(25) && NPC.velocity.Length() < 5f) //do some idle movements if player dies
					{
						NPC.velocity += Main.rand.NextVector2Circular(3, 3);
					}

					if (AttackDelay > 0)
					{
						NPC.velocity *= 0.985f;
						Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 25, new Color(150, 120, 255), Main.rand.NextFloat(0.2f, 0.5f));

						if (AttackDelay == 1)
						{
							for (int i = 0; i < 25; i++)
							{
								Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.One.RotatedByRandom(6.28f) * 2f, 25, new Color(150, 120, 255), 0.75f);
							}
						}

						if (Main.rand.NextBool() && AttackDelay == 120 && !blinked)
							blinking = true;

						if (blinking)
						{
							if (++NPC.frameCounter > 7)
							{
								eyeFrame++;
								NPC.frameCounter = 0;
							}

							if (eyeFrame >= 7)
							{
								eyeFrame = 0;
								blinked = true;
								blinking = false;
							}
						}

						//recharge barrier if left alone
						BarrierNPC barrierNPC = NPC.GetGlobalNPC<BarrierNPC>();

						barrierNPC.maxBarrier = NPC.lifeMax;
						barrierNPC.rechargeRate = 12;
						barrierNPC.rechargeDelay = 240;
						barrierNPC.drawGlow = false;
					}
					else
					{
						BarrierNPC barrierNPC = NPC.GetGlobalNPC<BarrierNPC>();
						barrierNPC.rechargeRate = 0;
						barrierNPC.drawGlow = false;
					}

					if (NPC.Distance(player.Center) < 800f && AttackDelay <= 0 && !player.dead)
					{
						blinked = false;
						initializeAnimation = false;
						animating = false;
						curving = false;
						pointOnChain = 0;
						Timer = 0;
						AIState = 1;
						NPC.netUpdate = true;
						offset = Main.rand.NextVector2Circular(15, 15);
					}

					break;

				case 1: //target found

					Player target = Main.player[NPC.target];

					if (Timer == 0)
					{
						var curve = new BezierCurve(new Vector2[] { target.Center + new Vector2((150 + offset.X) * flip, -150 + offset.Y), target.Bottom + new Vector2(0, 325), target.Center + new Vector2((-150 + offset.X) * flip, -150 + offset.Y) });
						curvePositions = curve.GetPoints(15).ToArray();
					}

					if (NPC.Distance(curvePositions[0]) > 20f && !animating)
					{
						float speed = NPC.Distance(curvePositions[0]) * 0.065f;
						speed = Utils.Clamp(speed, 3.5f, 12f);
						NPC.velocity = NPC.DirectionTo(curvePositions[0]) * speed;
						NPC.rotation += NPC.velocity.Length() * 0.025f;
					}

					else if (!curving)
					{
						if (!animating)
						{
							animating = true;
							lerpTimer = 35;
						}

						if (--lerpTimer > 0)
						{
							NPC.Center = Vector2.Lerp(NPC.Center, curvePositions[0], 1 - lerpTimer / 35f);
							NPC.rotation += 0.1f;
						}
						else
						{
							if (!initializeAnimation)
							{
								Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.05f }, NPC.position);
								initializeAnimation = true;
								animationTimer = 35;
								flashTimer = 60;

								for (int i = 0; i < 30; i++)
								{
									Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.One.RotatedByRandom(6.28f) * 2f, 25, new Color(150, 120, 255), 0.75f);
								}
							}

							NPC.Center = curvePositions[0];

							if (animationTimer > 0)
								NPC.rotation += MathHelper.Lerp(0.1f, 0.55f, 1 - animationTimer / 35f);
							else
								curving = true;

						}
					}
					else
					{
						Timer++;

						if (Timer % 4 == 0 && pointOnChain < 14)
							pointOnChain++;

						Vector2 pos = curvePositions[pointOnChain];
						NPC.velocity = NPC.DirectionTo(pos) * (15f * EaseBuilder.EaseCircularInOut.Ease(pointOnChain / 14f));
						NPC.rotation += 0.55f;

						if (pointOnChain == 7 && !playedWhoosh)
						{
							Helper.PlayPitched("Effects/HeavyWhoosh", 0.5f, -0.05f, NPC.Center);
							playedWhoosh = true;
						}

						if (pointOnChain >= 14 && NPC.Distance(curvePositions[pointOnChain]) < 20f)
						{
							curving = false;
							playedWhoosh = false;
							AttackDelay = 180;
							AIState = 0;
							initialized = false;
							NPC.velocity *= 0.35f;
						}
					}

					break;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D eyeTex = ModContent.Request<Texture2D>(Texture + "_Eyes").Value;
			Vector2 origin = texture.Size() / 2f;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			for (int k = 0; k < NPC.oldPos.Length; k++)
			{
				float progress = (float)((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
				Vector2 drawPos = NPC.oldPos[k] - screenPos + origin + new Vector2(0f, NPC.gfxOffY);
				Color color = new Color(100, 60, 255) * EaseFunction.EaseQuarticOut.Ease(progress);
				Main.EntitySpriteDraw(texture, drawPos, null, color, NPC.rotation, origin, NPC.scale * EaseFunction.EaseQuadOut.Ease(progress), SpriteEffects.None, 0);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(texture, NPC.Center - screenPos, null, Color.White, NPC.rotation, texture.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);

			Color glowColor = new Color(150, 120, 255, 0) * Math.Clamp(EaseFunction.EaseQuadOut.Ease((float)Math.Sin((double)(Main.GlobalTimeWrappedHourly * 2f))), 0.25f, 1f);
			Main.spriteBatch.Draw(glowTex, NPC.Center - screenPos, null, glowColor, NPC.rotation, glowTex.Size() / 2, NPC.scale, SpriteEffects.None, 0f);

			if (flashTimer > 0)
				Main.spriteBatch.Draw(glowTex, NPC.Center - screenPos, null, new Color(255, 255, 255, 0) * MathHelper.Lerp(1, 0, 1 - flashTimer / 60f), NPC.rotation, glowTex.Size() / 2, NPC.scale, SpriteEffects.None, 0f);

			var frameRect = new Rectangle(0, 10 * eyeFrame, 16, 10);
			Vector2 directionTo = NPC.DirectionTo(Main.player[NPC.target].Center);
			Vector2 eyePosition = NPC.Center + new Vector2(-2, 14).RotatedBy(NPC.rotation) + (NPC.HasPlayerTarget ? directionTo * 5f : NPC.Center);
			Main.spriteBatch.Draw(eyeTex, eyePosition - screenPos, frameRect, Color.White, NPC.HasPlayerTarget ? directionTo.ToRotation() : NPC.rotation, frameRect.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);

			eyePosition = NPC.Center + new Vector2(-12, 8).RotatedBy(NPC.rotation) + (NPC.HasPlayerTarget ? directionTo * 5f : NPC.Center);
			Main.spriteBatch.Draw(eyeTex, eyePosition - screenPos, frameRect, Color.White, NPC.HasPlayerTarget ? directionTo.ToRotation() : NPC.rotation, frameRect.Size() / 2f, NPC.scale, SpriteEffects.None, 0f);

			return false;
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			if (NPC.life <= 0)
			{
				for (int i = 0; i < 2; i++)
				{
					Projectile.NewProjectileDirect(NPC.GetSource_Death(), NPC.Center, i == 1 ? NPC.velocity.RotatedBy(-MathHelper.PiOver2) * Main.rand.NextFloat(0.3f, 0.7f) :
						NPC.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(0.3f, 0.7f),
						ModContent.ProjectileType<AngryCrescentDeathProjectile>(), 0, 0, Main.myPlayer).frame = i;
				}

				for (int i = 0; i < 15; i++)
				{
					Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), NPC.velocity * Main.rand.NextFloat(2f), 35, new Color(150, 120, 255), Main.rand.NextFloat(0.3f, 0.5f));
				}

				for (int i = 0; i < 25; i++)
				{
					Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Vector2.One.RotatedByRandom(6.28f) * 2f, 25, new Color(150, 120, 255), 0.75f);
				}

				for (int i = 0; i < Main.rand.Next(2, 4); i++)
				{
					Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, NPC.velocity * 0.65f, Mod.Find<ModGore>("AngryCrescent" + "_Gore" + Main.rand.Next(1, 4)).Type).timeLeft = 60;
				}
			}
			else
			{
				for (int i = 0; i < Main.rand.Next(1, 3); i++)
				{
					Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, NPC.velocity * 0.55f, Mod.Find<ModGore>("AngryCrescent" + "_Gore" + Main.rand.Next(1, 4)).Type).timeLeft = 60;
				}

				for (int i = 0; i < 15; i++)
				{
					Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), (NPC.velocity * Main.rand.NextFloat(2f)).RotatedByRandom(0.45f), 35, new Color(150, 120, 255), Main.rand.NextFloat(0.3f, 0.5f));
				}
			}
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return curving;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return spawnInfo.Player.InModBiome(ModContent.GetInstance<MoonstoneBiome>()) ? 50 : 0;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MoonstoneOreItem>(), 3, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DianesPendant>(), 150));
		}
	}

	class AngryCrescentDeathProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.MoonstoneNPC + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescent Shard");
			Main.projFrames[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.frame = (int)Projectile.ai[0];

			if (Projectile.frame == 0)
			{
				Projectile.width = 24;
				Projectile.height = 36;
			}
			else
			{
				Projectile.width = 32;
				Projectile.height = 24;
			}

			Projectile.timeLeft = 90;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			Projectile.velocity *= 0.97f;
			Projectile.rotation += Projectile.velocity.Length() * 0.02f;
		}

		public override void Kill(int timeLeft)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath with { Volume = 0.7f, PitchVariance = 0.1f }, Projectile.position);

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0f, 0f, 35, new Color(150, 120, 255), Main.rand.NextFloat(0.3f, 0.5f));
			}

			for (int i = 0; i < Main.rand.Next(1, 3); i++)
			{
				Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * -1.25f, Mod.Find<ModGore>("AngryCrescent" + "_Gore" + Main.rand.Next(1, 4)).Type).timeLeft = 60;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White, Projectile.rotation, sourceRectangle.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

			Color glowColor = new Color(150, 120, 255, 0) * MathHelper.Lerp(0, 1f, 1 - Projectile.timeLeft / 90f);
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, glowTex.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame), glowColor, Projectile.rotation, glowTex.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame).Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}

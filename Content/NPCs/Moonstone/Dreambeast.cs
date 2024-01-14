using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Physics;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Moonstone
{
	internal class Dreambeast : ModNPC, IHintable, IDrawAdditive
	{
		public enum AIState : int
		{
			Idle,
			Rest,
			Charge,
			Shoot,
			MirageCharge,
			MirageShoot
		}

		public VerletChain[] chains = new VerletChain[6];

		public Vector2 homePos;
		public int flashTime;
		public int projChargeTime;
		public int frameCounter = 0;
		public bool idle = true;
		public bool driftClockwise = true; // Drift direction for shoot attack
		private bool hasLoaded = false; // Tentacle load state
		private int mirageCount = 0;
		private int parent = -1;

		private bool AppearVisible => Main.LocalPlayer.GetModPlayer<LunacyPlayer>().Insane;
		private Player Target => Main.player[NPC.target];
		private bool Mirage => Phase == AIState.MirageCharge || Phase == AIState.MirageShoot;

		public AIState Phase
		{
			get => (AIState)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		public float Rotation
		{
			get => NPC.ai[1];
			set
			{
				NPC.ai[1] = value;
				NPC.rotation = value;
			}
		}

		public ref float AttackTimer => ref NPC.ai[2];
		public ref float RandomTime => ref NPC.ai[3];

		public int TelegraphTime => 180;
		public Vector2 OrbPos => NPC.Center + Rotation.ToRotationVector2() * 80;

		public override string Texture => AssetDirectory.MoonstoneNPC + "Dreambeast";

		public override void SetDefaults()
		{
			NPC.width = 66;
			NPC.height = 66;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.aiStyle = -1;
			NPC.lifeMax = 666666;
			NPC.damage = 66;
			NPC.dontTakeDamage = true;
			NPC.immortal = true;
			NPC.knockBackResist = 0;
		}

		// Can only hit slightly lunatic players
		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return NPC.Opacity > 0.5f && target.GetModPlayer<LunacyPlayer>().lunacy > 20 && !Mirage;
		}

		// Hit damage scales with lunacy
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.GetModPlayer<LunacyPlayer>().ReturnSanity(10);
			modifiers.FinalDamage *= target.GetModPlayer<LunacyPlayer>().GetInsanityDamageMult();

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				var packet = new SanityHitPacket(target.whoAmI, 10);
				packet.Send();
			}
		}

		#region AI

		public override void AI()
		{
			// Generate chains if not loaded
			if (!hasLoaded && Main.netMode != NetmodeID.Server && !Mirage)
				InitChains();

			// Update chain position and color
			if (hasLoaded)
				UpdateChains();

			AttackTimer++;

			// Reset dreambeast flash if not visible yet
			if (!AppearVisible)
				flashTime = 0;

			if (homePos == default)
				homePos = NPC.Center;

			if (flashTime < 30 && Phase != 0)
				flashTime++;

			// Despawn when no players are lunatic
			if (Main.player.Count(n => n.active && n.GetModPlayer<LunacyPlayer>().lunacy > 0 && n.position.Distance(NPC.position) < 2000) == 0)
				NPC.active = false;

			if (Phase == AIState.Idle)
				PassiveBehavior();
			else if (Phase == AIState.Rest)
				AttackRest();
			else if (Phase == AIState.Charge)
				AttackCharge();
			else if (Phase == AIState.Shoot)
				AttackShoot();
			else if (Phase == AIState.MirageShoot || Phase == AIState.MirageCharge)
				AttackFakeout();

			// Idle animation
			if (idle && AttackTimer % 4 == 0)
				frameCounter = ++frameCounter % 7;
		}

		private void InitChains()
		{
			hasLoaded = true;

			for (int k = 0; k < chains.Length; k++)
			{
				VerletChain chain = chains[k];

				if (chain is null)
				{
					chains[k] = new VerletChain(24 + 2 * Main.rand.Next(4), true, NPC.Center, 4, false)
					{
						constraintRepetitions = 10,
						drag = 1.2f,
						forceGravity = -Vector2.UnitX,
						scale = 0.6f,
						parent = NPC,
					};
				}
			}
		}

		private void UpdateChains()
		{
			for (int k = 0; k < chains.Length; k++)
			{
				VerletChain chain = chains[k];
				chain.forceGravity = -Rotation.ToRotationVector2();
				float chainOriginRotation = (k - chains.Length / 2 + 1) * MathHelper.PiOver2 / chains.Length;
				chain?.UpdateChain(NPC.Center - Rotation.ToRotationVector2() * 80 + Rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2 - chainOriginRotation) * (k - chains.Length / 2 + 1) * 15);

				for (int i = 0; i < chain.ropeSegments.Count; i++)
				{
					chain.ropeSegments[i].posNow += Rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * (float)Math.Sin(StarlightWorld.visualTimer + 251 % (k + 1) + i / 4f) * i / 30;

					float sin = (float)Math.Sin(StarlightWorld.visualTimer + k);

					chain.ropeSegments[i].color = new Color(66 + (int)(30 * sin), 33, 144 - (int)(40 * sin));
				}
			}
		}

		/// <summary>
		/// picks a random valid target. Meaning a player within range of the beasts home base and that has the insanity debuff.
		/// </summary>
		private void PickTarget()
		{
			var possibleTargets = new List<Player>();
			float totalLunacy = 0;

			NPC.target = -1;

			// Logic to choose random insane player to be the target (chance scales with lunacy)
			foreach (Player player in Main.player)
			{
				if (player.active && player.GetModPlayer<LunacyPlayer>().Insane && Vector2.Distance(player.Center, homePos) < 2000)
				{
					possibleTargets.Add(player);
					totalLunacy += player.GetModPlayer<LunacyPlayer>().lunacy;
				}
			}

			if (possibleTargets.Count <= 0)
				return;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				float random = Main.rand.NextFloat(totalLunacy);

				foreach (Player player in possibleTargets)
				{
					if (random < player.GetModPlayer<LunacyPlayer>().lunacy)
					{
						NPC.target = player.whoAmI;
						break;
					}

					random -= player.GetModPlayer<LunacyPlayer>().lunacy;
				}

				NPC.netUpdate = true;
			}
		}

		/// <summary>
		/// Teleports the beast to a given location, as well as all of his chains' points. 
		/// </summary>
		/// <param name="target">The position to teleport to</param>
		private void Teleport(Vector2 target)
		{
			Vector2 diff = target - NPC.Center;
			NPC.Center = target;
			NPC.velocity *= 0;

			if (Phase == AIState.Idle)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.direction = Main.rand.NextBool() ? -1 : 1;
					NPC.netUpdate = true;
				}
			}
			else
			{
				Rotation = (Target.Center - NPC.Center).ToRotation();
				NPC.direction = (Target.Center - NPC.Center).X > 0 ? 1 : -1;
			}

			//We need to do this so the chains dont snap back like a rubber band (it still does so beats me lol)
			if (hasLoaded)
			{
				foreach (VerletChain chain in chains)
				{
					chain.startPoint += diff;

					foreach (RopeSegment segment in chain.ropeSegments)
					{
						segment.posOld += diff;
						segment.posNow += diff;
					}
				}
			}
		}

		/// <summary>
		/// What the NPC will be doing while its not actively attacking anyone.
		/// </summary>
		private void PassiveBehavior()
		{
			// Transition to Attack Rest logic when targets are available
			if (Main.player.Any(n => n.active && n.GetModPlayer<LunacyPlayer>().Insane && Vector2.Distance(n.Center, homePos) < 3000))
			{
				Phase = AIState.Rest;
				NPC.Opacity = 1;
				AttackTimer = 0;
				RandomTime = 90;
				return;
			}

			// Rotate back to normal
			NPC.Center += Vector2.One.RotatedBy(AttackTimer + RandomTime * 0.005f) * 0.25f;
			Rotation = NPC.direction == 1 ? 0 : MathHelper.Pi;

			if (NPC.Opacity < 1)
				NPC.Opacity += 0.05f;

			// Wait random time before teleporting
			if (AttackTimer > RandomTime)
			{
				NPC.Opacity = (20 - (AttackTimer - RandomTime)) / 20f;

				if (AttackTimer > RandomTime + 20)
				{
					AttackTimer = 0;

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						Player player = Main.player.FirstOrDefault(n => n.active && Vector2.Distance(n.Center, homePos) < 3000);

						RandomTime = Main.rand.Next(240, 360);

						if (player != null)
						{
							Teleport(player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(400, 600));
						}
						else
						{
							Teleport(homePos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(180, 420));
						}

						NPC.netUpdate = true;
					}
				}
			}
		}

		/// <summary>
		/// What the NPC does while it is attacking but isnt currently executing a specific attack. It chooses its attack from here.
		/// </summary>
		private void AttackRest()
		{
			// Disappear
			if (AttackTimer > 30)
				NPC.Opacity = (50 - AttackTimer) / 20f;

			// Slows down and rotates back to normal
			NPC.velocity *= 0.99f;

			float targetRotation = Rotation.ToRotationVector2().X > 0 ? 0 : MathHelper.Pi;
			float rotDifference = ((targetRotation - Rotation) % MathHelper.TwoPi + MathHelper.Pi * 3) % MathHelper.TwoPi - MathHelper.Pi;

			Rotation = MathHelper.Lerp(Rotation, Rotation + rotDifference, 0.005f);
			NPC.direction = Rotation.ToRotationVector2().X > 0 ? 1 : -1;

			// After disappearing
			if (AttackTimer > 60)
			{
				// Teleport
				if (AttackTimer == RandomTime)
				{
					PickTarget();

					if (NPC.target == -1)
					{
						Phase = 0;
						return;
					}

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						Teleport(Target.Center + (Main.rand.NextBool() ? -1 : 1) * Vector2.UnitX.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(450, 600));
						NPC.netUpdate = true;
					}
				}

				// Start next attack
				else if (AttackTimer > RandomTime + 90 || mirageCount < 1 && Main.rand.NextBool(5) && Main.netMode != NetmodeID.MultiplayerClient)
				{
					AttackTimer = 0;

					if (Main.netMode != NetmodeID.MultiplayerClient && NPC.target != -1)
					{
						RandomTime = Main.rand.Next(90, 150);
						Phase = Main.rand.NextBool(4) ? AIState.Shoot : AIState.Charge;

						NPC.netUpdate = true;
					}
				}

				// Spawn mirages
				else if (Main.netMode != NetmodeID.MultiplayerClient && (Main.rand.NextBool(300) || mirageCount < 1))
				{
					// Logic to choose random insane player to be the target of clone
					var possibleTargets = new List<Player>();
					foreach (Player player in Main.player)
					{
						if (player.active && player.GetModPlayer<LunacyPlayer>().Insane && Vector2.Distance(player.Center, homePos) < 2000)
						{
							possibleTargets.Add(player);
						}
					}

					if (possibleTargets.Count > 0)
					{
						Player target = possibleTargets[Main.rand.Next(possibleTargets.Count)];
						Vector2 clonePos = target.Center + (Main.rand.NextBool() ? -1 : 1) * Vector2.UnitX.RotatedByRandom(MathHelper.PiOver4) * Main.rand.NextFloat(450, 600);

						float cloneRotation = (target.Center - clonePos).RotatedByRandom(0.3f).RotatedByRandom(0.2f).ToRotation();
						AIState fakeout = Main.rand.NextBool(4) ? AIState.MirageShoot : AIState.MirageCharge;
						float stayTime = Main.rand.Next(60, 180);

						int cloneId = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clonePos.X, (int)clonePos.Y, ModContent.NPCType<Dreambeast>(), 0, (float)fakeout, cloneRotation, 0, stayTime, target.whoAmI);
						(Main.npc[cloneId].ModNPC as Dreambeast).parent = NPC.whoAmI;

						mirageCount++;
					}
				}
			}
		}

		/// <summary>
		/// Charge at the targeted player
		/// </summary>
		private void AttackCharge()
		{
			idle = false;

			if (NPC.Opacity < 1 && AttackTimer >= TelegraphTime - 90)
				NPC.Opacity += 0.01f;

			// When not charging, adjust aim
			if (AttackTimer < TelegraphTime)
			{
				frameCounter = 0;
				NPC.Center += Vector2.One.RotatedBy(AttackTimer + RandomTime * 0.005f) * 0.25f;

				float rotDifference = (((Target.Center - NPC.Center).ToRotation() - Rotation) % MathHelper.TwoPi + MathHelper.Pi * 3) % MathHelper.TwoPi - MathHelper.Pi;
				Rotation = MathHelper.WrapAngle(MathHelper.Lerp(Rotation, Rotation + rotDifference, 0.1f));

				NPC.direction = (Target.Center - NPC.Center).X > 0 ? 1 : -1;
			}

			if (AttackTimer == TelegraphTime)
				Helper.PlayPitched("VitricBoss/CeirosRoar", 0.8f, 0.5f, NPC.Center);

			// Funny animation numbers
			if (AttackTimer == TelegraphTime || AttackTimer == TelegraphTime + 2 || AttackTimer == TelegraphTime + 5 || AttackTimer == TelegraphTime + 12 || AttackTimer == TelegraphTime + 40)
				frameCounter++;

			if (AttackTimer == TelegraphTime + 45)
				frameCounter = 2;

			if (AttackTimer == TelegraphTime + 50 || AttackTimer == TelegraphTime + 55)
				frameCounter--;

			// Tentacle animation
			if (hasLoaded)
			{
				if (AttackTimer > TelegraphTime && AttackTimer < TelegraphTime + 15)
				{
					for (int k = 0; k < chains.Length; k++)
					{
						VerletChain chain = chains[k];

						for (int i = 0; i < chain.ropeSegments.Count; i++)
						{
							chain.ropeSegments[i].posNow += Rotation.ToRotationVector2() * 2 + Rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * (k - chains.Length / 2 + 1) * (float)(Math.Pow(2.5f * i / chain.ropeSegments.Count - 1, 2) + 1);
						}
					}
				}
			}

			// Acceleration and deceleration controls
			if (AttackTimer > TelegraphTime + 2 && AttackTimer < TelegraphTime + 12)
				NPC.velocity += Rotation.ToRotationVector2() * 6f;

			if (AttackTimer >= TelegraphTime + 38 && AttackTimer < TelegraphTime + 43)
				NPC.velocity -= Rotation.ToRotationVector2() * 4.8f;

			NPC.velocity *= 0.975f;

			// Charge ends
			if (AttackTimer > TelegraphTime + 90)
			{
				AttackTimer = 0;
				Phase = AIState.Rest;
				idle = true;
				return;
			}
		}

		/// <summary>
		/// Charge up a ranged attack at the targeted player
		/// </summary>
		private void AttackShoot()
		{
			if (NPC.Opacity < 1 && AttackTimer >= TelegraphTime - 90)
				NPC.Opacity += 0.01f;

			if (AttackTimer == 1)
				driftClockwise = !(Rotation < 0 && Rotation > -MathHelper.PiOver2 || Rotation < MathHelper.Pi && Rotation > MathHelper.PiOver2);

			idle = AttackTimer > TelegraphTime + 310;

			// Unfun animation numbers
			if (AttackTimer <= TelegraphTime + 40)
				frameCounter = 0;

			else if (AttackTimer == TelegraphTime + 305)
				frameCounter = 2;

			else if (AttackTimer == TelegraphTime + 45 || AttackTimer == TelegraphTime + 48 || AttackTimer == TelegraphTime + 50 || AttackTimer == TelegraphTime + 300)
				frameCounter++;

			else if (AttackTimer == TelegraphTime + 243 || AttackTimer == TelegraphTime + 308 || AttackTimer == TelegraphTime + 310)
				frameCounter--;

			// Orb sfx begins
			if (AttackTimer == TelegraphTime + 45)
				Helper.PlayPitched("VitricBoss/LaserCharge", 0.5f, 0.4f, NPC.Center);

			// Dreambeast orb charging
			if (AttackTimer > TelegraphTime + 45 && AttackTimer < TelegraphTime + 200)
			{
				projChargeTime++;

				Vector2 pos = Vector2.One.RotatedByRandom(MathHelper.TwoPi);
				pos.X /= 2;
				pos = pos.RotatedBy(Rotation);
				Dust.NewDustDirect(OrbPos + pos * 50, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 35, new Color(150, 120, 255) * 0.5f, Main.rand.NextFloat(0.6f, 0.8f)).velocity = -pos * 3 - Rotation.ToRotationVector2() * 3f;
			}

			// Major dreambeast orb charges
			if (AttackTimer == TelegraphTime + 45 || AttackTimer == TelegraphTime + 105 || AttackTimer == TelegraphTime + 165)
			{
				SoundEngine.PlaySound(SoundID.DD2_BookStaffCast, NPC.Center);

				for (int i = 0; i < 32; i++)
				{
					Vector2 pos = Vector2.One.RotatedBy(MathHelper.TwoPi * i / 32);
					pos.X /= 2;
					pos = pos.RotatedBy(Rotation);
					Dust.NewDustDirect(OrbPos + pos * 50, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 35, new Color(150, 120, 255), Main.rand.NextFloat(0.9f, 1.2f)).velocity = -pos * 3 - Rotation.ToRotationVector2() * 6f;
				}

				NPC.velocity -= Rotation.ToRotationVector2() * 3f;
			}

			// When the dreambeast bites down on the orb
			if (AttackTimer == TelegraphTime + 240)
			{
				Helper.PlayPitched("VitricBoss/CeirosPillarImpact", 0.5f, 0.5f, NPC.Center);
				Helper.PlayPitched("Magic/HolyCastShort", 1.2f, 0f, NPC.Center);

				// Crunch frame
				frameCounter = 5;
				projChargeTime = 0;

				for (int i = 0; i < 32; i++)
				{
					Dust.NewDustDirect(OrbPos, 0, 0, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0, 0, 35, new Color(150, 120, 255), Main.rand.NextFloat(1.5f, 2f)).velocity *= 2;
				}

				CameraSystem.shake += 8;
				NPC.velocity -= Rotation.ToRotationVector2() * 10f;

				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int i = 0; i < 3; i++)
					{
						Projectile.NewProjectile(NPC.GetSource_FromThis(), OrbPos, Rotation.ToRotationVector2().RotatedBy(0.2f * (i - 1)) * 5, ModContent.ProjectileType<DreambeastProj>(), 66, 2);
					}

					for (int i = 0; i < 2; i++)
					{
						Projectile.NewProjectile(NPC.GetSource_FromThis(), OrbPos, Rotation.ToRotationVector2().RotatedBy(MathHelper.Pi * 2 / 3 * (i == 0 ? -1 : 1)).RotatedByRandom(MathHelper.Pi / 3) * 10, ModContent.ProjectileType<DreambeastProjHome>(), 66, 2, -1, NPC.target);
					}
				}
			}

			// Dreambeast drift & general movement
			if (AttackTimer < TelegraphTime + 45)
				NPC.position += (Rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - AttackTimer / (TelegraphTime + 45)) * 2.5f * (driftClockwise ? 1 : -1);

			if (AttackTimer > TelegraphTime + 225)
				NPC.position += (Rotation + MathHelper.PiOver2).ToRotationVector2() * (AttackTimer - TelegraphTime - 210) / 100 * 2f * (driftClockwise ? 1 : -1);

			NPC.position += Rotation.ToRotationVector2() * (AttackTimer - TelegraphTime + 30) / 120;

			if (NPC.Center.Distance(Target.Center) > 600)
				NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.01f * (NPC.Center.Distance(Target.Center) - 600) / 400f);

			float rotDifference = (((Target.Center - NPC.Center).ToRotation() - Rotation) % MathHelper.TwoPi + MathHelper.Pi * 3) % MathHelper.TwoPi - MathHelper.Pi;
			Rotation = MathHelper.WrapAngle(MathHelper.Lerp(Rotation, Rotation + rotDifference, 0.1f));

			Rotation = MathHelper.Lerp(Rotation, Rotation + rotDifference, 0.1f);

			NPC.direction = Rotation < MathHelper.PiOver2 && Rotation > -MathHelper.PiOver2 ? 1 : -1;

			NPC.velocity += Target.velocity * 0.025f;

			NPC.velocity *= 0.975f;

			// Attack ends
			if (AttackTimer > TelegraphTime + 320)
			{
				NPC.velocity = NPC.position - NPC.oldPosition;
				AttackTimer = 0;
				projChargeTime = 0;
				Phase = AIState.Rest;
			}
		}

		/// <summary>
		/// Fake dreambeast AI
		/// </summary>
		private void AttackFakeout()
		{
			idle = false;

			NPC.direction = Rotation < MathHelper.PiOver2 && Rotation > -MathHelper.PiOver2 ? 1 : -1;

			float rotDifference = (((Target.Center - NPC.Center).ToRotation() - Rotation) % MathHelper.TwoPi + MathHelper.Pi * 3) % MathHelper.TwoPi - MathHelper.Pi;

			// Adjust aim and movement as if real attack (charge fakeout)
			if (Phase == AIState.MirageCharge)
			{
				if (AttackTimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.1f, 0.25f);
					NPC.netUpdate = true;
				}

				frameCounter = 0;
				NPC.Center += Vector2.One.RotatedBy(AttackTimer + RandomTime * 0.005f) * 0.25f;

				Rotation = MathHelper.Lerp(Rotation, Rotation + rotDifference, 0.1f);
			}
			else if (Phase == AIState.MirageShoot)
			{
				if (AttackTimer == 1)
					driftClockwise = !(Rotation < 0 && Rotation > -MathHelper.PiOver2 || Rotation < MathHelper.Pi && Rotation > MathHelper.PiOver2);

				NPC.position += (Rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - AttackTimer / 90) * 2.5f * (driftClockwise ? 1 : -1);

				NPC.position += Rotation.ToRotationVector2() * AttackTimer / 120;

				if (NPC.Center.Distance(Target.Center) > 600)
					NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.01f * (NPC.Center.Distance(Target.Center) - 600) / 400f);

				Rotation = MathHelper.WrapAngle(MathHelper.Lerp(Rotation, Rotation + rotDifference, 0.1f));

				NPC.direction = Rotation < MathHelper.PiOver2 && Rotation > -MathHelper.PiOver2 ? 1 : -1;

				NPC.velocity += Target.velocity * 0.025f;

				NPC.velocity *= 0.975f;
			}

			if (AttackTimer == RandomTime && Main.netMode != NetmodeID.MultiplayerClient && parent != -1)
				(Main.npc[parent].ModNPC as Dreambeast).mirageCount--;

			if (AttackTimer >= RandomTime + 60)
				NPC.active = false;
		}

		#endregion AI

		#region Drawing

		// Selects idle and attack frames
		public override void FindFrame(int frameHeight)
		{
			NPC.frame.Width = 244;
			NPC.frame.Height = 198;

			NPC.frame.X = 0;

			if (!idle)
				NPC.frame.X = 244;

			NPC.frame.Y = 198 * frameCounter;
		}

		// Draw aura effect using metaballs
		public void DrawToMetaballs(SpriteBatch spriteBatch)
		{
			if (NPC.active && !Mirage)
			{
				Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneNPC + "Dreambeast").Value;

				if (NPC.Opacity > 0.8f)
					spriteBatch.Draw(tex, (NPC.Center - Main.screenPosition) / 2, NPC.frame, Color.Black, Rotation + (NPC.direction == -1 ? MathHelper.Pi : 0), new Vector2(122, 99), 0.5f, NPC.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0);

				if (AppearVisible)
				{
					Effect effect = Filters.Scene["MoonstoneRunes"].GetShader().Shader;
					effect.Parameters["intensity"].SetValue(50f * MathF.Min(1 - NPC.Opacity, 1));
					effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.1f);

					effect.Parameters["noiseTexture1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise3").Value);
					effect.Parameters["noiseTexture2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise4").Value);
					effect.Parameters["color1"].SetValue(Color.Lerp(Color.Magenta, Color.Gray, (NPC.Opacity - 0.9f) * 10).ToVector4());
					effect.Parameters["color2"].SetValue(Color.Lerp(Color.Cyan, Color.Gray, (NPC.Opacity - 0.9f) * 10).ToVector4());
					effect.Parameters["opacity"].SetValue(NPC.Opacity);

					effect.Parameters["screenWidth"].SetValue(tex.Width);
					effect.Parameters["screenHeight"].SetValue(tex.Height);
					effect.Parameters["screenPosition"].SetValue(NPC.position);
					effect.Parameters["drawOriginal"].SetValue(false);

					spriteBatch.End();
					spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect);
				}

				spriteBatch.Draw(tex, (NPC.Center - Main.screenPosition) / 2, NPC.frame, Color.White * NPC.Opacity, Rotation + (NPC.direction == -1 ? MathHelper.Pi : 0), new Vector2(122, 99), 0.5f, NPC.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0);

				if (AppearVisible)
				{
					float opacity = 1 - (float)Math.Pow(2 * NPC.Opacity - 1, 2);

					Effect effect = Filters.Scene["MoonstoneBeastEffect"].GetShader().Shader;
					effect.Parameters["baseTexture"].SetValue(tex);
					effect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/MiscNoise2").Value);
					effect.Parameters["size"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
					effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.005f);
					effect.Parameters["opacity"].SetValue(opacity);
					effect.Parameters["noiseSampleSize"].SetValue(new Vector2(800, 800));
					effect.Parameters["noisePower"].SetValue(100f);

					spriteBatch.End();
					spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect);
				}

				// Draw afterimages when teleporting
				if (!Mirage)
				{
					for (int i = 0; i < 6; i++)
					{
						spriteBatch.Draw(tex, (NPC.Center + Vector2.UnitY.RotatedBy((1 - NPC.Opacity) * MathHelper.Pi + MathHelper.TwoPi * i / 6) * (1 - NPC.Opacity) * 100 - Main.screenPosition) / 2, NPC.frame, Color.White * NPC.Opacity, Rotation + (NPC.direction == -1 ? MathHelper.Pi : 0), new Vector2(122, 99), 0.5f, NPC.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0);
					}
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			// Tentacles
			if (AppearVisible && hasLoaded && NPC.Opacity > 0)
			{
				Effect shadowEffect = Filters.Scene["FireShader"].GetShader().Shader;

				var matrix = new Matrix
				(
					Main.GameViewMatrix.TransformationMatrix.M11, 0, 0, 0,
					0, Main.GameViewMatrix.TransformationMatrix.M22, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1
				);

				shadowEffect.Parameters["time"].SetValue(-Main.GameUpdateCount / 45f);
				shadowEffect.Parameters["upscale"].SetValue(matrix);
				shadowEffect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "ShadowTrail").Value);

				foreach (VerletChain chain in chains)
				{
					chain.DrawStrip(PrepareStrip(chain), shadowEffect);
				}
			}

			// Draw flash
			if (AppearVisible && flashTime > 0 && !Mirage)
			{
				Texture2D flashTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
				Color color = Color.White * (1 - flashTime / 30f);
				color.A = 0;

				spriteBatch.Draw(flashTex, NPC.Center - Main.screenPosition, null, color, 0, flashTex.Size() / 2, flashTime, 0, 0);
			}

			// Draw Moonball
			if (AppearVisible && Phase == AIState.Shoot && projChargeTime > 0)
			{
				Effect effect = Filters.Scene["CrescentOrb"].GetShader().Shader;
				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/CrescentQuarterstaffMap").Value);
				effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/LaserBallDistort").Value);
				effect.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.01f);
				effect.Parameters["opacity"].SetValue(1);

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

				Texture2D orb = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneItem + "CrescentOrb").Value;
				spriteBatch.Draw(orb, OrbPos - Main.screenPosition, null, Color.White * (projChargeTime / 30f), Main.GameUpdateCount * 0.01f, orb.Size() / 2, projChargeTime / 150f, 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			var glowColor = new Color(78, 87, 191);
			spriteBatch.Draw(tex, OrbPos - Main.screenPosition, tex.Frame(), glowColor * Math.Min(projChargeTime / 30f, 1), 0, tex.Size() / 2, 1.8f * projChargeTime / 150f, 0, 0);

			// Draw Mirage
			if (AppearVisible && (Mirage || (Phase == AIState.Charge || Phase == AIState.Shoot) && AttackTimer < TelegraphTime))
			{
				float mirageOpacity = 0.5f;

				if (AttackTimer <= 50)
					mirageOpacity = AttackTimer * 0.01f;
				else if (AttackTimer > RandomTime)
					mirageOpacity = 0.5f - 0.01f * (AttackTimer - RandomTime);

				Texture2D mirageTex = ModContent.Request<Texture2D>(AssetDirectory.MoonstoneNPC + "Dreambeast").Value;

				Effect effect = Filters.Scene["MoonstoneRunes"].GetShader().Shader;
				effect.Parameters["intensity"].SetValue(50f * MathF.Min(1 - mirageOpacity, 1));
				effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.1f);

				effect.Parameters["noiseTexture1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise3").Value);
				effect.Parameters["noiseTexture2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/MiscNoise4").Value);
				effect.Parameters["color1"].SetValue(Color.Lerp(Color.Magenta, Color.Gray, (mirageOpacity - 0.9f) * 10).ToVector4());
				effect.Parameters["color2"].SetValue(Color.Lerp(Color.Cyan, Color.Gray, (mirageOpacity - 0.9f) * 10).ToVector4());
				effect.Parameters["opacity"].SetValue(mirageOpacity);

				effect.Parameters["screenWidth"].SetValue(mirageTex.Width);
				effect.Parameters["screenHeight"].SetValue(mirageTex.Height);
				effect.Parameters["screenPosition"].SetValue(NPC.position);
				effect.Parameters["drawOriginal"].SetValue(false);

				spriteBatch.Draw(mirageTex, NPC.Center + NPC.netOffset - Main.screenPosition, NPC.frame, Color.White * mirageOpacity, Rotation + (NPC.direction == -1 ? MathHelper.Pi : 0), new Vector2(122, 99), 1, NPC.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0);

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, effect);

				spriteBatch.Draw(mirageTex, NPC.Center + NPC.netOffset - Main.screenPosition, NPC.frame, Color.White * mirageOpacity, Rotation + (NPC.direction == -1 ? MathHelper.Pi : 0), new Vector2(122, 99), 1, NPC.direction == -1 ? SpriteEffects.FlipHorizontally : 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		// Vertex Buffer for tentacle primitives (pulled from Ceiros)
		public Func<Vector2, VertexBuffer> PrepareStrip(VerletChain chain)
		{
			return (Vector2 offset) =>
			{
				var buff = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColorTexture), chain.segmentCount * 9 - 6, BufferUsage.WriteOnly);
				var verticies = new VertexPositionColorTexture[chain.segmentCount * 9 - 6];

				float rotation = (chain.ropeSegments[0].ScreenPos - chain.ropeSegments[1].ScreenPos).ToRotation() + (float)Math.PI / 2;

				verticies[0] = new VertexPositionColorTexture((chain.ropeSegments[0].ScreenPos + offset + Vector2.UnitY.RotatedBy(rotation - Math.PI / 4) * -5).Vec3().ScreenCoord(), chain.ropeSegments[0].color * NPC.Opacity, new Vector2(0, 0.2f));
				verticies[1] = new VertexPositionColorTexture((chain.ropeSegments[0].ScreenPos + offset + Vector2.UnitY.RotatedBy(rotation + Math.PI / 4) * -5).Vec3().ScreenCoord(), chain.ropeSegments[0].color * NPC.Opacity, new Vector2(0, 0.8f));
				verticies[2] = new VertexPositionColorTexture((chain.ropeSegments[1].ScreenPos + offset).Vec3().ScreenCoord(), chain.ropeSegments[1].color, new Vector2(0, 0.5f));

				for (int k = 1; k < chain.segmentCount - 1; k++)
				{
					float progress = k / 3f;
					float rotation2 = (chain.ropeSegments[k - 1].ScreenPos - chain.ropeSegments[k].ScreenPos).ToRotation() + (float)Math.PI / 2;
					float scale = 2.4f;

					int point = k * 9 - 6;

					verticies[point] = new VertexPositionColorTexture((chain.ropeSegments[k].ScreenPos + offset + Vector2.UnitY.RotatedBy(rotation2 - Math.PI / 4) * -(chain.segmentCount - k) * scale).Vec3().ScreenCoord(), chain.ropeSegments[k].color * NPC.Opacity, new Vector2(progress, 0.2f));
					verticies[point + 1] = new VertexPositionColorTexture((chain.ropeSegments[k].ScreenPos + offset + Vector2.UnitY.RotatedBy(rotation2 + Math.PI / 4) * -(chain.segmentCount - k) * scale).Vec3().ScreenCoord(), chain.ropeSegments[k].color * NPC.Opacity, new Vector2(progress, 0.8f));
					verticies[point + 2] = new VertexPositionColorTexture((chain.ropeSegments[k + 1].ScreenPos + offset).Vec3().ScreenCoord(), chain.ropeSegments[k + 1].color * NPC.Opacity, new Vector2(progress + 1 / 3f, 0.5f));

					int extra = k == 1 ? 0 : 6;
					verticies[point + 3] = verticies[point];
					verticies[point + 4] = verticies[point - (3 + extra)];
					verticies[point + 5] = verticies[point - (1 + extra)];

					verticies[point + 6] = verticies[point - (2 + extra)];
					verticies[point + 7] = verticies[point + 1];
					verticies[point + 8] = verticies[point - (1 + extra)];
				}

				buff.SetData(verticies);

				return buff;
			};
		}

		// Draw tentacle glowy bits
		public void DrawAdditive(SpriteBatch sb)
		{
			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;

			if (hasLoaded)
			{
				for (int k = 0; k < chains.Length; k++)
				{
					VerletChain chain = chains[k];

					for (int i = 0; i < chain.ropeSegments.Count; i++)
					{
						RopeSegment segment = chain.ropeSegments[i];
						float progress = 1.35f - (float)k / chain.segmentCount;
						float progress2 = 1.22f - (float)k / chain.segmentCount;
						sb.Draw(tex, segment.posNow - Main.screenPosition, null, segment.color * progress * 0.175f * NPC.Opacity, 0, tex.Size() / 2, progress2, 0, 0);
					}
				}
			}
		}

		#endregion Drawing

		public string GetHint()
		{
			return "It's not real. It's not real. It's not real. IT'S NOT REAL. IT'S NOT REAL. IT'S NOT REAL.";
		}
	}
}
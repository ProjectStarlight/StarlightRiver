using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core.Systems.AuroraWaterSystem
{
	class SwimPlayer : ModPlayer
	{
		int boostCD = 0;
		float targetRotation = 0;
		float realRotation = 0;
		float armRotation;

		int emergeTime = 0;
		float emergeRotatio = 0;

		int emergeBoostTime = 0;
		Vector2 emergeBoostSpeed = default;

		public bool wasSwimming;

		public bool ShouldSwim { get; set; }

		public float SwimSpeed { get; set; }

		private void CheckAuroraSwimming() //checks for if the Player should be swimming
		{
			bool canSwim = Player.grapCount <= 0 && !Player.mount.Active;

			if (canSwim)
			{
				bool swimming = false;

				if (Player.HasBuff(BuffType<PrismaticDrown>())) //TODO: Change this to be set on the arena instead of checking for this buff probably
					swimming = true;

				for (int x = 0; x < 2; x++)
				{
					for (int y = 0; y < 3; y++)
					{
						int realX = (int)(Player.position.X / 16) + x;
						int realY = (int)(Player.position.Y / 16) + y;

						if (WorldGen.InWorld(realX, realY))
						{
							Tile tile = Framing.GetTileSafely(realX, realY);

							if (tile.Get<AuroraWaterData>().HasAuroraWater)
								swimming = true;
						}
					}
				}

				if (swimming)
				{
					if (!wasSwimming)
					{
						AuroraWaterSystem.AddRipple(Player.Center, 1.5f, 0.03f);

						SoundHelper.PlayPitched("Magic/WaterWoosh", 0.8f, 0, Player.Center);

						for (int k = 0; k < 20; k++)
						{
							Dust.NewDustPerfect(Player.Center, DustType<Content.Dusts.AuroraWaterFast>(), Main.rand.NextVector2Circular(2, 2), 0, new Color(200, 220, 255) * 0.4f, Main.rand.NextFloat(0.2f, 0.8f));
						}

						wasSwimming = true;
					}

					ShouldSwim = true;
				}
			}
		}

		public override void PreUpdate()
		{
			CheckAuroraSwimming();

			if (emergeBoostTime > 0)
			{
				// Special effects for when we re-enter out of an exit boost
				if (ShouldSwim)
				{
					emergeBoostTime = 0;
					Player.velocity = Vector2.Normalize(Player.velocity) * 0.5f;

					SoundHelper.PlayPitched("Magic/WaterWoosh", 1f, -0.3f, Player.Center);

					for (int k = 0; k < 20; k++)
					{
						Dust.NewDustPerfect(Player.Center, DustType<Content.Dusts.AuroraWaterFast>(), -Vector2.Normalize(Player.velocity).RotatedByRandom(0.5f) * Main.rand.NextFloat(15), 0, new Color(200, 220, 255) * 0.4f, Main.rand.NextFloat(0.2f, 0.8f));
					}
				}
				else
				{
					// Makes the exit boost "flat"
					emergeBoostTime--;
					Player.velocity = emergeBoostSpeed * (0.1f + emergeBoostTime / 20f);

					for (int k = 0; k < 5; k++)
					{
						Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(16, 16), DustType<Content.Dusts.PixelatedGlow>(), -Vector2.Normalize(Player.velocity).RotatedByRandom(0.5f) * Main.rand.NextFloat(5), 0, new Color(100, Main.rand.Next(150, 255), 255, 0), Main.rand.NextFloat(0.1f, 0.2f));
					}
				}
			}

			if (emergeTime == 18) //reset jumps
			{
				Player.RefreshExtraJumps();
				Player.rocketTime = Player.rocketTimeMax;
				Player.wingTime = Player.wingTimeMax;
				emergeRotatio = realRotation;

				SoundHelper.PlayPitched("Magic/WaterWoosh", 0.8f, 0, Player.Center);

				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(Player.Center, DustType<Content.Dusts.AuroraWaterFast>(), Main.rand.NextVector2Circular(2, 2), 0, new Color(200, 220, 255) * 0.4f, Main.rand.NextFloat(0.2f, 0.8f));
				}

				if (boostCD > 20 && Player.velocity.Length() > 0)
				{
					SoundHelper.PlayPitched("SquidBoss/LightSplash", 1f, 0.2f, Player.Center);

					for (int k = 0; k < 20; k++)
					{
						Dust.NewDustPerfect(Player.Center, DustType<Content.Dusts.AuroraWaterFast>(), Vector2.Normalize(Player.velocity).RotatedByRandom(0.5f) * Main.rand.NextFloat(15), 0, new Color(200, 220, 255) * 0.4f, Main.rand.NextFloat(0.2f, 0.8f));
					}

					emergeBoostTime = 20;
					emergeBoostSpeed = Vector2.Normalize(Player.velocity) * 20;

					boostCD = 0;
				}

				wasSwimming = false;
			}

			if (!ShouldSwim) //reset stuff when the Player isnt swimming
			{
				if (boostCD > 0)
				{
					//boostCD = 0;
					Player.UpdateRotation(0);
				}

				if (emergeTime <= 0) //20 frames for the Player to rotate back
					return;
			}

			targetRotation = ShouldSwim ? Player.velocity.ToRotation() : 1.57f + 3.14f;

			realRotation %= 6.28f; //handles the rotation, ensures the Player wont randomly snap to rotation when entering/leaving swimming

			if (Math.Abs(targetRotation - realRotation) % 6.28f > 0.21f)
			{
				static float Mod(float a, float b)
				{
					return a % b > 0 ? a % b : a % b + b;
				}

				if (Mod(targetRotation, 6.28f) > Mod(realRotation, 6.28f))
					realRotation += 0.2f;
				else
					realRotation -= 0.2f;
			}
			else
			{
				realRotation = targetRotation;
			}

			// Forces the rotation target to be upright if the player is emerging
			if (emergeTime < 18)
				realRotation = -MathHelper.PiOver2 + (emergeRotatio + MathHelper.PiOver2) * (emergeTime - 1) / 19f;

			Player.fullRotationOrigin = Player.Size / 2; //so the Player rotates around their center... why is this not the default?
			Player.fullRotation = realRotation + MathHelper.PiOver2;

			if (Player.itemAnimation != 0 && Player.HeldItem.useStyle != Terraria.ID.ItemUseStyleID.Swing && Player.itemAnimation == Player.itemAnimationMax - 1) //corrects the rotation on used Items
				Player.itemRotation -= realRotation + 1.57f;

			if (!ShouldSwim) //return later so rotation logic still runs
			{
				if (boostCD > 0)
					boostCD--;

				if (emergeTime > 0)
					emergeTime--;

				return;
			}

			Player.ConsumeAllExtraJumps();
			Player.rocketTime = -1;
			Player.wingTime = -1;

			emergeTime = 20; //20 frames for the Player to rotate back, reset while swimming

			Player.fallStart = (int)Player.position.Y; //Reset fall damage constantly while swimming

			if (Player.itemAnimation == 0)
				Player.bodyFrame = new Rectangle(0, 56 * (int)(1 + Main.GameUpdateCount / 10 % 5), 40, 56);

			Player.legFrame = new Rectangle(0, 56 * (int)(5 + Main.GameUpdateCount / 7 % 3), 40, 56);

			float speed = 0.02f * SwimSpeed;

			Vector2 dir = Vector2.Zero;
			if (Player.controlRight)
				dir.X += 1;

			if (Player.controlLeft)
				dir.X -= 1;

			if (Player.controlDown)
				dir.Y += 1;

			if (Player.controlUp)
				dir.Y -= 1;

			if (dir.Length() > 0)
				Player.velocity += Vector2.Normalize(dir) * speed;

			Player.gravity = 0;

			float slow = Player.velocity.Length() > SwimSpeed ? 0.5f : 0.95f;
			Player.velocity *= slow;

			if (Main.GameUpdateCount % 10 == 0)
				AuroraWaterSystem.AddRipple(Player.Center, 0.5f + Player.velocity.Length() * 0.05f, 0.02f);

			if (Player.controlJump && boostCD <= 0)
			{
				SoundHelper.PlayPitched("SquidBoss/LightSplash", Main.rand.NextFloat(0.5f, 0.8f), Main.rand.NextFloat(-0.9f, -0.6f), Player.Center);
				SoundHelper.PlayPitched("Magic/WaterWoosh", Main.rand.NextFloat(0.3f, 0.6f), Main.rand.NextFloat(-0.5f, -0.2f), Player.Center);
				AuroraWaterSystem.AddRipple(Player.Center, 1.5f, 0.03f);

				boostCD = 60;
			}

			if (boostCD > 20)
			{
				float timer = (boostCD - 20) / 40f;
				float angle = timer * 6.28f * 2f;
				Vector2 vel = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.15f);
				Player.UpdateRotation(angle);

				for (int k = 0; k < 2; k++)
				{
					float prog = k / 2f;
					var off = new Vector2((float)Math.Cos(angle + 1 / 20f * 6.28f * prog) * 18, (float)Math.Sin(angle + 1 / 20f * 6.28f * prog) * 4);
					Color color = new Color(timer, 1 - timer, 0.5f + 0.5f * MathF.Sin(timer * 3.14f), 0) * MathF.Sin(timer * 3.14f) * 0.3f;

					var l = Dust.NewDustPerfect(Player.Center + Player.velocity * prog + off.RotatedBy(Player.fullRotation), DustType<Content.Dusts.PixelatedEmber>(), vel, 0, color, Main.rand.NextFloat(0.1f, 0.23f));
					var r = Dust.NewDustPerfect(Player.Center + Player.velocity * prog - off.RotatedBy(Player.fullRotation), DustType<Content.Dusts.PixelatedEmber>(), vel, 0, color, Main.rand.NextFloat(0.1f, 0.23f));
					l.noGravity = true;
					r.noGravity = true;
				}
			}

			if (boostCD > 40)
			{
				if (Player.velocity == Vector2.Zero)
					Player.velocity = new Vector2(0, -0.01f);

				Player.velocity += Vector2.Normalize(Player.velocity) * 0.08f * SwimSpeed;
				Player.AddBuff(Terraria.ID.BuffID.Cursed, 1, true);
			}
			else
			{
				Player.UpdateRotation(0);
			}

			if (boostCD > 0)
				boostCD--;

			if (emergeTime > 0)
				emergeTime--;
		}

		public override void PostUpdate()
		{
			if (emergeTime > 0)
			{
				armRotation += 0.04f + Player.velocity.Length() * 0.1f * (float)Math.Sin((armRotation + 1.57f) % 3.14f);

				Player.SetCompositeArmFront(true, 0, armRotation * Player.direction);
				Player.SetCompositeArmBack(true, 0, armRotation * Player.direction + 3.14f);

				Player.legFrame = new Rectangle(0, 56 * (6 + (int)(Main.GameUpdateCount * 0.35f) % 14), 40, 56);
			}
		}

		public override void ResetEffects()
		{
			ShouldSwim = false;
			SwimSpeed = 10f;
		}
	}
}
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

		public bool wasSwimming;

		public bool ShouldSwim { get; set; }

		public float SwimSpeed { get; set; }

		private void CheckAuroraSwimming() //checks for if hte Player should be swimming
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

							if (tile.Get<AuroraWaterData>().HasAuroraWater) //TODO: Integrate with properly ported aurora water system
								swimming = true;
						}
					}
				}

				if (swimming)
				{
					if (!wasSwimming)
					{
						Helper.PlayPitched("Magic/WaterWoosh", 0.8f, 0, Player.Center);

						for (int k = 0; k < 20; k++)
						{
							Dust.NewDustPerfect(Player.Center, DustType<Content.Dusts.AuroraWaterFast>(), Main.rand.NextVector2Circular(2, 2), 0, new Color(200, 220, 255) * 0.4f, Main.rand.NextFloat(0.2f, 0.8f));
						}

						wasSwimming = true;
					}

					ShouldSwim = true;
					SwimSpeed *= 0.7f;
				}
			}
		}

		public override void PreUpdate()
		{
			CheckAuroraSwimming();

			if (emergeTime == 18) //reset jumps
			{
				Player.canJumpAgain_Fart = true;
				Player.canJumpAgain_Sail = true;
				Player.canJumpAgain_Cloud = true;
				Player.canJumpAgain_Blizzard = true;
				Player.canJumpAgain_Sandstorm = true;
				Player.rocketTime = Player.rocketTimeMax;
				Player.wingTime = Player.wingTimeMax;

				Helper.PlayPitched("Magic/WaterWoosh", 0.8f, 0, Player.Center);

				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(Player.Center, DustType<Content.Dusts.AuroraWaterFast>(), Main.rand.NextVector2Circular(2, 2), 0, new Color(200, 220, 255) * 0.4f, Main.rand.NextFloat(0.2f, 0.8f));
				}

				wasSwimming = false;
			}

			if (!ShouldSwim) //reset stuff when the Player isnt swimming
			{
				if (boostCD > 0)
				{
					boostCD = 0;
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

			Player.canJumpAgain_Fart = false;
			Player.canJumpAgain_Sail = false;
			Player.canJumpAgain_Cloud = false;
			Player.canJumpAgain_Blizzard = false;
			Player.canJumpAgain_Sandstorm = false;
			Player.rocketTime = -1;
			Player.wingTime = -1;

			emergeTime = 20; //20 frames for the Player to rotate back, reset while swimming

			if (Player.itemAnimation == 0)
				Player.bodyFrame = new Rectangle(0, 56 * (int)(1 + Main.GameUpdateCount / 10 % 5), 40, 56);

			Player.legFrame = new Rectangle(0, 56 * (int)(5 + Main.GameUpdateCount / 7 % 3), 40, 56);

			float speed = 0.2f * SwimSpeed;

			if (Player.controlRight)
				Player.velocity.X += speed; //there should probably be a better way of doing this?

			if (Player.controlLeft)
				Player.velocity.X -= speed;

			if (Player.controlDown)
				Player.velocity.Y += speed;

			if (Player.controlUp)
				Player.velocity.Y -= speed;

			Player.gravity = 0;
			Player.velocity *= 0.95f;

			if (Player.controlJump && boostCD <= 0)
			{
				Helper.PlayPitched("SquidBoss/MagicSplash", 1f, -0.5f, Player.Center);
				Helper.PlayPitched("SquidBoss/MagicSplash", 1f, 0f, Player.Center);

				boostCD = 60;
			}

			if (boostCD > 40)
			{
				float timer = (boostCD - 40) / 20f;
				float angle = timer * 6.28f;
				Vector2 vel = -Player.velocity * 0f;
				Player.UpdateRotation(angle);

				for (int k = 0; k < 2; k++)
				{
					float prog = k / 2f;
					var off = new Vector2((float)Math.Cos(angle + 1 / 20f * 6.28f * prog) * 18, (float)Math.Sin(angle + 1 / 20f * 6.28f * prog) * 4);

					var l = Dust.NewDustPerfect(Player.Center + Player.velocity * prog + off.RotatedBy(Player.fullRotation), DustType<Content.Dusts.Cinder>(), vel, 0, new Color(1 - timer, timer, 1), Main.rand.NextFloat(0.4f, 0.7f));
					var r = Dust.NewDustPerfect(Player.Center + Player.velocity * prog - off.RotatedBy(Player.fullRotation), DustType<Content.Dusts.Cinder>(), vel, 0, new Color(1 - timer, timer, 1), Main.rand.NextFloat(0.4f, 0.7f));
					l.noGravity = true;
					r.noGravity = true;
				}

				if (Player.velocity == Vector2.Zero)
					Player.velocity = new Vector2(0, -0.01f);

				Player.velocity += Vector2.Normalize(Player.velocity) * 0.8f * SwimSpeed;
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
			SwimSpeed = 1f;
		}
	}
}

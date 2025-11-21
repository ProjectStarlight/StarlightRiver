using StarlightRiver.Content.NPCs.BaseTypes;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	internal class VitricBossPlatformUp : MovingPlatform
	{
		public const int MaxHeight = 880;
		public override string Texture => AssetDirectory.VitricBoss + "VitricBossPlatform";

		public VitricBackdropLeft parent;
		public Vector2 storedCenter;

		public int masterExpirationTimer;

		public override bool CheckActive()
		{
			return false;
		}

		public override void SafeSetDefaults()
		{
			NPC.width = 220;
			NPC.height = 16;
			NPC.noTileCollide = true;
			NPC.dontCountMe = true;
			NPC.lifeMax = 10;
		}

		public virtual bool FindParent()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC NPC = Main.npc[i];
				if (NPC.active && NPC.type == ModContent.NPCType<VitricBackdropLeft>())
				{
					parent = NPC.ModNPC as VitricBackdropLeft;
					return true;
				}
			}

			return false;
		}

		public override void SafeAI()
		{
			/*AI fields:
             * 0: state
             * 1: rise time left
             * 2: acceleration delay
             */

			if (parent == null || !parent.NPC.active)
				FindParent();

			if (NPC.ai[0] == 0)
			{
				if (NPC.ai[1] > 0)
				{
					NPC.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Risetime;
					NPC.ai[1]--;
				}
				else
				{
					NPC.velocity.Y = 0;
				}
			}

			if (NPC.ai[0] == 1)
			{
				NPC.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Scrolltime * (1f / parent.NPC.ai[3]);
				if (NPC.position.Y <= StarlightWorld.vitricBiome.Y * 16 + 16 * 16)
					NPC.position.Y += MaxHeight;
			}

			if (storedCenter == Vector2.Zero && NPC.velocity.Y == 0)
				storedCenter = NPC.Center;

			if (Main.masterMode)
			{
				if (!dontCollide)
				{
					Rectangle checkBox = NPC.Hitbox;
					checkBox.Inflate(8, 8);

					if (Main.player.Any(Player => Player.active && Player.velocity.Y == 0 && Player.Hitbox.Intersects(checkBox)))
						masterExpirationTimer += 2;
					else if (masterExpirationTimer > 0)
						masterExpirationTimer--;

					if (masterExpirationTimer > 30)
					{
						Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), 12), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.UnitY * Main.rand.NextFloat(-4, -1), 0, new Color(255, (int)(50 + masterExpirationTimer / 150f * 200), 50, 0), Main.rand.NextFloat(0.05f, 0.2f) * masterExpirationTimer / 150f);
					}

					if (masterExpirationTimer > 150)
					{
						dontCollide = true;
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<FireRingHostile>(), 40, 0, Main.myPlayer, NPC.width * 0.4f);
						Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, NPC.Center);
						SoundHelper.PlayPitched("Magic/FireHit", 1f, -0.5f, NPC.Center);

						for (int k = 0; k < NPC.width; k += Main.rand.Next(2, 4))
						{
							Dust.NewDustPerfect(NPC.position + new Vector2(k, Main.rand.NextFloat(-4, 4)), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(40, 40), 0, new Color(255, Main.rand.Next(120, 200), 50, 0), Main.rand.NextFloat(0.2f, 0.3f));
						}
					}
				}
				else
				{
					masterExpirationTimer--;

					if (masterExpirationTimer <= 0)
					{
						dontCollide = false;

						for (int k = 0; k < NPC.width; k += Main.rand.Next(2, 4))
						{
							Dust.NewDustPerfect(NPC.position + new Vector2(k, Main.rand.NextFloat(-4, 4)), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(6, 6), 0, new Color(50, 190, 255, 0), Main.rand.NextFloat(0.05f, 0.2f));
						}
					}
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.Bosses.VitricBoss.VitricBossPlatform.Value;

			if (dontCollide)
				drawColor *= 0.25f;

			drawColor.A = 255;

			spriteBatch.Draw(tex, NPC.Center + Vector2.UnitY * 20 - screenPos, null, drawColor, 0, tex.Size() / 2, 1, 0, 0);

			if (!dontCollide && masterExpirationTimer > 0)
			{
				Color burnColor = Helpers.CommonVisualEffects.HeatedToCoolColor((1f - masterExpirationTimer / 260f) * 110);
				burnColor.A = 0;

				Vector2 offset = Main.rand.NextVector2Circular(1, 1) * (masterExpirationTimer / 30f);

				spriteBatch.Draw(tex, NPC.Center + Vector2.UnitY * 20 - screenPos + offset, null, burnColor, 0, tex.Size() / 2, 1, 0, 0);
			}

			return false;
		}
	}

	internal class VitricBossPlatformDown : VitricBossPlatformUp
	{

		public override bool FindParent()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC NPC = Main.npc[i];
				if (NPC.active && NPC.type == ModContent.NPCType<VitricBackdropRight>())
				{
					parent = NPC.ModNPC as VitricBackdropRight;
					return true;
				}
			}

			return false;
		}
		public override void SafeAI()
		{
			/*AI fields:
             * 0: state
             * 1: rise time left
             * 2: acceleration delay
             */

			if (parent == null || !parent.NPC.active)
				FindParent();

			if (NPC.ai[0] == 0)
			{
				if (NPC.ai[1] > 0)
				{
					NPC.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Risetime;
					NPC.ai[1]--;
				}
				else
				{
					NPC.velocity.Y = 0;
				}
			}

			if (NPC.ai[0] == 1)
			{
				NPC.velocity.Y = (float)MaxHeight / VitricBackdropLeft.Scrolltime * (1f / parent.NPC.ai[3]);
				if (NPC.position.Y >= StarlightWorld.vitricBiome.Y * 16 + 16 * 16 + MaxHeight)
					NPC.position.Y -= MaxHeight;
			}

			if (storedCenter == Vector2.Zero && NPC.velocity.Y == 0)
				storedCenter = NPC.Center;

			if (Main.masterMode)
			{
				if (!dontCollide)
				{
					Rectangle checkBox = NPC.Hitbox;
					checkBox.Inflate(8, 8);

					if (Main.player.Any(Player => Player.active && Player.velocity.Y == 0 && Player.Hitbox.Intersects(checkBox)))
						masterExpirationTimer += 2;
					else if (masterExpirationTimer > 0)
						masterExpirationTimer--;

					if (masterExpirationTimer > 30)
					{
						Dust.NewDustPerfect(NPC.position + new Vector2(Main.rand.Next(NPC.width), 12), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Vector2.UnitY * Main.rand.NextFloat(-4, -1), 0, new Color(255, (int)(50 + masterExpirationTimer / 150f * 200), 50, 0), Main.rand.NextFloat(0.05f, 0.2f) * masterExpirationTimer / 150f);
					}

					if (masterExpirationTimer > 150)
					{
						dontCollide = true;
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<FireRingHostile>(), 40, 0, Main.myPlayer, NPC.width * 0.4f);
						Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, NPC.Center);
						SoundHelper.PlayPitched("Magic/FireHit", 1f, -0.5f, NPC.Center);

						for (int k = 0; k < NPC.width; k += Main.rand.Next(2, 4))
						{
							Dust.NewDustPerfect(NPC.position + new Vector2(k, Main.rand.NextFloat(-4, 4)), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(40, 40), 0, new Color(255, Main.rand.Next(120, 200), 50, 0), Main.rand.NextFloat(0.2f, 0.3f));
						}
					}
				}
				else
				{
					masterExpirationTimer--;

					if (masterExpirationTimer <= 0)
					{
						dontCollide = false;

						for (int k = 0; k < NPC.width; k += Main.rand.Next(2, 4))
						{
							Dust.NewDustPerfect(NPC.position + new Vector2(k, Main.rand.NextFloat(-4, 4)), ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(6, 6), 0, new Color(50, 190, 255, 0), Main.rand.NextFloat(0.05f, 0.2f));
						}
					}
				}
			}
		}
	}

	internal class VitricBossPlatformUpSmall : VitricBossPlatformUp
	{
		public override string Texture => AssetDirectory.VitricBoss + "VitricBossPlatformSmall";

		public override void SafeSetDefaults()
		{
			NPC.width = 100;
			NPC.height = 16;
			NPC.noTileCollide = true;
			NPC.dontCountMe = true;
			NPC.lifeMax = 10;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.Bosses.VitricBoss.VitricBossPlatformSmall.Value;

			if (dontCollide)
				drawColor *= 0.25f;

			drawColor.A = 255;

			spriteBatch.Draw(tex, NPC.Center + Vector2.UnitY * 20 - screenPos, null, drawColor, 0, tex.Size() / 2, 1, 0, 0);

			if (!dontCollide && masterExpirationTimer > 0)
			{
				Color burnColor = Helpers.CommonVisualEffects.HeatedToCoolColor((1f - masterExpirationTimer / 260f) * 110);
				burnColor.A = 0;

				Vector2 offset = Main.rand.NextVector2Circular(1, 1) * (masterExpirationTimer / 30f);

				spriteBatch.Draw(tex, NPC.Center + Vector2.UnitY * 20 - screenPos + offset, null, burnColor, 0, tex.Size() / 2, 1, 0, 0);
			}

			return false;
		}
	}

	internal class VitricBossPlatformDownSmall : VitricBossPlatformDown
	{
		public override string Texture => AssetDirectory.VitricBoss + "VitricBossPlatformSmall";

		public override void SafeSetDefaults()
		{
			NPC.width = 100;
			NPC.height = 16;
			NPC.noTileCollide = true;
			NPC.dontCountMe = true;
			NPC.lifeMax = 10;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Assets.Bosses.VitricBoss.VitricBossPlatformSmall.Value;

			if (dontCollide)
				drawColor *= 0.25f;

			drawColor.A = 255;

			spriteBatch.Draw(tex, NPC.Center + Vector2.UnitY * 20 - screenPos, null, drawColor, 0, tex.Size() / 2, 1, 0, 0);

			if (!dontCollide && masterExpirationTimer > 0)
			{
				Color burnColor = Helpers.CommonVisualEffects.HeatedToCoolColor((1f - masterExpirationTimer / 260f) * 110);
				burnColor.A = 0;

				Vector2 offset = Main.rand.NextVector2Circular(1, 1) * (masterExpirationTimer / 30f);

				spriteBatch.Draw(tex, NPC.Center + Vector2.UnitY * 20 - screenPos + offset, null, burnColor, 0, tex.Size() / 2, 1, 0, 0);
			}

			return false;
		}
	}
}
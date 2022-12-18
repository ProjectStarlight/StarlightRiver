using StarlightRiver.Content.NPCs.BaseTypes;
using System.Linq;

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

		public virtual bool findParent()
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
				findParent();

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
				if (!DontCollide)
				{
					if (Main.player.Any(Player => Player.active && Player.velocity.Y == 0 && Player.Hitbox.Intersects(NPC.Hitbox)))
						masterExpirationTimer += 2;
					else if (masterExpirationTimer > 0)
						masterExpirationTimer--;

					if (masterExpirationTimer > 150)
					{
						DontCollide = true;
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<FireRingHostile>(), 40, 0, Main.myPlayer, 50);
					}
				}
				else
				{
					masterExpirationTimer--;

					if (masterExpirationTimer <= 0)
						DontCollide = false;
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			if (DontCollide)
				drawColor *= 0.25f;

			drawColor.A = 255;

			spriteBatch.Draw(tex, NPC.Center + Vector2.UnitY * 4 - screenPos, null, drawColor, 0, tex.Size() / 2, 1, 0, 0);

			if (!DontCollide && masterExpirationTimer > 0)
				spriteBatch.Draw(tex, NPC.Center + Vector2.UnitY * 4 - screenPos, null, Color.Lerp(Color.Transparent, Color.Red, masterExpirationTimer / 300f), 0, tex.Size() / 2, 1, 0, 0);

			return false;
		}
	}

	internal class VitricBossPlatformDown : VitricBossPlatformUp
	{

		public override bool findParent()
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
				findParent();

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
				if (!DontCollide)
				{
					if (Main.player.Any(Player => Player.active && Player.velocity.Y == 0 && Player.Hitbox.Intersects(NPC.Hitbox)))
						masterExpirationTimer += 2;
					else if (masterExpirationTimer > 0)
						masterExpirationTimer--;

					if (masterExpirationTimer > 150)
					{
						DontCollide = true;
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<FireRingHostile>(), 40, 0, Main.myPlayer, 50);
					}
				}
				else
				{
					masterExpirationTimer--;

					if (masterExpirationTimer <= 0)
						DontCollide = false;
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
	}
}
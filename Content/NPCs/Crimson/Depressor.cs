using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Bosses.TheThinkerBoss;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Physics;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.Crimson
{
	internal class Depressor : ModNPC
	{
		public static List<Depressor> toDraw = new();

		public Vector2 homePos;
		public VerletChain attachedChain;

		public ref float Timer => ref NPC.ai[0];
		public ref float State => ref NPC.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			GraymatterBiome.onDrawOverHallucinationMap += DrawDepressors;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Depressor");
		}

		public override void SetDefaults()
		{
			NPC.width = 64;
			NPC.height = 64;
			NPC.knockBackResist = 1.8f;
			NPC.lifeMax = 200;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath4;

			attachedChain = new VerletChain(40, true, NPC.Center, 10);
			attachedChain.useEndPoint = true;
			attachedChain.useStartPoint = true;
			attachedChain.forceGravity = Vector2.Zero;
		}

		public override void AI()
		{
			if (homePos == default)
				homePos = NPC.Center;

			attachedChain.endPoint = NPC.Center;
			attachedChain.startPoint = homePos;
			attachedChain.UpdateChain();

			float height = homePos.Y - NPC.Center.Y;

			Player target = Main.player[NPC.target];

			if (target is null || !target.active || !target.HasBuff<CrimsonHallucination>() || Vector2.Distance(target.Center, NPC.Center) >= 1600)
			{
				if (TryPickTarget(ref NPC.target))
				{
					target = Main.player[NPC.target];
					State = 1;
				}
				else
				{
					State = 0;
				}
			}
			else
			{
				State = 1;
			}

			if (State == 0)
			{
				Timer = 0;

				if (height > 0 && NPC.velocity.Y < 8)
					NPC.velocity.Y += 0.2f;
			}
			else if (State == 1)
			{
				Timer++;

				NPC.velocity.Y = 0;

				if (height < 500)
					NPC.position.Y -= 2f * (1f - Eases.EaseQuadIn(height / 500f));

				NPC.position.X += MathF.Sin(Timer * 0.02f) * MathF.Cos(Timer * 0.05f) * 5;
				NPC.position.Y += MathF.Cos(Timer * 0.02f) * MathF.Sin(Timer * 0.05f) * 1;

				if (Timer % 120 == 0)
				{
					//Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(target.Center) * 3, ModContent.ProjectileType<BrainBolt>(), StarlightMathHelper.GetProjectileDamage(10, 20, 40), 1, Main.myPlayer, 180, 1, 60);

					for (int k = 0; k < 6; k++)
					{
						float rot = k / 6f * 6.28f;
						
						if (Timer % 240 == 0)
							rot += 6.28f / 12;

						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitX.RotatedBy(k) * 3, ModContent.ProjectileType<BrainBolt>(), StarlightMathHelper.GetProjectileDamage(10, 20, 40), 1, Main.myPlayer, 180, 1, 60);
					}
				}
			}

			NPC.velocity *= 0.95f;

			NPC.position.X += (homePos.X - NPC.Center.X) * 0.05f;
		}

		public bool TryPickTarget(ref int target)
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.HasBuff<CrimsonHallucination>() && Vector2.Distance(player.Center, NPC.Center) < 1600)
				{
					target = player.whoAmI;
					return true;
				}
			}

			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (!NPC.IsABestiaryIconDummy)
				toDraw.Add(this);

			return false;
		}

		private void DrawDepressors(SpriteBatch batch)
		{
			var tex = Assets.NPCs.Crimson.Depressor.Value;
			var chain = Assets.NPCs.Crimson.DepressorChain.Value;

			foreach (var depressor in toDraw)
			{
				if (depressor.attachedChain?.ropeSegments != null)
				{
					float height = Vector2.Distance(depressor.homePos, depressor.NPC.Center);

					for (int k = 0; k < height; k += 10)
					{
						Vector2 pos = Vector2.Lerp(depressor.homePos, depressor.NPC.Center, k / height);
						batch.Draw(chain, pos - Main.screenPosition, null, Main.DiscoColor, 0, chain.Size() / 2f, 1, 0, 0);
					}
				}

				batch.Draw(tex, depressor.NPC.Center - Main.screenPosition, null, Main.DiscoColor, depressor.NPC.rotation, tex.Size() / 2f, depressor.NPC.scale, 0, 0);
			}

			toDraw.Clear();
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	internal class ArenaBottom : ModNPC, IDrawAdditive
	{
		public VitricBoss Parent;

		private float prevState = 0;
		public override string Texture => AssetDirectory.VitricBoss + "CrystalWaveHot";

		public override bool? CanBeHitByProjectile(Projectile Projectile)
		{
			return false;
		}

		public override bool? CanBeHitByItem(Player Player, Item Item)
		{
			return false;
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("");
		}

		public override void SetDefaults()
		{
			NPC.height = 16;
			NPC.width = 1260;
			NPC.aiStyle = -1;
			NPC.lifeMax = 2;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.dontTakeDamage = true;
			NPC.dontCountMe = true;
			NPC.hide = true;
			NPC.damage = 0;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			database.Entries.Remove(bestiaryEntry);
		}

		public override void DrawBehind(int index)
		{
			Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
		}

		public void findParent()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC NPC = Main.npc[i];
				if (NPC.active && NPC.type == ModContent.NPCType<VitricBoss>())
				{
					Parent = NPC.ModNPC as VitricBoss;
					return;
				}
			}
		}

		public override void AI()
		{
			/* AI fields:
             * 0: timer
             * 1: state
             * 2: mirrored?
             */
			if (Parent is null)
				findParent();

			if (Parent?.NPC.active != true)
			{
				NPC.active = false;
				return;
			}

			if (Parent.Phase == (int)VitricBoss.AIStates.FirstPhase && Parent.AttackPhase == 0 && NPC.ai[1] != 2)
			{
				NPC.ai[1] = 2;
				NPC.ai[0] = 0;
				NPC.ai[3]++;//the 4th ai slot is used here as a random seed
			}

			else if (NPC.velocity.Y == 0 && Main.masterMode) //bigger hitbox in master
			{
				NPC.height = 42;
			}

			switch (NPC.ai[1])
			{
				case 0:
					if (Main.player.Any(n => n.Hitbox.Intersects(NPC.Hitbox)))
						NPC.ai[0]++; //ticks the enrage timer when Players are standing on the ground

					if (NPC.ai[0] > 120) //after standing there for too long a wave comes by
					{
						NPC.ai[1] = 1; //wave mode
						NPC.ai[0] = 0; //reset timer so it can be reused

						NPC.TargetClosest();
						if (Main.player[NPC.target].Center.X > NPC.Center.X)
							NPC.ai[2] = 0;
						else
							NPC.ai[2] = 1;
					}

					break;

				case 1:

					if (Parent.Phase > (int)VitricBoss.AIStates.SecondPhase)
						return;

					NPC.ai[0] += 8; //timer is now used to track where we are in the crystal wave
					if (NPC.ai[0] % 32 == 0) //summons a crystal at every tile covered by the NPC
					{
						var pos = new Vector2(NPC.ai[2] == 1 ? NPC.position.X + NPC.width - NPC.ai[0] : NPC.position.X + NPC.ai[0], NPC.position.Y + 48);

						if (Main.netMode != NetmodeID.MultiplayerClient)
							Projectile.NewProjectile(NPC.GetSource_FromThis(), pos, Vector2.Zero, ProjectileType<CrystalWave>(), 20, 1, Owner: -1, ai0: pos.Y);
					}

					if (NPC.ai[0] > NPC.width)
					{
						NPC.ai[1] = 0; //go back to waiting for enrage time
						NPC.ai[0] = 0; //reset timer
					}

					break;

				case 2: //during every crystal phase

					if (Parent.Phase == (int)VitricBoss.AIStates.FirstPhase && Parent.AttackPhase == 0)
					{
						NPC.ai[0]++;
					}
					else if (Parent.Phase == (int)VitricBoss.AIStates.FirstPhase || Parent.Phase == (int)VitricBoss.AIStates.Dying)
					{
						if (NPC.ai[0] > 150)
							NPC.ai[0] = 150;

						NPC.ai[0]--;

						if (NPC.ai[0] <= 0)
							NPC.ai[1] = 0;
					}

					if (NPC.ai[0] < 120 && Main.netMode != NetmodeID.Server) //dust before rising
						Dust.NewDust(NPC.position, NPC.width, NPC.height, Terraria.ID.DustID.Torch);

					if (NPC.ai[0] >= 150)
					{
						foreach (Player target in Main.player.Where(n => n.active))
						{

							if (target.Hitbox.Intersects(NPC.Hitbox))
							{
								target.Hurt(PlayerDeathReason.ByCustomReason(target.name + " was impaled..."), Main.expertMode ? 80 : 40, 0);
								target.GetModPlayer<StarlightPlayer>().platformTimer = 15;
								target.velocity.Y = -Main.rand.Next(9, 13);

								target.rocketTime = target.rocketTimeMax;
								target.wingTime = target.wingTimeMax;
								target.RefreshExtraJumps();
							}

							var topColission = new Rectangle((int)NPC.position.X, (int)NPC.position.Y - 840, NPC.width, NPC.height);

							if (target.Hitbox.Intersects(topColission))
							{
								target.Hurt(PlayerDeathReason.ByCustomReason(target.name + " was impaled..."), Main.expertMode ? 80 : 40, 0);
								target.GetModPlayer<StarlightPlayer>().platformTimer = 15;
								target.velocity.Y = Main.rand.Next(9, 13);
							}

							if (Main.masterMode)
							{
								var leftColission = new Rectangle((int)NPC.position.X, (int)NPC.position.Y - 840, NPC.height, 900);
								var rightColission = new Rectangle((int)NPC.position.X + NPC.width - NPC.height, (int)NPC.position.Y - 840, NPC.height, 900);

								if (target.Hitbox.Intersects(rightColission) || target.Hitbox.Intersects(leftColission))
								{
									target.Hurt(PlayerDeathReason.ByCustomReason(target.name + " was impaled..."), Main.expertMode ? 80 : 40, 0);
									target.GetModPlayer<StarlightPlayer>().platformTimer = 15;

									target.velocity.X = target.Hitbox.Intersects(leftColission) ? 10 : -10;

									target.GetModPlayer<Abilities.AbilityHandler>().ActiveAbility?.Deactivate();
								}
							}
						}
					}

					break;
			}

			if (NPC.ai[1] != prevState && Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.netUpdate = true;
				prevState = NPC.ai[1];
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			return false;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.ai[1] == 2 && NPC.ai[0] > 120) //in the second phase after the crystals have risen
			{
				var rand = new Random((int)NPC.ai[3] + NPC.whoAmI);//the seed changes each time this attack activates, whoAmI is added so the top/bottom are different

				int eggIndex = rand.Next(25) == 0 ? rand.Next(0, NPC.width / 18) * 18 : -1;// 1/25 chance

				float maxOffset = Main.masterMode ? 64 : 32;
				float off = Math.Min((NPC.ai[0] - 120) / 30f * maxOffset, maxOffset);
				Texture2D tex = Assets.Bosses.VitricBoss.CrystalWaveHot.Value;

				for (int k = 0; k < NPC.width; k += 18)
				{
					Vector2 pos = NPC.position + new Vector2(k, 36 - off + (float)Math.Sin(Main.GameUpdateCount * 0.05f + rand.Next(100) * 0.2f) * 6) - screenPos; //actually draw the crystals
					Vector2 pos2 = NPC.position + new Vector2(k, -940 + 32 + off - (float)Math.Sin(Main.GameUpdateCount * 0.05f + rand.Next(100) * 0.2f) * 6) - screenPos;

					if (eggIndex == k)//ugly but I this way its only checked once
					{
						Texture2D eggTex = Assets.Bosses.VitricBoss.MagMegg.Value;
						spriteBatch.Draw(eggTex, pos, null, Color.White, 0.1f * ((float)rand.NextDouble() - 0.5f), default, 1, default, default);
						spriteBatch.Draw(eggTex, pos2, null, Color.White, 0.1f * ((float)rand.NextDouble() - 0.5f), default, 1, default, default);
					}
					else
					{
						spriteBatch.Draw(tex, pos, null, Color.White, 0.1f * ((float)rand.NextDouble() - 0.5f), default, 1, default, default);
						spriteBatch.Draw(tex, pos2, null, Color.White, 0.1f * ((float)rand.NextDouble() - 0.5f), default, 1, default, default);
					}
				}

				if (Main.masterMode)
				{
					for (int k = 0; k < 900; k += 18)
					{
						Vector2 pos = NPC.position + new Vector2(-20 + off + (float)Math.Sin(Main.GameUpdateCount * 0.05f + rand.Next(100) * 0.2f) * 6, -900 + k) - screenPos; //actually draw the crystals
						Vector2 pos2 = NPC.position + new Vector2(1348 - off - (float)Math.Sin(Main.GameUpdateCount * 0.05f + rand.Next(100) * 0.2f) * 6, -900 + k) - screenPos;

						if (eggIndex == k)//ugly but I this way its only checked once
						{
							Texture2D eggTex = Assets.Bosses.VitricBoss.MagMegg.Value;
							spriteBatch.Draw(eggTex, pos, null, Color.White, 1.57f + 0.1f * ((float)rand.NextDouble() - 0.5f), default, 1, default, default);
							spriteBatch.Draw(eggTex, pos2, null, Color.White, 1.57f + 0.1f * ((float)rand.NextDouble() - 0.5f), default, 1, default, default);
						}
						else
						{
							spriteBatch.Draw(tex, pos, null, Color.White, 1.57f + 0.1f * ((float)rand.NextDouble() - 0.5f), default, 1, default, default);
							spriteBatch.Draw(tex, pos2, null, Color.White, 1.57f + 0.1f * ((float)rand.NextDouble() - 0.5f), default, 1, default, default);
						}
					}
				}
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Assets.Bosses.VitricBoss.LongGlow.Value;

			if (NPC.ai[1] == 2)
			{
				Color color;

				if (NPC.ai[0] < 360)
				{
					color = Color.Lerp(Color.Transparent, Color.Red, NPC.ai[0] / 360f);
				}
				else if (NPC.ai[0] < 390)
				{
					color = Color.Lerp(Color.Red, Color.Red * 0.6f, (NPC.ai[0] - 360f) / 30f);
				}
				else
				{
					color = Color.Red * (0.5f + ((float)Math.Sin(NPC.ai[0] / 20f) + 1) * 0.1f);
					color.G += (byte)((Math.Sin(NPC.ai[0] / 50f) + 1) * 25);
				}

				spriteBatch.Draw(tex, new Rectangle(NPC.Hitbox.X - (int)Main.screenPosition.X, NPC.Hitbox.Y - 66 - (int)Main.screenPosition.Y, NPC.Hitbox.Width, 100), null, color, 0, default, default, default);
				spriteBatch.Draw(tex, new Rectangle(NPC.Hitbox.X - (int)Main.screenPosition.X, NPC.Hitbox.Y - 848 - (int)Main.screenPosition.Y, NPC.Hitbox.Width, 100), null, color, 0, default, SpriteEffects.FlipVertically, default);

				if (Main.masterMode)
				{
					spriteBatch.Draw(tex, new Rectangle(NPC.Hitbox.X + 106 - (int)Main.screenPosition.X, NPC.Hitbox.Y - 866 - (int)Main.screenPosition.Y, 900, 100), null, color, 1.57f, default, default, default);
					spriteBatch.Draw(tex, new Rectangle(NPC.Hitbox.X + 10 + NPC.width - (int)Main.screenPosition.X, NPC.Hitbox.Y - 866 - (int)Main.screenPosition.Y, 900, 100), null, color, 1.57f, default, SpriteEffects.FlipVertically, default);
				}
			}
		}
	}

	internal class CrystalWave : ModProjectile
	{
		public ref float StartY => ref Projectile.ai[0];

		private bool loaded = false;

		public override string Texture => AssetDirectory.VitricBoss + Name;

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 16;
			Projectile.height = 48;
			Projectile.timeLeft = 30;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (!loaded && Main.netMode != NetmodeID.Server)
			{
				loaded = true;
				Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.DD2_WitherBeastCrystalImpact, Projectile.Center);

				for (int k = 0; k < Main.rand.Next(6); k++)
				{
					int type = Mod.Find<ModGore>("MagmiteGore").Type;
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center - Vector2.UnitY * 30, (Vector2.UnitY * -8).RotatedByRandom(0.2f), type, Main.rand.NextFloat(0.3f, 0.5f));
					Dust.NewDustPerfect(Projectile.Center - Vector2.UnitY * 30, DustType<Dusts.Glow>(), (Vector2.UnitY * Main.rand.Next(-5, -2)).RotatedByRandom(0.8f), 0, new Color(255, 200, 100), 0.3f);
				}
			}

			float off = 128 * Projectile.timeLeft / 15 - 64 * (float)Math.Pow(Projectile.timeLeft, 2) / 225;

			Projectile.position.Y = StartY - off;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override void PostDraw(Color lightColor)
		{
			Main.spriteBatch.Draw(Request<Texture2D>(Texture).Value, Projectile.position - Main.screenPosition, Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16 - 2) * 1.4f);

			Color color = Color.White * (Projectile.timeLeft / 30f);
			Main.spriteBatch.Draw(Request<Texture2D>(Texture + "Hot").Value, Projectile.position - Main.screenPosition, color);
		}
	}
}
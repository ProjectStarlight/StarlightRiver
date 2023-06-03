using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	internal class BoomBug : ModNPC
	{
		private int yFrame = 0;

		private int bugTimer = 0;

		public bool dying = false;

		private bool chargingMagma = false;
		private float magmaCharge = 0;

		private Player Target => Main.player[NPC.target];

		public override string Texture => AssetDirectory.VitricNpc + Name;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 3;
			DisplayName.SetDefault("Firebug");
		}

		public override void SetDefaults()
		{
			NPC.width = 34;
			NPC.height = 40;
			NPC.knockBackResist = 1f;
			NPC.lifeMax = 150;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.value = 200;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath4;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("A swarming insect found in the Vitric Desert. Rapidly calls forth its lesser bretheren as a means of attack, flying close to their target before detonating their molten nectar to burn and cook their prey.")
			});
		}

		public override bool CheckDead()
		{
			if (dying)
			{
				Helper.PlayPitched("Magic/FireHit", 0.3f, 0.3f, NPC.Center);
				for (int i = 0; i < 8; i++)
				{
					var dust = Dust.NewDustDirect(NPC.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDust>());
					dust.velocity = Main.rand.NextVector2Circular(3, 3);
					dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
					dust.alpha = 70 + Main.rand.Next(60);
					dust.rotation = Main.rand.NextFloat(6.28f);
				}

				for (int i = 0; i < 8; i++)
				{
					Vector2 dir = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
					Dust.NewDustPerfect(NPC.Center + dir * 45, ModContent.DustType<Dusts.GlowLineFast>(), dir * 12, 0, Color.OrangeRed, Main.rand.NextFloat(0.85f, 1.45f));
				}

				for (int i = 0; i < 8; i++)
				{
					var dust = Dust.NewDustDirect(NPC.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDustTwo>());
					dust.velocity = Main.rand.NextVector2Circular(3, 3);
					dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
					dust.alpha = Main.rand.Next(80) + 40;
					dust.rotation = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustGlow>()).scale = 0.9f;
				}

				for (int i = 0; i < 3; i++)
				{
					Vector2 velocity = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3);
					var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), 0, 0, 255);
					proj.friendly = false;
					proj.hostile = true;
					proj.scale = Main.rand.NextFloat(0.55f, 0.85f);
				}

				Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CoachGunRing>(), 60, 4, Target.whoAmI);
				return true;
			}

			NPC.life = 1;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			dying = true;
			return false;
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			NPC.spriteDirection = NPC.direction;

			Lighting.AddLight(NPC.Center, Color.OrangeRed.ToVector3() * 0.8f);

			if (dying)
			{
				chargingMagma = false;
				NPC.velocity.Y += 0.1f;
				NPC.velocity.X += Math.Sign(NPC.velocity.X) * 0.08f;

				if (NPC.collideX || NPC.collideY)
					NPC.Kill();

				return;
			}

			if (chargingMagma)
			{
				bugTimer = 0;
				magmaCharge += 0.01f;
				NPC.velocity.Y += TileGapDown() * 0.0005f;
				NPC.velocity.X *= 0.95f;

				Vector2 dustDir = Main.rand.NextVector2CircularEdge(1, 1);

				if (magmaCharge < 1)
					Dust.NewDustPerfect(new Vector2(NPC.spriteDirection * -8, 14) + NPC.Center + dustDir * 40, ModContent.DustType<Dusts.Glow>(), dustDir * -2, 0, Color.OrangeRed, magmaCharge);

				if (magmaCharge > 1.75f)
				{
					SoundEngine.PlaySound(SoundID.Item45, NPC.Center);
					Vector2 projVel = ArcVelocityHelper.GetArcVel(NPC.Center, Target.Center, 0.2f, 100, 400, 12);
					NPC.velocity = projVel * -1;
					magmaCharge = 0;
					chargingMagma = false;
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projVel, ModContent.ProjectileType<FirebugMagma>(), 70, 4);
				}
			}
			else
			{

				if (bugTimer < 20)
					NPC.velocity *= 0.9f;

				if (++bugTimer % 190 == 0)
					NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<LesserFirebug>(), 0, NPC.whoAmI);

				if (TileGapDown() < 15 && TileGapUp() > 5)
					NPC.velocity.Y -= 0.1f;
				else
					NPC.velocity.Y += 0.1f;

				NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -4, 4);

				NPC.velocity.X += Math.Sign(Target.Center.X - NPC.Center.X) * 0.1f;
				NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -6, 6);

				if (bugTimer > 700)
				{
					NPC.velocity.Y = 0;
					chargingMagma = true;
				}
			}
		}
		public override void FindFrame(int frameHeight)
		{
			if (!chargingMagma)
			{
				NPC.frameCounter++;

				if (NPC.frameCounter % 4 == 0)
					yFrame++;
			}

			yFrame %= Main.npcFrameCount[NPC.type];
			NPC.frame = new Rectangle(0, frameHeight * yFrame, NPC.width, frameHeight);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D magmaTex = Request<Texture2D>(AssetDirectory.Keys + "GlowHarsh").Value;

			var magmaOffset = new Vector2(-13 * NPC.spriteDirection, 8);
			SpriteEffects effects = SpriteEffects.None;
			if (NPC.spriteDirection == 1)
				effects = SpriteEffects.FlipHorizontally;

			spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);

			if (!NPC.IsABestiaryIconDummy || drawColor == Color.White)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				for (int i = 0; i < 6; i++)
				{
					float angle = i / 6f * MathHelper.TwoPi;

					float cos = (float)Math.Cos(Main.timeForVisualEffects * 0.05f);
					float distance = 1.5f + cos;

					float opacity = 0.6f + 0.2f * cos;

					Vector2 offset = angle.ToRotationVector2() * distance;
					spriteBatch.Draw(glowTex, offset + NPC.Center - screenPos, NPC.frame, Color.White * opacity, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);
				}

				for (int j = 0; j < 4; j++)
				{
					spriteBatch.Draw(magmaTex, magmaOffset + NPC.Center - screenPos, null, Color.OrangeRed, NPC.rotation, magmaTex.Size() / 2, NPC.scale * magmaCharge, effects, 0f);
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return spawnInfo.Player.InModBiome(GetInstance<VitricDesertBiome>()) ? 100 : 0;
		}

		private int TileGapDown()
		{
			int i = 0;
			for (; i < 50; i++)
			{
				int x = (int)(NPC.Center.X / 16);
				int y = (int)(NPC.Center.Y / 16);
				Tile tile = Framing.GetTileSafely(x, y + i);
				if (tile.HasTile && Main.tileSolid[tile.TileType])
					break;
			}

			return i;
		}

		private int TileGapUp()
		{
			int i = 0;
			for (; i < 50; i++)
			{
				int x = (int)(NPC.Center.X / 16);
				int y = (int)(NPC.Center.Y / 16);
				Tile tile = Framing.GetTileSafely(x, y - i);
				if (tile.HasTile && Main.tileSolid[tile.TileType])
					break;
			}

			return i;
		}
	}

	internal class LesserFirebug : ModNPC
	{
		private int yFrame = 0;

		private bool parentless = false;

		private int timer = 0;

		private Player Target => Main.player[NPC.target];

		private NPC Parent => Main.npc[(int)NPC.ai[0]];

		public override string Texture => AssetDirectory.VitricNpc + Name;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 2;
			DisplayName.SetDefault("Lesser Firebug");
		}

		public override void SetDefaults()
		{
			NPC.width = 16;
			NPC.height = 16;
			NPC.knockBackResist = 1.5f;
			NPC.lifeMax = 5;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath4;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("A lesser form of the Firebug, these are at the bottom of the nest's heirarchy. They are regularly sent to their deaths in order to detonate their own bodys to help the hive hunt.")
			});
		}

		public override void AI()
		{
			if (timer == 0)
				NPC.velocity.Y = 10;
			NPC.TargetClosest(true);
			NPC.spriteDirection = NPC.direction;

			if (NPC.Distance(Target.Center) < 600 && timer++ > 600)
			{
				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Target.Center) * 9, 0.05f);
			}
			else if (Parent.active && !parentless)
			{
				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(Parent.Center) * 6, 0.045f);
			}
			else
			{
				timer = 601;
				parentless = true;
			}

			if (Parent.ModNPC is BoomBug modNPC && modNPC.dying)
			{
				timer = 601;
				parentless = true;
			}

			if (NPC.collideX || NPC.collideY)
				NPC.Kill();
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			NPC.Kill();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;
			Texture2D glowTex = Request<Texture2D>(Texture + "_Glow").Value;

			SpriteEffects effects = SpriteEffects.None;
			if (NPC.spriteDirection == 1)
				effects = SpriteEffects.FlipHorizontally;

			spriteBatch.Draw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);

			if (!NPC.IsABestiaryIconDummy || drawColor == Color.White)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
				for (int i = 0; i < 6; i++)
				{
					float angle = i / 6f * MathHelper.TwoPi;

					float cos = (float)Math.Cos(Main.timeForVisualEffects * 0.05f);
					float distance = 1.5f + cos;

					float opacity = 0.6f + 0.2f * cos;

					Vector2 offset = angle.ToRotationVector2() * distance;
					spriteBatch.Draw(glowTex, offset + NPC.Center - screenPos, NPC.frame, Color.White * opacity, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}

			return false;
		}

		public override void OnKill()
		{
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

			for (int i = 0; i < 4; i++)
			{
				var dust = Dust.NewDustDirect(NPC.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDust>());
				dust.velocity = Main.rand.NextVector2Circular(2, 2);
				dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 8; i++)
			{
				Vector2 dir = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
				Dust.NewDustPerfect(NPC.Center + dir * 24, ModContent.DustType<Dusts.GlowLineFast>(), dir * 12, 0, Color.OrangeRed, Main.rand.NextFloat(0.65f, 1.15f));
			}

			for (int i = 0; i < 4; i++)
			{
				var dust = Dust.NewDustDirect(NPC.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<CoachGunDustTwo>());
				dust.velocity = Main.rand.NextVector2Circular(2, 2);
				dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
				dust.alpha = Main.rand.Next(80) + 40;
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustGlow>()).scale = 0.9f;
			}

			for (int i = 0; i < 2; i++)
			{
				Vector2 velocity = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(1, 2);
				var proj = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), 0, 0, 255);
				proj.friendly = false;
				proj.hostile = true;
				proj.scale = Main.rand.NextFloat(0.55f, 0.85f);
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter++;

			if (NPC.frameCounter % 4 == 0)
				yFrame++;

			yFrame %= Main.npcFrameCount[NPC.type];
			NPC.frame = new Rectangle(0, frameHeight * yFrame, NPC.width, frameHeight);
		}
	}

	public class FirebugMagma : ModProjectile, IDrawAdditive
	{
		private readonly List<Vector2> oldPos = new();

		public override string Texture => AssetDirectory.Keys + "GlowHarsh";

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.damage = 5;
			Projectile.hide = true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Molten glass");
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.2f;

			oldPos.Add(Projectile.Center);
			if (oldPos.Count > 16)
				oldPos.RemoveAt(0);
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
			for (int k = 0; k <= 10; k++)
			{
				var d = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 150, 50), 0.5f);
				d.noGravity = false;
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>(Texture).Value;

			for (int i = 0; i < oldPos.Count; i++)
			{
				spriteBatch.Draw(tex, oldPos[i] - Main.screenPosition, tex.Frame(),
					Color.OrangeRed * (i / (float)oldPos.Count), 0, tex.Size() / 2, 1.5f, 0, 0);

				spriteBatch.Draw(tex, oldPos[i] - Main.screenPosition, tex.Frame(),
					Color.White * (i / (float)oldPos.Count) * 0.5f, 0, tex.Size() / 2, 0.75f, 0, 0);
			}
		}
	}
}
﻿using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.GameContent.Bestiary;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	internal class KettleCreature : ModNPC
	{
		private const int WALK_RADIUS = 16;

		KettleLimb leftLeg;
		KettleLimb rightLeg;

		private KettleLimb footOnGround;

		private bool Floating => !leftLeg.FootOnGround && !rightLeg.FootOnGround;
		private KettleLimb FootOffGround => footOnGround == leftLeg ? rightLeg : leftLeg;

		private ref float Timer => ref NPC.ai[0];
		private ref float WalkTimer => ref NPC.ai[1];
		private ref float Landed => ref NPC.ai[2];
		private ref float State => ref NPC.ai[3];

		public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/KettleCreature";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magmortar");
		}

		public override void SetDefaults()
		{
			NPC.width = 32;
			NPC.height = 32;
			NPC.knockBackResist = 0.8f;
			NPC.lifeMax = 100;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 15;
			NPC.aiStyle = -1;
			//NPC.HitSound = SoundID.NPCHit1;
			//NPC.DeathSound = SoundID.NPCDeath4;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("A construct built by an unknown people, seemingly harvesting crystal and processing it internally. It can be assumed that it has run out of storage after an extremely long amount of time unmaintained, so it uses its excess molten crystal as an automatic defense mechanism.")
			});
		}

		public override void AI()
		{
			Timer++;

			if (Timer == 1) // On-Spawn
			{
				leftLeg = new KettleLimb(this, new Vector2(-10, 0));
				rightLeg = new KettleLimb(this, new Vector2(10, 0));
			}

			if (State == 0)
			{
				if (!Floating)
				{
					NPC.velocity *= 0;
					NPC.Center = Vector2.Lerp(leftLeg.foot, rightLeg.foot, 0.5f) + new Vector2(0, -70);

					if (leftLeg.savedPoint == default && rightLeg.savedPoint == default && Landed == 0) //landing case
					{
						WalkTimer = 0;
						rightLeg.savedPoint = rightLeg.foot + Vector2.UnitX * WALK_RADIUS;
						rightLeg.foot.Y -= 5;
						footOnGround = leftLeg;
						Landed = 1;
					}

					WalkTimer += 0.14f;
					FootOffGround.foot = FootOffGround.savedPoint + (Vector2.UnitX * -WALK_RADIUS * NPC.direction).RotatedBy(WalkTimer * NPC.direction);

					if (WalkTimer > 4.68f)
					{
						NPC.velocity.Y -= 4;
						NPC.velocity.X += 4 * NPC.direction;
						leftLeg.MoveWholeLimb(NPC.velocity);
						rightLeg.MoveWholeLimb(NPC.velocity);
						WalkTimer = 0;
					}

					if (FootOffGround.FootOnGround && WalkTimer > 0.5f)
					{
						if (Timer % 300 > 200) //shoot
						{
							Timer = 0;
							State = 1;
							return;
						}

						WalkTimer = 0;
						footOnGround.savedPoint = footOnGround.foot + Vector2.UnitX * WALK_RADIUS * NPC.direction;
						footOnGround.foot.Y -= 16;
						footOnGround = FootOffGround;

						NPC.TargetClosest(true);
						NPC.direction = Main.player[NPC.target].Center.X > NPC.Center.X ? -1 : 1;
					}
				}
				else
				{
					leftLeg.MoveWholeLimb(NPC.velocity);
					rightLeg.MoveWholeLimb(NPC.velocity);
					NPC.velocity.Y += 0.43f;
					NPC.velocity.X *= 0.95f;

					if (Landed == 1)
					{
						rightLeg.savedPoint = rightLeg.foot + Vector2.UnitX * WALK_RADIUS;
						leftLeg.savedPoint = leftLeg.foot + Vector2.UnitX * WALK_RADIUS;
					}
				}

				leftLeg.Constrain();
				rightLeg.Constrain();
			}

			if (State == 1)
			{
				if (Timer == 30)
				{
					float angle = 0;
					Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitY.RotatedBy(angle) * -15, ProjectileType<KettleMortar>(), 20, 1, Main.myPlayer);
				}

				for (int k = 0; k < 2; k++)
				{
					if (Timer == 30 + k * 5)
						Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.UnitY.RotatedByRandom(1) * -15, ProjectileType<KettleMortar>(), 20, 1, Main.myPlayer);
				}

				if (Timer == 60)
				{
					Timer = 2;
					State = 0;
				}
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			leftLeg?.Draw(spriteBatch);
			rightLeg?.Draw(spriteBatch);
		}
	}

	class KettleLimb
	{
		public Vector2 joint;
		public Vector2 foot;
		public Vector2 attachPoint;
		public Vector2 savedPoint;
		private Vector2 attachOff;

		private readonly KettleCreature parent;
		private NPC ParentNPC => parent.NPC;

		public bool FootOnGround => Helper.PointInTile(foot);

		public KettleLimb(KettleCreature parent, Vector2 attachOff)
		{
			this.parent = parent;
			attachPoint = parent.NPC.Center + attachOff;
			foot = attachPoint + new Vector2(0, 64);
			joint = attachPoint + new Vector2(0, 32);
			this.attachOff = attachOff;
		}

		public void MoveWholeLimb(Vector2 velocity)
		{
			attachPoint += velocity;
			joint += velocity;
			foot += velocity;
		}

		public void Constrain()
		{
			joint = Vector2.Lerp(foot, attachPoint, 0.5f) + Vector2.UnitY * -15;

			if (Vector2.Distance(foot, attachPoint) > 120)
				foot -= Vector2.Normalize(foot - attachPoint) * 3;

			attachPoint = ParentNPC.Center + attachOff;
		}

		public void Draw(SpriteBatch sb)
		{
			Texture2D limbTex = Assets.NPCs.Vitric.KettleCreatureLimb.Value;
			Texture2D jointTex = Assets.NPCs.Vitric.KettleCreatureJoint.Value;

			sb.Draw(jointTex, joint - Main.screenPosition, null, Color.White, 0, jointTex.Size() / 2, 1, 0, 0);
			sb.Draw(jointTex, attachPoint - Main.screenPosition, null, Color.White, 0, jointTex.Size() / 2, 1, 0, 0);
			sb.Draw(jointTex, foot - Main.screenPosition, null, Color.White, 0, jointTex.Size() / 2, 1, 0, 0);

			sb.Draw(limbTex, joint - Main.screenPosition, null, Color.White, (joint - attachPoint).ToRotation() + 1.57f, Vector2.UnitX * limbTex.Width / 2, 1, 0, 0);
			sb.Draw(limbTex, foot - Main.screenPosition, null, Color.White, (foot - joint).ToRotation() + 1.57f, Vector2.UnitX * limbTex.Width / 2, 1, 0, 0);
		}
	}

	class KettleMortar : ModProjectile, IDrawAdditive, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magma Shot");
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.3f;

			Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(16), DustType<Dusts.Glow>(), Vector2.UnitY * -1, 0, new Color(255, 150, 50), 0.5f);

			ManageCaches();
			ManageTrail();
		}

		public override void Kill(int timeLeft)
		{
			for (int k = 0; k < 20; k++)
				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(32), DustType<Dusts.Glow>(), Vector2.UnitY * -1.5f, 0, new Color(255, 100, 50), 0.6f);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 30; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => factor * 40, factor =>
							{
								float alpha = 1;

								if (factor.X > 0.99f)
									return Color.Transparent;

								if (Projectile.timeLeft < 20)
									alpha = Projectile.timeLeft / 20f;

								return new Color(255, 175 + (int)((float)Math.Sin(factor.X * 3.14f * 5) * 25), 100) * factor.X * alpha;
							});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

			trail?.Render(effect);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Assets.Keys.GlowSoft.Value;
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 150, 50), 0, tex.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, 0.8f, 0, 0);
		}
	}
}
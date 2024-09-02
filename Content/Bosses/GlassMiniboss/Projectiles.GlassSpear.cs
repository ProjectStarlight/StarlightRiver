using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassSpear : ModProjectile
	{
		Vector2 origin;

		public bool boundToParent = true;

		public override string Texture => AssetDirectory.Glassweaver + Name;

		public ref float Timer => ref Projectile.ai[0];
		public ref float ParentWhoAmI => ref Projectile.ai[1];

		public NPC Parent => Main.npc[(int)Projectile.ai[1]];

		public bool isLoaded = false;

		public bool isMagmaVariant = false;

		/// <summary>
		/// Way of ensuring magma variant is correct on the first frame this is created since setting later would be delayed and cost packets
		/// </summary>
		public static bool setMagmaVariant;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Woven Hammer");
		}

		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 100;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.hide = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			isMagmaVariant = setMagmaVariant;
			setMagmaVariant = false;
		}

		public override void AI()
		{
			if (!isLoaded)
			{
				Helpers.Helper.PlayPitched("GlassMiniboss/WeavingLong", 1f, 0f, Projectile.Center);
				isLoaded = true;
			}

			Timer++;

			if (!Parent.active || Parent.type != NPCType<Glassweaver>())
				Projectile.Kill();

			Glassweaver glassweaver = Parent.ModNPC as Glassweaver;

			if (boundToParent)
			{
				float stabLerp = (float)Math.Pow(Utils.GetLerpValue(66, 82, Timer, true), 2f);
				Projectile.rotation = MathHelper.ToRadians(MathHelper.Lerp(-60, -10, stabLerp)) * Parent.direction - MathHelper.Pi;
			}
			else
			{
				if (Projectile.velocity.Length() > 0)
					Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
			}

			if (isMagmaVariant)
			{
				if (glassweaver.AttackTimer == 65)
				{

					boundToParent = false;
					Projectile.velocity = (glassweaver.moveTarget + Vector2.UnitY * 32 - Projectile.Center) * 0.05f;
					Projectile.velocity.X *= 2.5f;

					glassweaver.NPC.velocity -= Projectile.velocity * 0.35f;

					Projectile.netUpdate = true;
					NetMessage.SendData(MessageID.SyncNPC, number: glassweaver.NPC.whoAmI);
				}

				if (glassweaver.AttackTimer > 65 && glassweaver.AttackTimer < 85)
					Projectile.velocity.X *= 0.95f;

				if (glassweaver.AttackTimer == 85)
				{
					Projectile.velocity *= 0;

					Helpers.Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0.3f, glassweaver.NPC.Center);

					if (Main.netMode != NetmodeID.MultiplayerClient)
						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileType<BurningGround>(), 1, 0, Owner: -1);

					Projectile.netUpdate = true;
				}
			}
			else
			{
				if (glassweaver.AttackTimer > 65 && glassweaver.AttackTimer < 90 && glassweaver.NPC.collideY && glassweaver.NPC.velocity.Y > 0)
				{
					glassweaver.AttackTimer = 90;
					Timer = 80;

					Helpers.Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0.3f, glassweaver.NPC.Center);

					Vector2 lobPos = glassweaver.NPC.Bottom + new Vector2(70 * glassweaver.NPC.direction, -2);

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						int lobCount = 3;

						if (Main.masterMode)
							lobCount = 10;
						else if (Main.expertMode)
							lobCount = 4;

						for (int i = 0; i < lobCount; i++)
						{
							float lobVel = MathHelper.ToRadians(MathHelper.Lerp(6, 90, (float)i / lobCount)) * glassweaver.NPC.direction;

							if (Main.masterMode && i % 3 != 0)
								LavaLob.staticScaleToSet = 0.5f;

							Projectile.NewProjectile(Projectile.GetSource_FromThis(), lobPos, Vector2.Zero, ProjectileType<LavaLob>(), 10, 0.2f, Owner: -1, -44 - i, lobVel);
						}

						Projectile.netUpdate = true;
						NetMessage.SendData(MessageID.SyncNPC, number: glassweaver.NPC.whoAmI);
					}

					if (Main.netMode != NetmodeID.Server)
					{
						for (int j = 0; j < 50; j++)
						{
							Dust.NewDustPerfect(lobPos + Main.rand.NextVector2Circular(20, 1), DustType<Dusts.GlassGravity>(), -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(1f, 6f));
						}
					}
				}
			}

			Vector2 handPos;

			if (Projectile.velocity.Y != 0)
				handPos = new Vector2(30, -50);
			else
				handPos = new Vector2(45, 5);

			handPos.X *= Parent.direction;

			if (boundToParent)
			{
				origin = Parent.Center + handPos.RotatedBy(Parent.rotation);
				Projectile.Center = origin + new Vector2(0, -25).RotatedBy(Projectile.rotation) * Helpers.Helper.BezierEase(Utils.GetLerpValue(0, 70, Timer, true));
				Projectile.velocity = Parent.velocity;
			}

			if (Timer > 170)
				Projectile.Kill();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return projHitbox.Intersects(targetHitbox) && Timer > 100;
		}

		public override void Kill(int timeLeft)
		{
			Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, Main.rand.NextFloat(0.1f), Projectile.Center);

			for (int k = 0; k < 15; k++)
				Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, Projectile.Center + new Vector2(0, 130).RotatedBy(Projectile.rotation), Main.rand.NextFloat()), DustType<Dusts.GlassGravity>());
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Asset<Texture2D> spear = Request<Texture2D>(Texture);
			Rectangle frame = spear.Frame(3, 1, 0);
			Rectangle hotFrame = spear.Frame(3, 1, 1);
			Rectangle smallFrame = spear.Frame(3, 1, 2);
			Vector2 spearOrigin = frame.Size() * new Vector2(0.5f, 0.33f);

			float scaleIn = Projectile.scale * Helpers.Helper.BezierEase(Utils.GetLerpValue(10, 50, Timer, true));

			var fadeIn = Color.Lerp(lightColor, Color.White, Utils.GetLerpValue(150, 0, Timer, true));
			Main.EntitySpriteDraw(spear.Value, Projectile.Center - Main.screenPosition, frame, fadeIn, Projectile.rotation, spearOrigin, scaleIn, 0, 0);

			Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(70, 55, Timer, true);
			Main.EntitySpriteDraw(spear.Value, Projectile.Center - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, spearOrigin, scaleIn, 0, 0);

			float scaleOut = Projectile.scale * Utils.GetLerpValue(80, 70, Timer, true);
			Main.EntitySpriteDraw(spear.Value, Projectile.Center - Main.screenPosition, smallFrame, hotFade, Projectile.rotation, spearOrigin, scaleOut, 0, 0);

			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(boundToParent);
			writer.Write(isMagmaVariant);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			boundToParent = reader.ReadBoolean();
			isMagmaVariant = reader.ReadBoolean();
		}
	}

	class LavaLob : ModProjectile
	{
		public const int CRACK_TIME = 850;
		public const float EXPLOSION_RADIUS = 300f;

		private float speed;
		public float bounces = 0;

		public override string Texture => AssetDirectory.Glassweaver + Name;

		public ref float Timer => ref Projectile.ai[0];
		//TODO: wtf is this rotation adjacent shadow ai[1] thats being used?

		/// <summary>
		/// Way of ensuring projectile scale is correct on the first frame this is created since setting later would be delayed and cost an additional packet
		/// </summary>
		public static float staticScaleToSet = 1f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hot Lob");
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ProjectileID.Sets.TrailCacheLength[Type] = 12;
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.hostile = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 360;
			Projectile.decidesManualFallThrough = true;
			Projectile.shouldFallThrough = false;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.scale = staticScaleToSet;
			staticScaleToSet = 1f;
		}

		public override void AI()
		{
			Timer++;

			Projectile.tileCollide = Timer > 0;
			speed = Main.expertMode ? 15f : 13f;

			if (Timer > -1)
			{
				Projectile.velocity.Y += Main.expertMode ? 0.5f : 0.4f;

				if (Main.rand.NextBool(8))
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18, 18), DustType<Dusts.Cinder>(), -Projectile.velocity.RotatedByRandom(0.5f) * 0.1f, 0, Glassweaver.GlowDustOrange, 1f);
			}
			else
			{
				Projectile.velocity = Vector2.Zero;

				if (Main.rand.NextBool(6))
				{
					Vector2 magVel = -Vector2.UnitY.RotatedBy(Projectile.ai[1]).RotatedByRandom(0.2f) * Main.rand.NextFloat(10f, 15f) * Utils.GetLerpValue(-50, 0, Timer, true);
					var magma = Dust.NewDustPerfect(Projectile.Bottom + Main.rand.NextVector2Circular(10, 2), DustType<Dusts.Cinder>(), magVel, 0, Glassweaver.GlowDustOrange, 1.5f);
					magma.noGravity = false;
				}
			}

			if (Timer == 0)
			{
				Helpers.Helper.PlayPitched("GlassMiniboss/WeavingShort", 1f, Main.rand.NextFloat(0.33f), Projectile.Center);
				Projectile.velocity = new Vector2(0, -speed).RotatedBy(Projectile.ai[1]);
			}

			Projectile.frameCounter++;

			if (Projectile.frameCounter > 4)
			{
				Projectile.frame++;
				Projectile.frameCounter = 0;
			}

			if (Projectile.frame > 2)
				Projectile.frame = 0;

			if (Math.Abs(Projectile.velocity.X) < 1f && Timer > 0)
				Projectile.Kill();

			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.Pi;

			Lighting.AddLight(Projectile.Center, Color.OrangeRed.ToVector3() * 0.4f);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Timer > 0)
			{
				if (bounces > 2)
					return true;

				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0)
				{
					Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundExtinguish", 0.4f, 1f, Projectile.Center);
					Projectile.velocity.Y = -oldVelocity.Y * 0.95f;
					bounces++;
				}
			}

			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Projectile.Distance(targetHitbox.Center.ToVector2()) < 40;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Timer >= -1)
			{
				float scale = Projectile.scale * Utils.GetLerpValue(0, 16, Timer, true);

				Asset<Texture2D> glob = Request<Texture2D>(Texture);
				Rectangle frame = glob.Frame(1, 3, 0, Projectile.frame);
				var origin = new Vector2(frame.Height * 0.5f);

				Asset<Texture2D> bloom = Assets.Bosses.GlassMiniboss.BubbleBloom;
				Color bloomFade = Color.OrangeRed;
				bloomFade.A = 0;

				Main.EntitySpriteDraw(glob.Value, Projectile.Center - Main.screenPosition, frame, new Color(255, 255, 255, 128), Projectile.rotation, origin, scale, 0, 0);

				//bloom
				for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
				{
					float fade = 1f - (float)i / ProjectileID.Sets.TrailCacheLength[Type];
					float trailScale = Projectile.scale * MathHelper.Lerp(0.3f, 2f, fade) * 0.5f * scale;
					Main.EntitySpriteDraw(bloom.Value, Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, null, bloomFade * fade * 0.2f, Projectile.oldRot[i], bloom.Size() * 0.5f, trailScale * new Vector2(1f, 0.8f), 0, 0);
				}

				Main.EntitySpriteDraw(bloom.Value, Projectile.Center - Main.screenPosition, null, bloomFade * 0.8f, Projectile.rotation, bloom.Size() * 0.5f, (scale + 0.1f) * new Vector2(0.66f, 0.5f), 0, 0);
			}
			else //Draw tell before firing
			{
				Texture2D line = TextureAssets.Extra[60].Value;
				Color color = Color.OrangeRed;
				color.A = 0;

				color *= (float)Math.Sin(-Timer / 44f * 3.14f) * 0.8f;

				Main.EntitySpriteDraw(line, Projectile.Center - Main.screenPosition, null, color, Projectile.ai[1], line.Size() * new Vector2(0.5f, 0.6f), new Vector2(0.1f, 3), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(line, Projectile.Center - Main.screenPosition, null, color * 0.75f, Projectile.ai[1], line.Size() * new Vector2(0.5f, 0.6f), new Vector2(0.15f, 2), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(line, Projectile.Center - Main.screenPosition, null, color * 0.5f, Projectile.ai[1], line.Size() * new Vector2(0.5f, 0.6f), new Vector2(0.2f, 1), SpriteEffects.None, 0);
			}

			return false;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 15; i++)
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18, 18), DustType<Dusts.Cinder>(), Projectile.velocity.RotatedByRandom(0.5f) * 0.1f, 0, Glassweaver.GlowDustOrange, 1.5f);

			Helpers.Helper.PlayPitched("GlassMiniboss/RippedSoundExtinguish", 0.8f, 0.5f, Projectile.Center);
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Projectile.scale);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.scale = reader.ReadSingle();
		}
	}
}
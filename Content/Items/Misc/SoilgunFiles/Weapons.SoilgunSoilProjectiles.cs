using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc.SoilgunFiles
{
	public abstract class BaseSoilProjectile : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public int dustID;

		public bool gravity = true;

		public bool drawTrail = true;

		public Dictionary<string, Color> Colors = new()
		{
			{ "SmokeColor", Color.White },
			{ "TrailColor", Color.White },
			{ "TrailInsideColor", Color.White },
			{ "RingOutsideColor", Color.White },
			{ "RingInsideColor", Color.White },
		};

		public float AmmoType => Projectile.ai[0];

		public ref float Time => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible; //using the item id for texture was dumb when it can just be requested

		protected BaseSoilProjectile(Color trailColor, Color trailInsideColor, Color ringOutsideColor, Color ringInsideColor, Color smokeColor, int dustID)
		{
			Colors["TrailColor"] = trailColor;
			Colors["TrailInsideColor"] = trailInsideColor;
			Colors["RingOutsideColor"] = ringOutsideColor;
			Colors["RingInsideColor"] = ringInsideColor;
			Colors["SmokeColor"] = smokeColor;

			this.dustID = dustID;
		}

		public sealed override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Soil");
		}

		public virtual void SafeSetDefaults() { }

		public sealed override void SetDefaults()
		{
			Projectile.penetrate = 1;
			SafeSetDefaults();

			Projectile.DamageType = DamageClass.Ranged;
			Projectile.Size = new Vector2(12);
			Projectile.friendly = true;
			Projectile.timeLeft = 240;
		}

		public virtual void SafeAI() { }

		public sealed override void AI()
		{
			SafeAI();

			Time++;

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Projectile.timeLeft < 225 && gravity)
			{
				if (Projectile.velocity.Y < 18f)
				{
					Projectile.velocity.Y += 0.55f;

					if (Projectile.velocity.Y > 0)
					{
						if (Projectile.velocity.Y < 12f)
							Projectile.velocity.Y *= 1.030f;
						else
							Projectile.velocity.Y *= 1.015f;
					}
				}
			}

			Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<SoilgunSmoke>(),
				 Main.rand.NextVector2Circular(1f, 1f), Main.rand.Next(180, 220), Colors["SmokeColor"], Main.rand.NextFloat(0.03f, 0.05f));

			if (Main.rand.NextBool(6))
				Dust.NewDustPerfect(Projectile.Center, dustID, Vector2.Zero, 100).noGravity = true;

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public virtual void SafeOnKill()
		{
			
		}

		public sealed override void OnKill(int timeLeft)
		{
			SafeOnKill();

			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, dustID, -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f),
					Main.rand.Next(100), default, Main.rand.NextFloat(1f, 1.5f)).noGravity = true;

				Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelSmokeColor>(), -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f),
					Main.rand.Next(125, 180), Colors["TrailColor"], Main.rand.NextFloat(0.03f, 0.05f));

				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = Colors["TrailInsideColor"];
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Colors["RingInsideColor"] with { A = 0 }, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.35f, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, Colors["RingOutsideColor"] with { A = 0 }, Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * 0.25f, 0f, 0f);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 13; i++)
				{
					cache.Add(Projectile.Center + Projectile.velocity);
				}
			}

			cache.Add(Projectile.Center + Projectile.velocity);

			while (cache.Count > 13)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 8, factor => Colors["TrailColor"] * factor.X);

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 4, factor => Colors["TrailInsideColor"] * factor.X * 0.5f);

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			if (!drawTrail)
				return;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

				var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
				Matrix view = Main.GameViewMatrix.TransformationMatrix;
				var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
				effect.Parameters["repeats"].SetValue(1);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value);

				trail?.Render(effect);
				trail2?.Render(effect);
			});
		}
	}

	public class SoilgunDirtSoil : BaseSoilProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.DirtBlock;
		public SoilgunDirtSoil() : base(new Color(30, 19, 12), new Color(60, 35, 20), new Color(81, 47, 27), new Color(105, 67, 44), new Color(82, 45, 22), DustID.Dirt) { }
		public override void SafeOnKill()
		{

		}
	}

	public class SoilgunSandSoil : BaseSoilProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.SandBlock;
		public SoilgunSandSoil() : base(new Color(58, 49, 18), new Color(30, 19, 12), new Color(139, 131, 59), new Color(212, 192, 100), new Color(30, 19, 12), DustID.Sand) { }
		public override void SafeAI()
		{
			if (Main.rand.NextBool(8))
			{
				var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 35, default, Main.rand.NextFloat(0.8f, 1.2f));
				dust.noGravity = true;
			}
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			for (int i = 0; i < 12; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.1f));

				Dust.NewDustPerfect(Projectile.Center, DustID.Sand, (Vector2.UnitY * Main.rand.NextFloat(-3, -1)).RotatedByRandom(0.35f), 35, default, Main.rand.NextFloat(0.8f, 1.1f));
			}

			for (int i = 0; i < 6; i++)
			{
				if (Main.myPlayer == Projectile.owner)
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-6.5f, -1)).RotatedByRandom(0.45f), ModContent.ProjectileType<SoilgunSandGrain>(), (int)(Projectile.damage * 0.33f), 0f, Projectile.owner);
			}
		}
	}

	public class SoilgunCrimsandSoil : BaseSoilProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.CrimsandBlock;

		public SoilgunCrimsandSoil() : base(new Color(39, 17, 14), new Color(30, 19, 12), new Color(56, 17, 14), new Color(135, 43, 34), new Color(30, 19, 12), DustID.CrimsonPlants) { }

		public override void SafeAI()
		{
			if (Main.rand.NextBool(10))
			{
				var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonPlants, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.25f));
				dust.noGravity = true;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextBool(5) && Main.player[Projectile.owner].statLife < Main.player[Projectile.owner].statLifeMax2)
			{
				for (int i = 0; i < 12; i++)
				{
					var dust = Dust.NewDustPerfect(Projectile.Center, DustID.LifeDrain, (Projectile.DirectionTo(Main.player[Projectile.owner].Center) * Main.rand.NextFloat(2f, 3f)).RotatedByRandom(MathHelper.ToRadians(5f)), 50, default, Main.rand.NextFloat(0.75f, 1f));
					dust.noGravity = true;
				}

				if (Main.myPlayer == Projectile.owner && !target.SpawnedFromStatue && target.lifeMax > 5 && target.type != NPCID.TargetDummy)
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionTo(Main.player[Projectile.owner].Center), ModContent.ProjectileType<SoilgunLifeSteal>(), 0, 0f, Projectile.owner, 1);
			}
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CrimsonPlants, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
			}
		}
	}

	public class SoilgunEbonsandSoil : BaseSoilProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.EbonsandBlock;
		public SoilgunEbonsandSoil() : base(new Color(26, 18, 31), new Color(30, 19, 12), new Color(62, 45, 75), new Color(119, 106, 138), new Color(30, 19, 12), DustID.Ebonwood) { }
		public override void SafeAI()
		{
			if (Main.rand.NextBool(8))
			{
				var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ebonwood, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.15f));
				dust.noGravity = true;
				if (Main.rand.NextBool(2))
				{
					var dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 25, default, Main.rand.NextFloat(0.9f, 1.2f));
					dust2.noGravity = true;
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SoilgunGlobalNPC globalNPC = target.GetGlobalNPC<SoilgunGlobalNPC>();
			globalNPC.HauntedSoulDamage = damageDone * 3;
			globalNPC.HauntedStacks++;
			globalNPC.HauntedTimer = 420;
			globalNPC.HauntedSoulOwner = Projectile.owner;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			for (int i = 0; i < 12; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ebonwood, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
			}
		}
	}

	public class SoilgunPearlsandSoil : BaseSoilProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.PearlsandBlock;

		private bool foundTarget;
		public SoilgunPearlsandSoil() : base(new Color(87, 77, 106), new Color(30, 19, 12), new Color(87, 77, 106), new Color(246, 235, 228), new Color(30, 19, 12), DustID.Pearlsand) { }
		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
		}

		public override void SafeAI()
		{
			gravity = !foundTarget;

			Vector2 npcCenter = Projectile.Center;
			NPC npc = Projectile.FindTargetWithinRange(1500f);

			if (npc != null && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1) && !npc.dontTakeDamage && !npc.immortal)
			{
				npcCenter = npc.Center;
				foundTarget = true;
			}

			if (foundTarget)
			{
				float speed = Main.player[Projectile.owner].HeldItem.shootSpeed;
				Vector2 velo = Utils.SafeNormalize(npcCenter - Projectile.Center, Vector2.UnitY);
				Projectile.velocity = (Projectile.velocity * 20f + velo * speed) / 21f;
			}

			if (Main.rand.NextBool(5))
			{
				Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Pearlsand, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1.1f)).noGravity = true;
				if (Main.rand.NextBool(2))
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.MoonstoneShimmer>(), Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.1f, 0.2f), 25, new Color(0.3f, 0.2f, 0.3f, 0f), Main.rand.NextFloat(0.2f, 0.3f)).fadeIn = 90f;
			}
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			for (int i = 0; i < 10; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pearlsand, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
			}

			for (int i = 0; i < 6; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.MoonstoneShimmer>(), Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.3f, 0.4f), 25, new Color(0.3f, 0.2f, 0.3f, 0f), Main.rand.NextFloat(0.3f, 0.4f)).fadeIn = 90f;
			}
		}
	}

	public class SoilgunVitricSandSoil : BaseSoilProjectile
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricSandItem";

		public SoilgunVitricSandSoil() : base(new Color(87, 129, 140), new Color(30, 19, 12), new Color(87, 129, 140), new Color(171, 230, 167), new Color(30, 19, 12), ModContent.DustType<VitricSandDust>()) { }

		//yeah this is copied from vitric bullet they kinda similar tho
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SoilgunGlobalNPC globalNPC = target.GetGlobalNPC<SoilgunGlobalNPC>();
			if (globalNPC.ShardAmount < 10)
			{
				var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), target.position, Vector2.Zero, ModContent.ProjectileType<SoilgunVitricCrystals>(), Projectile.damage / 2, 0f, Projectile.owner);

				proj.rotation = Projectile.rotation + Main.rand.NextFloat(-1f, 1f);

				if (proj.ModProjectile is SoilgunVitricCrystals Crystal)
				{
					//Vector2 Offset = 0;
					Crystal.offset = Projectile.position - target.position;
					//Crystal.offset += Offset;
					Crystal.enemyID = target.whoAmI;
				}

				globalNPC.ShardAmount++;
			}

			globalNPC.ShardTimer = 600;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			for (int i = 0; i < 3; i++)
			{
				Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlassGravity>(), 0f, 0f).scale = Main.rand.NextFloat(0.6f, 0.9f);
				for (int d = 0; d < 4; d++)
				{
					Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<VitricSandDust>(), 0f, 0f).scale = Main.rand.NextFloat(0.8f, 1.2f);
				}
			}
		}
	}

	public class SoilgunSlushSoil : BaseSoilProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.SlushBlock;
		public SoilgunSlushSoil() : base(new Color(27, 40, 51), new Color(30, 19, 12), new Color(27, 40, 51), new Color(164, 182, 180), new Color(30, 19, 12), DustID.Slush) { }
		public override void SafeAI()
		{
			if (Main.rand.NextBool(8))
			{
				var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0f, 0f, 35, default, Main.rand.NextFloat(0.8f, 1.2f));
				dust.noGravity = true;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SoilgunGlobalNPC globalNPC = target.GetGlobalNPC<SoilgunGlobalNPC>();
			globalNPC.GlassPlayerID = Projectile.owner;
			globalNPC.GlassAmount++;

			if (Main.myPlayer == Projectile.owner)
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SoilgunIcicleProj>(), (int)(Projectile.damage * 0.65f), 0f, Projectile.owner, target.whoAmI);

			if (globalNPC.GlassAmount > 10)
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];
					if (proj.type == ModContent.ProjectileType<SoilgunIcicleProj>() && proj.active && proj.ai[0] == target.whoAmI)
					{
						proj.ai[1] = 1f;
						proj.Kill();
					}
				}

				globalNPC.GlassAmount = 0;
				SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath.WithVolumeScale(3f), Projectile.position);
				CameraSystem.shake += 5;
			}

			target.AddBuff(BuffID.Frostburn, 180);
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
			for (int i = 0; i < 15; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Slush, 0f, 0f, 15, default, Main.rand.NextFloat(0.8f, 1f));
			}
		}
	}

	public class SoilgunSiltSoil : BaseSoilProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.SiltBlock;
		public SoilgunSiltSoil() : base(new Color(22, 24, 32), new Color(30, 19, 12), new Color(49, 51, 61), new Color(106, 107, 118), new Color(30, 19, 12), DustID.Silt) { }
		public override void SafeAI()
		{
			if (Main.rand.NextBool(8))
			{
				var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, Main.rand.Next(new int[] { DustID.CopperCoin, DustID.SilverCoin, DustID.GoldCoin, DustID.PlatinumCoin }), 0f, 0f, 35, default, Main.rand.NextFloat(0.8f, 1.2f));
				dust.noGravity = true;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 12; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, Main.rand.Next(new int[] { DustID.CopperCoin, DustID.SilverCoin, DustID.GoldCoin, DustID.PlatinumCoin }), (Vector2.UnitY * Main.rand.NextFloat(-4, -1)).RotatedByRandom(0.25f), 35, default, Main.rand.NextFloat(1f, 1.3f));
			}

			for (int i = 0; i < 1 + Main.rand.Next(2); i++)
			{
				if (Main.myPlayer == Projectile.owner)
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-9f, -1)).RotatedByRandom(0.35f), ModContent.ProjectileType<SoilgunCoinsProjectile>(), (int)(Projectile.damage * 0.66f), 1f, Projectile.owner);
			}
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			for (int i = 0; i < 15; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Silt, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 1f));
			}
		}
	}

	public class SoilgunMudSoil : BaseSoilProjectile
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.MudBlock;
		public override void SafeSetDefaults()
		{
			Projectile.penetrate = 3;
			Projectile.usesLocalNPCImmunity = true; //without local immunity this was by far the worse ammo, about 3x less dps than just dirt. high hit cooldown to compensate though.
			Projectile.localNPCHitCooldown = 20;
		}
		public SoilgunMudSoil() : base(new Color(30, 21, 24), new Color(30, 19, 12), new Color(73, 57, 63), new Color(111, 83, 89), new Color(30, 19, 12), DustID.Mud) { }
		public override void SafeAI()
		{
			if (Main.rand.NextBool(4))
			{
				var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Mud, 0f, 0f, 35, default, Main.rand.NextFloat(0.75f, 1.15f));
				dust.noGravity = true;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.velocity.X *= -1;

			Projectile.damage = (int)(Projectile.damage * 0.66f);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.penetrate--;
			if (Projectile.penetrate <= 0)
			{
				Projectile.Kill();
			}
			else
			{
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

				SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

				if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
				{
					Projectile.velocity.X = -oldVelocity.X;
				}

				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
				{
					Projectile.velocity.Y = -oldVelocity.Y;
				}
			}

			return false;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			for (int i = 0; i < 12; i++)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Mud, 0f, 0f, 25, default, Main.rand.NextFloat(0.8f, 0.9f));
			}
		}
	}
}
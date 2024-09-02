﻿//TODO:
//Make it work with lucky coin
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Hell
{
	public class CharonsObol : CursedAccessory
	{
		public override string Texture => AssetDirectory.HellItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Charon's Obol");
			Tooltip.SetDefault("Converts all dropped money to ancient coins, which ricochet projectiles off themselves\nCoins lose their power when they contact any surface\n'Soon, may the Ferryman come...'");
		}

		public override void Load()
		{
			On_NPC.NPCLoot_DropMoney += SpawnObols;
		}

		public override void Unload()
		{
			On_NPC.NPCLoot_DropMoney -= SpawnObols;
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = ItemRarityID.Orange;
		}

		private void SpawnObols(On_NPC.orig_NPCLoot_DropMoney orig, NPC self, Player closestPlayer)
		{
			if (!Equipped(closestPlayer))
			{
				orig(self, closestPlayer);
				return;
			}

			if (self.value < 100)
			{
				CreateCoin(ModContent.ProjectileType<CopperObol>(), 100, closestPlayer, self);

				if (self.value > 50)
					CreateCoin(ModContent.ProjectileType<CopperObol>(), 100, closestPlayer, self);
			}
			else if (self.value < 1000)
			{
				CreateCoin(ModContent.ProjectileType<SilverObol>(), 150, closestPlayer, self);

				if (self.value > 500)
					CreateCoin(ModContent.ProjectileType<SilverObol>(), 150, closestPlayer, self);
			}
			else if (self.value < 10000)
			{
				CreateCoin(ModContent.ProjectileType<GoldObol>(), 200, closestPlayer, self);

				if (self.value > 5000)
					CreateCoin(ModContent.ProjectileType<GoldObol>(), 200, closestPlayer, self);
			}
			else
			{
				CreateCoin(ModContent.ProjectileType<PlatinumObol>(), 300, closestPlayer, self);

				if (self.value > 50000)
					CreateCoin(ModContent.ProjectileType<PlatinumObol>(), 300, closestPlayer, self);
			}
		}

		private void CreateCoin(int type, int damage, Player closestPlayer, NPC self)
		{
			Projectile.NewProjectile(self.GetSource_Loot(), self.Center, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 7) * 0.5f, type, damage, 3, closestPlayer.whoAmI);
		}
	}

	public abstract class ObolProj : ModProjectile
	{
		const int TRAILLENGTH = 600;

		public List<Vector2> cache;
		public Trail trail;
		public Trail trail2;

		public List<Vector2> offsetCache;

		float trailWidth = 4;

		private int pauseTimer;

		public int flashTimer = 0;

		public bool disappeared = false;
		public bool bouncedOff = false;
		public bool embedded = false;

		public float jerk = 0.05f;

		private Entity target = default;

		public Vector2 Center => Projectile.Center;

		public Vector2 Top => (Projectile.rotation - 1.57f).ToRotationVector2() * 5;

		private readonly List<NPC> alreadyHit = new();

		public virtual Color TrailColor => Color.Gold;

		public virtual int DustType => 1;

		private Player Player => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.HellItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Obol");
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 700;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.extraUpdates = 2;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.X * 0.2f;
			flashTimer--;

			if (embedded)
			{
				Projectile.alpha += 5;
				Projectile.velocity = Vector2.Zero;
				return;
			}

			Projectile.frameCounter++;

			if (Projectile.frameCounter % 12 == 0)
				Projectile.frame++;

			Projectile.frame %= Main.projFrames[Projectile.type];

			if (bouncedOff)
			{
				Projectile.friendly = false;
				Projectile.extraUpdates = 0;

				Projectile.velocity.Y += jerk - 0.1f;
				jerk += 0.01f;
				return;
			}

			if (!Projectile.friendly && !disappeared)
			{
				if (Main.rand.NextBool(15))
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), DustType, Vector2.Zero);

				Projectile.velocity.Y += 0.0325f;

				if (Projectile.timeLeft > 670)
					return;

				Rectangle detectionHitbox = Projectile.Hitbox;
				detectionHitbox.Inflate(14, 14);
				Projectile propeller = Main.projectile.Where(n => n.active && n != Projectile && n.friendly && (n.Hitbox.Intersects(Projectile.Hitbox) || n.ModProjectile is not ObolProj && n.Hitbox.Intersects(detectionHitbox))).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();

				if (propeller != default)
				{
					if (propeller.ModProjectile is ObolProj modProj)
					{
						Projectile.damage += propeller.damage / 2;
						modProj.bouncedOff = true;
						modProj.flashTimer = 15;
						propeller.timeLeft = 3000;
						propeller.extraUpdates = 0;
						propeller.velocity.X = Math.Sign(Projectile.velocity.X);
						propeller.velocity.Y = -1f;
						Projectile.Center = propeller.Center;
						cache = modProj.cache;
					}
					else
					{
						propeller.penetrate--;
					}

					Terraria.Audio.SoundEngine.PlaySound(SoundID.CoinPickup with { Pitch = -0.15f }, Projectile.Center);

					var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ObolImpact>(), 0, 0, Player.whoAmI);
					(proj.ModProjectile as ObolImpact).color = TrailColor;
					ManageCaches();

					Projectile.timeLeft = 3000;
					Projectile.friendly = true;

					PickTarget();
					pauseTimer = 15;
				}
			}
			else
			{
				Projectile.extraUpdates = 50;

				if (!disappeared)
				{
					ManageCaches();

					if (target != default)
					{
						if (!target.active)
							PickTarget();

						if (target is Projectile proj && proj.ModProjectile is ObolProj modproj && (modproj.disappeared || modproj.bouncedOff || proj.friendly || modproj.embedded))
							PickTarget();

						if (pauseTimer-- < 0)
							Projectile.velocity = Projectile.DirectionTo(target.Center) * 2;
						else
							Projectile.velocity = Vector2.Zero;
					}
				}
				else
				{
					if (trailWidth > 0.3f)
					{
						trailWidth *= 0.998f;

						if (trailWidth < 3.5f)
							trailWidth *= 0.998f;
					}
					else
					{
						trailWidth = 0;
					}
				}

				ManageTrail();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.penetrate++;

			CameraSystem.shake += 3;
			Helper.PlayPitched("Impacts/Ricochet", 0.2f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
			ManageCaches();
			Projectile.velocity = Vector2.Zero;
			disappeared = true;
			Projectile.friendly = false;
			Projectile.timeLeft = 3000;
			alreadyHit.Add(target);
		}

		public override bool? CanHitNPC(NPC hitTarget)
		{
			if (alreadyHit.Contains(hitTarget))
				return false;

			return base.CanHitNPC(hitTarget);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (!disappeared && !embedded)
			{
				Projectile.velocity = Vector2.Zero;

				if (Projectile.friendly)
				{
					ManageCaches();
					disappeared = true;
					Projectile.friendly = false;
					Projectile.timeLeft = 3000;
				}
				else
				{
					Projectile.timeLeft = 100;
					embedded = true;
				}
			}

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D bloom = Assets.Keys.GlowAlpha.Value;
			Color bloomColor = TrailColor;
			bloomColor.A = 0;

			if (!bouncedOff && !embedded)
			{
				if (!disappeared)
					Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, bloomColor * 0.08f, 0, bloom.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

				DrawPrimitives();
			}

			if (disappeared)
				return false;

			Color coinColor = bouncedOff ? lightColor : Color.White;
			Texture2D tex = Assets.Items.Hell.CharonsObol.Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			var frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frameBox, coinColor * ((255 - Projectile.alpha) / 255f), Projectile.rotation, frameBox.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			if (flashTimer <= 0)
				return false;

			Texture2D whiteTex = Assets.Items.Hell.ObolFlash.Value;
			Main.spriteBatch.Draw(whiteTex, Projectile.Center - Main.screenPosition, frameBox, Color.White * (flashTimer / 15f), Projectile.rotation, frameBox.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}

		private void PickTarget()
		{
			Projectile closestCoin = Main.projectile.Where(n => n.active && n != Projectile && !n.friendly && n.ModProjectile is ObolProj modProj2 && !modProj2.disappeared && !modProj2.bouncedOff && !modProj2.embedded && n.Distance(Projectile.Center) < 1000 && Helper.ClearPath(n.Center, Projectile.Center)).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();

			if (closestCoin != default)
			{
				target = closestCoin;
				Projectile.velocity = Projectile.DirectionTo(closestCoin.Center) * 7;
			}
			else
			{
				NPC closestNPC = Main.npc.Where(n => n.active && n.CanBeChasedBy() && !n.friendly && Helper.ClearPath(n.Center, Projectile.Center)).OrderBy(n => n.lifeMax).LastOrDefault();

				if (closestNPC != default)
				{
					target = closestNPC;
					Projectile.velocity = Projectile.DirectionTo(closestNPC.Center) * 7;
				}
				else
				{
					Projectile.velocity = Main.rand.NextVector2CircularEdge(2f, 2f);
				}
			}
		}

		private void ManageCaches()
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < TRAILLENGTH; i++)
					cache.Add(Center);
			}

			cache.Add(Center);

			while (cache.Count > TRAILLENGTH)
				cache.RemoveAt(0);
		}

		private void ManageTrail()
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			if (trail is null || trail.IsDisposed)
				trail = new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new NoTip(), factor => trailWidth * 3f, factor => TrailColor);

			if (trail2 is null || trail2.IsDisposed)
				trail2 = new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new NoTip(), factor => trailWidth * 1.5f, factor => Color.White * 0.75f);

			trail.Positions = cache.ToArray();
			trail2.Positions = cache.ToArray();

			if (!disappeared)
			{
				trail.NextPosition = Center;
				trail2.NextPosition = Center;
			}
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);
			effect.Parameters["alpha"].SetValue(trailWidth / 4f);

			trail?.Render(effect);
			//trail2?.Render(effect);
		}
	}

	internal class ObolImpact : ModProjectile
	{
		public Color color;

		public override string Texture => AssetDirectory.Assets + "StarTexture";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Obol Star");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(2, 2);
			Projectile.penetrate = -1;
			Projectile.timeLeft = 270;
		}

		public override void AI()
		{
			if (Projectile.scale < 0.75f)
				Projectile.alpha += 60;
			else
				Projectile.scale *= 0.93f;

			if (Projectile.alpha > 255)
				Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Color color2 = color * (1 - Projectile.alpha / 255f);
			color2.A = 0;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color2, Projectile.rotation, tex.Size() / 2, Projectile.scale * 0.25f * new Vector2(1.5f, 1f), SpriteEffects.None, 0f);
			return false;
		}
	}

	public class CopperObol : ObolProj
	{
		public override Color TrailColor => new(255, 139, 65);

		public override int DustType => DustID.CopperCoin;
	}

	public class SilverObol : ObolProj
	{
		public override Color TrailColor => new(210, 221, 222);

		public override int DustType => DustID.SilverCoin;
	}

	public class GoldObol : ObolProj
	{
		public override Color TrailColor => new(254, 255, 113);

		public override int DustType => DustID.GoldCoin;
	}

	public class PlatinumObol : ObolProj
	{
		public override Color TrailColor => new(253, 181, 249);

		public override int DustType => DustID.PlatinumCoin;
	}
}
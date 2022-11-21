//TODO:
//Sfx
//Screenshake
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.NPCs;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System;
using System.Media;

namespace StarlightRiver.Content.Items.Hell
{
	public class CharonsObol : CursedAccessory
    {
        public override string Texture => AssetDirectory.HellItem + Name;

		public CharonsObol() : base(ModContent.Request<Texture2D>(AssetDirectory.HellItem + "CharonsObol").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Charon's Obol");
			Tooltip.SetDefault("Converts all dropped money to reflective coins, which ricochet projectiles off themselves\nCoins lose their power when they contact any surface\n'Money is the root of all evil'");
		}

		public override void Load()
		{
			On.Terraria.NPC.NPCLoot_DropMoney += SpawnObols;
		}

		public override void Unload()
		{
			On.Terraria.NPC.NPCLoot_DropMoney -= SpawnObols;
		}

		public override void SafeSetDefaults()
        {
            Item.value = Item.sellPrice(0, 3, 0, 0);
            Item.rare = ItemRarityID.Orange;
        }

		private void SpawnObols(On.Terraria.NPC.orig_NPCLoot_DropMoney orig, NPC self, Player closestPlayer)
		{
			if (!Equipped(closestPlayer))
			{
				orig(self, closestPlayer);
				return;
			}

			if (self.value < 100)
			{
				CreateCoin(ModContent.ProjectileType<CopperObol>(), 20, closestPlayer, self);
				if (self.value > 50)
					CreateCoin(ModContent.ProjectileType<CopperObol>(), 20, closestPlayer, self);
			}
			else if (self.value < 1000)
			{
				CreateCoin(ModContent.ProjectileType<SilverObol>(), 30, closestPlayer, self);
				if (self.value > 500)
					CreateCoin(ModContent.ProjectileType<SilverObol>(), 30, closestPlayer, self);
			}
			else if (self.value < 10000)
			{
				CreateCoin(ModContent.ProjectileType<GoldObol>(), 40, closestPlayer, self);
				if (self.value > 5000)
					CreateCoin(ModContent.ProjectileType<GoldObol>(), 40, closestPlayer, self);
			}
			else
			{
				CreateCoin(ModContent.ProjectileType<PlatinumObol>(), 60, closestPlayer, self);
				if (self.value > 50000)
					CreateCoin(ModContent.ProjectileType<PlatinumObol>(), 60, closestPlayer, self);
			}
		}

		private void CreateCoin(int type, int damage, Player closestPlayer, NPC self) => Projectile.NewProjectile(self.GetSource_Loot(), self.Center, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 7) * 0.5f, type, damage, 3, closestPlayer.whoAmI);
	}
	public abstract class ObolProj : ModProjectile
	{

		public override string Texture => AssetDirectory.HellItem + Name;

		private Player Player => Main.player[Projectile.owner];

		readonly int TRAILLENGTH = 600;

		public List<Vector2> cache;
		public Trail trail;
		public Trail trail2;

		public List<Vector2> offsetCache;

		float trailWidth = 4;

		private int pauseTimer;

		public bool disappeared = false;
		public bool bouncedOff = false;
		public bool embedded = false;

		public float Agravity = 0.05f; //The gravity at which it falls after being bounced off of

		public Vector2 A4 => Projectile.Center;

		public Vector2 Top => (Projectile.rotation - 1.57f).ToRotationVector2() * 5;

		private Entity target = default;

		public virtual Color trailColor => Color.Gold;

		public virtual int dustType => 1;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Obol");
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
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

				Projectile.velocity.Y += Agravity - 0.1f;
				Agravity += 0.01f;
				return;
			}

			if (!Projectile.friendly && !disappeared)
			{
				if (Main.rand.NextBool(15))
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), dustType, Vector2.Zero);

				Projectile.velocity.Y += 0.0325f;

				if (Projectile.timeLeft > 670)
					return;

				Rectangle detectionHitbox = Projectile.Hitbox;
				detectionHitbox.Inflate(14, 14);
				var propeller = Main.projectile.Where(n => n.active && n != Projectile && n.friendly && (n.Hitbox.Intersects(Projectile.Hitbox) || (n.ModProjectile is not ObolProj && n.Hitbox.Intersects(detectionHitbox)))).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
				if (propeller != default)
				{
					if (propeller.ModProjectile is ObolProj modProj)
					{
						Projectile.damage += propeller.damage / 2;
						modProj.bouncedOff = true;
						propeller.timeLeft = 3000;
						propeller.extraUpdates = 0;
						propeller.velocity.X = Math.Sign(Projectile.velocity.X);
						propeller.velocity.Y = -1f;
						Projectile.Center = propeller.Center;
						cache = modProj.cache;
					}
					else
						propeller.penetrate--;

					//Helper.PlayPitched("Impacts/Ricochet", 0.2f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.CoinPickup with { Pitch = -0.15f}, Projectile.Center);

					Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ObolImpact>(), 0, 0, Player.whoAmI);
					(proj.ModProjectile as ObolImpact).color = trailColor;
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
						{
							trailWidth *= 0.998f;
						}
					}
					else
						trailWidth = 0;
				}
				ManageTrail();
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (!disappeared)
			{
				ManageCaches();
				Projectile.penetrate++;
				Projectile.velocity = Vector2.Zero;
				disappeared = true;
				Projectile.friendly = false;
				Projectile.timeLeft = 3000;
			}
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
			Texture2D bloom = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Color bloomColor = trailColor;
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
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			Rectangle frameBox = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frameBox, coinColor * ((255 - Projectile.alpha) / 255f), Projectile.rotation, frameBox.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		private void PickTarget()
		{
			var closestCoin = Main.projectile.Where(n => n.active && n != Projectile && !n.friendly && n.ModProjectile is ObolProj modProj2 && !modProj2.disappeared && !modProj2.bouncedOff && !modProj2.embedded && n.Distance(Projectile.Center) < 1000).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();

			if (closestCoin != default)
			{
				target = closestCoin;
				Projectile.velocity = Projectile.DirectionTo(closestCoin.Center) * 7;
			}
			else
			{
				var closestNPC = Main.npc.Where(n => n.active && n.CanBeChasedBy() && !n.friendly).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
				if (closestNPC != default)
				{
					target = closestNPC;
					Projectile.velocity = Projectile.DirectionTo(closestNPC.Center) * 7;
				}
				else
					Projectile.velocity = Main.rand.NextVector2CircularEdge(2f, 2f);
			}
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < TRAILLENGTH; i++)
					cache.Add(A4);
			}

			cache.Add(A4);

			while (cache.Count > TRAILLENGTH)
				cache.RemoveAt(0);
		}

		private void ManageTrail()
		{

			trail = trail ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new NoTip(), factor => trailWidth * 3f, factor =>
			{
				return trailColor;
			});

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new NoTip(), factor => trailWidth * 1.5f, factor =>
			{
				return Color.White * 0.75f;
			});

			trail.Positions = cache.ToArray();
			trail2.Positions = cache.ToArray();

			if (!disappeared)
			{
				trail.NextPosition = A4;
				trail2.NextPosition = A4;
			}
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["OrbitalStrikeTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["alpha"].SetValue(trailWidth / 4f);

			trail?.Render(effect);
			//trail2?.Render(effect);
		}
	}

	internal class ObolImpact : ModProjectile
	{
		public override string Texture => AssetDirectory.Assets + "StarTexture";

		public Color color;
		Player owner => Main.player[Projectile.owner];

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

			Color color2 = color * (1 - (Projectile.alpha / 255f));
			color2.A = 0;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color2, Projectile.rotation, tex.Size() / 2, Projectile.scale * 0.25f * new Vector2(1.5f, 1f), SpriteEffects.None, 0f);
			return false;
		}
	}

	public class CopperObol : ObolProj 
	{ 
		public override Color trailColor => new Color(255, 139, 65);

		public override int dustType => DustID.CopperCoin;

	}

	public class SilverObol : ObolProj 
	{ 
		public override Color trailColor => new Color(210, 221, 222);

		public override int dustType => DustID.SilverCoin;
	}

	public class GoldObol : ObolProj
	{
		public override Color trailColor => new Color(254, 255, 113);

		public override int dustType => DustID.GoldCoin;
	}

	public class PlatinumObol : ObolProj
	{
		public override Color trailColor => new Color(253, 181, 249);

		public override int dustType => DustID.PlatinumCoin;
	}
}
//TODO:
//Balance
//Momentum
//Obtainment
//Tooltip
//Fix gaps between coin trails
//Work on texture and colors
//Make coins unable to seek out inactive entities
//Adjust coin damage
//Adjust coin hitbox
//Particles
using Humanizer;
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

namespace StarlightRiver.Content.Items.Hell
{
	public class CharonsObol : CursedAccessory
    {
        public override string Texture => AssetDirectory.HellItem + Name;

		public CharonsObol() : base(ModContent.Request<Texture2D>(AssetDirectory.HellItem + "CharonsObol").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Charon's Obol");
			Tooltip.SetDefault("[PH] update later");
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
				Projectile.NewProjectile(self.GetSource_Loot(), self.Center, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 7) * 0.5f, ModContent.ProjectileType<CopperObol>(), 20, 3, closestPlayer.whoAmI);
				if (self.value > 50)
					Projectile.NewProjectile(self.GetSource_Loot(), self.Center, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 7) * 0.5f, ModContent.ProjectileType<CopperObol>(), 20, 3, closestPlayer.whoAmI);
			}
			else if (self.value < 1000)
			{
				Projectile.NewProjectile(self.GetSource_Loot(), self.Center, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 7) * 0.5f, ModContent.ProjectileType<SilverObol>(), 40, 3, closestPlayer.whoAmI);
				if (self.value > 500)
					Projectile.NewProjectile(self.GetSource_Loot(), self.Center, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 7) * 0.5f, ModContent.ProjectileType<SilverObol>(), 40, 3, closestPlayer.whoAmI);
			}
			else
			{
				Projectile.NewProjectile(self.GetSource_Loot(), self.Center, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 7) * 0.5f, ModContent.ProjectileType<GoldObol>(), 40, 3, closestPlayer.whoAmI);
				if (self.value > 5000)
					Projectile.NewProjectile(self.GetSource_Loot(), self.Center, -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 7) * 0.5f, ModContent.ProjectileType<GoldObol>(), 40, 3, closestPlayer.whoAmI);
			}
		}
	}
	public abstract class ObolProj : ModProjectile
	{

		public override string Texture => AssetDirectory.HellItem + Name;

		private Player Player => Main.player[Projectile.owner];

		const int TRAILLENGTH = 150;

		public List<Vector2> cache;
		public Trail trail;

		public List<Vector2> offsetCache;

		float trailWidth = 4;

		public bool disappeared = false;

		public Vector2 A4 => Projectile.Center;

		public Vector2 Top => (Projectile.rotation - 1.57f).ToRotationVector2() * 5;

		private Entity target = default;

		public virtual Color trailColor => Color.Gold;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Obol");
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;
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
			Projectile.frameCounter++;
			if (Projectile.frameCounter % 12 == 0)
				Projectile.frame++;
			Projectile.frame %= Main.projFrames[Projectile.type];
			if (!Projectile.friendly && !disappeared)
			{
				Projectile.velocity.Y += 0.0325f;

				if (Projectile.timeLeft > 670)
					return;

				var propeller = Main.projectile.Where(n => n.active && n != Projectile && n.friendly && n.Hitbox.Intersects(Projectile.Hitbox)).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
				if (propeller != default)
				{
					if (propeller.ModProjectile is ObolProj modProj)
					{
						propeller.active = false;
						cache = modProj.cache;
						//trail = modProj.trail;
						offsetCache = modProj.offsetCache;
					}
					ManageCaches();
					Projectile.friendly = true;
					var closestCoin = Main.projectile.Where(n => n.active && n != Projectile && !n.friendly && n.ModProjectile is ObolProj modProj2 && !modProj2.disappeared && n.Distance(Projectile.Center) < 1000).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();

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
							Projectile.velocity = propeller.velocity * 0.5f;
					}
				}
			}
			else 
			{
				Projectile.extraUpdates = 6;
				if (!disappeared)
				{
					ManageCaches();
					if (target != default)
						Projectile.velocity = Projectile.DirectionTo(target.Center) * 25;
				}
				else
				{
					if (trailWidth > 0.3f)
					{
						trailWidth *= 0.994f;
						if (trailWidth < 3.5f)
						{
							trailWidth *= 0.994f;
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
				Projectile.penetrate++;
				Projectile.velocity = Vector2.Zero;
				disappeared = true;
				Projectile.friendly = false;
				Projectile.timeLeft = 300;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (!disappeared)
			{
				if (Projectile.friendly)
				{
					Projectile.velocity = Vector2.Zero;
					disappeared = true;
					Projectile.friendly = false;
					Projectile.timeLeft = 300;
				}
				else
					return true;
			}
			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();
			if (disappeared)
				return false;
			return true;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				offsetCache = new List<Vector2>();
				for (int i = 0; i < TRAILLENGTH; i++)
				{
					cache.Add(A4);

					offsetCache.Add(Vector2.Zero);
				}
			}

			cache.Add(A4);
			offsetCache.Add((Projectile.velocity.ToRotation() + 1.57f).ToRotationVector2() * Main.rand.NextFloat(-0.02f, 0.02f));

			while (cache.Count > TRAILLENGTH)
			{
				cache.RemoveAt(0);
				offsetCache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{

			trail = trail ?? new Trail(Main.instance.GraphicsDevice, TRAILLENGTH, new TriangularTip(4), factor => trailWidth * 3f, factor =>
			{
				return trailColor;
			});

			if (disappeared)
			{
				for (int i = 0; i < cache.Count; i++)
				{
					cache[i] += offsetCache[i];
				}

			}

			trail.Positions = cache.ToArray();

			if (!disappeared)
				trail.NextPosition = A4;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["RebarTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "RebarTrailTexture").Value);
			effect.Parameters["noiseTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "RebarNoiseTexture").Value);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["progress"].SetValue(trailWidth / 4f);
			effect.Parameters["repeats"].SetValue(18);
			effect.Parameters["midColor"].SetValue(trailColor.ToVector3());


			trail?.Render(effect);
		}
	}

	public class CopperObol : ObolProj { public override Color trailColor => Color.Orange; }

	public class SilverObol : ObolProj { public override Color trailColor => Color.Silver; }

	public class GoldObol : ObolProj { public override Color trailColor => Color.Gold; }

}
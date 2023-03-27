using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class NoxiousNode : DummyTile
	{
		public override string Texture => AssetDirectory.Debug;

		public override int DummyType => ModContent.ProjectileType<NoxiousNodeDummy>();
	}

	internal class NoxiousNodeDummy : Dummy, IFaeWhippable
	{
		public ref float DetachedLife => ref Projectile.ai[0];

		public NoxiousNodeDummy() : base(ModContent.TileType<NoxiousNode>(), 8, 8) { }

		public override void Update()
		{
			if (DetachedLife > 0)
			{
				Projectile.velocity.Y += 0.5f;
				Projectile.tileCollide = true;
				Projectile.rotation += Projectile.velocity.X * 0.5f;

				DetachedLife--;
			}

			if (Main.rand.NextBool(4))
			{
				Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(128, 128);
				Vector2 vel = Vector2.UnitY.RotatedByRandom(6.28f) * -Main.rand.NextFloat(1);
				Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Cinder>(), vel, 0, new Color(20, 230, 255), 0.5f);
			}

			Vector2 gaspos = Projectile.Center + Main.rand.NextVector2Circular(100, 100);
			Dust.NewDustPerfect(gaspos, ModContent.DustType<Dusts.NoxiousGas>(), Vector2.Zero, 0, new Color(20, 230, 255) * 0.1f, 1.0f);

			Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.5f, 0.5f));

			base.Update();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Helpers.Helper.CheckCircularCollision(Projectile.Center, 128, targetHitbox);
		}

		public override void Collision(Player Player)
		{
			Player.AddBuff(Terraria.ID.BuffID.Venom, 12, false);
		}

		public override bool ValidTile(Tile tile)
		{
			return base.ValidTile(tile) || DetachedLife > 0;
		}

		public override void Kill(int timeLeft)
		{
			Helpers.Helper.PlayPitched("Effects/Splat", 1, 0, Projectile.Center);

			for (int k = 0; k < 50; k++)
			{
				Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(32, 32);
				Vector2 vel = Vector2.UnitY.RotatedByRandom(6.28f) * -Main.rand.NextFloat(4);
				Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Cinder>(), vel, 0, new Color(20, 230, 255), 0.5f);

				Vector2 gaspos = Projectile.Center + Main.rand.NextVector2Circular(32, 32);
				Vector2 gasvel = Vector2.UnitY.RotatedByRandom(6.28f) * -Main.rand.NextFloat(12);
				Dust.NewDustPerfect(gaspos, ModContent.DustType<Dusts.NoxiousGas>(), gasvel, 0, new Color(20, 230, 255) * 0.1f, 1.0f);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Color color = new Color(20, 230, 255) * (0.15f + 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.02f));
			Color color2 = new Color(20, 255, 220) * (0.15f + 0.05f * (float)Math.Sin(Main.GameUpdateCount * 0.05f));

			color.A = 0;
			color2.A = 0;

			float opacity = DetachedLife > 0 ? (DetachedLife - 110) / 10f : 1;

			Main.spriteBatch.Draw(tex, pos, null, color * opacity, 0, tex.Size() / 2, 5, 0, 0);
			Main.spriteBatch.Draw(tex, pos, null, color2 * opacity, 0, tex.Size() / 2, 5, 0, 0);

			Texture2D flowerTex = ModContent.Request<Texture2D>(AssetDirectory.OvergrowTile + "NoxiousNode").Value;
			Main.spriteBatch.Draw(flowerTex, pos, null, lightColor, Projectile.rotation, flowerTex.Size() / 2, 1, 0, 0);

			if (DetachedLife <= 0)
			{
				Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.OvergrowTile + "NoxiousNodeGlow").Value;
				Main.spriteBatch.Draw(glowTex, pos, null, Helpers.Helper.IndicatorColorProximity(400, 512, Projectile.Center), Projectile.rotation, glowTex.Size() / 2, 1, 0, 0);
			}
		}

		public void UpdateWhileWhipped(Whip whip)
		{
			if (DetachedLife <= 0)
			{
				if (Vector2.Distance(Main.MouseWorld, Projectile.Center) < 100)
				{
					whip.tipsPosition = Projectile.Center;
				}
				else
				{
					DetachedLife = 120;
					WorldGen.KillTile(ParentX, ParentY);
					Projectile.velocity = (Main.MouseWorld - Projectile.Center) * 0.1f;
					Helpers.Helper.PlayPitched("Effects/PickupHerbs", 1, -0.5f, Projectile.Center);
					CameraSystem.shake += 10;
				}
			}
			else
			{
				whip.tipsPosition = Projectile.Center;
			}
		}

		public bool DetachCondition()
		{
			return !Projectile.active;
		}

		public bool IsWhipColliding(Vector2 whipPosition)
		{
			return Vector2.Distance(whipPosition, Projectile.Center) < 21;
		}
	}

	internal class NoxiousNodeItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public NoxiousNodeItem() : base("Noxious Node", "Debug item", "NoxiousNode") { }
	}
}
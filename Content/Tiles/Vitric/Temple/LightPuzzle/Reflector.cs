using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using System;
using System.IO;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	class Reflector : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<ReflectorDummy>();

		public override string Texture => AssetDirectory.Debug;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Dirt, SoundID.Dig, new Color(1, 1, 1));
		}

		public override bool SpawnConditions(int i, int j)
		{
			return true;
		}

		public override bool RightClick(int i, int j)
		{
			var dummy = Dummy(i, j).ModProjectile as ReflectorDummy;

			if (dummy != null)
			{
				dummy.DeactivateDownstream();
				dummy.rotating = true;
			}

			return true;
		}
	}

	class ReflectorDummy : Dummy, IDrawAdditive
	{
		public bool rotating = false;
		public int frame = 0;

		private int rotateAnimation = 0;
		private Vector2 endPoint;

		public ref float Rotation => ref Projectile.ai[0];
		public ref float Emit => ref Projectile.ai[1];

		public ReflectorDummy() : base(ModContent.TileType<Reflector>(), 16, 16) { }

		public override void SafeSetDefaults()
		{
			frame = Main.rand.Next(5);
		}

		public override void Update()
		{
			if (rotating)
			{
				if (Vector2.Distance(Main.MouseWorld, Projectile.Center) > 48)
					Rotation += Helper.CompareAngle((Main.MouseWorld - Projectile.Center).ToRotation(), Rotation) * 0.1f;

				if (rotateAnimation < 15)
					rotateAnimation++;

				if (!Main.mouseRight)
				{
					rotating = false;
					FindEndpoint();
					Projectile.netUpdate = true;

					Parent.TileFrameX = (short)(Rotation / 6.28f * 3600);
					Rotation = Parent.TileFrameX / 3600f * 6.28f;
				}
			}
			else
			{
				Rotation = Parent.TileFrameX / 3600f * 6.28f;

				if (rotateAnimation > 0)
					rotateAnimation--;
			}
		}

		protected void FindEndpoint()
		{
			for (int k = 2; k < 160; k++)
			{
				Vector2 posCheck = Projectile.Center + Vector2.UnitX.RotatedBy(Rotation) * k * 8;

				if (Framing.GetTileSafely((int)posCheck.X / 16, (int)posCheck.Y / 16).TileType == ModContent.TileType<Reflector>())
				{
					endPoint = posCheck;

					if (Emit > 0)
						ActivateDownstream();

					break;
				}

				if (Framing.GetTileSafely((int)posCheck.X / 16, (int)posCheck.Y / 16).TileType == ModContent.TileType<LightGoal>())
				{
					endPoint = posCheck;
					Main.NewText("Solved!");
					LightPuzzleHandler.solvedPoints++;
				}

				if (Helper.PointInTile(posCheck) || k == 159)
				{
					endPoint = posCheck;
					break;
				}
			}
		}

		private void ActivateDownstream()
		{
			Emit = 1;
			Projectile dummy = DummyTile.GetDummy<ReflectorDummy>((int)endPoint.X / 16, (int)endPoint.Y / 16);

			if (dummy != null)
			{
				if ((dummy.ModProjectile as ReflectorDummy).Emit > 0) //base case to prevent infinite reflections
					return;

				(dummy.ModProjectile as ReflectorDummy).Emit = 1;
				(dummy.ModProjectile as ReflectorDummy).FindEndpoint();
			}
		}

		public void DeactivateDownstream(bool disableSelf = false)
		{
			if (disableSelf)
				Emit = 0;

			if (Framing.GetTileSafely((int)endPoint.X / 16, (int)endPoint.Y / 16).TileType == ModContent.TileType<LightGoal>())
			{
				Main.NewText("Unsolved...");
				LightPuzzleHandler.solvedPoints--;
			}

			Projectile dummy = DummyTile.GetDummy<ReflectorDummy>((int)endPoint.X / 16, (int)endPoint.Y / 16);

			if (dummy != null)
			{
				if ((dummy.ModProjectile as ReflectorDummy).Emit <= 0) //base case to prevent infinite reflections
					return;

				(dummy.ModProjectile as ReflectorDummy).DeactivateDownstream(true);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D texUnder = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MirrorUnder").Value;
			Main.spriteBatch.Draw(texUnder, Projectile.Center - Main.screenPosition, null, Color.White, 0, texUnder.Size() / 2, 1, 0, 0);

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "Reflector").Value;
			var drawFrame = new Rectangle(50 * frame, 0, 50, 50);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, drawFrame, Color.White, Rotation - 1.57f, Vector2.One * 25, 1, 0, 0);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tickTexture = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "TickLine").Value;
			float opacity = rotateAnimation / 15f;

			for (int k = 0; k < 30; k++)
			{
				float rot = k / 30f * 6.28f;
				float rotOpacity = 1.0f - Math.Abs(Helper.CompareAngle(rot, Rotation)) / 6.28f;
				Color color = new Color(100, 220, 255) * rotOpacity * opacity;
				spriteBatch.Draw(tickTexture, Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * 32, null, color, rot + 1.57f, tickTexture.Size() / 2, 0.5f + rotOpacity * 0.5f, 0, 0);
			}

			spriteBatch.Draw(tickTexture, Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(Rotation) * 60, null, new Color(150, 220, 255) * opacity, Rotation + 1.57f, tickTexture.Size() / 2, 3f, 0, 0);

			if (Emit <= 0)
				return;

			//Laser
			int sin = (int)(Math.Sin(StarlightWorld.visualTimer * 3) * 20f); //Just a copy/paste of the boss laser. Need to tune this later
			Color color2 = new Color(100, 200 + sin, 255) * 0.65f;

			Texture2D texBeam = ModContent.Request<Texture2D>(AssetDirectory.MiscTextures + "BeamCore").Value;
			Texture2D texBeam2 = ModContent.Request<Texture2D>(AssetDirectory.MiscTextures + "BeamCore").Value;

			var origin = new Vector2(0, texBeam.Height / 2);
			var origin2 = new Vector2(0, texBeam2.Height / 2);

			Effect effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value;

			effect.Parameters["uColor"].SetValue(color2.ToVector3() * 0.35f);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

			float height = texBeam.Height / 10f * (1 - opacity);
			int width = (int)(Projectile.Center - endPoint).Length() - 6;

			Vector2 pos = Projectile.Center + Vector2.UnitX.RotatedBy(Rotation) * 6 - Main.screenPosition;

			var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 2.75f));
			var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);

			var source = new Rectangle((int)(Main.GameUpdateCount / 140f * -texBeam.Width), 0, texBeam.Width, texBeam.Height);
			var source2 = new Rectangle((int)(Main.GameUpdateCount / 80f * -texBeam2.Width), 0, texBeam2.Width, texBeam2.Height);

			spriteBatch.Draw(texBeam, target, source, color2, Rotation, origin, 0, 0);
			spriteBatch.Draw(texBeam2, target2, source2, color2 * 0.5f, Rotation, origin2, 0, 0);

			for (int i = 0; i < width; i += 10)
			{
				Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(Rotation) * i + Main.screenPosition, color2.ToVector3() * height * 0.010f);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			Texture2D impactTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;
			Texture2D impactTex2 = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ItemGlow").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value;

			target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 4.5f));
			spriteBatch.Draw(glowTex, target, source, color2 * 0.75f, Rotation, new Vector2(0, glowTex.Height / 2), 0, 0);

			spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color2 * (height * 0.024f), 0, impactTex.Size() / 2, 2.2f, 0, 0);
			spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color2 * 0.5f * (height * 0.1f), StarlightWorld.visualTimer * 2, impactTex2.Size() / 2, 0.2f, 0, 0);
			spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color2 * 1.5f * (height * 0.1f), StarlightWorld.visualTimer * 2.2f, impactTex2.Size() / 2, 0.1f, 0, 0);
		}

		public override void SafeSendExtraAI(BinaryWriter writer)
		{
			writer.WriteVector2(endPoint);
		}

		public override void SafeReceiveExtraAI(BinaryReader reader)
		{
			endPoint = reader.ReadVector2();
		}
	}

	[SLRDebug]
	class ReflectorItem : QuickTileItem
	{
		public ReflectorItem() : base("Reflector", "Debug Item", "Reflector") { }
	}
}
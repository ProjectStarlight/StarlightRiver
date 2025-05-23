﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Packets;
using StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.LightPuzzle
{
	class Reflector : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<ReflectorDummy>();

		public override string Texture => AssetDirectory.Debug;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Dirt, SoundID.Dig, new Color(1, 1, 1));
		}

		public override bool SpawnConditions(int i, int j)
		{
			return true;
		}
	}

	class ReflectorDummy : Dummy
	{
		public bool rotating = false;
		public int frame = 0;

		private int rotateAnimation = 0;
		protected Vector2 endPoint;

		public float rotation;
		public float emitting;

		public ReflectorDummy() : base(ModContent.TileType<Reflector>(), 16, 16) { }

		public override void SafeSetDefaults()
		{
			frame = Main.rand.Next(5);
		}

		public override Rectangle? GetClickbox()
		{
			Rectangle box = Hitbox;
			box.Inflate(16, 16);
			return box;
		}

		public override void RightClick(int i, int j)
		{
			DeactivateDownstream();
			rotating = true;
		}

		public override void RightClickHover(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<GearTilePlacer>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override void Update()
		{
			if (rotating) // this rotation is all done client side and then updated to other clients when key is released in one packet
			{
				if (Vector2.Distance(Main.MouseWorld, Center) > 48)
					rotation += GeometryHelper.CompareAngle((Main.MouseWorld - Center).ToRotation(), rotation) * 0.1f;

				if (rotateAnimation < 15)
					rotateAnimation++;

				if (!Main.mouseRight)
				{
					rotating = false;
					LightPuzzleUpdatePacket puzzlePacket = new LightPuzzleUpdatePacket((int)(position.X / 16), (int)(position.Y / 16), type, rotation);
					puzzlePacket.Send();
				}
			}
			else
			{
				rotation = Parent.TileFrameX / 3600f * 6.28f;

				if (rotateAnimation > 0)
					rotateAnimation--;
			}
		}

		public void FindEndpoint()
		{
			for (int k = 2; k < 160; k++)
			{
				Vector2 posCheck = Center + Vector2.UnitX.RotatedBy(rotation) * k * 8;

				if (Framing.GetTileSafely((int)posCheck.X / 16, (int)posCheck.Y / 16).TileType == ModContent.TileType<Reflector>())
				{
					endPoint = posCheck;

					if (emitting > 0 && !rotating)
						ActivateDownstream();

					break;
				}

				if (Framing.GetTileSafely((int)posCheck.X / 16, (int)posCheck.Y / 16).TileType == ModContent.TileType<LightGoal>() && emitting == 1)
				{
					endPoint = posCheck;
					LightPuzzleHandler.solved = true;
				}

				if (CollisionHelper.PointInTile(posCheck) || k == 159)
				{
					endPoint = posCheck;
					break;
				}
			}
		}

		private void ActivateDownstream()
		{
			emitting = 1;
			Dummy dummy = DummyTile.GetDummy<ReflectorDummy>((int)endPoint.X / 16, (int)endPoint.Y / 16);

			if (dummy != null)
			{
				if ((dummy as ReflectorDummy).emitting > 0) //base case to prevent infinite reflections
					return;

				(dummy as ReflectorDummy).emitting = 1;
				(dummy as ReflectorDummy).FindEndpoint();
			}
		}

		public void DeactivateDownstream(bool disableSelf = false)
		{
			if (disableSelf)
				emitting = 0;

			if (Framing.GetTileSafely((int)endPoint.X / 16, (int)endPoint.Y / 16).TileType == ModContent.TileType<LightGoal>())
			{
				LightPuzzleHandler.solved = false;
			}

			Dummy dummy = DummyTile.GetDummy<ReflectorDummy>((int)endPoint.X / 16, (int)endPoint.Y / 16);

			if (dummy != null)
			{
				if ((dummy as ReflectorDummy).emitting <= 0) //base case to prevent infinite reflections
					return;

				(dummy as ReflectorDummy).DeactivateDownstream(true);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D texUnder = Assets.Tiles.Vitric.MirrorUnder.Value;
			Main.spriteBatch.Draw(texUnder, Center - Main.screenPosition, null, lightColor, 0, texUnder.Size() / 2, 1, 0, 0);

			Texture2D tex = Assets.Tiles.Vitric.Reflector.Value;
			var drawFrame = new Rectangle(50 * frame, 0, 50, 50);
			Main.spriteBatch.Draw(tex, Center - Main.screenPosition, drawFrame, lightColor, rotation - 1.57f, Vector2.One * 25, 1, 0, 0);

			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Texture2D tickTexture = Assets.Tiles.Vitric.TickLine.Value;
			float opacity = rotateAnimation / 15f;

			for (int k = 0; k < 30; k++)
			{
				float rot = k / 30f * 6.28f;
				float rotOpacity = 1.0f - Math.Abs(GeometryHelper.CompareAngle(rot, rotation)) / 6.28f;
				Color color = new Color(100, 220, 255, 0) * rotOpacity * opacity;
				Main.spriteBatch.Draw(tickTexture, Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * 32, null, color, rot + 1.57f, tickTexture.Size() / 2, 0.5f + rotOpacity * 0.5f, 0, 0);
			}

			Main.spriteBatch.Draw(tickTexture, Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rotation) * 60, null, new Color(150, 220, 255, 0) * opacity, rotation + 1.57f, tickTexture.Size() / 2, 3f, 0, 0);

			if (emitting <= 0)
				return;

			//Laser
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				int sin = (int)(Math.Sin(StarlightWorld.visualTimer * 3) * 20f); //Just a copy/paste of the boss laser. Need to tune this later
				Color color2 = new Color(100, 200 + sin, 255, 0) * 0.65f;

				Texture2D texBeam = Assets.Misc.BeamCore.Value;
				Texture2D texBeam2 = Assets.Misc.BeamCore.Value;

				var origin = new Vector2(0, texBeam.Height / 2);
				var origin2 = new Vector2(0, texBeam2.Height / 2);

				Effect effect = ShaderLoader.GetShader("GlowingDust").Value;

				if (effect != null)
				{
					effect.Parameters["uColor"].SetValue(color2.ToVector3() * 0.35f);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, SamplerState.PointWrap, default, RasterizerState.CullNone, effect);

					float height = texBeam.Height / 10f * (1 - opacity);
					int width = (int)(Center - endPoint).Length();

					Vector2 pos = Center - Main.screenPosition;

					var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 2.75f));
					var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 4f));

					var source = new Rectangle((int)(Main.GameUpdateCount / 140f * -texBeam.Width), 0, texBeam.Width, texBeam.Height);
					var source2 = new Rectangle((int)(Main.GameUpdateCount / 80f * -texBeam2.Width), 0, width, texBeam2.Height);

					Main.spriteBatch.Draw(texBeam, target, source, color2, rotation, origin, 0, 0);
					Main.spriteBatch.Draw(texBeam2, target2, source2, color2 * 0.5f, rotation, origin2, 0, 0);

					for (int i = 0; i < width; i += 10)
					{
						Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(rotation) * i + Main.screenPosition, color2.ToVector3() * height * 0.010f);
					}

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default);

					Texture2D impactTex = Assets.Masks.GlowSoftAlpha.Value;
					Texture2D impactTex2 = Assets.GUI.ItemGlow.Value;
					Texture2D glowTex = Assets.GlowTrail.Value;

					target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 4.5f));
					Main.spriteBatch.Draw(glowTex, target, source, color2 * 0.75f, rotation, new Vector2(0, glowTex.Height / 2), 0, 0);

					Main.spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color2 * (height * 0.024f), 0, impactTex.Size() / 2, 2.8f, 0, 0);
					Main.spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color2 * 0.5f * (height * 0.1f), StarlightWorld.visualTimer * 2, impactTex2.Size() / 2, 0.2f, 0, 0);
					Main.spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color2 * 1.5f * (height * 0.1f), StarlightWorld.visualTimer * 2.2f, impactTex2.Size() / 2, 0.1f, 0, 0);
				}
			});
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
		public ReflectorItem() : base("Reflector", "{{Debug}} Item", "Reflector") { }
	}
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using System.IO;
using StarlightRiver.Content.Items;

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

		public override bool RightClick(int i, int j)
		{
			var dummy = Dummy(i, j).ModProjectile as ReflectorDummy;

			if (dummy != null)
			{
				if (Main.LocalPlayer.HeldItem.type == ModContent.ItemType<DebugStick>())
				{
					dummy.Emit = 1;
					return true;
				}

				dummy.DeactivateDownstream();
				dummy.rotating = true;
			}

			return true;
		}
	}

	class ReflectorDummy : Dummy, IDrawAdditive
	{
		public bool rotating = false;

		private int rotateAnimation = 0;
		private Vector2 endPoint;

		public ref float Rotation => ref Projectile.ai[0];
		public ref float Emit => ref Projectile.ai[1];

		public ReflectorDummy() : base(ModContent.TileType<Reflector>(), 16, 16) { }

		public override void Update()
		{
			if (rotating)
			{
				if(Vector2.Distance(Main.MouseWorld, Projectile.Center) > 48)
					Rotation += Helpers.Helper.CompareAngle((Main.MouseWorld - Projectile.Center).ToRotation(), Rotation) * 0.1f;

				if (rotateAnimation < 15)
					rotateAnimation++;

				if (!Main.mouseRight)
				{
					rotating = false;
					FindEndpoint();
					Projectile.netUpdate = true;
				}
			}
			else if (rotateAnimation > 0)
				rotateAnimation--;
		}

		private void FindEndpoint()
		{
			for (int k = 2; k < 160; k++)
			{
				Vector2 posCheck = Projectile.Center + Vector2.UnitX.RotatedBy(Rotation) * k * 8;

				if(Framing.GetTileSafely((int)posCheck.X / 16, (int)posCheck.Y / 16).TileType == ModContent.TileType<Reflector>())
				{
					endPoint = posCheck;

					if (Emit > 0)
						ActivateDownstream();

					break;
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
			var dummy = DummyTile.GetDummy<ReflectorDummy>((int)endPoint.X / 16, (int)endPoint.Y / 16);

			if (dummy != null)
			{
				if ((dummy.ModProjectile as ReflectorDummy).Emit > 0) //base case to prevent infinite reflections
					return;

				(dummy.ModProjectile as ReflectorDummy).ActivateDownstream();
			}
		}

		public void DeactivateDownstream(bool disableSelf = false)
		{
			if(disableSelf)
				Emit = 0;

			var dummy = DummyTile.GetDummy<ReflectorDummy>((int)endPoint.X / 16, (int)endPoint.Y / 16);

			if (dummy != null)
			{
				if ((dummy.ModProjectile as ReflectorDummy).Emit <= 0) //base case to prevent infinite reflections
					return;

				(dummy.ModProjectile as ReflectorDummy).DeactivateDownstream(true);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			var tex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MirrorOver").Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Rotation - 3.14f - 1.57f / 2, tex.Size() / 2, 1, 0, 0);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			var tickTexture = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "TickLine").Value;
			float opacity = rotateAnimation / 15f;

			for(int k = 0; k < 30; k++)
			{
				float rot = k / 30f * 6.28f;
				float rotOpacity = (1.0f - (Math.Abs(Helpers.Helper.CompareAngle(rot, Rotation)) / 6.28f));
				Color color = new Color(100, 220, 255) * rotOpacity * opacity;
				spriteBatch.Draw(tickTexture, Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(rot) * 32, null, color, rot + 1.57f, tickTexture.Size() / 2, 0.5f + rotOpacity * 0.5f, 0, 0);
			}

			spriteBatch.Draw(tickTexture, Projectile.Center - Main.screenPosition + Vector2.UnitX.RotatedBy(Rotation) * 60, null, new Color(150, 220, 255) * opacity, Rotation + 1.57f, tickTexture.Size() / 2, 3f, 0, 0);

			if (Emit <= 0)
				return;

			//Laser
			int sin = (int)(Math.Sin(StarlightWorld.rottime * 3) * 20f); //Just a copy/paste of the boss laser. Need to tune this later
			var color2 = new Color(100, 200 + sin, 255);

			var texBeam = ModContent.Request<Texture2D>(AssetDirectory.MiscTextures + "BeamCore").Value;
			var texBeam2 = ModContent.Request<Texture2D>(AssetDirectory.MiscTextures + "BeamCore").Value;

			Vector2 origin = new Vector2(0, texBeam.Height / 2);
			Vector2 origin2 = new Vector2(0, texBeam2.Height / 2);

			var effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value;

			effect.Parameters["uColor"].SetValue(color2.ToVector3());

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

			float height = texBeam.Height / 10f * (1 - opacity);
			int width = (int)(Projectile.Center - endPoint).Length() - 6;

			var pos = Projectile.Center + Vector2.UnitX.RotatedBy(Rotation) * 6 - Main.screenPosition;

			var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 2.5f));
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

			var impactTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowSoft").Value;
			var impactTex2 = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ItemGlow").Value;
			var glowTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value;

			target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 3.5f));
			spriteBatch.Draw(glowTex, target, source, color2 * 0.75f, Rotation, new Vector2(0, glowTex.Height / 2), 0, 0);

			spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color2 * (height * 0.024f), 0, impactTex.Size() / 2, 2.8f, 0, 0);
			spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color2 * (height * 0.1f), StarlightWorld.rottime * 2, impactTex2.Size() / 2, 0.25f, 0, 0);
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

	class ReflectorItem : QuickTileItem
	{
		public ReflectorItem() : base("Reflector", "Debug Item", "Reflector") { }
	}
}

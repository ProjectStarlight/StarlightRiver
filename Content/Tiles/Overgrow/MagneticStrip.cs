using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	public enum GravDirection
	{
		down, up, left, right
	}

	internal class MagneticStrip : ModTile
	{
		public override string Texture => AssetDirectory.Debug;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, 0, 0, SoundID.Tink, new Color(10, 10, 10), ModContent.ItemType<MagneticStripItem>());
			Main.tileFrameImportant[Type] = true;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			var tile = Framing.GetTileSafely(i, j);

			Rectangle rect = new Rectangle(i * 16, j * 16, 16, 160);

			switch (tile.TileFrameX)
			{
				case 0: rect = new Rectangle(i * 16, j * 16 - 160, 16, 160); break;
				case 1: rect = new Rectangle(i * 16, j * 16, 16, 160); break;
				case 2: rect = new Rectangle(i * 16, j * 16, 160, 16); break;
				case 3: rect = new Rectangle(i * 16 - 160, j * 16, 160, 16); break;
				default: rect = new Rectangle(i * 16, j * 16, 160, 16); break;
			}

			var player = Main.LocalPlayer;

			if (player.Hitbox.Intersects(rect))
			{
				var mp = player.GetModPlayer<MagneticStripPlayer>();

				if (mp.direction == GravDirection.down)
					mp.ChangeDirection((GravDirection)tile.TileFrameX);

				mp.direction = (GravDirection)tile.TileFrameX;
				mp.holdOver = 10;
				mp.cutoffCoordinate = tile.TileFrameX <= 1 ? j * 16 : i * 16;
			}
		}

		public override bool Slope(int i, int j)
		{
			var tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX++;
			tile.TileFrameX %= 4;
			return false;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			Main.instance.TilesRenderer.AddSpecialLegacyPoint(new Point(i, j));
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var tile = Framing.GetTileSafely(i, j);

			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;
			Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + Helpers.Helper.TileAdj * 16;
			Rectangle target = new Rectangle((int)pos.X + 16, (int)pos.Y + 176, 16, 160);
			Rectangle source = new Rectangle(0, 0, 1, tex.Height / 2);
			var color = new Color(100, 255, 255) * 0.35f;
			color.A = 0;

			float rotation;

			switch (tile.TileFrameX)
			{
				case 0: rotation = 0; break;
				case 1: rotation = 3.14f; break;
				case 2: rotation = 1.57f; break;
				case 3: rotation = 3.14f + 1.57f; target.Offset(new Point(-176, -160)); break;
				default: rotation = 0; break;
			}

			spriteBatch.Draw(tex, target, source, color, rotation, Vector2.Zero, 0, 0);

			var tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;

			for (int k = 0; k < 8; k++)
			{
				float time = ((Main.GameUpdateCount + k * 75) % 600) / 600f;
				Rectangle target2 = new Rectangle((int)pos.X + 16, (int)pos.Y + 16, 16, 2);
				target2.Offset(new Vector2(0, 160 - time * 160).RotatedBy(rotation + 3.14f).ToPoint());
				spriteBatch.Draw(tex2, target2, source, color * (time), rotation, Vector2.Zero, 0, 0);
			}
		}
	}

	internal class MagneticStripItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public MagneticStripItem() : base("Magnetic Strip", "Debug item", "MagneticStrip") { }
	}

	internal class MagneticStripPlayer : ModPlayer //TODO: Consider generalizing this later?
	{
		public int holdOver;
		public int cutoffCoordinate;
		public float fakeXVel;

		public GravDirection direction;

		public override void PreUpdateMovement()
		{
			if (holdOver == 1)
			{
				Player.fullRotation = 0;
				return;
			}

			if (direction == GravDirection.up)
			{
				Player.fullRotation = 3.14f;
				Player.fullRotationOrigin = Player.Size / 2;

				Player.direction = Player.velocity.X > 0 ? -1 : 1;

				if (Player.velocity.Y > -Player.maxFallSpeed)
					Player.velocity.Y += Player.gravity * -2;

				if (Player.position.Y <= cutoffCoordinate + 18)
					Player.velocity.Y = 0;

				if (Player.velocity.Y == 0 && Player.controlJump)
				{
					Player.velocity.Y = Player.jumpHeight;
					Player.releaseJump = false;
					Player.controlJump = false;
					Player.justJumped = true;
				}

				if (Player.releaseJump)
				{
					Player.releaseJump = false;
					Player.controlJump = false;
					Player.justJumped = true;
				}
			}

			if (direction == GravDirection.right)
			{
				Player.fullRotation = 3.14f + 1.57f;
				Player.fullRotationOrigin = Player.Size / 2;

				Player.direction = Player.velocity.X > 0 ? 1 : -1;

				if (fakeXVel < Player.maxFallSpeed)
					fakeXVel += Player.gravity;

				Player.position.X += fakeXVel;

				Player.position.Y += -Player.velocity.X;

				Player.velocity.Y -= Player.gravity;

				if (Player.position.X >= cutoffCoordinate - 32)
				{
					Player.position.X = cutoffCoordinate - 32;
					Player.velocity.Y = 0;
					fakeXVel = 0;
				}

				else if (Player.velocity.Y == 0)
					Player.velocity.Y = 0.01f;

				Player.position.X -= Player.velocity.X;

				if (Player.velocity.Y == 0 && Player.controlJump)
				{
					fakeXVel = -Player.jumpHeight;
					Player.releaseJump = false;
					Player.controlJump = false;
					Player.justJumped = true;
				}

				if (Player.releaseJump)
				{
					Player.releaseJump = false;
					Player.controlJump = false;
					Player.justJumped = true;
				}
			}
		}

		public override void ResetEffects()
		{
			if (holdOver <= 1)
			{
				direction = GravDirection.down;
			}
			else
				holdOver--;
		}

		internal void ChangeDirection(GravDirection direction)
		{
			if (direction == GravDirection.right)
			{
				fakeXVel = Math.Max(1, Player.velocity.X);
				Player.velocity.X = Player.velocity.Y;
			}
		}
	}
}

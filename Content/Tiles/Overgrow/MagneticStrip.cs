using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	public enum GravDirection //for readability
	{
		down, up, left, right
	}

	internal class MagneticStrip : ModTile
	{
		public override string Texture => AssetDirectory.OvergrowTile + "MagnetStrip";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSet(this, 0, 0, SoundID.Tink, new Color(10, 10, 10), ModContent.ItemType<MagneticStripItem>());
			Main.tileFrameImportant[Type] = true;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			var tile = Framing.GetTileSafely(i, j);

			// Dont switch gravity if toggled
			if (tile.IsActuated)
				return;

			var rect = tile.TileFrameX switch
			{
				0 * 18 => new Rectangle(i * 16, j * 16 - 160, 16, 160),
				1 * 18 => new Rectangle(i * 16, j * 16, 16, 160),
				2 * 18 => new Rectangle(i * 16, j * 16, 160, 16),
				3 * 18 => new Rectangle(i * 16 - 160, j * 16, 160, 16),
				_ => new Rectangle(i * 16, j * 16, 160, 16),
			};

			var player = Main.LocalPlayer;

			if (player.Hitbox.Intersects(rect))
			{
				var mp = player.GetModPlayer<MagneticStripPlayer>();

				mp.direction = (GravDirection)(tile.TileFrameX / 18);
				mp.holdOver = 10;
				mp.cutoffCoordinate = tile.TileFrameX <= 18 ? j * 16 : i * 16;
			}

			if (Main.rand.NextBool(10))
			{
				var dist = Main.rand.NextFloat(160);
				var vel = dist / 30f;

				var rot = tile.TileFrameX switch
				{
					0 * 18 => -MathHelper.PiOver2,
					1 * 18 => MathHelper.PiOver2,
					2 * 18 => 0f,
					3 * 18 => MathHelper.Pi,
					_ => 0f
				};

				var pos = new Vector2(i, j) * 16 + Vector2.One * 8;

				Dust.NewDustPerfect(pos + Vector2.UnitX.RotatedBy(rot) * dist, ModContent.DustType<Dusts.PixelatedEmber>(), Vector2.UnitX.RotatedBy(rot + 3.14f) * vel, 0, new Color(100, 255, 255, 0), Main.rand.NextFloat(0.1f));
			}
		}

		public override bool Slope(int i, int j)
		{
			//sloping the tile will rotate it instead of normal sloping behavior!
			var tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX += 18;
			tile.TileFrameX %= 4 * 18;
			return false;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			Main.instance.TilesRenderer.AddSpecialLegacyPoint(new Point(i, j));
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var tile = Framing.GetTileSafely(i, j);

			if (tile.IsActuated)
				return;

			//Draw the glow
			var tex = Assets.GlowTrail.Value;
			Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + Vector2.One * Main.offScreenRange;
			Rectangle target = new Rectangle((int)pos.X + 16, (int)pos.Y + 176, 16, 160);
			Rectangle source = new Rectangle(0, 0, 1, tex.Height / 2);
			var color = new Color(100, 200, 255) * 0.15f;
			color.A = 0;

			float rotation;

			switch (tile.TileFrameX)
			{
				case 0 * 18: rotation = 0; target.Offset(new Point(-16, -336)); break;
				case 1 * 18: rotation = 3.14f; break;
				case 2 * 18: rotation = 1.57f; target.Offset(new Point(160, -176)); break;
				case 3 * 18: rotation = 3.14f + 1.57f; target.Offset(new Point(-176, -160)); break;
				default: rotation = 0; break;
			}

			spriteBatch.Draw(tex, target, source, color, rotation, Vector2.Zero, 0, 0); 

			var tex2 = Assets.MagicPixel.Value;

			//Draw the lines moving in the direction of gravity
			for (int k = 0; k < 8; k++)
			{
				float time = ((Main.GameUpdateCount + k * 75) % 600) / 600f;
				Rectangle target2 = new Rectangle((int)pos.X + 16, (int)pos.Y + 16, 16, 2);

				switch (tile.TileFrameX)
				{
					case 0 * 18: target2.Offset(new Point(-16, -16)); break;
					case 1 * 18: break;
					case 2 * 18: target2.Offset(new Point(0, -16)); break;
					case 3 * 18: target2.Offset(new Point(-16, 0)); break;
					default: break;
				}

				target2.Offset(new Vector2(0, 160 - time * 160).RotatedBy(rotation + 3.14f).ToPoint());
				spriteBatch.Draw(tex2, target2, source, color * (time), rotation, Vector2.Zero, 0, 0);
			}
		}
	}

	internal class MagneticStripItem : BaseTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public MagneticStripItem() : base("Magnetic Strip", "Debug item", "MagneticStrip") { }
	}

	internal class MagneticStripPlayer : ModPlayer //Maybe consider generalizing this later?
	{
		public int holdOver;
		public int cutoffCoordinate;
		public float fakeXVel;

		public GravDirection direction;
		public GravDirection previous;

		public Vector2 enterVel;
		public int enterTime;
		public float lastYVel;

		public override void PreUpdateMovement()
		{
			//Adjust player dimensions when appropriate
			if (direction != previous)
			{
				ChangeDirection(direction);
				enterVel = Player.velocity;
				enterTime = 15;
			}

			previous = direction;

			//Reset the player's rotation so we dont get stuck rotated the wrong way when not in a field
			if (holdOver == 1)
			{
				Player.fullRotation = 0;
				return;
			}

			//we obviously dont need any special behavior for downwards gravity!

			//upside down
			if (direction == GravDirection.up)
			{
				//flip the player sprite around for visual effect
				Player.fullRotation = 3.14f;
				Player.fullRotationOrigin = Player.Size / 2;

				//corrects their facing direction since it would be reversed due to rotation otherwise
				Player.direction = Player.velocity.X > 0 ? -1 : 1;

				//cancels the vanilla gravity and also applies upwards gravity
				if (Player.velocity.Y > -Player.maxFallSpeed)
					Player.velocity.Y += Player.gravity * -2;

				//'ground' detection. Without this the player wont properly reset jumps or animations
				if (Player.position.Y <= cutoffCoordinate + 18)
					Player.velocity.Y = 0;

				//logic for jumping in the correct direction
				if (Player.velocity.Y == 0 && Player.controlJump)
				{
					Player.velocity.Y = Player.jumpHeight;
					Player.releaseJump = false; //'consume' the player's jump input so the normal jump wont run
					Player.controlJump = false;
					Player.justJumped = true;
				}

				//Prevent jumping more than once
				if (Player.releaseJump)
				{
					Player.releaseJump = false;
					Player.controlJump = false;
					Player.justJumped = true;
				}
			}

			//sideways to the right
			if (direction == GravDirection.right)
			{
				// Constraint on the top
				bool free = false;
				while(!free && Player.velocity.X != 0)
				{
					free = true;

					int x = (int)(Player.position.X / 16);
					int y = (int)(Player.position.Y / 16);

					for (int k = 0; k < 2; k++)
					{
						var tile = Framing.GetTileSafely(x + k, y);
						if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
							free = false;
					}

					if (!free)
					{
						Player.position.Y += 1;
					}
				}

				// Constraint on the bottom
				free = false;
				while (!free && Player.velocity.X != 0)
				{
					free = true;

					int x = (int)(Player.position.X / 16);
					int y = (int)(Player.position.Y / 16) + 3;

					for (int k = 0; k < 2; k++)
					{
						var tile = Framing.GetTileSafely(x + k, y);
						if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
							free = false;
					}

					if (!free)
					{
						Player.position.Y -= 1;
					}
				}

				//rotate the player sprite for visual effect
				Player.fullRotation = 3.14f + 1.57f;
				Player.fullRotationOrigin = Player.Size / 2 + new Vector2(0, 10);
				Player.gfxOffY = -10;

				//corrects their facing direction since it would be reversed due to rotation otherwise
				Player.direction = Player.velocity.X > 0 ? 1 : -1;

				//creates gravity using an extra sideways gravity velocity field. Since we need to translate the vanilla X velocity into movement along the Y axis
				//this is converted to/from the proper vanilla velocity on entering/leaving for a smooth transition
				if (fakeXVel < Player.maxFallSpeed)
					fakeXVel += Player.gravity;

				//apply said fake sideways gravity
				Player.position.X += fakeXVel;

				//translate vanilla X velocity to Y. Allows us to easily simulate the desired controls with compatability with things like dashes
				Player.position.Y -= Player.velocity.X;

				//negate vanilla gravity
				Player.velocity.Y -= Player.gravity;

				//'ground' detection. Without this the player wont properly reset jumps or animations
				if (Player.position.X >= cutoffCoordinate - 32)
				{
					Player.position.X = cutoffCoordinate - 32;
					Player.velocity.Y = 0;
					fakeXVel = 0;
				}
				else if (Player.velocity.Y == 0) //We need this to prevent the game thinking were on ground if were jumping but not moving along the axis perpendicular to gravity
					Player.velocity.Y = 0.01f;

				//forces vanilla X-velocity to not apply, since were using our own fake gravity velocity.
				Player.position.X -= Player.velocity.X;

				//logic for jumping in the correct direction
				if (Player.velocity.Y == 0 && Player.controlJump)
				{
					fakeXVel = -Player.jumpHeight;
					Player.releaseJump = false;
					Player.controlJump = false;
					Player.justJumped = true;
				}

				//Prevents jumping more than once
				if (Player.releaseJump)
				{
					Player.releaseJump = false;
					Player.controlJump = false;
					Player.justJumped = true;
				}
			}

			//Essentially the same as above but with direction reversed.
			if (direction == GravDirection.left)
			{
				// Constraint on the top
				bool free = false;
				while (!free && Player.velocity.X != 0)
				{
					free = true;

					int x = (int)(Player.position.X / 16);
					int y = (int)(Player.position.Y / 16);

					for (int k = 0; k < 2; k++)
					{
						var tile = Framing.GetTileSafely(x + k, y);
						if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
							free = false;
					}

					if (!free)
					{
						Player.position.Y += 1;
					}
				}

				// Constraint on the bottom
				free = false;
				while (!free && Player.velocity.X != 0)
				{
					free = true;

					int x = (int)(Player.position.X / 16);
					int y = (int)(Player.position.Y / 16) + 4;

					for (int k = 0; k < 2; k++)
					{
						var tile = Framing.GetTileSafely(x + k, y);
						if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
							free = false;
					}

					if (!free)
					{
						Player.position.Y -= 1;
					}
				}

				Player.fullRotation = 1.57f;
				Player.fullRotationOrigin = Player.Size / 2 + new Vector2(0, -10);
				Player.gfxOffY = 10;

				Player.direction = Player.velocity.X > 0 ? 1 : -1;

				if (fakeXVel > -Player.maxFallSpeed)
					fakeXVel -= Player.gravity;

				Player.position.X += fakeXVel;

				Player.position.Y += Player.velocity.X;

				Player.velocity.Y -= Player.gravity;

				if (Player.position.X <= cutoffCoordinate + 32)
				{
					Player.position.X = cutoffCoordinate + 26;
					Player.velocity.Y = 0;
					fakeXVel = 0;
				}

				else if (Player.velocity.Y == 0)
					Player.velocity.Y = 0.01f;

				Player.position.X -= Player.velocity.X;

				if (Player.velocity.Y == 0 && Player.controlJump)
				{
					fakeXVel = Player.jumpHeight;
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

			if (enterTime > 0)
			{
				//Player.position += enterVel * (enterTime / 15f);
				enterTime--;
			}

			if (Player.velocity.Y == 0 && lastYVel != 0)
			{
				Vector2 off = direction switch
				{
					GravDirection.down => Vector2.Zero,
					GravDirection.up => new Vector2(0, -16),
					GravDirection.left => new Vector2(-24, 0),
					GravDirection.right => new Vector2(16, 0),
					_ => Vector2.Zero
				};

				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(Player.Center + off, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), Main.rand.NextVector2Circular(9, 9), 0, new Color(100, 255, 255, 0), 0.1f);
					SoundHelper.PlayPitched("Impacts/PanBonkSmall", 0.6f, -0.4f + Main.rand.NextFloat(-0.1f, 0.1f), Player.Center);
				}
			}

			lastYVel = Player.velocity.Y;
		}

		public override void ResetEffects()
		{
			if (holdOver <= 1) //Effects which trigger on leaving a gravity field. Sets the player's real X velocity to the fake one for a smooth transition.
			{
				if (direction == GravDirection.right || direction == GravDirection.left)
					Player.velocity.X = fakeXVel;

				direction = GravDirection.down; //Reset to vanilla gravity direction
			}
			else
				holdOver--;
		}

		/// <summary>
		/// Effects which should trigger when entering a gravity field. Used for the left/right gravity fields to swap vanilla X velocity
		/// with the fake gravity velocity so we can use X-velocity for inputs. Also converts their X-velocity to Y-velocity for a smooth
		/// transition.
		/// </summary>
		/// <param name="direction">The direction being changed to</param>
		internal void ChangeDirection(GravDirection direction)
		{
			if (direction == GravDirection.right)
			{
				fakeXVel = Math.Max(1, Player.velocity.X);
				Player.velocity.X = -Player.velocity.Y;
				Player.width = 42;
			}
			else if (direction == GravDirection.left)
			{
				fakeXVel = Math.Min(-1, Player.velocity.X);
				Player.velocity.X = Player.velocity.Y;
				Player.width = 42;
			}
			else
			{
				Player.width = 24;
			}
		}
	}
}

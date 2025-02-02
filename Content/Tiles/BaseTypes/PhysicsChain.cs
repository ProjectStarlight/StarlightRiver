using StarlightRiver.Content.Physics;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.BaseTypes
{
	public abstract class PhysicsChain : DummyTile
	{
		/// <summary>
		/// The amount of pixels seperating segments approximately, this will vary as the physics update
		/// </summary>
		public int segmentLength = 16;

		/// <summary>
		/// How much longer this chain should be than a straight line between it and its end point. 1 = a straight line, anything greater = more slack<br/>
		/// anything less than 1 will be under "tension" and will likely jitter strangely.
		/// </summary>
		public float segmentLengthMultiplier = 1.1f;

		public override int DummyType => DummySystem.DummyType<ChainDummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Dirt, SoundID.Dig, Color.Transparent);
		}

		public override bool SpawnConditions(int i, int j)
		{
			return true;
		}

		/// <summary>
		/// Rendering logic that will occur at each point in the chain
		/// </summary>
		/// <param name="spriteBatch">The spriteBatch to draw with</param>
		/// <param name="worldPos">The world position of this segment</param>
		/// <param name="nextPos">The world position of the next segment, or this segment if its the last</param>
		public virtual void PerPointDraw(SpriteBatch spriteBatch, Vector2 worldPos, Vector2 nextPos) { }

		public override bool RightClick(int i, int j)
		{
			var tile = Framing.GetTileSafely(i, j);

			if (Main.LocalPlayer.controlDown)
				tile.TileFrameY++;
			else
				tile.TileFrameX++;

			return true;
		}
	}

	internal class ChainDummy : Dummy
	{
		PhysicsChain parentSingleton;
		public VerletChain chain;
		public Vector2 endPoint;

		public Point16 lastFrame;

		public ChainDummy() : base() { }

		private void BuildChain()
		{
			endPoint = Center + new Vector2(Parent.TileFrameX, Parent.TileFrameY) * 16 + Vector2.One * 8;

			int segments = (int)(Vector2.Distance(Center, endPoint) / parentSingleton.segmentLength * parentSingleton.segmentLengthMultiplier);

			if (segments > 0)
			{
				chain = new(segments, true, Center, parentSingleton.segmentLength)
				{
					useEndPoint = true,
					endPoint = endPoint,
					forceGravity = new Vector2(0, 0.2f),
					drag = 1.01f,
					constraintRepetitions = 30
				};
			}

			chain?.IterateRope(k => chain.ropeSegments[k].posNow = Vector2.Lerp(Center, endPoint, k / (float)segments));

			lastFrame = new(Parent.TileFrameX, Parent.TileFrameY);
		}

		public override bool ValidTile(Tile tile)
		{
			return ModContent.GetModTile(tile.TileType) is PhysicsChain;
		}

		public override void PostDraw(Color lightColor)
		{
			chain?.IterateRope(k => parentSingleton.PerPointDraw(Main.spriteBatch, chain.ropeSegments[k].posNow, k == chain.segmentCount - 1 ? chain.ropeSegments[k].posNow : chain.ropeSegments[k+1].posNow));

			if (StarlightRiver.debugMode)
			{
				chain?.IterateRope(k =>
				{
					var pos = chain.ropeSegments[k].posNow;
					Vector2 lerped = Vector2.Lerp(Center, endPoint, k / (float)chain.segmentCount);
					Main.instance.TilesRenderer.Wind.GetWindTime((int)pos.X / 16, (int)pos.Y / 16, 20, out int windTimeLeft, out int directionX, out int directionY);
					var wind = new Vector2(directionX, directionY) * (windTimeLeft / 20f) * 2f;
					wind.X += Main.windSpeedCurrent * (Math.Abs(pos.Y - lerped.Y) / 180f);

					var tex = Assets.MagicPixel.Value;
					Main.spriteBatch.Draw(tex, new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), (int)(wind.Length() * 64f), 1), null, new Color(0.5f + wind.X, 0.5f + wind.Y, 0), wind.ToRotation(), Vector2.Zero, 0, 0);

					if (k > 0)
					{
						var dist = Vector2.Distance(chain.ropeSegments[k].posNow, chain.ropeSegments[k - 1].posNow) / parentSingleton.segmentLength;
						var color = new Color(Math.Abs(dist - 1), 0f, 0f);
						Main.spriteBatch.Draw(tex, new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), 4, 4), null, color, 0f, Vector2.One * 2, 0, 0);
					}
				});
			}
		}

		public override void Update()
		{
			parentSingleton ??= ModContent.GetModTile(Parent.TileType) as PhysicsChain;

			if (new Point16(Parent.TileFrameX, Parent.TileFrameY) != lastFrame)
				BuildChain();

			chain?.IterateRope(k =>
			{
				var pos = chain.ropeSegments[k].posNow;
				Vector2 lerped = Vector2.Lerp(Center, endPoint, k / (float)chain.segmentCount);
				Main.instance.TilesRenderer.Wind.GetWindTime((int)pos.X / 16, (int)pos.Y / 16, 20, out int windTimeLeft, out int directionX, out int directionY);
				var wind = new Vector2(directionX, directionY) * (windTimeLeft / 20f) * 2f;
				wind.X += Main.windSpeedCurrent * (Math.Abs(pos.Y - lerped.Y) / 180f);

				chain.ropeSegments[k].posNow += wind;
			});

			chain?.UpdateChain(Center, endPoint);
		}
	}
}

//TODO on loot wraith:
//Make it aggro
//Bestiary entry
//Basic balance
//Sound effects
//Death effect
//Money dropping
//Loot dropping
//Hypothetical animations
//Make them screech
//Make them not a garaunteed spawn

using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Physics;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.Utilities;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Misc
{
	internal class LootWraith : ModNPC
	{
		private readonly int NUM_SEGMENTS = 20;

		private int xFrame = 0;
		private int yFrame = 0;
		private int frameCounter = 0;

		public bool enraged = false;
		public int xTile = 0;
		public int yTile = 0;

		public VerletChain chain;

		private List<Vector2> cache;
		private Trail trail;

		private Player Target => Main.player[NPC.target];

		private Vector2 ChainStart => new Vector2(xTile + 1, yTile + 1) * 16;

		public override string Texture => AssetDirectory.MiscNPC + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Loot Wraith");
			Main.npcFrameCount[NPC.type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 62;
			NPC.height = 46;
			NPC.damage = 0;
			NPC.defense = 5;
			NPC.lifeMax = 150;
			NPC.value = 100f;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.Grass;
			NPC.DeathSound = SoundID.Grass;
			NPC.noGravity = true;
			NPC.noTileCollide= true;
		}

		public override bool NeedSaving()
		{
			return !enraged;
		}

		public override void LoadData(TagCompound tag)
		{
			xTile = tag.GetInt("xTile");
			yTile = tag.GetInt("yTile");
			NPC.Center = ChainStart + new Vector2(0,1);
			chain = new VerletChain(NUM_SEGMENTS, true, ChainStart, 5, false)
			{
				forceGravity = new Vector2(0, 0.1f),
				simStartOffset = 0,
				useEndPoint = true,
				endPoint = NPC.Center
			};
		}

		public override void SaveData(TagCompound tag)
		{
			tag["xTile"] = xTile;
			tag["yTile"] = yTile;
		}

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, Color.Cyan.ToVector3() * 0.5f);
			NPC.TargetClosest(true);
			if (!enraged)
			{
				NPC.velocity += NPC.DirectionTo(Target.Center) * 0.2f;
				if (NPC.velocity.Length() > 10)
				{
					NPC.velocity.Normalize();
					NPC.velocity *= 10;
				}
				NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.DirectionTo(ChainStart), NPC.Distance(ChainStart) * 0.001f);
			}
			if (chain is null)
				return;

			UpdateChain();

			if (!Main.dedServ)
			{
				ManageCache();
				ManageTrail();
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = Request<Texture2D>(Texture).Value;

			SpriteEffects effects = SpriteEffects.None;
			var origin = new Vector2(NPC.width / 2, NPC.height / 2);

			if (NPC.spriteDirection != 1)
				effects = SpriteEffects.FlipHorizontally;

			DrawChain();

			var slopeOffset = new Vector2(0, NPC.gfxOffY);

			for (int i = 0; i < 2; i++)
				Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}

		public override void FindFrame(int frameHeight)
		{
			int frameWidth = NPC.width;
			NPC.frame = new Rectangle(frameWidth * xFrame, frameHeight * yFrame, frameWidth, frameHeight);
		}

		private void UpdateChain()
		{

			chain.endPoint = ChainStart;
			chain.startPoint = NPC.Center;
			chain.UpdateChain();
		}

		private void ManageCache()
		{
			cache = new List<Vector2>();

			float pointLength = TotalLength(GetChainPoints()) / NUM_SEGMENTS;

			float pointCounter = 0;

			int presision = 30; //This normalizes length between points so it doesnt squash super weirdly on certain parts
			for (int i = 0; i < NUM_SEGMENTS - 1; i++)
			{
				for (int j = 0; j < presision; j++)
				{
					pointCounter += (chain.ropeSegments[i].posNow - chain.ropeSegments[i + 1].posNow).Length() / presision;

					while (pointCounter > pointLength)
					{
						float lerper = j / (float)presision;
						cache.Add(Vector2.Lerp(chain.ropeSegments[i].posNow, chain.ropeSegments[i + 1].posNow, lerper));
						pointCounter -= pointLength;
					}
				}
			}

			while (cache.Count < NUM_SEGMENTS)
			{
				cache.Add(chain.ropeSegments[NUM_SEGMENTS - 1].posNow);
			}

			while (cache.Count > NUM_SEGMENTS)
			{
				cache.RemoveAt(cache.Count - 1);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, NUM_SEGMENTS, new TriangularTip(1), factor => 7, factor => Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16)) * MathF.Sqrt(1 - factor.X));

			List<Vector2> positions = cache;
			trail.NextPosition = NPC.Center;

			trail.Positions = positions.ToArray();
		}


		private void DrawChain()
		{
			if (trail == null || trail == default)
				return;

			Main.spriteBatch.End();
			Effect effect = Terraria.Graphics.Effects.Filters.Scene["RepeatingChain"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(Texture + "_Chain").Value);
			effect.Parameters["flip"].SetValue(false);

			List<Vector2> points;

			if (cache == null)
				points = GetChainPoints();
			else
				points = trail.Positions.ToList();

			effect.Parameters["repeats"].SetValue(TotalLength(points) / 20f);

			BlendState oldState = Main.graphics.GraphicsDevice.BlendState;
			Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;
			trail?.Render(effect);
			Main.graphics.GraphicsDevice.BlendState = oldState;

			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		private List<Vector2> GetChainPoints()
		{
			var points = new List<Vector2>();

			foreach (RopeSegment ropeSegment in chain.ropeSegments)
				points.Add(ropeSegment.posNow);

			return points;
		}

		private float TotalLength(List<Vector2> points)
		{
			float ret = 0;

			for (int i = 1; i < points.Count; i++)
			{
				ret += (points[i] - points[i - 1]).Length();
			}

			return ret;
		}

	}
}
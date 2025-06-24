using ReLogic.Threading;
using StarlightRiver.Content.Configs;
using System.Collections.Generic;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Core
{
	public class ParticleSystem
	{
		public enum AnchorOptions
		{
			World,
			Screen,
			UI
		}

		public delegate void Update(Particle particle);

		private readonly List<Particle> particles = new();
		private readonly Queue<Particle> pool = new();

		private Texture2D texture;
		private readonly Update updateFunction;

		private readonly AnchorOptions anchorType;

		private readonly int maxParticles;
		private int lastParticleCount;

		private DynamicVertexBuffer vertexBuffer;
		private DynamicIndexBuffer indexBuffer;

		private readonly VertexPositionColorTexture[] verticies;
		private readonly short[] indicies;

		private bool buffersNeedUpdated;

		private BasicEffect effect;

		public ParticleSystem(string texture, Update updateDelegate, AnchorOptions anchor = AnchorOptions.World, int maxParticles = 10000)
		{
			if (Main.dedServ)
				return;

			this.texture = Request<Texture2D>(texture, AssetRequestMode.ImmediateLoad).Value;
			updateFunction = updateDelegate;
			anchorType = anchor;
			this.maxParticles = maxParticles;

			verticies = new VertexPositionColorTexture[maxParticles * 4];
			indicies = new short[maxParticles * 6];

			Main.QueueMainThreadAction(() =>
			{
				effect = new BasicEffect(Main.instance.GraphicsDevice)
				{
					TextureEnabled = true,
					VertexColorEnabled = true,
					Texture = this.texture
				};

				vertexBuffer = new DynamicVertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), maxParticles * 4, BufferUsage.WriteOnly);
				indexBuffer = new DynamicIndexBuffer(Main.instance.GraphicsDevice, IndexElementSize.SixteenBits, maxParticles * 6, BufferUsage.WriteOnly);
			});
		}

		/// <summary>
		/// Rebuilds the Vertex and Index buffers for this particle system when it updates
		/// </summary>
		public void PopulateBuffers()
		{
			if (Main.dedServ)
				return;

			FastParallel.For(0, particles.Count, (from, to, context) =>
			{
				Rectangle plane = default;

				for (int k = from; k < to; k++)
				{
					Particle particle = particles[k];

					plane.X = (int)(particle.Position.X - particle.Frame.Width / 2f * particle.Scale);
					plane.Y = (int)(particle.Position.Y - particle.Frame.Height / 2f * particle.Scale);

					plane.Width = (int)(particle.Frame.Width * particle.Scale);
					plane.Height = (int)(particle.Frame.Height * particle.Scale);

					float x = particle.Frame.X / (float)texture.Width;
					float y = particle.Frame.Y / (float)texture.Height;
					float w = particle.Frame.Width / (float)texture.Width;
					float h = particle.Frame.Height / (float)texture.Height;

					var center = plane.Center.ToVector2();
					Color color = particle.Color * particle.Alpha;
					int baseIdx = 4 * k;
					int baseIndexIdx = 6 * k;

					verticies[baseIdx + 0].Position = plane.TopLeft().RotatedBy(particle.Rotation, center).ToVector3();
					verticies[baseIdx + 0].Color = color;
					verticies[baseIdx + 0].TextureCoordinate = new Vector2(x, y);

					verticies[baseIdx + 1].Position = plane.TopRight().RotatedBy(particle.Rotation, center).ToVector3();
					verticies[baseIdx + 1].Color = color;
					verticies[baseIdx + 1].TextureCoordinate = new Vector2(x + w, y);

					verticies[baseIdx + 2].Position = plane.BottomLeft().RotatedBy(particle.Rotation, center).ToVector3();
					verticies[baseIdx + 2].Color = color;
					verticies[baseIdx + 2].TextureCoordinate = new Vector2(x, y + h);

					verticies[baseIdx + 3].Position = plane.BottomRight().RotatedBy(particle.Rotation, center).ToVector3();
					verticies[baseIdx + 3].Color = color;
					verticies[baseIdx + 3].TextureCoordinate = new Vector2(x + w, y + h);

					indicies[baseIndexIdx + 0] = (short)(baseIdx + 0);
					indicies[baseIndexIdx + 1] = (short)(baseIdx + 1);
					indicies[baseIndexIdx + 2] = (short)(baseIdx + 2);
					indicies[baseIndexIdx + 3] = (short)(baseIdx + 1);
					indicies[baseIndexIdx + 4] = (short)(baseIdx + 3);
					indicies[baseIndexIdx + 5] = (short)(baseIdx + 2);
				}
			});

			for (int k = particles.Count * 4; k < lastParticleCount * 4; k++)
			{
				verticies[k] = default;
			}

			lastParticleCount = particles.Count;

			vertexBuffer?.SetData(verticies);
			indexBuffer?.SetData(indicies);

			buffersNeedUpdated = false;
		}

		/// <summary>
		/// Renders your particles with a BasicEffect
		/// </summary>
		/// <param name="spriteBatch">The current spriteBatch instance</param>
		public void DrawParticles(SpriteBatch spriteBatch)
		{
			if (Main.dedServ)
				return;

			if (GetInstance<GraphicsConfig>().ParticlesActive && particles.Count > 0 && effect != null)
			{
				spriteBatch.End();

				if (buffersNeedUpdated)
					PopulateBuffers();

				Main.instance.GraphicsDevice.SetVertexBuffer(vertexBuffer);
				Main.instance.GraphicsDevice.Indices = indexBuffer;

				Matrix zoom = anchorType switch
				{
					AnchorOptions.World => Main.GameViewMatrix.TransformationMatrix,
					AnchorOptions.Screen => Matrix.Identity,
					AnchorOptions.UI => Main.UIScaleMatrix,
					_ => default
				};

				Vector2 offset = anchorType == AnchorOptions.World ? Main.screenPosition : Vector2.Zero;

				effect.World = Matrix.CreateTranslation(-offset.ToVector3());
				effect.View = anchorType == AnchorOptions.UI ? Matrix.Identity : zoom;
				effect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
				}

				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, zoom);
			}
		}

		/// <summary>
		/// Renders your particles with a custom shader. This shader must accept 4 parameters, World, View, Projection, and texture0.
		/// </summary>
		/// <param name="spriteBatch">The current spriteBatch instance</param>
		/// <param name="effect">The custom effect to use to render your particles</param>
		public void DrawParticlesWithEffect(SpriteBatch spriteBatch, Effect effect)
		{
			if (Main.dedServ || effect is null)
				return;

			if (GetInstance<GraphicsConfig>().ParticlesActive && particles.Count > 0 && effect != null)
			{
				spriteBatch.End();

				if (buffersNeedUpdated)
					PopulateBuffers();

				Main.instance.GraphicsDevice.SetVertexBuffer(vertexBuffer);
				Main.instance.GraphicsDevice.Indices = indexBuffer;

				Matrix zoom = anchorType switch
				{
					AnchorOptions.World => Main.GameViewMatrix.TransformationMatrix,
					AnchorOptions.Screen => Matrix.Identity,
					AnchorOptions.UI => Main.UIScaleMatrix,
					_ => default
				};

				Vector2 offset = anchorType == AnchorOptions.World ? Main.screenPosition : Vector2.Zero;

				effect.Parameters["World"].SetValue(Matrix.CreateTranslation(-offset.ToVector3()));
				effect.Parameters["View"].SetValue(anchorType == AnchorOptions.UI ? Matrix.Identity : zoom);
				effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1));
				effect.Parameters["texture0"].SetValue(texture);

				foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
				}

				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, zoom);
			}
		}

		/// <summary>
		/// Updates all particles in this system, needed to make your update function take effect
		/// </summary>
		public void UpdateParticles()
		{
			if (Main.dedServ || !GetInstance<GraphicsConfig>().ParticlesActive || particles.Count <= 0)
				return;

			int activeCount = 0;

			for (int k = 0; k < particles.Count; k++)
			{
				Particle particle = particles[k];
				updateFunction(particle);

				if (particle.Timer <= 0)
				{
					pool.Enqueue(particle);
					continue;
				}

				particles[activeCount++] = particle;
			}

			particles.RemoveRange(activeCount, particles.Count - activeCount);
			buffersNeedUpdated = true;
		}

		/// <summary>
		/// Spawns a particle into this particle system, drawing first from the pool if possible
		/// </summary>
		/// <param name="position"></param>
		/// <param name="velocity"></param>
		/// <param name="rotation"></param>
		/// <param name="scale"></param>
		/// <param name="color"></param>
		/// <param name="timer"></param>
		/// <param name="storedPosition"></param>
		/// <param name="frame"></param>
		/// <param name="alpha"></param>
		/// <param name="type"></param>
		public void AddParticle(Vector2 position, Vector2 velocity, float rotation, float scale, Color color, int timer, Vector2 storedPosition, Rectangle frame = default, float alpha = 1, int type = 0)
		{
			if (Main.dedServ || !GetInstance<GraphicsConfig>().ParticlesActive || particles.Count > maxParticles)
				return;

			Particle particle = pool.Count > 0 ? pool.Dequeue() : new();

			if (frame == default)
				frame = texture.Frame();

			particle.SetData(position, velocity, rotation, scale, color, timer, storedPosition, frame, alpha, type);
			particles.Add(particle);
		}

		/// <summary>
		/// Removes all particles from this system forcibly, resetting the pool
		/// </summary>
		public void ClearParticles()
		{
			particles.Clear();
		}

		/// <summary>
		/// Changes the texture used by all particles in this system
		/// </summary>
		/// <param name="texture">The new texture to use</param>
		public void SetTexture(Texture2D texture)
		{
			this.texture = texture;

			if (effect != null)
				effect.Texture = texture;
		}
	}

	public class Particle
	{
		internal Vector2 Position;
		internal Vector2 Velocity;
		internal Vector2 StoredPosition;
		internal float Rotation;
		internal float Scale;
		internal float Alpha;
		internal Color Color;
		internal int Timer;
		internal int Type;
		internal Rectangle Frame;

		public Particle()
		{

		}

		public Particle(Vector2 position, Vector2 velocity, float rotation, float scale, Color color, int timer, Vector2 storedPosition, Rectangle frame = default, float alpha = 1, int type = 0)
		{
			Position = position;
			Velocity = velocity;
			Rotation = rotation;
			Scale = scale;
			Color = color;
			Timer = timer;
			StoredPosition = storedPosition;
			Frame = frame;
			Alpha = alpha;
			Type = type;
		}

		public void SetData(Vector2 position, Vector2 velocity, float rotation, float scale, Color color, int timer, Vector2 storedPosition, Rectangle frame = default, float alpha = 1, int type = 0)
		{
			Position = position;
			Velocity = velocity;
			Rotation = rotation;
			Scale = scale;
			Color = color;
			Timer = timer;
			StoredPosition = storedPosition;
			Frame = frame;
			Alpha = alpha;
			Type = type;
		}
	}
}
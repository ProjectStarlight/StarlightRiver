using ReLogic.Threading;
using StarlightRiver.Content.Configs;
using StarlightRiver.Helpers;
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
		private Texture2D texture;
		private readonly Update updateFunction;

		private readonly AnchorOptions anchorType;

		private readonly int maxParticles;

		private DynamicVertexBuffer vertexBuffer;
		private DynamicIndexBuffer indexBuffer;

		readonly VertexPositionColorTexture[] verticies;
		readonly short[] indicies;

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

		public void PopulateBuffers()
		{
			if (Main.dedServ)
				return;

			FastParallel.For(0, particles.Count, (from, to, context) =>
			{
				for (int k = from; k < to; k++)
				{
					Particle particle = particles[k];

					if (!Main.gameInactive)
						updateFunction(particle);

					Rectangle frame = particle.Frame != default ? particle.Frame : texture.Frame();

					Rectangle plane = new Rectangle(0, 0, frame.Width, frame.Height);
					plane.Offset((particle.Position - plane.Size() / 2f * particle.Scale).ToPoint());

					plane.Width = (int)(plane.Width * particle.Scale);
					plane.Height = (int)(plane.Height * particle.Scale);

					float x = frame.X / (float)texture.Width;
					float y = frame.Y / (float)texture.Height;
					float w = frame.Width / (float)texture.Width;
					float h = frame.Height / (float)texture.Height;

					verticies[4 * k + 0] = new(plane.TopLeft().RotatedBy(particle.Rotation, plane.Center.ToVector2()).Vec3(), particle.Color * particle.Alpha, new Vector2(x, y));
					verticies[4 * k + 1] = new(plane.TopRight().RotatedBy(particle.Rotation, plane.Center.ToVector2()).Vec3(), particle.Color * particle.Alpha, new Vector2(x + w, y));
					verticies[4 * k + 2] = new(plane.BottomLeft().RotatedBy(particle.Rotation, plane.Center.ToVector2()).Vec3(), particle.Color * particle.Alpha, new Vector2(x, y + h));
					verticies[4 * k + 3] = new(plane.BottomRight().RotatedBy(particle.Rotation, plane.Center.ToVector2()).Vec3(), particle.Color * particle.Alpha, new Vector2(x + w, y + h));

					indicies[6 * k + 0] = (short)(4 * k + 0);
					indicies[6 * k + 1] = (short)(4 * k + 1);
					indicies[6 * k + 2] = (short)(4 * k + 2);
					indicies[6 * k + 3] = (short)(4 * k + 1);
					indicies[6 * k + 4] = (short)(4 * k + 3);
					indicies[6 * k + 5] = (short)(4 * k + 2);
				}
			});

			for (int k = particles.Count * 4; k < maxParticles * 4; k++)
			{
				verticies[k] = new();
			}

			vertexBuffer?.SetData(verticies);
			indexBuffer?.SetData(indicies);
		}

		public void DrawParticles(SpriteBatch spriteBatch)
		{
			if (Main.dedServ)
				return;

			if (GetInstance<GraphicsConfig>().ParticlesActive && particles.Count > 0 && effect != null)
			{
				spriteBatch.End();

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
				effect.World = Matrix.CreateTranslation(-offset.Vec3());
				effect.View = anchorType == AnchorOptions.UI ? Matrix.Identity : zoom;
				effect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					Main.instance.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
				}

				Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, RasterizerState.CullNone, default, zoom);

				particles.RemoveAll(n => n is null || n.Timer <= 0);
			}
		}

		public void AddParticle(Particle particle)
		{
			if (Main.dedServ)
				return;

			if (GetInstance<GraphicsConfig>().ParticlesActive && !Main.gameInactive && particles.Count < maxParticles)
				particles.Add(particle);
		}

		public void ClearParticles()
		{
			particles.Clear();
		}

		public void SetTexture(Texture2D texture)
		{
			this.texture = texture;
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

		public Particle(Vector2 position, Vector2 velocity, float rotation, float scale, Color color, int timer, Vector2 storedPosition, Rectangle frame = new Rectangle(), float alpha = 1, int type = 0)
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
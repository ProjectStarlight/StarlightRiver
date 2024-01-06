using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Systems.ArmatureSystem
{
	internal struct ArmSegment
	{
		public Vector2 start;
		public float rotation;
		public float length;
		public Rectangle frame;

		public Vector2 Endpoint => start + Vector2.UnitX.RotatedBy(rotation) * length;

		/// <summary>
		/// Initialize the segment to an arm
		/// </summary>
		/// <param name="length">The length of this arm segment</param>
		/// <param name="frame">The frame of the texture to be used. Defaults to using the whole texture.</param>
		public ArmSegment(float length, Rectangle frame = default)
		{
			start = Vector2.Zero;
			rotation = 0;
			this.length = length;
			this.frame = frame;
		}

		/// <summary>
		/// Draws this segment in the world
		/// </summary>
		/// <param name="spriteBatch">The SpriteBatch to draw with</param>
		/// <param name="texture">The texture used by the arm this segment belongs to</param>
		public void DrawSegment(SpriteBatch spriteBatch, Texture2D texture)
		{
			Vector2 pos = start - Main.screenPosition;
			Rectangle frame = this.frame == default ? texture.Frame() : this.frame;
			Vector2 origin = new Vector2(0, frame.Height / 2f);
			Vector2 midpoint = Vector2.Lerp(start, Endpoint, 0.5f);

			var color = Lighting.GetColor((midpoint / 16).ToPoint());

			spriteBatch.Draw(texture, pos, frame, color, rotation, origin, 1f, 0, 0);
		}
	}

	internal class Arm
	{
		public Vector2 start;
		public ArmSegment[] segments;
		public Texture2D texture;

		public int SegmentCount => segments.Length;
		public float MaxLen => segments.Sum(a => a.length);
		public Vector2 Endpoint => segments[^1].Endpoint;

		/// <summary>
		/// Initialize an arm with a predefined set of ArmSegment structures. Useful for intitialzing complex limbs with differing lengths
		/// </summary>
		/// <param name="start">The joint position of the first segment</param>
		/// <param name="segments">The segments to initialize the arm with</param>
		public Arm(Vector2 start, ArmSegment[] segments, Texture2D texture)
		{
			if (segments is null || segments.Length <= 0)
				throw new Exception("Arm was initialized with null or empty segment collection!");

			this.start = start;
			this.segments = segments;
			this.texture = texture;
		}

		/// <summary>
		/// Initialize a simple arm, with an amount of segments of a given length
		/// </summary>
		/// <param name="start">The joint position of the first segment</param>
		/// <param name="segments">The amount of segments in the arm</param>
		/// <param name="segmentLength">The length of each segment in the arm</param>
		public Arm(Vector2 start, int segments, float segmentLength, Texture2D texture)
		{
			if (segments <= 0)
				throw new Exception("Arm was initialized with null or empty segment collection!");

			this.start = start;
			this.segments = new ArmSegment[segments];

			for(int k = 0; k < segments; k++)
			{
				this.segments[k] = new ArmSegment(segmentLength);
			}

			this.texture = texture;
		}

		/// <summary>
		/// Perform inverse kinematics calculations to arrange the various arm segments such that they reach the given target
		/// </summary>
		/// <param name="target">The point the arm should go to</param>
		public void IKToPoint(Vector2 target)
		{
			for (int i = segments.Length - 1; i >= 0; i--)
			{
				ArmSegment currentSegment = segments[i];

				Vector2 toTarget = target - currentSegment.start;
				float targetRotation = toTarget.ToRotation();

				if (targetRotation != targetRotation) // == NaN
					targetRotation = 0;

				segments[i].rotation = targetRotation;
				target += Vector2.UnitX.RotatedBy(targetRotation) * -currentSegment.length;
			}
		}

		/// <summary>
		/// Updates the position of each segment sequentially
		/// </summary>
		public void Update()
		{
			segments[0].start = start;

			for(int k = 1; k < segments.Length; k++)
			{
				segments[k].start = segments[k - 1].Endpoint;
			}

			for(int k = 0; k < segments.Length; k++)
			{
				if (segments[k].rotation != segments[k].rotation) // == NaN
					segments[k].rotation = 0;
			}
		}

		/// <summary>
		/// Draws the given arm, based on the given world coordinates and textures
		/// </summary>
		/// <param name="spriteBatch"></param>
		public void DrawArm(SpriteBatch spriteBatch)
		{
			foreach (ArmSegment segment in segments)
			{
				segment.DrawSegment(spriteBatch, texture);
			}
		}
	}
}

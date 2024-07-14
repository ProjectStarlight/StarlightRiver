﻿using Microsoft.Xna.Framework.Audio;
using ReLogic.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Utilities;

namespace StarlightRiver.Helpers
{
	public static partial class Helper
	{
		static readonly List<SoundEffectInstance> instances = new();

		public static Vector2 TileAdj => (Lighting.Mode == Terraria.Graphics.Light.LightMode.Retro || Lighting.Mode == Terraria.Graphics.Light.LightMode.Trippy) ? Vector2.Zero : Vector2.One * 12;

		public static Vector2 ScreenSize => new(Main.screenWidth, Main.screenHeight);

		public static Rectangle ScreenTiles => new((int)Main.screenPosition.X / 16, (int)Main.screenPosition.Y / 16, Main.screenWidth / 16, Main.screenHeight / 16);

		public static Color IndicatorColor => Color.White * (float)(0.2f + 0.8f * (1 + Math.Sin(StarlightWorld.visualTimer)) / 2f);

		public static Rectangle ToRectangle(this Vector2 vector)
		{
			return new Rectangle(0, 0, (int)vector.X, (int)vector.Y);
		}

		public static Vector2 Round(this Vector2 vector)
		{
			return new Vector2((float)Math.Round(vector.X), (float)Math.Round(vector.Y));
		}

		/// <summary>
		/// Runs math.min on both the X and Y seperately, returns the smaller value for each
		/// </summary>
		public static Vector2 TwoValueMin(this Vector2 vector, Vector2 vector2)
		{
			return new Vector2(Math.Min(vector.X, vector2.X), Math.Min(vector.Y, vector2.Y));
		}

		/// <summary>
		/// Runs math.max on both the X and Y seperately, returns the largest value for each
		/// </summary>
		public static Vector2 TwoValueMax(this Vector2 vector, Vector2 vector2)
		{
			return new Vector2(Math.Max(vector.X, vector2.X), Math.Max(vector.Y, vector2.Y));
		}

		public static Player Owner(this Projectile proj)
		{
			return Main.player[proj.owner];
		}

		/// <summary>
		/// Seperates all flags stored in a enum out into an array
		/// </summary>
		public static IEnumerable<Enum> GetFlags(this Enum input)
		{
			foreach (Enum value in Enum.GetValues(input.GetType()))
			{
				if (input.HasFlag(value))
					yield return value;
			}
		}

		/// <summary>
		/// Updates the value used for flipping rotation on the Player. Should be reset to 0 when not in use.
		/// </summary>
		/// <param name="Player"></param>
		/// <param name="value"></param>
		public static void UpdateRotation(this Player Player, float value)
		{
			Player.GetModPlayer<StarlightPlayer>().rotation = value;
		}

		public static Vector3 Vec3(this Vector2 vector)
		{
			return new Vector3(vector.X, vector.Y, 0);
		}

		public static Vector3 ScreenCoord(this Vector3 vector)
		{
			return new Vector3(-1 + vector.X / Main.screenWidth * 2, (-1 + vector.Y / Main.screenHeight * 2f) * -1, 0);
		}

		public static Color IndicatorColorProximity(int minRadius, int maxRadius, Vector2 center)
		{
			float distance = Vector2.Distance(center, Main.LocalPlayer.Center);

			if (distance > maxRadius)
				return Color.White * 0f;

			return IndicatorColor * (1 - Math.Min(1, (distance - minRadius) / (maxRadius - minRadius)));
		}

		public static Color MoltenVitricGlow(float time)
		{
			Color MoltenGlowc = Color.White;
			if (time > 30 && time < 60)
				MoltenGlowc = Color.Lerp(Color.White, Color.Orange, Math.Min((time - 30f) / 20f, 1f));
			else if (time >= 60)
				MoltenGlowc = Color.Lerp(Color.Orange, Color.Lerp(Color.Red, Color.Transparent, Math.Min((time - 60f) / 50f, 1f)), Math.Min((time - 60f) / 30f, 1f));
			return MoltenGlowc;
		}

		public static float RotationDifference(float rotTo, float rotFrom)
		{
			return ((rotTo - rotFrom) % 6.28f + 9.42f) % 6.28f - 3.14f;
		}

		/// <summary>
		/// determines if an NPC is "fleshy" based on it's hit sound
		/// </summary>
		/// <param name="NPC"></param>
		/// <returns></returns>
		public static bool IsFleshy(NPC NPC)
		{
			return !
				(
					NPC.HitSound == SoundID.NPCHit2 ||
					NPC.HitSound == SoundID.NPCHit3 ||
					NPC.HitSound == SoundID.NPCHit4 ||
					NPC.HitSound == SoundID.NPCHit41 ||
					NPC.HitSound == SoundID.NPCHit42 ||
					NPC.HitSound == new SoundStyle($"{nameof(StarlightRiver)}/Sounds/VitricBoss/ceramicimpact")
				);
		}

		public static Vector2 Centeroid(List<NPC> input) //Helper overload for NPCs for support NPCs
		{
			var centers = new List<Vector2>();

			for (int k = 0; k < input.Count; k++)
				centers.Add(input[k].Center);

			return Centeroid(centers);
		}

		public static Vector2 Centeroid(List<Vector2> input) //this gets the centeroid of the points. see: https://math.stackexchange.com/questions/1801867/finding-the-centre-of-an-abritary-set-of-points-in-two-dimensions
		{
			float sumX = 0;
			float sumY = 0;

			for (int k = 0; k < input.Count; k++)
			{
				sumX += input[k].X;
				sumY += input[k].Y;
			}

			return new Vector2(sumX / input.Count, sumY / input.Count);
		}

		public static float LerpFloat(float min, float max, float val)
		{
			float difference = max - min;
			return min + difference * val;
		}

		public static bool CheckLinearCollision(Vector2 point1, Vector2 point2, Rectangle hitbox, out Vector2 intersectPoint)
		{
			intersectPoint = Vector2.Zero;

			if (StarlightRiver.debugMode)
			{
				for (int k = 0; k < 20; k++)
					Dust.NewDustPerfect(Vector2.Lerp(point1, point2, k / 20f), DustID.MagicMirror, Vector2.Zero, 0, default, 0.5f);
			}

			return
				LinesIntersect(point1, point2, hitbox.TopLeft(), hitbox.TopRight(), out intersectPoint) ||
				LinesIntersect(point1, point2, hitbox.TopLeft(), hitbox.BottomLeft(), out intersectPoint) ||
				LinesIntersect(point1, point2, hitbox.BottomLeft(), hitbox.BottomRight(), out intersectPoint) ||
				LinesIntersect(point1, point2, hitbox.TopRight(), hitbox.BottomRight(), out intersectPoint);
		}

		//algorithm taken from http://web.archive.org/web/20060911055655/http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/
		public static bool LinesIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersectPoint)
		{
			intersectPoint = Vector2.Zero;

			float denominator = (point4.Y - point3.Y) * (point2.X - point1.X) - (point4.X - point3.X) * (point2.Y - point1.Y);

			float a = (point4.X - point3.X) * (point1.Y - point3.Y) - (point4.Y - point3.Y) * (point1.X - point3.X);
			float b = (point2.X - point1.X) * (point1.Y - point3.Y) - (point2.Y - point1.Y) * (point1.X - point3.X);

			if (denominator == 0)
			{
				if (a == 0 || b == 0) //lines are coincident
				{
					intersectPoint = point3; //possibly not the best fallback?
					return true;
				}
				else
				{
					return false; //lines are parallel
				}
			}

			float ua = a / denominator;
			float ub = b / denominator;

			if (ua > 0 && ua < 1 && ub > 0 && ub < 1)
			{
				intersectPoint = new Vector2(point1.X + ua * (point2.X - point1.X), point1.Y + ua * (point2.Y - point1.Y));
				return true;
			}

			return false;
		}

		public static bool CheckCircularCollision(Vector2 center, int radius, Rectangle hitbox)
		{
			if (Vector2.Distance(center, hitbox.TopLeft()) <= radius)
				return true;

			if (Vector2.Distance(center, hitbox.TopRight()) <= radius)
				return true;

			if (Vector2.Distance(center, hitbox.BottomLeft()) <= radius)
				return true;

			return Vector2.Distance(center, hitbox.BottomRight()) <= radius;
		}

		public static bool CheckConicalCollision(Vector2 center, int radius, float angle, float width, Rectangle hitbox)
		{
			if (CheckPoint(center, radius, hitbox.TopLeft(), angle, width))
				return true;

			if (CheckPoint(center, radius, hitbox.TopRight(), angle, width))
				return true;

			if (CheckPoint(center, radius, hitbox.BottomLeft(), angle, width))
				return true;

			return CheckPoint(center, radius, hitbox.BottomRight(), angle, width);
		}

		private static bool CheckPoint(Vector2 center, int radius, Vector2 check, float angle, float width)
		{
			float thisAngle = (center - check).ToRotation() % 6.28f;
			return Vector2.Distance(center, check) <= radius && thisAngle > angle - width && thisAngle < angle + width;
		}

		public static bool PointInTile(Vector2 point)
		{
			var startCoords = new Point16((int)point.X / 16, (int)point.Y / 16);
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					Point16 thisPoint = startCoords + new Point16(x, y);

					if (!WorldGen.InWorld(thisPoint.X, thisPoint.Y))
						return false;

					Tile tile = Framing.GetTileSafely(thisPoint);

					if (Main.tileSolid[tile.TileType] && tile.HasTile && !Main.tileSolidTop[tile.TileType])
					{
						var rect = new Rectangle(thisPoint.X * 16, thisPoint.Y * 16, 16, 16);

						if (rect.Contains(point.ToPoint()))
							return true;
					}
				}
			}

			return false;
		}

		public static string TicksToTime(int ticks)
		{
			int sec = ticks / 60;
			return sec / 60 + ":" + (sec % 60 < 10 ? "0" + sec % 60 : "" + sec % 60);
		}

		public static bool ScanForTypeDown(int startX, int startY, int type, int maxDown = 50)
		{
			for (int k = 0; k <= maxDown && k + startY < Main.maxTilesY; k++)
			{
				Tile tile = Framing.GetTileSafely(startX, startY + k);

				if (tile.HasTile && tile.TileType == type)
					return true;
			}

			return false;
		}

		public static float CompareAngle(float baseAngle, float targetAngle)
		{
			return (baseAngle - targetAngle + (float)Math.PI * 3) % MathHelper.TwoPi - (float)Math.PI;
		}

		public static float ConvertAngle(float angleIn)
		{
			return CompareAngle(0, angleIn) + (float)Math.PI;
		}

		public static string WrapString(string input, int length, DynamicSpriteFont font, float scale)
		{
			string output = "";
			string[] words = input.Split();

			string line = "";
			foreach (string str in words)
			{
				if (str == "NEWBLOCK")
				{
					output += "\n\n";
					line = "";
					continue;
				}

				if (font.MeasureString(line).X * scale < length)
				{
					output += " " + str;
					line += " " + str;
				}
				else
				{
					output += "\n" + str;
					line = str;
				}
			}

			return output[1..];
		}

		public static List<T> RandomizeList<T>(List<T> input)
		{
			int n = input.Count();

			while (n > 1)
			{
				n--;
				int k = Main.rand.Next(n + 1);
				(input[n], input[k]) = (input[k], input[n]);
			}

			return input;
		}

		public static List<T> RandomizeList<T>(List<T> input, UnifiedRandom rand)
		{
			int n = input.Count();

			while (n > 1)
			{
				n--;
				int k = rand.Next(n + 1);
				(input[n], input[k]) = (input[k], input[n]);
			}

			return input;
		}

		public static Player FindNearestPlayer(Vector2 position)
		{
			Player Player = null;

			for (int k = 0; k < Main.maxPlayers; k++)
			{
				if (Main.player[k] != null && Main.player[k].active && (Player == null || Vector2.DistanceSquared(position, Main.player[k].Center) < Vector2.DistanceSquared(position, Player.Center)))
					Player = Main.player[k];
			}

			return Player;
		}

		public static float BezierEase(float time)
		{
			return time * time / (2f * (time * time - time) + 1f);
		}

		public static float SwoopEase(float time)
		{
			return 3.75f * (float)Math.Pow(time, 3) - 8.5f * (float)Math.Pow(time, 2) + 5.75f * time;
		}

		public static float Lerp(float a, float b, float f)
		{
			return a * (1.0f - f) + b * f;
		}

		/// <summary>
		/// <para>Animations are interpolated with a cubic bezier. You will define the bezier using the p1 and p2 parameters.</para>
		/// <para>This function serves as a constructor for the real interpolation function</para>
		/// <para>Use https://cubic-bezier.com/ to find appropriate parameters.</para>
		/// </summary>
		public static Func<float, float> CubicBezier(float p1x, float p1y, float p2x, float p2y)
		{
			if (p1x < 0 || p1x > 1 || p2x < 0 || p2x > 1)
			{
				throw new ArgumentException("X point parameters of cubic bezier timing function should be between values 0 and 1!");
			}

			Vector2 p0 = Vector2.Zero;
			var p1 = new Vector2(p1x, p1y);
			var p2 = new Vector2(p2x, p2y);
			Vector2 p3 = Vector2.One;

			float timing(float t)
			{
				return (float)(Math.Pow(1 - t, 3) * p0.X + 3 * Math.Pow(1 - t, 2) * t * p1.X + 3 * (1 - t) * Math.Pow(t, 2) * p2.X + Math.Pow(t, 3) * p3.X);
			}

			float progression(float x)
			{
				float time;
				if (x <= 0)
					time = 0;
				else if (x >= 1)
					time = 1;
				else
					time = BinarySolve(timing, x, 0.001f);

				return (float)(Math.Pow(1 - time, 3) * p0.Y + 3 * Math.Pow(1 - time, 2) * time * p1.Y + 3 * (1 - time) * Math.Pow(time, 2) * p2.Y + Math.Pow(time, 3) * p3.Y);
			}

			return progression;
		}

		// Binary solver for cubic bezier
		private static float BinarySolve(in Func<float, float> function, in float target, in float precision, float start = 0, float end = 1, int iteration = 0)
		{
			if (iteration > 1000)
			{
				throw new ArgumentException("Could not converge to an answer in over 1000 iterations.");
			}

			float halfway = (start + end) / 2;
			float res = function(halfway);

			if (Math.Abs(res - target) < precision)
				return halfway;
			else if (target < res)
				return BinarySolve(function, target, precision, start, halfway, iteration + 1);
			else
				return BinarySolve(function, target, precision, halfway, end, iteration + 1);
		}

		public static T[] FastUnion<T>(this T[] front, T[] back)
		{
			var combined = new T[front.Length + back.Length];

			Array.Copy(front, combined, front.Length);
			Array.Copy(back, 0, combined, front.Length, back.Length);

			return combined;
		}

		public static bool IsEdgeTile(int x, int y)
		{
			Tile leftTile = Framing.GetTileSafely(x - 1, y);
			Tile rightTile = Framing.GetTileSafely(x + 1, y);
			Tile topTile = Framing.GetTileSafely(x, y - 1);
			Tile bottomTile = Framing.GetTileSafely(x, y + 1);

			bool isEdge =
				!(leftTile.HasTile && Main.tileSolid[leftTile.TileType]) ||
				!(rightTile.HasTile && Main.tileSolid[rightTile.TileType]) ||
				!(topTile.HasTile && Main.tileSolid[topTile.TileType]) ||
				!(bottomTile.HasTile && Main.tileSolid[bottomTile.TileType]);

			return isEdge;
		}

		public static SlotId PlayPitched(string path, float volume, float pitch, Vector2? position = null)
		{
			if (Main.netMode == NetmodeID.Server)
				return SlotId.Invalid;

			/*for (int i = 0; i < instances.Count; i++)
            {
                var instance = instances[i];
                if (instance == null)
                {
                    instances.RemoveAt(i);
                    i--;
                }
                if (instance.State == SoundState.Stopped)
                {
                    instances[i].Dispose();
                    instances.RemoveAt(i);
                    i--;
                }
            }*/

			var style = new SoundStyle($"{nameof(StarlightRiver)}/Sounds/{path}")
			{
				Volume = volume,
				Pitch = pitch,
				MaxInstances = 0
			};

			return SoundEngine.PlaySound(style, position);

			/*float distFactor = 1;

            if (position != default)
                distFactor = 1 - MathHelper.Clamp(Vector2.Distance(Main.LocalPlayer.Center, position) / 2000f, 0, 1);

            soundEffect.Volume = MathHelper.Clamp(volume * Main.soundVolume * distFactor, 0, 1);
            soundEffect.Pitch = pitch;

            instances.Add(soundEffect);
            soundEffect.Play();*/
		}

		public static SlotId PlayPitched(SoundStyle style, float volume, float pitch, Vector2? position = null)
		{
			if (Main.netMode == NetmodeID.Server)
				return SlotId.Invalid;

			style.Volume *= volume;
			style.Pitch += pitch;
			style.MaxInstances = 0;

			return SoundEngine.PlaySound(style, position);
		}

		public static Point16 FindTile(Point16 start, Func<Tile, bool> condition, int radius = 30, int w = 1, int h = 1)
		{
			Point16 output = Point16.Zero;

			for (int x = 0; x < radius; x++)
			{
				for (int y = 0; y < radius; y++)
				{
					Point16 check1 = start + new Point16(x, y);
					Point16 attempt1 = CheckTiles(check1, condition, w, h);
					if (attempt1 != Point16.Zero)
						return attempt1;

					Point16 check2 = start + new Point16(-x, y);
					Point16 attempt2 = CheckTiles(check2, condition, w, h);
					if (attempt2 != Point16.Zero)
						return attempt2;

					Point16 check3 = start + new Point16(x, -y);
					Point16 attempt3 = CheckTiles(check3, condition, w, h);
					if (attempt3 != Point16.Zero)
						return attempt3;

					Point16 check4 = start + new Point16(-x, -y);
					Point16 attempt4 = CheckTiles(check4, condition, w, h);
					if (attempt4 != Point16.Zero)
						return attempt4;
				}
			}

			return output;
		}

		private static Point16 CheckTiles(Point16 check, Func<Tile, bool> condition, int w, int h)
		{
			if (WorldGen.InWorld(check.X, check.Y))
			{
				for (int x = 0; x < w; x++)
				{
					for (int y = 0; y < h; y++)
					{
						Tile checkTile = Framing.GetTileSafely(check.X + x, check.Y + y);

						if (!condition(checkTile))
							return Point16.Zero;
					}
				}

				return check;
			}

			return Point16.Zero;
		}

		public static bool ClearPath(Vector2 point1, Vector2 point2)
		{
			Vector2 direction = point2 - point1;

			for (int i = 0; i < direction.Length(); i += 4)
			{
				Vector2 toLookAt = point1 + Vector2.Normalize(direction) * i;

				if (Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).HasTile && Main.tileSolid[Framing.GetTileSafely((int)(toLookAt.X / 16), (int)(toLookAt.Y / 16)).TileType])
					return false;
			}

			return true;
		}
	}
}
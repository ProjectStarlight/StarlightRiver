using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;


namespace StarlightRiver.Helpers
{
    public static partial class Helper
    {
        private static int tiltTime;
        private static float tiltMax;

        public static Rectangle ToRectangle(this Vector2 vector) => new Rectangle(0, 0, (int)vector.X, (int)vector.Y);
        public static Player Owner(this Projectile proj) => Main.player[proj.owner];
        public static Vector2 TileAdj => Lighting.lightMode > 1 ? Vector2.Zero : Vector2.One * 12;
        public static Vector2 ScreenSize => new Vector2(Main.screenWidth, Main.screenHeight);

        public static Rectangle ScreenTiles => new Rectangle((int)Main.screenPosition.X / 16, (int)Main.screenPosition.Y / 16, Main.screenWidth / 16, Main.screenHeight / 16);

        /// <summary>
        /// Updates the value used for flipping rotation on the player. Should be reset to 0 when not in use.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="value"></param>
        public static void UpdateRotation(this Player player, float value) => player.GetModPlayer<StarlightPlayer>().rotation = value;

        public static bool OnScreen(Vector2 pos) => pos.X > -16 && pos.X < Main.screenWidth + 16 && pos.Y > -16 && pos.Y < Main.screenHeight + 16;

        public static bool OnScreen(Rectangle rect) => rect.Intersects(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight));

        public static bool OnScreen(Vector2 pos, Vector2 size) => OnScreen(new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y));

        public static Vector3 Vec3(this Vector2 vector) => new Vector3(vector.X, vector.Y, 0);

        public static Vector3 ScreenCoord(this Vector3 vector) => new Vector3(-1 + vector.X / Main.screenWidth * 2, (-1 + vector.Y / Main.screenHeight * 2f) * -1, 0);

        public static Color IndicatorColor => Color.White * (float)(0.2f + 0.8f * (1 + Math.Sin(StarlightWorld.rottime)) / 2f);

        public static Vector2 Centeroid (List<NPC> input) //Helper overload for NPCs for support NPCs
		{
            List<Vector2> centers = new List<Vector2>();

            for (int k = 0; k < input.Count; k++)
                centers.Add(input[k].Center);

            return Centeroid(centers);
		}

        public static Vector2 Centeroid (List<Vector2> input) //this gets the centeroid of the points. see: https://math.stackexchange.com/questions/1801867/finding-the-centre-of-an-abritary-set-of-points-in-two-dimensions
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
            return min + (difference * val);
        }

        public static void UnlockEntry<type>(Player player)
        {
            CodexHandler mp = player.GetModPlayer<CodexHandler>();
            CodexEntry entry = mp.Entries.FirstOrDefault(n => n is type);

            if (entry.RequiresUpgradedBook && mp.CodexState != 2) return; //dont give the player void entries if they dont have the void book
            if (entry.Locked)
            {
                entry.Locked = false;
                entry.New = true;
                if (mp.CodexState != 0) UILoader.GetUIState<CodexPopup>().TripEntry(entry.Title);
                Main.PlaySound(SoundID.Item30);
            }
        }

        public static void SpawnGem(int ID, Vector2 position)
        {
            int item = Item.NewItem(position, ItemType<Content.Items.Misc.StarlightGem>());
            (Main.item[item].modItem as Content.Items.Misc.StarlightGem).gemID = ID;
        }

        public static void DrawSymbol(SpriteBatch spriteBatch, Vector2 position, Color color)
        {
            Texture2D tex = GetTexture("StarlightRiver/Assets/Symbol");
            spriteBatch.Draw(tex, position, tex.Frame(), color * 0.8f, 0, tex.Size() / 2, 1, 0, 0);

            Texture2D tex2 = GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2");

            float fade = StarlightWorld.rottime / 6.28f;
            spriteBatch.Draw(tex2, position, tex2.Frame(), color * (1 - fade), 0, tex2.Size() / 2f, fade * 1.1f, 0, 0);
        }

        public static bool CheckCircularCollision(Vector2 center, int radius, Rectangle hitbox)
        {
            if (Vector2.Distance(center, hitbox.TopLeft()) <= radius) return true;
            if (Vector2.Distance(center, hitbox.TopRight()) <= radius) return true;
            if (Vector2.Distance(center, hitbox.BottomLeft()) <= radius) return true;
            return Vector2.Distance(center, hitbox.BottomRight()) <= radius;
        }

        public static bool CheckConicalCollision(Vector2 center, int radius, float angle, float width, Rectangle hitbox)
        {
            if (CheckPoint(center, radius, hitbox.TopLeft(), angle, width)) return true;
            if (CheckPoint(center, radius, hitbox.TopRight(), angle, width)) return true;
            if (CheckPoint(center, radius, hitbox.BottomLeft(), angle, width)) return true;
            return CheckPoint(center, radius, hitbox.BottomRight(), angle, width);
        }

        private static bool CheckPoint(Vector2 center, int radius, Vector2 check, float angle, float width)
        {
            float thisAngle = (center - check).ToRotation() % 6.28f;
            return Vector2.Distance(center, check) <= radius && thisAngle > angle - width && thisAngle < angle + width;
        }

        public static bool PointInTile(Vector2 point)
        {
            Point16 startCoords = new Point16((int)point.X / 16, (int)point.Y / 16);
            for(int x = -1; x <= 1; x++)
                for(int y = -1; y <= 1; y++)
                {                 
                    var thisPoint = startCoords + new Point16(x, y);

                    if (!WorldGen.InWorld(thisPoint.X, thisPoint.Y)) return false;

                        var tile = Framing.GetTileSafely(thisPoint);
                    if(tile.collisionType == 1)
                    {
                        var rect = new Rectangle(thisPoint.X * 16, thisPoint.Y * 16, 16, 16);
                        if (rect.Contains(point.ToPoint())) return true;
                    }
                }

            return false;
        }

        public static string TicksToTime(int ticks)
        {
            int sec = ticks / 60;
            return sec / 60 + ":" + (sec % 60 < 10 ? "0" + sec % 60 : "" + sec % 60);
        }

        public static void DoTilt(float intensity)
        {
            tiltMax = intensity; tiltTime = 0;
            StarlightRiver.Rotation = 0.01f;
        }

        public static void UpdateTilt()
        {
            if (Math.Abs(tiltMax) > 0)
            {
                tiltTime++;
                if (tiltTime >= 1 && tiltTime < 40)
                {
                    float tilt = tiltMax - tiltTime * tiltMax / 40f;
                    StarlightRiver.Rotation = tilt * (float)Math.Sin(Math.Pow(tiltTime / 40f * 6.28f, 0.9f));
                }

                if (tiltTime >= 40) { StarlightRiver.Rotation = 0; tiltMax = 0; }
            }
        }

        public static bool ScanForTypeDown(int startX, int startY, int type, int maxDown = 50)
        {
            for (int k = 0; k <= maxDown && k + startY < Main.maxTilesY; k++)
            {
                Tile tile = Framing.GetTileSafely(startX, startY + k);
                if (tile.active() && tile?.type == type)
                    return true;
            }
            return false;
        }

        public static int SamplePerlin2D(int x, int y, int min, int max)
        {
            Texture2D perlin = TextureManager.Load("Images/Misc/Perlin");

            Color[] rawData = new Color[perlin.Width]; //array of colors
            Rectangle row = new Rectangle(0, y, perlin.Width, 1); //one row of the image
            perlin.GetData(0, row, rawData, 0, perlin.Width); //put the color data from the image into the array
            return (int)(min + rawData[x % 512].R / 255f * max);
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
            return output;
        }

        public static List<T> RandomizeList<T>(List<T> input)
        {
            int n = input.Count();
            while (n > 1)
            {
                n--;
                int k = Main.rand.Next(n + 1);
                T value = input[k];
                input[k] = input[n];
                input[n] = value;
            }
            return input;
        }

        public static Player FindNearestPlayer(Vector2 position)
        {
            Player player = null;

            for (int k = 0; k < Main.maxPlayers; k++)
                if (Main.player[k] != null && Main.player[k].active && (player == null || Vector2.DistanceSquared(position, Main.player[k].Center) < Vector2.DistanceSquared(position, player.Center)))
                    player = Main.player[k];
            return player;
        }

        public static float BezierEase(float time)
        {
            return time * time / (2f * (time * time - time) + 1f);
        }

        public static T[] FastUnion<T>(this T[] front, T[] back)
        {
            T[] combined = new T[front.Length + back.Length];

            Array.Copy(front, combined, front.Length);
            Array.Copy(back, 0, combined, front.Length, back.Length);

            return combined;
        }

        public static bool IsEdgeTile(int x, int y)
        {
            return
                !Framing.GetTileSafely(x - 1, y).active() ||
                !Framing.GetTileSafely(x + 1, y).active() ||
                !Framing.GetTileSafely(x, y - 1).active() ||
                !Framing.GetTileSafely(x, y + 1).active();

        }
    }
}


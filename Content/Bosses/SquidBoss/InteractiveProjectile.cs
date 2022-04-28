using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	public abstract class InteractiveProjectile : ModProjectile
    {
        public List<Point16> ValidPoints { get; set; } = new List<Point16>(); //the points this Projectile allows tile placement at

        public bool CheckPoint(int x, int y) => ValidPoints.Contains(new Point16(x, y));

        public virtual void SafeKill(int timeLeft) { }

        public sealed override void Kill(int timeLeft)
        {
            SafeKill(timeLeft);

            if (ValidPoints.Count(n => Main.tile[n.X, n.Y].HasTile) == ValidPoints.Count) 
                GoodEffects();
            else 
                BadEffects();

            foreach (Point16 point in ValidPoints)
            {
                WorldGen.KillTile(point.X, point.Y);
            }
        }

        public sealed override void PostAI() //need to do this early to make sure all blocks get killed
        {
            foreach (Point16 point in ValidPoints.Where(n => !Main.tile[n.X, n.Y].HasTile))
            {
                Framing.GetTileSafely(point.X, point.Y).IsActuated = false;
            }

            if (Projectile.timeLeft < 10)
                foreach (Point16 point in ValidPoints)
                {
                    WorldGen.KillTile(point.X, point.Y);
                }
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.SquidBoss + "Highlight").Value;
            Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/SquareGlow").Value;

            int off = 16 * ((int)Projectile.ai[0] % 5);

            if (Projectile.timeLeft > 10)
                foreach (Point16 point in ValidPoints)
                {
                    if (!Main.tile[point.X, point.Y].HasTile)
                        Main.spriteBatch.Draw(tex, point.ToVector2() * 16 - Main.screenPosition, new Rectangle(0, off, 16, 16), Color.White);

                    float sin = (float)System.Math.Sin(Main.GameUpdateCount * 0.1f);

                    var colorGlow = new Color(50, 80, 150 + (int)(30 * sin));
                    colorGlow.A = 0;

                    Main.spriteBatch.Draw(tex2, point.ToVector2() * 16 - Main.screenPosition + Vector2.One * 8, null, colorGlow, 0, tex2.Size() / 2, 0.6f + 0.05f * sin, 0, 0);
                }

            Projectile.ai[0] += 0.2f;
        }

        /// <summary>
        /// what happens when the Projectile dies and tiles are placed appropriately
        /// </summary>
        public virtual void BadEffects() { }

        /// <summary>
        /// what happens if the Projectile dies and tiles are not placed
        /// </summary>
        public virtual void GoodEffects() { }

    }
}

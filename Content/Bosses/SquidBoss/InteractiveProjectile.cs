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

            if (ValidPoints.Count(n => Main.tile[n.X, n.Y].HasTile) == ValidPoints.Count) GoodEffects();
            else BadEffects();

            foreach (Point16 point in ValidPoints) WorldGen.KillTile(point.X, point.Y);
        }

        public sealed override void PostAI() //need to do this early to make sure all blocks get cucked
        {
            foreach (Point16 point in ValidPoints.Where(n => !Main.tile[n.X, n.Y].HasTile))
                Main.tile[point.X, point.Y].inActive(false);

            if (Projectile.timeLeft < 10)
                foreach (Point16 point in ValidPoints) WorldGen.KillTile(point.X, point.Y);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.SquidBoss + "Highlight").Value;
            int off = 16 * ((int)Projectile.ai[0] % 5);

            if (Projectile.timeLeft > 10)
                foreach (Point16 point in ValidPoints)
                    if (!Main.tile[point.X, point.Y].HasTile) spriteBatch.Draw(tex, point.ToVector2() * 16 - Main.screenPosition, new Rectangle(0, off, 16, 16), Color.White);
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

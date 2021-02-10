using Microsoft.Xna.Framework;
using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Physics
{
    public class RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;
        public Color color;

        public Vector2 posScreen => posNow - Main.screenPosition;

        public RopeSegment(Vector2 pos)
        {
            posNow = pos;
            posOld = pos;
            color = Color.White;
        }

        public RopeSegment(Vector2 pos, Color color)
        {
            posNow = pos;
            posOld = pos;
            this.color = color;
        }
    }
}
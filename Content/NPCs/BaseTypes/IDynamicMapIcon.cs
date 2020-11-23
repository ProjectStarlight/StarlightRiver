using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;

namespace StarlightRiver.NPCs
{
    internal interface IDynamicMapIcon
    {
        void DrawOnMap(SpriteBatch spriteBatch, Vector2 center, float scale, Color color);
    }
}
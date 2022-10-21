using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Core
{
	internal interface IDynamicMapIcon
	{
		void DrawOnMap(SpriteBatch spriteBatch, Vector2 center, float scale, Color color);
	}
}
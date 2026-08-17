using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Core.Systems.WaterAddonSystem
{
	public abstract class WaterAddon : ModType
	{
		public float Priority => 1f;

		/// <summary>
		/// call Main.SpriteBatch.Begin with the parameters you want for the front of water. Primarily used for applying shaders
		/// </summary>
		public abstract void SpritebatchChange();
		/// <summary>
		/// call Main.SpriteBatch.Begin with the parameters you want for the back of water. Primarily used for applying shaders
		/// </summary>
		public abstract void SpritebatchChangeBack();

		public abstract bool Visible { get; }

		public abstract Texture2D BlockTexture(Texture2D normal, int x, int y);

		public sealed override void Register()
		{

		}
	}
}
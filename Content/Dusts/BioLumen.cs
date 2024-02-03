using System;

namespace StarlightRiver.Content.Dusts
{
	internal class BioLumen : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;
		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.position.Y += (float)Math.Sin(StarlightWorld.visualTimer + dust.fadeIn) * 0.3f;
			dust.position += dust.velocity;
			dust.scale *= 0.994f;
			//Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale);
			if (dust.scale <= 0.2f)
				dust.active = false;
			return false;
		}
	}
}
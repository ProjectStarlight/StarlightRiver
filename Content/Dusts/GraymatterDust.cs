using StarlightRiver.Content.Biomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Dusts
{
	internal class GraymatterDust : ModDust
	{
		public int grayDustVisibleTimer = 0;

		public override string Texture => "StarlightRiver/Assets/Invisible";

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawGrayDusts;
		}

		private void DrawGrayDusts(SpriteBatch spriteBatch)
		{
			if (grayDustVisibleTimer > 0)
			{
				foreach(Dust dust in Main.dust)
				{
					if (dust.active && dust.type == Type)
					{
						var tex = Assets.Keys.GlowAlpha.Value;
						spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, new Color(255, 255, 255, 0), dust.rotation, tex.Size() / 2f, dust.scale, 0, 0);
					}
				}

				grayDustVisibleTimer--;
			}
		}

		public override bool Update(Dust dust)
		{
			grayDustVisibleTimer = 30;
			GraymatterBiome.forceGrayMatter = true;

			dust.velocity.X *= 0.98f;
			dust.velocity.Y -= 0.04f;

			dust.scale *= 0.98f;

			if (dust.scale < 0.1f)
				dust.active = false;

			dust.position += dust.velocity;

			Lighting.AddLight(dust.position, Color.White.ToVector3() * 0.4f * dust.scale);

			return false;
		}
	}
}

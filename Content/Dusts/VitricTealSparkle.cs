using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Dusts
{
	public class VitricTealSparkle : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;

		public override void OnSpawn(Dust dust)
		{
			dust.velocity *= 0.3f;
			dust.noGravity = true;
			dust.noLight = false;
			dust.scale *= 1.4f;
			dust.color.R = 160;
			dust.color.G = 235;
			dust.color.B = 255;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * (1 - dust.fadeIn);
		}

		public override bool Update(Dust dust)
		{
			dust.position.Y += dust.velocity.Y * 2;
			dust.velocity.Y += 0.01f;
			dust.position.X += dust.velocity.X * 2;
			dust.rotation += 0.06f;

			dust.scale *= 0.97f;
			dust.color *= 0.995f;

			if (dust.scale < 0.4f)
			{
				dust.active = false;
			}

			return false;
		}
	}

	public class VitricTealSparkleGravity : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Air";

		public override void OnSpawn(Dust dust)
		{
			dust.velocity *= 0.3f;
			dust.noGravity = true;
			dust.noLight = false;
			dust.scale *= 1.4f;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.position.Y += dust.velocity.Y * 2;
			dust.velocity.Y += 0.01f;
			dust.position.X += dust.velocity.X * 2;
			dust.rotation += 0.05f;

			dust.scale *= 0.97f;

			if (dust.scale < 0.4f)
			{
				dust.active = false;
			}

			return false;
		}
	}

	public class VitricTealSparkleAlternate : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Air";
		public override void OnSpawn(Dust dust)
		{
			dust.velocity *= 0.3f;
			dust.noGravity = true;
			dust.noLight = false;
			dust.scale *= 0.8f;
			dust.color.R = 170;
			dust.color.G = 235;
			dust.color.B = 255;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.rotation += 0.05f;
			dust.color *= 0.99f;

			dust.scale *= 0.98f;

			if (dust.scale < 0.4f)
			{
				dust.active = false;
			}

			return false;
		}
	}
}

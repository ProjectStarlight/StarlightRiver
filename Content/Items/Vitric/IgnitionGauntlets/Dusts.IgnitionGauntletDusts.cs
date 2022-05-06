using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Vitric
{
	public class IgnitionGauntletSmoke : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDustThree";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.scale *= 0.3f;
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 40)
				ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 40f);
			else if (dust.alpha < 80)
				ret = Color.Lerp(Color.Orange, Color.Red, (dust.alpha - 40) / 40f);
			else if (dust.alpha < 160)
				ret = Color.Lerp(Color.Red, gray, (dust.alpha - 80) / 80f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData == null)
			{
				dust.position -= Vector2.One * 35 * dust.scale;
				dust.customData = 0;
			}

			if ((int)dust.customData < 10)
			{
				dust.scale *= 1.1f;
				dust.customData = (int)dust.customData + 1;
			}
			else
			{
				if (dust.alpha > 60)
				{
					dust.scale *= 0.96f;
				}
				else
				{
					dust.scale *= 0.93f;
				}
			}


			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.85f;
			else
				dust.velocity *= 0.92f;

			if (dust.alpha > 60)
			{
				dust.alpha += 12;
			}
			else
			{
				dust.alpha += 8;
			}

			Lighting.AddLight(dust.position, ((Color)(GetAlpha(dust, Color.White))).ToVector3() * 0.5f);

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
	public class IgnitionGauntletSmoke2 : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDustThree";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.8f, 2f);
			dust.scale *= 0.3f;
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = new Color(25, 25, 25);
			Color ret;
			if (dust.alpha < 40)
				ret = Color.Lerp(Color.Yellow, Color.Orange, dust.alpha / 40f);
			else if (dust.alpha < 80)
				ret = Color.Lerp(Color.Orange, Color.Red, (dust.alpha - 40) / 40f);
			else if (dust.alpha < 160)
				ret = Color.Lerp(Color.Red, gray, (dust.alpha - 80) / 80f);
			else
				ret = gray;

			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData == null)
			{
				dust.position -= Vector2.One * 35 * dust.scale;
				dust.customData = 0;
			}

			if ((int)dust.customData < 10)
			{
				dust.scale *= 1.07f;
				dust.customData = (int)dust.customData + 1;
			}
			else
			{
				if (dust.alpha > 60)
				{
					dust.scale *= 0.98f;
				}
				else
				{
					dust.scale *= 0.96f;
				}
			}


			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.92f;
			else
				dust.velocity *= 0.96f;

			dust.alpha += 30;

			Lighting.AddLight(dust.position, ((Color)(GetAlpha(dust, Color.White))).ToVector3() * 0.5f);

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
	public class IgnitionGauntletSmoke3 : IgnitionGauntletSmoke
	{
		public override bool Update(Dust dust)
		{
			if (dust.customData == null)
			{
				dust.customData = 0;
			}

			if ((int)dust.customData < 7)
			{
				dust.scale *= 1.13f;
				dust.customData = (int)dust.customData + 1;
			}
			else
			{
				if (dust.alpha > 60)
				{
					dust.scale *= 0.96f;
				}
				else
				{
					dust.scale *= 0.93f;
				}
			}


			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.84f;
			else
				dust.velocity *= 0.93f;

			if (dust.alpha > 60)
			{
				dust.alpha += 6;
			}
			else
			{
				dust.alpha += 3;
			}

			Lighting.AddLight(dust.position, ((Color)(GetAlpha(dust, Color.White))).ToVector3() * 0.5f);

			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}
	class IgnitionGauntletSpark : BuzzSpark
	{
		public override bool Update(Dust dust)
		{
			if (dust.customData is null)
			{
				dust.position -= new Vector2(2.5f, 25).RotatedBy(dust.rotation) * dust.scale;
				dust.customData = 1;
			}

			dust.frame.Y++;
			dust.frame.Height--;

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.position += dust.velocity;

			dust.color.G -= 8;
			dust.color.A -= 5;

			dust.velocity.X *= 0.98f;
			dust.velocity.Y *= 0.95f;

			dust.velocity.Y += 0.15f;

			float mult = 1;

			if (dust.fadeIn < 5)
				mult = dust.fadeIn / 5f;

			dust.shader.UseSecondaryColor(new Color((int)(255 * (1 - (dust.fadeIn / 20f))), 0, 0) * mult);
			dust.shader.UseColor(dust.color * mult);
			dust.fadeIn += 2;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.02f);

			if (dust.fadeIn > 45)
				dust.active = false;
			return false;
		}
	}
	class IgnitionGauntletWind : BuzzSpark
	{
		public override bool Update(Dust dust)
		{
			if (dust.customData is null)
			{
				dust.position -= new Vector2(2.5f, 25).RotatedBy(dust.rotation) * dust.scale;
				dust.customData = 1;
			}

			dust.frame.Y++;
			dust.frame.Height--;

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.position += dust.velocity;

			dust.color.G -= 8;
			dust.color.A -= 5;

			dust.velocity.X *= 0.98f;
			dust.velocity.Y *= 0.95f;

			dust.velocity.Y += 0.15f;

			float mult = MathHelper.Min(dust.fadeIn / 15f, (45 - dust.fadeIn) / 15f);

			dust.shader.UseSecondaryColor(new Color((int)(255 * (1 - (dust.fadeIn / 20f))), 0, 0) * mult);
			dust.shader.UseColor(dust.color * mult);
			dust.fadeIn += 2;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.02f);

			if (dust.fadeIn > 45)
				dust.active = false;
			return false;
		}
	}
	public class IgnitionGauntletStar : ModDust
	{
		public override string Texture => AssetDirectory.VitricItem + "IgnitionGauntletStar";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = true;
			dust.frame = new Rectangle(0, 0, 74, 74);
			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
			dust.shader.UseColor(Color.White);

		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return Color.White;
		}

		public override bool Update(Dust dust)
        {
			dust.shader.UseColor(Color.White);

			dust.rotation += 0.25f * dust.scale;
			dust.position += dust.velocity;
			dust.fadeIn++;
			dust.scale = (1 + (float)(2 * Math.Sin(dust.fadeIn * 0.157f))) * 0.3f;
			if (dust.scale <= 0)
				dust.active = false;
			return false;
        }
    }
	class IgnitionGlowDust : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Keys/GlowVerySoft";

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var curveOut = Curve(1 - dust.fadeIn / 20f);
			var color = Color.Lerp(dust.color, new Color(255, 100, 0), dust.fadeIn / 10f);
			dust.color = color * (curveOut + 0.4f);
			return new Color(255, 100, 0);
		}

		float Curve(float input) //shrug it works, just a cubic regression for a nice looking curve
		{
			return -2.65f + 19.196f * input - 32.143f * input * input + 15.625f * input * input * input;
		}

		public override void OnSpawn(Dust dust)
		{
			dust.color = Color.Transparent;
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.scale *= 0.3f;
			dust.frame = new Rectangle(0, 0, 64, 64);
			dust.velocity *= 2;
			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			if (dust.fadeIn == 0)
				dust.position -= Vector2.One * 32 * dust.scale;

			//dust.rotation += dust.velocity.Y * 0.1f;
			dust.position += dust.velocity;
			dust.velocity *= 0.86f;
			dust.shader.UseColor(dust.color);
			dust.scale *= 0.95f;
			dust.fadeIn++;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

			if (dust.fadeIn > 25)
				dust.active = false;

			return false;
		}
	}
}
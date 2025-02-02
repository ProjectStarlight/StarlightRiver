﻿using StarlightRiver.Core.Systems.PixelationSystem;
using System;

namespace StarlightRiver.Content.Dusts
{
	public class Glow : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Keys/GlowSoft";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 64, 64);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is null)
			{
				dust.position -= Vector2.One * 32 * dust.scale;
				dust.customData = true;
			}

			Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 32 * dust.scale;

			dust.scale *= 0.95f;
			Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation + 0.06f) * 32 * dust.scale;

			dust.rotation += 0.06f;
			dust.position += currentCenter - nextCenter;

			dust.shader.UseColor(dust.color);

			dust.position += dust.velocity;

			if (!dust.noGravity)
				dust.velocity.Y += 0.1f;

			dust.velocity *= 0.99f;
			dust.color *= 0.95f;

			if (!dust.noLight)
				Lighting.AddLight(dust.position, dust.color.ToVector3());

			if (dust.scale < 0.05f)
				dust.active = false;

			return false;
		}
	}

	public class GlowFastDecelerate : Glow
	{
		public override bool Update(Dust dust)
		{
			dust.velocity *= 0.95f;
			return base.Update(dust);
		}
	}

	public class GlowFollowPlayer : Glow
	{
		public override bool Update(Dust dust)
		{
			base.Update(dust);

			if (dust.customData is object[] pair)
			{
				var player = pair[0] as Player;
				var pos = (Vector2)pair[1];

				dust.position = player.Center + Vector2.UnitY * player.gfxOffY + pos - Vector2.One * 32 * dust.scale;
				pair[1] = pos + dust.velocity;
			}

			return false;
		}
	}

	public class PixelatedGlow : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity *= 0.95f;

			dust.alpha += 5;

			dust.alpha = (int)(dust.alpha * 1.01f);
			dust.scale *= 0.965f;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, dust.scale * lerper, 0f, 0f);

				float glowScale = dust.scale * 0.25f;

				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * lerper, dust.rotation, tex.Size() / 2f, glowScale * lerper, 0f, 0f);
			});

			return false;
		}
	}

	public class PixelatedEmber : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is int)
				dust.customData = new object[] { dust.customData, false };

			object[] data = dust.customData as object[];

			dust.position += dust.velocity;
			dust.velocity *= 0.98f;
			dust.velocity = dust.velocity.RotatedBy(0.04f * (int)data[0]);

			if (dust.alpha > 80 && !(bool)data[1])
			{
				data[0] = (int)data[0] * -1;
				data[1] = true;
			}

			dust.alpha += 2;

			dust.alpha = (int)(dust.alpha * 1.01f);
			dust.scale *= 0.99f;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
			{
				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color * lerper, dust.rotation, tex.Size() / 2f, dust.scale * lerper, 0f, 0f);

				float glowScale = dust.scale * 0.25f;

				Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * lerper, dust.rotation, tex.Size() / 2f, glowScale * lerper, 0f, 0f);
			});

			return false;
		}
	}
}
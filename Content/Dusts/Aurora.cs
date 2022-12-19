using StarlightRiver.Content.NPCs.Actors;
using System;

namespace StarlightRiver.Content.Dusts
{
	class Aurora : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Aurora";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			dust.fadeIn = 60;
			dust.frame = new Rectangle(0, 0, 100, 100);
			dust.scale = 0;

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
			dust.shader.UseColor(Color.Transparent);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var col = Vector3.Lerp(dust.color.ToVector3(), Color.White.ToVector3(), dust.scale / (dust.customData is null ? 0.5f : (float)dust.customData));
			return new Color(col.X, col.Y, col.Z) * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 2.5f);

			Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 50 * dust.scale;

			dust.scale = (dust.fadeIn / 15f - (float)Math.Pow(dust.fadeIn, 2) / 900f) * (dust.customData is null ? 0.5f : (float)dust.customData) * 0.2f;
			Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation + 0.06f) * 50 * dust.scale;

			dust.rotation += 0.06f;
			dust.position += currentCenter - nextCenter;

			dust.fadeIn--;
			dust.position += dust.velocity * 0.25f;

			dust.shader.UseColor(dust.color);

			if (dust.fadeIn <= 0)
				dust.active = false;
			return false;
		}
	}

	class AuroraFast : Aurora
	{
		public override bool Update(Dust dust)
		{
			dust.fadeIn--;
			return base.Update(dust);
		}
	}

	class AuroraSuction : Aurora
	{
		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			var col = Vector3.Lerp(dust.color.ToVector3(), Color.White.ToVector3(), dust.scale / (dust.customData is null ? 0.5f : ((AuroraSuctionData)dust.customData).scale));
			return new Color(col.X, col.Y, col.Z) * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			Lighting.AddLight(dust.position, dust.color.ToVector3() * dust.scale * 2.5f);

			Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 50 * dust.scale;

			dust.scale = (dust.fadeIn / 15f - (float)Math.Pow(dust.fadeIn, 2) / 900f) * (dust.customData is null ? 0.5f : ((AuroraSuctionData)dust.customData).scale) * 0.2f;
			Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation + 0.06f) * 50 * dust.scale;

			dust.rotation += 0.06f;
			dust.position += currentCenter - nextCenter;

			dust.fadeIn--;
			if (dust.customData is null || ((AuroraSuctionData)dust.customData).actor.targetItem is null)
				dust.position += dust.velocity * 0.25f;
			else
				dust.position += Vector2.Normalize(((AuroraSuctionData)dust.customData).actor.targetItem.Center - dust.position) * 1.5f;

			dust.shader.UseColor(dust.color);

			if (dust.fadeIn <= 0)
				dust.active = false;
			return false;
		}
	}

	struct AuroraSuctionData
	{
		public readonly StarlightWaterActor actor;
		public readonly float scale;

		public AuroraSuctionData(StarlightWaterActor actor, float scale)
		{
			this.actor = actor;
			this.scale = scale;
		}
	}
}

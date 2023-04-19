﻿namespace StarlightRiver.Content.Dusts
{
	public class GlassAttracted : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Glass";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return lightColor;
		}

		public override bool Update(Dust dust)
		{
			dust.fadeIn++;
			Vector2 origin = Vector2.Zero;

			if (dust.customData is Player player)
				origin = player.Center;

			if (dust.customData is NPC npc)
				origin = npc.Center;

			if (dust.customData is Vector2 vector)
				origin = vector;

			dust.position += dust.velocity;
			dust.position += Vector2.Normalize(origin - dust.position) * 6;
			dust.velocity *= 0.94f;
			dust.rotation = (origin - dust.position).Length() * 0.1f;
			dust.scale *= 0.99f;

			if (Vector2.Distance(dust.position, origin) <= 5)
				dust.active = false;

			return false;
		}
	}

	public class GlassAttractedGlow : GlassAttracted
	{
		public override string Texture => AssetDirectory.Dust + "GlassWhite";

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return Helpers.Helper.MoltenVitricGlow(dust.fadeIn * 3);
		}
	}

	public class GlassGravity : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Glass";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity.Y += 0.15f;
			dust.rotation += 0.1f;
			dust.scale *= 0.98f;

			if (dust.scale <= 0.2)
				dust.active = false;

			return false;
		}
	}

	public class GlassNoGravity : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Glass";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.rotation += 0.12f;
			dust.scale *= 0.98f;

			if (dust.scale <= 0.2)
				dust.active = false;

			return false;
		}
	}
}
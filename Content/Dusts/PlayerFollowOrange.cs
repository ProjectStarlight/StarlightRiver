﻿namespace StarlightRiver.Content.Dusts
{
	public class PlayerFollowOrange : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;

		public override void OnSpawn(Dust dust)
		{
			dust.velocity *= 0.3f;
			dust.noGravity = true;
			dust.noLight = false;
			dust.color.R = 255;
			dust.color.G = 162;
			dust.color.B = 107;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * 0.15f;
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is int whoAmI && Main.player[whoAmI].active)
				dust.position = Main.player[whoAmI].Center + new Vector2(0, Main.player[whoAmI].gfxOffY) + dust.velocity;

			dust.rotation += 0.15f;

			dust.scale *= 0.95f;

			if (dust.scale < 0.4f)
				dust.active = false;
			return false;
		}
	}
}
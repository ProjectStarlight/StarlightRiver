using StarlightRiver.Content.Items.UndergroundTemple;

namespace StarlightRiver.Content.Dusts
{
	public class PickCharge : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "Aurora";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 100, 100);
			dust.scale = 0.1f;
			dust.alpha = 0;

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value), "GlowingDustPass");
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			if (dust.customData is int whoAmI && Main.player[whoAmI].active)
			{
				Player Player = Main.player[whoAmI];
				dust.position = Player.Center + new Vector2(0, Player.gfxOffY) + dust.velocity + Vector2.One * -50 * dust.scale;

				if (!Main.mouseRight || !(Player.HeldItem.ModItem is TemplePick))
					dust.active = false; //RIP multiPlayer TODO: Make this not gay
			}
			else
			{
				dust.active = false;
			}

			if (dust.alpha < 30)
				dust.alpha++;

			if (dust.scale < 0.2f)
				dust.scale += 0.025f;

			dust.shader.UseColor(dust.color * (dust.alpha / 30f));

			Vector2 currentCenter = dust.position + Vector2.One.RotatedBy(dust.rotation) * 50 * dust.scale;

			Vector2 nextCenter = dust.position + Vector2.One.RotatedBy(dust.rotation + 0.1f) * 50 * dust.scale;

			dust.rotation += 0.1f;
			dust.velocity += currentCenter - nextCenter;

			return false;
		}
	}
}
namespace StarlightRiver.Content.Dusts
{
	public class AuroraWater : ModDust
	{
		public override string Texture => "StarlightRiver/Assets/Invisible";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.98f;

			dust.rotation += 0.06f;
			dust.position += dust.velocity;

			dust.velocity.Y += 0.15f;

			dust.color.A = 255;

			if (dust.scale < 0.05f)
				dust.active = false;

			return false;
		}
	}
}
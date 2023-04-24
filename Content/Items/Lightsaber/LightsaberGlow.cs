namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberGlow : Dusts.Glow
	{
		public override bool Update(Dust dust)
		{
			dust.scale *= 0.95f;
			dust.velocity *= 0.98f;
			dust.color *= 1.05f;
			return base.Update(dust);
		}
	}

	public class LightsaberGlowSoft : LightsaberGlow
	{
		public override string Texture => AssetDirectory.Keys + "GlowVerySoft";

		public override bool Update(Dust dust)
		{
			dust.scale *= 0.95f;
			dust.velocity = Vector2.Zero;
			return base.Update(dust);
		}
	}
}
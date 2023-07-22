namespace StarlightRiver.Content.Dusts
{
	public class Drop : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;
		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = false;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 10, 14);
		}

		public override bool Update(Dust dust)
		{
			dust.fadeIn++;

			dust.position += dust.velocity;
			dust.velocity.Y += 0.15f;

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.scale *= 0.99f;

			if (dust.scale < 1)
				dust.alpha += 10;

			if (dust.alpha > 240)
				dust.active = false;

			return false;
		}
	}
}
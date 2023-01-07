namespace StarlightRiver.Content.Dusts
{
	public abstract class QuickSplash : ModDust
	{
		public override string Texture => AssetDirectory.Dust + Name;

		public override void SetStaticDefaults()
		{
			UpdateType = 33;
		}

		public override void OnSpawn(Dust dust)
		{
			dust.alpha = 170;
			dust.velocity *= 0.5f;
			dust.velocity.Y += 1f;
		}
	}

	public sealed class CorruptJungleSplash : QuickSplash { }

	public sealed class BloodyJungleSplash : QuickSplash { }

	public sealed class HolyJungleSplash : QuickSplash { }
}
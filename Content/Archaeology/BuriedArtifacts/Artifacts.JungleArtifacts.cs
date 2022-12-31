using StarlightRiver.Content.Items.BuriedArtifacts;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
	public class WindTotemArtifact : JungleArtifact
	{
		public override Vector2 Size => new(34, 32);

		public override float SpawnChance => 0.5f;

		public override int ItemType => ModContent.ItemType<WindTotemArtifactItem>();

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.LimeArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.LimeGreen;
	}

	public class RainTotemArtifact : JungleArtifact
	{
		public override Vector2 Size => new(28, 32);

		public override float SpawnChance => 0.5f;

		public override int ItemType => ModContent.ItemType<RainTotemArtifactItem>();

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.LimeArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.LimeGreen;
	}
}
using StarlightRiver.Content.Items.BuriedArtifacts;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
	public class ExoticGeodeArtifact : UndergroundArtifact
	{
		public override Vector2 Size => new(32, 32);

		public override float SpawnChance => 1f;

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GeodeArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Purple;

		public override int ItemType => ModContent.ItemType<ExoticGeodeArtifactItem>();
	}

	public class PerfectlyGenericArtifact : UndergroundArtifact
	{
		public override Vector2 Size => new(32, 32);

		public override float SpawnChance => 0.01f;

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.WhiteArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.White;

		public override int ItemType => ModContent.ItemType<PerfectlyGenericArtifactItem>();
	}
}
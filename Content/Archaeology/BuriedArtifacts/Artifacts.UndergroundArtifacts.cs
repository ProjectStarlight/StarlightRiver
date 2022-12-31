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

	public abstract class SteampunkArtifact : UndergroundArtifact
	{
		public override float SpawnChance => 0.5f;

		public override Color BeamColor => Color.Gold;

		public override int SparkleRate => 30;

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>();
	}

	public class CopperCogArtifact : SteampunkArtifact
	{
		public override Vector2 Size => new(32, 32);

		public override int ItemType => ModContent.ItemType<CopperCogArtifactItem>();
	}

	public class ImportantScrewArtifact : SteampunkArtifact
	{
		public override Vector2 Size => new(26, 19);

		public override int ItemType => ModContent.ItemType<ImportantScrewArtifactItem>();
	}

	public class SuspiciouslyStrangeBrewArtifact : SteampunkArtifact
	{
		public override Vector2 Size => new(18, 38);

		public override int ItemType => ModContent.ItemType<SuspiciouslyStrangeBrewArtifactItem>();
	}

	public class InconspicuousPlatingArtifact : SteampunkArtifact
	{
		public override Vector2 Size => new(30, 28);

		public override int ItemType => ModContent.ItemType<InconspicuousPlatingArtifactItem>();
	}
}
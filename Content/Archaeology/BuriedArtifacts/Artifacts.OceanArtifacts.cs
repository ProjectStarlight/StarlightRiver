using StarlightRiver.Content.Items.BuriedArtifacts;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
	public class PirateChestArtifact : OceanArtifact
	{
		public override Vector2 Size => new(32, 24);

		public override float SpawnChance => 0.5f;

		public override int ItemType => ModContent.ItemType<PirateChestArtifactItem>();

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Gold;
	}
}
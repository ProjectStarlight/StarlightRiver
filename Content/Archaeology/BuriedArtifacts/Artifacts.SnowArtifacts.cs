using StarlightRiver.Content.Items.Snow;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
	public class MeadHornArtifact : SnowArtifact
	{
		public override string TexturePath => AssetDirectory.SnowItem + "MeadHorn";

		public override Vector2 Size => new(46, 40);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<MeadHorn>();

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.BlueArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Cyan;
	}

	public class AquamarinePendantArtifact : SnowArtifact
	{
		public override string TexturePath => AssetDirectory.SnowItem + "AquamarinePendant";

		public override Vector2 Size => new(36, 48);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<AquamarinePendant>();

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.BlueArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Cyan;
	}
}
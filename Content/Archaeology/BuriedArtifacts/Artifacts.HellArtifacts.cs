using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Content.Items.Hell;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
	public class CharonsObolArtifact : HellArtifact
	{
		public override string TexturePath => AssetDirectory.HellItem + "CharonsObol_Sparkleless";

		public override Vector2 Size => new(32, 28);

		public override float SpawnChance => 1f;

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Gold;

		public override int ItemType => ModContent.ItemType<CharonsObol>();
	}

	public class WarriorKingsCrownArtifact : HellArtifact
	{
		public override string TexturePath => AssetDirectory.ArtifactItem + "WarriorKingsCrown";

		public override Vector2 Size => new(32, 28);

		public override float SpawnChance => 1f;

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.RedArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Orange;

		public override int ItemType => ModContent.ItemType<WarriorKingsCrown>();
	}
}
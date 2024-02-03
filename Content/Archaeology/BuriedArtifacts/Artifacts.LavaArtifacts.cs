using Terraria.ID;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
	public class LavaCharmArtifact : LavaArtifact
	{
		public override string TexturePath => "Terraria/Images/Item_906";

		public override Vector2 Size => new(30, 32);

		public override float SpawnChance => 1f;

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.RedArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Red;

		public override int ItemType => ItemID.LavaCharm;
	}
}
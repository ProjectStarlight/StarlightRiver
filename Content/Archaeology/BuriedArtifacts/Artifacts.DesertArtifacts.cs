﻿using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Content.Items.Desert;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
	public class WhipArtifact : DesertArtifact
	{

		public override string TexturePath => AssetDirectory.ArtifactItem + "ArchaeologistsWhip";

		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Gold;

		public override Vector2 Size => new(36, 50);

		public override float SpawnChance => 0.5f;

		public override int ItemType => ModContent.ItemType<ArchaeologistsWhip>();
	}

	public class SandscriptArtifact : DesertArtifact
	{
		public override string TexturePath => AssetDirectory.DesertItem + "Sandscript";

		public override int SparkleRate => 40;

		public override Vector2 Size => new(30, 34);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<Sandscript>();
	}

	public abstract class FossilArtifact : DesertArtifact
	{
		public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>();

		public override int SparkleRate => 40;

		public override Color BeamColor => Color.Gold;
	}

	public class DesertArtifact1 : FossilArtifact
	{
		public override Vector2 Size => new(36, 62);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<DesertArtifact1Item>();
	}

	public class DesertArtifact2 : FossilArtifact
	{
		public override Vector2 Size => new(30, 30);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<DesertArtifact2Item>();
	}

	public class DesertArtifact3 : FossilArtifact
	{
		public override Vector2 Size => new(30, 28);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<DesertArtifact3Item>();
	}

	public class DesertArtifact4 : FossilArtifact
	{
		public override Vector2 Size => new(24, 18);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<DesertArtifact4Item>();
	}

	public class DesertArtifact5 : FossilArtifact
	{
		public override Vector2 Size => new(28, 20);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<DesertArtifact5Item>();
	}

	public class DesertArtifact6 : FossilArtifact
	{
		public override Vector2 Size => new(38, 26);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<DesertArtifact6Item>();
	}

	public class DesertArtifact7 : FossilArtifact
	{
		public override Vector2 Size => new(30, 28);

		public override float SpawnChance => 1f;

		public override int ItemType => ModContent.ItemType<DesertArtifact7Item>();
	}
}
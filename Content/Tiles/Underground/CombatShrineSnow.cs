using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground
{
	class CombatShrineSnow : ShrineTile
	{
		public const int COMBAT_SHRINE_TILE_WIDTH = 3;
		public const int COMBAT_SHRINE_TILE_HEIGHT = 6;

		public override int DummyType => DummySystem.DummyType<CombatShrineSnowDummy>();

		public override string Texture => "StarlightRiver/Assets/Tiles/Underground/CombatShrine";

		public override int ShrineTileWidth => COMBAT_SHRINE_TILE_WIDTH;

		public override int ShrineTileHeight => COMBAT_SHRINE_TILE_HEIGHT;

		public override string GetCustomKey()
		{
			return "A shrine - to which deity, you do not know, though it wields a blade. Frost gathers on its surface, and cold runes dance across its pedestal.";
		}
	}

	[SLRDebug]
	class CombatShrineSnowItem : QuickTileItem
	{
		public CombatShrineSnowItem() : base("Combat shrine snow placer", "{{Debug}} item", "CombatShrineSnow") { }
	}

	class CombatShrineSnowDummy : CombatShrineBase
	{
		public override int MaxWaves => 6;

		public override int ShrineTileWidth => CombatShrineSnow.COMBAT_SHRINE_TILE_WIDTH;
		public override int ShrineTileHeight => CombatShrineSnow.COMBAT_SHRINE_TILE_HEIGHT;

		public CombatShrineSnowDummy() : base(ModContent.TileType<CombatShrineSnow>(), CombatShrineSnow.COMBAT_SHRINE_TILE_WIDTH * 16, CombatShrineSnow.COMBAT_SHRINE_TILE_HEIGHT * 16) { }

		protected override void SpawnWave()
		{
			// Snow-themed dust effects (blue/white instead of red)
			for (int k = 0; k < 20; k++)
				Dust.NewDustPerfect(Center + new Vector2(Main.rand.NextFloat(-24, 24), 28), ModContent.DustType<Dusts.Glow>(), Vector2.UnitY * -Main.rand.NextFloat(5), 0, new Color(150, 200, 255), 0.5f);

			if (state == 1)
			{
				SpawnNPC(Center + new Vector2(130, 50), NPCID.SpikedIceSlime, 20, hpOverride: 0.75f);
				SpawnNPC(Center + new Vector2(-130, 50), NPCID.SpikedIceSlime, 20, hpOverride: 0.75f);
				SpawnNPC(Center + new Vector2(267, -40), NPCID.SpikedIceSlime, 20, hpOverride: 0.75f);
				SpawnNPC(Center + new Vector2(-267, -40), NPCID.SpikedIceSlime, 20, hpOverride: 0.75f);
			}

			if (state == 2)
			{
				SpawnNPC(Center + new Vector2(110, 50), NPCID.SpikedIceSlime, 20);
				SpawnNPC(Center + new Vector2(-110, 50), NPCID.SpikedIceSlime, 20);
				SpawnNPC(Center + new Vector2(240, 40), NPCID.SnowFlinx, 20);
				SpawnNPC(Center + new Vector2(-240, 40), NPCID.SnowFlinx, 20);
				SpawnNPC(Center + new Vector2(0, -150), NPCID.IceBat, 20);
			}

			if (state == 3)
			{
				SpawnNPC(Center + new Vector2(130, 40), NPCID.UndeadViking, 20);
				SpawnNPC(Center + new Vector2(-130, 40), NPCID.UndeadViking, 20);
				SpawnNPC(Center + new Vector2(140, -140), NPCID.IceBat, 20);
				SpawnNPC(Center + new Vector2(-140, -140), NPCID.IceBat, 20);
			}

			if (state == 4)
			{
				SpawnNPC(Center + new Vector2(130, 50), NPCID.SnowFlinx, 20);
				SpawnNPC(Center + new Vector2(-130, 50), NPCID.SnowFlinx, 20);
				SpawnNPC(Center + new Vector2(267, -40), NPCID.UndeadViking, 20);
				SpawnNPC(Center + new Vector2(-267, -40), NPCID.UndeadViking, 20);
				SpawnNPC(Center + new Vector2(70, -140), NPCID.IceBat, 20);
				SpawnNPC(Center + new Vector2(-70, -140), NPCID.IceBat, 20);
			}

			if (state == 5)
			{
				SpawnNPC(Center + new Vector2(130, 50), NPCID.UndeadViking, 20);
				SpawnNPC(Center + new Vector2(-130, 50), NPCID.UndeadViking, 20);
				SpawnNPC(Center + new Vector2(120, -160), NPCID.IceBat, 20);
				SpawnNPC(Center + new Vector2(-120, -160), NPCID.IceBat, 20);
				SpawnNPC(Center + new Vector2(220, -110), NPCID.IceBat, 20);
				SpawnNPC(Center + new Vector2(-220, -110), NPCID.IceBat, 20);
			}

			if (state == 6)
			{
				SpawnNPC(Center + new Vector2(130, 50), NPCID.SpikedIceSlime, 20);
				SpawnNPC(Center + new Vector2(-130, 50), NPCID.SpikedIceSlime, 20);
				SpawnNPC(Center + new Vector2(267, -50), NPCID.UndeadViking, 20);
				SpawnNPC(Center + new Vector2(-267, -50), NPCID.UndeadViking, 20);
				SpawnNPC(Center + new Vector2(0, -170), NPCID.IceElemental, 40, hpOverride: 1.5f, scale: 1.5f);
			}
		}
	}
}

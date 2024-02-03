using StarlightRiver.Content.NPCs.Vitric;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	internal class CrystalSlimeSpawner : DummyTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override int DummyType => DummySystem.DummyType<CrystalSlimeSpawnerDummy>();

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(5, 3, 0, SoundID.Tink, new Color(255, 255, 255));
		}
	}

	[SLRDebug]
	internal class CrystalSlimeSpawnerItem : QuickTileItem
	{
		public CrystalSlimeSpawnerItem() : base("NPC Spawner", "", "CrystalSlimeSpawner", 1, AssetDirectory.VitricTile, false) { }
	}

	internal class CrystalSlimeSpawnerDummy : Dummy
	{
		public bool spawnerActive;

		public float timer;
		public float spawned;

		public CrystalSlimeSpawnerDummy() : base(ModContent.TileType<CrystalSlimeSpawner>(), 32, 48) { }

		public override void Update()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return; // As of this time no logic in here here is for multiplayer clients

			if (timer <= 0 && Main.player.Any(n => Vector2.Distance(n.Center, Center) < 300))
			{
				spawnerActive = true;
				timer = 1200;
			}

			if (timer > 0)
				timer--;

			if (spawnerActive && !Main.npc.Any(n => n.active && n.type == ModContent.NPCType<CrystalSlime>()))
			{
				NPC.NewNPC(GetSource_FromThis(), (int)Center.X, (int)Center.Y, ModContent.NPCType<CrystalSlime>(), 0, 0, 1);

				spawnerActive = false;
			}
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

		}
	}
}
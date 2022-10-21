using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class BoulderMaker : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			this.QuickSetFurniture(6, 1, DustType<Dusts.Sand>(), SoundID.Tink, false, new Color(100, 80, 10));
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (StarlightWorld.HasFlag(WorldFlags.DesertOpen) && tile.TileFrameX == 0 && !Main.npc.Any(n => n.active && n.type == NPCType<Boulder>()))
				NPC.NewNPC(new EntitySource_WorldEvent(), i * 16 + 48, j * 16, NPCType<Boulder>(), 0, j * 16);
		}
	}

	class BoulderMakerItem : QuickTileItem
	{
		public BoulderMakerItem() : base("Boulder Maker", "Debug Item", "BoulderMaker", 1, AssetDirectory.Debug, true) { }
	}

	class Boulder : ModNPC
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Temple Boulder");
		}

		public override void SetDefaults()
		{
			NPC.width = 80;
			NPC.height = 80;
			NPC.noTileCollide = true;
			NPC.dontTakeDamage = true;
			NPC.knockBackResist = 0f;
			NPC.lifeMax = 2;
			NPC.damage = 80;
			NPC.aiStyle = -1;
			NPC.behindTiles = true;
		}

		public override void AI()
		{
			if (NPC.position.Y > NPC.ai[0])
				NPC.noTileCollide = false;

			if (NPC.velocity.Y == 0 && NPC.velocity.X < 15)
				NPC.velocity.X += 0.05f;

			if (NPC.collideX)
				NPC.Kill();

			NPC.rotation += NPC.velocity.X / 40f;
		}

		public override void OnKill()
		{
			for (int k = 0; k < 100; k++)
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone);

			Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42 with { Volume = 1f, Pitch = -0.8f }, NPC.Center);
		}
	}
}

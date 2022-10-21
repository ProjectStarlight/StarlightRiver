using Microsoft.Xna.Framework;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricBossBarrier : ModTile, IOrderedLoadable
	{
		public float Priority => 0f;

		new public void Load()
		{
			On.Terraria.Main.Update += UpdateCollision; //TODO: Find a better/cleaner way to do this
		}

		new public void Unload()
		{
			On.Terraria.Main.Update -= UpdateCollision;
		}

		public override string Texture => AssetDirectory.Invisible;

		private void UpdateCollision(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
		{
			orig(self, gameTime);

			if (Main.gameMenu && Main.netMode != NetmodeID.Server)
				return;

			Main.tileSolid[TileType<VitricBossBarrier>()] = Main.npc.Any(n => n.active && n.type == NPCType<VitricBoss>());
		}

		public override void SetStaticDefaults()
		{
			TileID.Sets.DrawsWalls[Type] = true;
			Main.tileBlockLight[Type] = false;
			MinPick = int.MaxValue;
		}
	}
}
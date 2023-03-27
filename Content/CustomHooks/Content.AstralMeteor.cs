using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace StarlightRiver.Content.CustomHooks
{
	class AstralMeteor : HookGroup
	{
		//Swaps the vanilla meteor events out, could create conflicts if other mods attempt the same but shouldnt be anything fatal
		public override void Load()
		{
			Terraria.On_WorldGen.meteor += AluminumMeteor;
		}

		private bool AluminumMeteor(Terraria.On_WorldGen.orig_meteor orig, int i, int j, bool ignorePlayers)
		{
			CameraSystem.shake += 80;
			Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode);

			if (Main.rand.Next(3) > 1)
			{
				var target = new Point16();

				while (!CheckAroundMeteor(target))
				{
					int x = Main.rand.Next(Main.maxTilesX);

					for (int y = 0; y < Main.maxTilesY; y++)
					{
						if (Framing.GetTileSafely(x, y).HasTile)
						{
							target = new Point16(x, y - 20);
							break;
						}
					}
				}

				for (int x = -10; x < 10; x++)
				{
					for (int y = -30; y < 30; y++)
					{
						if (Math.Abs(x) < 10 - Math.Abs(y) / 3 + StarlightWorld.genNoise.GetPerlin(x * 4, y * 4) * 8)
							WorldGen.PlaceTile(target.X + x, target.Y + y, ModContent.TileType<Tiles.Moonstone.MoonstoneOre>(), true, true);
					}
				}

				for (int x = -15; x < 15; x++)
				{
					for (int y = 0; y < 40; y++)
					{
						if (Math.Abs(x) < 10 - Math.Abs(y) / 3 + StarlightWorld.genNoise.GetPerlin(x * 4, y * 4) * 8)
							WorldGen.PlaceTile(target.X + x, target.Y + y, ModContent.TileType<Tiles.Moonstone.MoonstoneOre>(), true, true);
					}
				}

				if (Main.netMode == NetmodeID.SinglePlayer)
					Main.NewText("A shard of the moon has landed!", new Color(107, 233, 231));

				else if (Main.netMode == NetmodeID.Server)
					Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("A shard of the moon has landed!"), new Color(107, 233, 231));

				return true;
			}

			else
			{
				return orig(i, j, ignorePlayers);
			}
		}

		private bool CheckAroundMeteor(Point16 test)
		{
			if (test == Point16.Zero)
				return false;

			for (int x = -35; x < 35; x++)
			{
				for (int y = -35; y < 35; y++)
				{
					if (WorldGen.InWorld(test.X + x, test.Y + y))
					{
						Tile tile = Framing.GetTileSafely(test + new Point16(x, y));

						if (tile.TileType == TileID.Containers || tile.TileType == TileID.Containers2)
							return false;
					}
				}
			}

			if (Main.npc.Any(n => n.active && n.friendly && Vector2.Distance(n.Center, test.ToVector2() * 16) <= 35 * 16))
				return false;
			else
				return true;
		}
	}
}
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.BlockerTileSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Tiles.Dungeon
{
	internal class ShardCage : DummyTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override int DummyType => DummySystem.DummyType<ShardCageDummy>();

		public override void SetStaticDefaults()
		{	
			QuickBlock.QuickSetFurniture(this, 5, 1, DustID.Stone, SoundID.Tink, true, new Color(100, 100, 100));
			Main.tileSolid[Type] = true;
		}
	}

	internal class ShardCageDummy : Dummy
	{
		List<int> attempts = new();

		static readonly int[] solution = new int[] { 1, 2, 3 };

		public ShardCageDummy() : base(ModContent.TileType<ShardCage>(), 5 * 16, 1 * 16) { }

		public override void OnLoad(Mod mod)
		{
			BlockerTileSystem.LoadBarrier("ShardCageBarrier", () => !CagePuzzleSystem.solved);
		}

		public override void Update()
		{
			if (CagePuzzleSystem.solved)
				return;

			for (int k = 0; k < attempts.Count; k++)
			{
				if (attempts[k] != solution[k])
				{
					attempts.Clear();
					break;
				}

				if (k == 2)
				{
					CagePuzzleSystem.solved = true;
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);

					for (int n = 0; n < 40; n++)
					{
						Dust.NewDust(position, width, height, DustID.Iron);
					}

					attempts.Clear();
				}
			}

			Vector2 topleft = position + new Vector2(0, -16 * 4);

			var leftBox = new Rectangle((int)TopLeft.X - 8, (int)TopLeft.Y - 16 * 4, 16, 16 * 5);
			var rightBox = new Rectangle((int)TopLeft.X + 16 * 5 - 8, (int)TopLeft.Y - 16 * 4, 16, 16 * 5);
			var bottomBox = new Rectangle((int)TopLeft.X, (int)TopLeft.Y + 8, 16 * 5, 16);

			foreach (Player player in Main.player.Where(n => n.active))
			{
				if (AbilityHelper.CheckDash(player, leftBox))
				{
					player.GetHandler().ActiveAbility?.Deactivate();
					player.velocity = Vector2.Normalize(player.velocity) * -10f;
					attempts.Add(1);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);
				}
				else if (AbilityHelper.CheckDash(player, rightBox))
				{
					player.GetHandler().ActiveAbility?.Deactivate();
					player.velocity = Vector2.Normalize(player.velocity) * -10f;
					attempts.Add(2);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);
				}
				else if (AbilityHelper.CheckDash(player, bottomBox))
				{
					player.GetHandler().ActiveAbility?.Deactivate();
					player.velocity = Vector2.Normalize(player.velocity) * -10f;
					attempts.Add(3);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.DungeonTile + "ShardCage").Value;

			if (CagePuzzleSystem.solved)
				tex = ModContent.Request<Texture2D>(AssetDirectory.DungeonTile + "ShardCageOpen").Value;

			Vector2 topleft = position + new Vector2(0, -16 * 4);

			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(tex, topleft - Main.screenPosition, Color.White);

			var leftBox = new Rectangle((int)TopLeft.X - 8, (int)TopLeft.Y - 16 * 4, 16, 16 * 5);
			var rightBox = new Rectangle((int)TopLeft.X + 16 * 5 - 8, (int)TopLeft.Y - 16 * 4, 16, 16 * 5);
			var bottomBox = new Rectangle((int)TopLeft.X, (int)TopLeft.Y + 8, 16 * 5, 16);
		}
	}

	class CagePuzzleSystem : ModSystem
	{
		public static bool solved = false;

		public override void SaveWorldData(TagCompound tag)
		{
			tag.Add("solved", solved);
		}

		public override void LoadWorldData(TagCompound tag)
		{
			solved = tag.GetBool("solved");
		}
	}
}

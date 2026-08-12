using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.BlockerTileSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
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
		readonly List<int> attempts = new();

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

			Lighting.AddLight(Center, new Vector3(0.1f, 0.2f, 0.2f));

			if (attempts.Count >= 3)
			{
				for (int k = 0; k < attempts.Count; k++)
				{
					if (attempts[k] != solution[k])
					{
						SoundHelper.PlayPitched("Effects/Chirp1", 1, -0.25f, Center);
						attempts.Clear();
						break;
					}

					if (k == 2)
					{
						CagePuzzleSystem.solved = true;
						Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);

						for (int n = 0; n < 40; n++)
						{
							Dust.NewDust(position + new Vector2(0, -48), 64, 64, DustID.Gold);
						}

						attempts.Clear();
					}
				}
			}

			Vector2 topleft = position + new Vector2(0, -16 * 4);

			var leftBox = new Rectangle((int)TopLeft.X - 8, (int)TopLeft.Y - 16 * 4, 16, 16 * 5);
			var rightBox = new Rectangle((int)TopLeft.X + 16 * 5 - 8, (int)TopLeft.Y - 16 * 4, 16, 16 * 5);
			var bottomBox = new Rectangle((int)TopLeft.X, (int)TopLeft.Y + 8, 16 * 5, 16);

			foreach (Player player in Main.player.Where(n => n.active))
			{
				if (AbilityHelper.CheckDash(player, leftBox) && !attempts.Contains(1))
				{
					player.GetHandler().ActiveAbility?.Deactivate();
					player.velocity = Vector2.Normalize(player.velocity) * -10f;
					attempts.Add(1);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);
				}
				else if (AbilityHelper.CheckDash(player, rightBox) && !attempts.Contains(2))
				{
					player.GetHandler().ActiveAbility?.Deactivate();
					player.velocity = Vector2.Normalize(player.velocity) * -10f;
					attempts.Add(2);
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);
				}
				else if (AbilityHelper.CheckDash(player, bottomBox) && !attempts.Contains(3))
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
			if (!CagePuzzleSystem.solved)
			{
				Texture2D gem = Assets.Tiles.Dungeon.ShardCageGem.Value;
				Texture2D glow = Assets.Tiles.Dungeon.ShardCageGemGlow.Value;

				Vector2 center = position + new Vector2(40, -24);

				Vector2 left = center + new Vector2(-42, 0);
				Vector2 bottom = center + new Vector2(0, 42);
				Vector2 right = center + new Vector2(42, 0);

				if (!attempts.Contains(1))
				{
					Main.spriteBatch.Draw(gem, left - Main.screenPosition, null, lightColor, 0, gem.Size() / 2f, 1, 0, 0);
					Main.spriteBatch.Draw(glow, left - Main.screenPosition, null, CommonVisualEffects.IndicatorColorProximity(256, 512, center), 0, glow.Size() / 2f, 1, 0, 0);
				}

				if (!attempts.Contains(3))
				{
					Main.spriteBatch.Draw(gem, bottom - Main.screenPosition, null, lightColor, 0, gem.Size() / 2f, 1, 0, 0);
					Main.spriteBatch.Draw(glow, bottom - Main.screenPosition, null, CommonVisualEffects.IndicatorColorProximity(256, 512, center), 0, glow.Size() / 2f, 1, 0, 0);
				}

				if (!attempts.Contains(2))
				{
					Main.spriteBatch.Draw(gem, right - Main.screenPosition, null, lightColor, 0, gem.Size() / 2f, 1, 0, 0);
					Main.spriteBatch.Draw(glow, right - Main.screenPosition, null, CommonVisualEffects.IndicatorColorProximity(256, 512, center), 0, glow.Size() / 2f, 1, 0, 0);
				}
			}
		}

		public override void DrawOverPlayer()
		{
			Texture2D tex = Assets.Tiles.Dungeon.ShardCage.Value;

			if (CagePuzzleSystem.solved)
				tex = Assets.Tiles.Dungeon.ShardCageOpen.Value;

			Vector2 topleft = position + new Vector2(-2, -16 * 4 - 2);

			Core.Systems.LightingSystem.LightingBufferRenderer.DrawWithLighting(tex, topleft - Main.screenPosition, Color.White);
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
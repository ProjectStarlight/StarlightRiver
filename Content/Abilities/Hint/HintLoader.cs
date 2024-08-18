using Humanizer;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Terraria.ID;

namespace StarlightRiver.Content.Abilities.Hint
{
	internal class HintLoader : ModSystem
	{
		public static Hints hints;

		private string lastLocale;

		public override void Load()
		{
			LoadFromFile();
		}

		public override void PostUpdateEverything()
		{
			// check if we need to and reload if locale changes
			string activeExtension = LanguageManager.Instance.ActiveCulture.Name;

			if (activeExtension != lastLocale)
				LoadFromFile();
		}

		public void LoadFromFile()
		{
			lastLocale = LanguageManager.Instance.ActiveCulture.Name;

			string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
			string path = Path.Combine("Localization", "Hints", activeExtension + ".json");

			// Fall back to english if no file exists
			if (!StarlightRiver.Instance.FileExists(path))
				path = Path.Combine("Localization", "Hints", "en-US.json");

			// Throw if we cant find english
			if (!StarlightRiver.Instance.FileExists(path))
				throw new FileNotFoundException("Could not find any hint file!");

			Stream stream = StarlightRiver.Instance.GetFileStream(path);

			hints = JsonSerializer.Deserialize<Hints>(stream);
			stream.Close();
		}

		public static string? GetNPCEntry(NPC npc)
		{
			string key;

			if (npc.ModNPC is ICustomHintable hintable)
			{
				key = hintable.GetCustomKey();
			}
			else
			{
				if (npc.ModNPC is null)
					key = $"Terraria/{NPCID.Search.GetName(npc.type)}";
				else
					key = $"{npc.ModNPC.Mod.Name}/{npc.ModNPC.Name}";
			}

			if (hints.Npc.ContainsKey(key))
				return hints.Npc[key];

			return hints.Npc["Default"].FormatWith(npc.FullName);
		}

		public static string? GetProjectileEntry(Projectile proj)
		{
			string key;

			if (proj.ModProjectile is ICustomHintable hintable)
			{
				key = hintable.GetCustomKey();
			}
			else
			{
				if (proj.ModProjectile is null)
					key = $"Terraria/{ProjectileID.Search.GetName(proj.type)}";
				else
					key = $"{proj.ModProjectile.Mod.Name}/{proj.ModProjectile.Name}";
			}

			if (hints.Projectile.ContainsKey(key))
				return hints.Projectile[key];

			return null;
		}

		public static string? GetTileEntry(Tile tile)
		{
			string key;
			ModTile modTile = ModContent.GetModTile(tile.TileType);

			if (modTile is ICustomHintable hintable)
			{
				key = hintable.GetCustomKey();
			}
			else
			{
				if (modTile is null)
					key = $"Terraria/{TileID.Search.GetName(tile.TileType)}";
				else
					key = $"{modTile.Mod.Name}/{modTile.Name}";
			}

			if (hints.Tile.ContainsKey(key))
				return hints.Tile[key];

			if (tile.HasTile && Main.tileSolid[tile.TileType])
				return hints.Tile["Default"].FormatWith(ProcessName(TileID.Search.GetName(tile.TileType)));

			return null;
		}

		private static string ProcessName(string input)
		{
			input = Regex.Replace(input, "(.*)/(.*)", "$2");
			input = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
			return input;
		}
	}

	public class Hints
	{
		public Dictionary<string, string> Npc { get; set; }
		public Dictionary<string, string> Projectile { get; set; }
		public Dictionary<string, string> Tile { get; set; }
		public Dictionary<string, string> AirHints { get; set; }
	}
}
using StarlightRiver.Content.GUI;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core
{
	class MedalPlayer : ModPlayer
	{
		public List<Medal> medals = new();
		public List<DeathCounter> deathCounters = new();

		private Medal attemptedMedal;
		private DeathCounter activeCounter;

		private static int Difficulty => Main.masterMode ? 2 : Main.expertMode ? 1 : Main.LocalPlayer.difficulty == 3 ? -1 : 0;

		public void QualifyForMedal(Medal medal)
		{
			//Main.NewText("Difficulty for current fight is:" + Difficulty);
			attemptedMedal = medal;

			if (!deathCounters.Any(n => n.name == medal.name))
				deathCounters.Add(new DeathCounter(medal.name, 0));

			activeCounter = deathCounters.FirstOrDefault(n => n.name == medal.name);
		}

		public void QualifyForMedal(string name, float order)
		{
			var medal = new Medal(name, Difficulty, order);
			QualifyForMedal(medal);
		}

		public void ProbeMedal(string name)
		{
			var medal = new Medal(name, Difficulty, attemptedMedal.order);

			if (attemptedMedal.Equals(medal) && !medals.Any(n => n.name == medal.name && n.difficulty >= medal.difficulty))
			{
				medals.RemoveAll(n => n.name == medal.name && n.difficulty < medal.difficulty);
				medals.Add(medal);
				medals.Sort((a, b) => a.order > b.order ? 1 : 0);
				attemptedMedal = default;

				Main.NewText("Medal earned!", new Color(255, 255, 100));
			}

			activeCounter = null;
		}

		public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter)
		{
			if (!pvp)
				attemptedMedal = default;
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			if (activeCounter != null && Main.masterMode) //death counters are for master only
			{
				activeCounter.deaths++;
				MasterDeathTicker.ShowDeathCounter(activeCounter.name, activeCounter.deaths);
			}

			activeCounter = null;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["medals"] = medals;
			tag["deathCounters"] = deathCounters;
		}

		public override void LoadData(TagCompound tag)
		{
			this.medals.Clear();
			deathCounters.Clear();

			var medals = new List<Medal>();
			IList<TagCompound> loadedMedals = tag.GetList<TagCompound>("medals");

			foreach (TagCompound c in loadedMedals)
			{
				medals.Add(Medal.Deserialize(c));
			}

			this.medals = medals;

			var deaths = new List<DeathCounter>();
			IList<TagCompound> loadedDeaths = tag.GetList<TagCompound>("deathCounters");

			foreach (TagCompound c in loadedDeaths)
			{
				deaths.Add(DeathCounter.Deserialize(c));
			}

			deathCounters = deaths;
		}

		public Texture2D GetMedalTexture(string name)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Medals/" + name).Value;

			if (tex is null)
				return ModContent.Request<Texture2D>("StarlightRiver/Assets/Medals/Cheater").Value;
			else
				return tex;
		}

		public int GetDeaths(string name)
		{
			if (deathCounters.Any(n => n.name == name))
				return deathCounters.FirstOrDefault(n => n.name == name).deaths;

			return -1;
		}
	}

	class DeathCounter : TagSerializable
	{
		public string name;
		public int deaths;

		public DeathCounter(string name, int deaths)
		{
			this.name = name;
			this.deaths = deaths;
		}

		public TagCompound SerializeData()
		{
			return new TagCompound()
			{
				["name"] = name,
				["deaths"] = deaths
			};
		}

		public static DeathCounter Deserialize(TagCompound tag)
		{
			return new DeathCounter(tag.GetString("name"), tag.GetInt("deaths"));
		}
	}

	struct Medal : TagSerializable
	{
		public string name;
		public int difficulty;
		public float order;

		public Medal(string name, int difficulty, float order)
		{
			this.name = name;
			this.difficulty = difficulty;
			this.order = order;
		}

		public override string ToString()
		{
			return name + ": " + (difficulty == 0 ? "Normal" : difficulty == 1 ? "Expert" : difficulty == -1 ? "Journey" : "Master");
		}

		public override bool Equals(object obj)
		{
			var med = (Medal)obj;
			return obj is Medal && med.name == name && med.difficulty == difficulty;
		}

		public TagCompound SerializeData()
		{
			return new TagCompound()
			{
				["name"] = name,
				["difficulty"] = difficulty,
				["order"] = order
			};
		}

		public static Medal Deserialize(TagCompound tag)
		{
			return new Medal(tag.GetString("name"), tag.GetInt("difficulty"), tag.GetFloat("order"));
		}
	}
}

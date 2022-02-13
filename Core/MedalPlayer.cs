using Microsoft.Xna.Framework;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Items.Armor;
using StarlightRiver.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Core
{
	class MedalPlayer : ModPlayer
	{
		public List<Medal> medals = new List<Medal>();

		private Medal attemptedMedal;

		private static int Difficulty => Main.expertMode ? 1 : 0;

		public void QualifyForMedal(Medal medal)
		{
			attemptedMedal = medal;
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
		}

		public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
			if (!pvp)
				attemptedMedal = default;
		}

		public override TagCompound Save()
		{
			return new TagCompound()
			{ 
				["medals"] = medals
			};
		}

		public override void Load(TagCompound tag)
		{
			medals.Clear();

			var list = new List<Medal>();

			var list2 = tag.GetList<TagCompound>("medals");

			foreach (TagCompound c in list2)
				list.Add(Medal.Deserialize(c));

			medals = list;
		}

		public Texture2D GetMedalTexture(string name)
		{
			var tex = ModContent.GetTexture("StarlightRiver/Assets/Medals/" + name);

			if (tex is null)
				return ModContent.GetTexture("StarlightRiver/Assets/Medals/Cheater");
			else return tex;
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
			return name + ": " + (difficulty == 0 ? "Normal" : difficulty == 1 ? "Expert" : "Master");
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

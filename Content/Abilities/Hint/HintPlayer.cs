using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Abilities.Hint
{
	internal class HintPlayer : ModPlayer
	{
		public static readonly Dictionary<string, float> sequences = new()
		{
			{ "Default", -1},
			{ "PreGlassweaver", 0},
			{ "PreWinds", 1},
			{ "PreCeiros", 2},
			{ "PostCeiros", 3},
			{ "PostGlassweaverMove", 4},
		};

		/// <summary>
		/// The key which indicates which air hint the player should get when they use starsight on 'nothing'. See 
		/// Localization/Hints for valid values.
		/// </summary>
		public string AirHintState { get; private set; } = "PreGlassweaver";

		/// <summary>
		/// Attempts to set the hint state. Prevents backtracking the sequence.
		/// </summary>
		/// <param name="state">The state key to try to assaign</param>
		public void SetHintState(string state)
		{
			float proposed = sequences[state];
			float current = sequences[AirHintState];

			if (proposed >= current)
				AirHintState = state;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["AirHintState"] = AirHintState;
		}

		public override void LoadData(TagCompound tag)
		{
			AirHintState = tag.GetString("AirHintState");
		}
	}
}
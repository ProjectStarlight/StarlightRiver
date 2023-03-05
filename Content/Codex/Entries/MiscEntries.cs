using StarlightRiver.Content.Abilities.ForbiddenWinds;
using static Terraria.ModLoader.ModContent;
using Terraria.Localization;

namespace StarlightRiver.Content.Codex.Entries
{
	internal class StaminaEntry : CodexEntry
	{
		public StaminaEntry()
		{
			Category = Categories.Misc;
			Title = textTool.MiscText("StaminaEntry.Title");
			Body = textTool.MiscText("StaminaEntry.Body");
			Hint = textTool.MiscText("StaminaEntry.Hint");
			Image = Request<Texture2D>("StarlightRiver/Assets/GUI/Stamina").Value;
			Icon = Request<Texture2D>("StarlightRiver/Assets/GUI/Stamina").Value;
		}
	}

	internal class StaminaShardEntry : CodexEntry
	{
		public StaminaShardEntry()
		{
			Category = Categories.Misc;
			Title = textTool.MiscText("StaminaShardEntry.Title");
			Body = textTool.MiscText("StaminaShardEntry.Body");
			Hint = textTool.MiscText("StaminaShardEntry.Hint");
			Image = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaEmpty").Value;
			Icon = Request<Texture2D>("StarlightRiver/Assets/Abilities/Stamina1").Value;
		}
	}

	internal class InfusionEntry : CodexEntry
	{
		public InfusionEntry()
		{
			Category = Categories.Misc;
			Title = textTool.MiscText("InfusionEntry.Title");
			Body = textTool.MiscText("InfusionEntry.Body");
			Hint = textTool.MiscText("InfusionEntry.Hint");
			Image = Request<Texture2D>(GetInstance<Astral>().Texture).Value;
			Icon = Image;
		}
	}

	internal class BarrierEntry : CodexEntry
	{
		public BarrierEntry()
		{
			Category = Categories.Misc;
			Title = textTool.MiscText("BarrierEntry.Title");
			Body = textTool.MiscText("BarrierEntry.Body");
			Hint = textTool.MiscText("BarrierEntry.Hint");
			Image = Request<Texture2D>(AssetDirectory.GUI + "ShieldHeartOver").Value;
			Icon = Image;
		}
	}
}
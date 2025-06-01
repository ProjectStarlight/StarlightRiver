using System;
using System.Reflection;

namespace StarlightRiver
{
	internal class LocalizationRewriter : ModSystem
	{
		public override void PostSetupContent()
		{
#if DEBUG
			LocalizationLoader.UpdateLocalizationFilesForMod(Mod, null, Language.ActiveCulture);
#endif
		}
	}

	internal static class LocalizationRoundabout
	{
		public static void SetDefault(this LocalizedText text, string value)
		{
#if DEBUG
			PropertyInfo valueProp = typeof(LocalizedText).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);

			LanguageManager.Instance.GetOrRegister(text.Key, () => value);
			valueProp.SetValue(text, value);
#endif
		}

		public static LocalizedText DefaultText(string key, string english)
		{
			LocalizedText text = Language.GetOrRegister($"Mods.StarlightRiver.{key}", () => english);
			text.SetDefault(english);

			return text;
		}
	}
}
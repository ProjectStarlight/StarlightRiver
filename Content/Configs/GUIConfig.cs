using StarlightRiver.Content.GUI.Config;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace StarlightRiver.Content.Configs
{
	public enum OverlayState
	{
		AlwaysOn = 0,
		WhileNotFull = 1,
		WhileUsing = 2,
		Never = 3
	}

	public enum KeywordStyle
	{
		Colors = 0,
		Brackets = 1,
		Both = 2,
		Neither = 3
	}

	public class GUIConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DrawTicks]
		[DefaultValue(typeof(OverlayState), "WhileNotFull")]
		public OverlayState OverheadStaminaState = OverlayState.WhileNotFull;

		[DrawTicks]
		public KeywordStyle KeywordStyle = KeywordStyle.Both;

		[DefaultValue(false)]
		public bool ObviousArmorCharge;

		[DefaultValue(true)]
		public bool AddMaterialIndicator;

		[DefaultValue(typeof(Vector2), "100, 300")]
		[CustomModConfigItem(typeof(AbilityUIReposition))]
		public Vector2 AbilityIconPosition;
	}
}
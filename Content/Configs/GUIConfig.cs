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

		static readonly Vector2 defaultAbilityIconPosition = new(100, 300);

		[Label("Overhead Starlight Display")]
		[DrawTicks]
		[Tooltip("When/If the overhead starlight meter should display")]
		[DefaultValue(typeof(OverlayState), "WhileNotFull")]
		public OverlayState OverheadStaminaState = OverlayState.WhileNotFull;

		[Label("Keyword Style")]
		[DrawTicks]
		[Tooltip("How keywords should be displayed in tooltips")]
		public KeywordStyle KeywordStyle = KeywordStyle.Both;

		[Label("Numeric Set Bonus Indicators")]
		[Tooltip("Adds a numeric display to various armor set bonuses over the player")]
		[DefaultValue(false)]
		public bool ObviousArmorCharge;

		[Label("Indicate Items with Added Recipes")]
		[Tooltip("Displays a star icon in vanilla item tooltips if used in Starlight River crafting recipes")]
		[DefaultValue(true)]
		public bool AddMaterialIndicator;

		[Label("Ability icon positon")]
		[Tooltip("Where the ability icons should be shown while the inventory is open")]
		[DefaultValue(typeof(Vector2), "100, 300")]
		[CustomModConfigItem(typeof(AbilityUIReposition))]
		public Vector2 AbilityIconPosition;
	}
}
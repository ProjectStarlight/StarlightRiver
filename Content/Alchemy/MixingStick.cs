using StarlightRiver.Core.Systems;
using Terraria.ID;

namespace StarlightRiver.Content.Alchemy
{
	[SLRDebug]
	public class MixingStick : ModItem
	{
		public override string Texture => AssetDirectory.Alchemy + Name;

		public override bool AltFunctionUse(Player Player)
		{
			return true;
		}

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Mixing Stick\nUse this to finalize alchemy recipes to craft them.");
		}

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.consumable = true;
			Item.value = 500;
		}
	}
}
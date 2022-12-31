using Terraria.ID;

namespace StarlightRiver.Content.Items.Brewing
{
	public abstract class QuickPotion : ModItem
	{
		private readonly string ItemName;
		private readonly string ItemTooltip;
		private readonly int Time;
		private readonly int BuffID;
		private readonly int Rare;

		protected QuickPotion(string name, string tooltip, int time, int buffID, int rare = 1)
		{
			ItemName = name;
			ItemTooltip = tooltip;
			Time = time;
			BuffID = buffID;
			Rare = rare;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(ItemName);
			Tooltip.SetDefault(ItemTooltip);
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 28;
			Item.useStyle = ItemUseStyleID.EatFood;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item3;
			Item.maxStack = 30;
			Item.consumable = true;
			Item.rare = Rare;
			Item.value = Item.buyPrice(gold: 1);
			Item.buffType = BuffID;
			Item.buffTime = Time;
		}
	}
}
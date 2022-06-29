using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Herbology.Potions
{
	public class Bottle : ModItem
    {
        public override string Texture => "StarlightRiver/Assets/Items/Brewing/Bottle";

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("A bottle made of thick vitric glass");
            DisplayName.SetDefault("Vitric Bottle");
        }

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 250;
            Item.rare = ItemRarityID.Green;
        }
    }

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
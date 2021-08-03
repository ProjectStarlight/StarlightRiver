using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.BaseTypes
{
	public abstract class SmartAccessory : ModItem
    {
        private readonly string ThisName;
        private readonly string ThisTooltip;

        protected SmartAccessory(string name, string tooltip) : base()
        {
            ThisName = name;
            ThisTooltip = tooltip;
        }

        public bool Equipped(Player player)
        {
            for (int k = 3; k <= 7 + player.extraAccessorySlots; k++)
                if (player.armor[k].type == item.type)
                    return true;

            return false;
        }

        public SmartAccessory GetEquippedInstance(Player player)
		{
            for (int k = 3; k <= 7 + player.extraAccessorySlots; k++)
                if (player.armor[k].type == item.type)
                    return player.armor[k].modItem as SmartAccessory;

            return null;
        }

        public virtual void SafeSetDefaults() { }

        public virtual void SafeUpdateEquip(Player player) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(ThisName);
            Tooltip.SetDefault(ThisTooltip);
        }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();
            item.width = 32;
            item.height = 32;
            item.accessory = true;
        }

        public sealed override void UpdateEquip(Player player)
        {
            SafeUpdateEquip(player);
        }
    }
}

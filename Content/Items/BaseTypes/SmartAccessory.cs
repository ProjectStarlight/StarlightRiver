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

        public bool Equipped(Player Player)
        {
            for (int k = 3; k <= 7 + Player.extraAccessorySlots; k++)
                if (Player.armor[k].type == Item.type)
                    return true;

            return false;
        }

        public SmartAccessory GetEquippedInstance(Player Player)
		{
            for (int k = 3; k <= 7 + Player.extraAccessorySlots; k++)
                if (Player.armor[k].type == Item.type)
                    return Player.armor[k].ModItem as SmartAccessory;

            return null;
        }

        public virtual void SafeSetDefaults() { }

        public virtual void SafeUpdateEquip(Player Player) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(ThisName);
            Tooltip.SetDefault(ThisTooltip);
        }

        public sealed override void SetDefaults()
        {
            SafeSetDefaults();
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
        }

        public sealed override void UpdateEquip(Player Player)
        {
            SafeUpdateEquip(Player);
        }
    }
}

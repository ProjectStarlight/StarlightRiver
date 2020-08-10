using StarlightRiver.Abilities;
using Terraria;

namespace StarlightRiver.Items.Infusions
{
    public class WispHomingItem : InfusionItem
    {
        public WispHomingItem() : base(3)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Feral Wisp");
            Tooltip.SetDefault("Faeflame Infusion\nRelease homing bolts that lower enemie's damage");
        }

        public override void UpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            if (!(mp.wisp is WispHoming) && !(mp.wisp is WispCombo))
            {
                if (mp.wisp is WispWIP)
                {
                    mp.wisp = new WispCombo(player)
                    {
                        Locked = false
                    };
                }
                else
                {
                    mp.wisp = new WispHoming(player)
                    {
                        Locked = false
                    };
                }
            }
        }

        public override bool CanEquipAccessory(Player player, int slot)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            return !mp.wisp.Locked;
        }

        public override void Unequip(Player player)
        {
            player.GetModPlayer<AbilityHandler>().wisp = new Abilities.Wisp(player)
            {
                Locked = false
            };
        }
    }
}
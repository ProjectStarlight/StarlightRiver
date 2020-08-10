using Terraria;

namespace StarlightRiver.Abilities
{
    public partial class AbilityHandler
    {
        public Item slot1;
        public Item slot2;
        public bool HasSecondSlot;

        public override void PostUpdate()
        {
            if (slot1 != null) slot1.modItem.UpdateEquip(player);
            if (slot2 != null) slot2.modItem.UpdateEquip(player);
        }
    }
}
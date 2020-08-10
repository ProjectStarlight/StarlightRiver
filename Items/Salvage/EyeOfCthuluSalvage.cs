using Terraria.ID;

namespace StarlightRiver.Items.Salvage
{
    public class EyeOfCthuluSalvage : SalvageItem
    {
        public EyeOfCthuluSalvage() : base(new Randstat(10, 40), new Randstat(10, 20), new Randstat(4, 10))
        {
        }

        public override void SetDefaults()
        {
            item.melee = true;
            item.useStyle = ItemUseStyleID.SwingThrow;
            RollStats();
            SetStats();
        }
    }
}
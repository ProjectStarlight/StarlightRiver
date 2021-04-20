using System.Linq;
using Terraria;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.Food;

namespace StarlightRiver.Content.Buffs
{
    public class FoodBuff : SmartBuff
    {
        public FoodBuff() : base("Nourished", "Nourised by rich food, granting:\n", false) { }

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            FoodBuffHandler mp = Main.LocalPlayer.GetModPlayer<FoodBuffHandler>();
            foreach (Item item in mp.Consumed.Where(n => n.modItem is Ingredient))
            {
                tip += (item.modItem as Ingredient).ItemTooltip + "\n";
            }
        }

        public override void Update(Player player, ref int buffIndex)
        {
            FoodBuffHandler mp = player.GetModPlayer<FoodBuffHandler>();
            foreach (Item item in mp.Consumed.Where(n => n.modItem is Ingredient))
            {
                (item.modItem as Ingredient).BuffEffects(player, mp.Multiplier);
            }
        }
    }
    public class Full : SmartBuff { public Full() : base("Stuffed", "Cannot consume any more rich food", true) { } }
}
using StarlightRiver.Content.Items.Food;
using StarlightRiver.Core;
using System.Linq;
using Terraria;

namespace StarlightRiver.Content.Buffs
{
	public class FoodBuff : SmartBuff
    {
        public FoodBuff() : base("Nourished", "Nourised by rich food, granting:\n", false) { }

        public override string Texture => AssetDirectory.Buffs + "FoodBuff";

        public override void ModifyBuffTip(ref string tip, ref int rare)
        {
            FoodBuffHandler mp = Main.LocalPlayer.GetModPlayer<FoodBuffHandler>();
            foreach (Item Item in mp.Consumed.Where(n => n.ModItem is Ingredient))
            {
                tip += (Item.ModItem as Ingredient).ItemTooltip + "\n";
            }
        }

        public override void Update(Player Player, ref int buffIndex)
        {
            FoodBuffHandler mp = Player.GetModPlayer<FoodBuffHandler>();
            foreach (Item Item in mp.Consumed.Where(n => n.ModItem is Ingredient))
            {
                (Item.ModItem as Ingredient).BuffEffects(Player, mp.Multiplier);
            }
        }
    }
    public class Full : SmartBuff 
    { 
        public Full() : base("Stuffed", "Cannot consume any more rich food", true) { }

        public override string Texture => AssetDirectory.Buffs + "Full";
    }
}
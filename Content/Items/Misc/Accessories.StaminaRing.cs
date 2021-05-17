using Terraria;
using Terraria.ID;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Abilities;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Items.Overgrow;

namespace StarlightRiver.Content.Items.Misc
{
    public class StaminaRing : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public StaminaRing() : base("Band of Endurance", "Increased max stamina and stamina regeneration") { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemType<StaminaUp>());
            recipe.AddIngredient(ItemType<MossSalve>());
            recipe.AddTile(TileID.TinkerersWorkbench);

            recipe.SetResult(ItemType<StaminaRing>());

            recipe.AddRecipe();
        }
        public override void SafeUpdateEquip(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.StaminaMaxBonus += 1;
        }
    }
}

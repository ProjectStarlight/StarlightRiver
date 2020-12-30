using StarlightRiver.Core;
using StarlightRiver.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Items;

namespace StarlightRiver.Content.Items.Misc
{
    public class BarbedKnife : SmartAccessory, IChestItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public BarbedKnife() : base("Barbed Knife", "Critical hits apply a stacking bleeding debuff\nStacks up to 5 at once, additional Critical hits refresh all stacks") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
            StarlightPlayer.OnHitNPCEvent += OnHitNPC;
            return true;
        }

        private void OnHitNPCAccessory(Player player, NPC target, int damage, float knockback, bool crit)
        {
            if (Equipped(player) && crit)
                BleedStack.ApplyBleedStack(target, 300, true);
        }

        private void OnHitNPCWithProjAccessory(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit) => OnHitNPCAccessory(player, target, damage, knockback, crit);
        private void OnHitNPC(Player player, Item item, NPC target, int damage, float knockback, bool crit) => OnHitNPCAccessory(player, target, damage, knockback, crit);

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.ShadowScale, 5);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();

            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.TissueSample, 5);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public int ItemStack(Chest chest) => 1;
        public bool GenerateCondition(Chest chest)
        {
            if (Main.tile[chest.x, chest.y].frameX == 0)//Wooden
                return true;

            return false;
        }

    }
}
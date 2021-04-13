using StarlightRiver.Core;
using StarlightRiver.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.WorldGeneration;

namespace StarlightRiver.Content.Items.Misc
{
    public class BarbedKnife : SmartAccessory, IChestItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public int Stack => 1;

        public ChestRegionFlags Regions => ChestRegionFlags.Surface;

        public BarbedKnife() : base("Barbed Knife", "Critical hits apply a bleeding debuff that stacks up to 5 times") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
            StarlightPlayer.OnHitNPCEvent += OnHitNPC;

            return true;
        }

        private void OnHit(Player player, NPC target, bool crit)
        {
            if (Equipped(player) && crit)
            {
                BleedStack.ApplyBleedStack(target, 300, true);
            }
        }

        private void OnHitNPCWithProjAccessory(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit) 
            => OnHit(player, target, crit);

        private void OnHitNPC(Player player, Item item, NPC target, int damage, float knockback, bool crit) 
            => OnHit(player, target, crit);

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
    }
}
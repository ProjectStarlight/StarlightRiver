using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Ammo
{
    public class VitricArrow : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
            Tooltip.SetDefault("Shatters on impact");
        }

        public override void SetDefaults()
        {
            item.damage = 8;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;
            item.knockBack = 0.5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ProjectileType<Projectiles.Ammo.VitricArrow>();
            item.shootSpeed = 1f;
            item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<Vitric.VitricGem>(), 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 50);
            recipe.AddRecipe();
        }
    }
}
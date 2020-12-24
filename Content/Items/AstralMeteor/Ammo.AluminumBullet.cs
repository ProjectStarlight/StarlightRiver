using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.AstralMeteor
{
    public class AluminumBullet : ModItem
    {
        public override string Texture => Directory.AluminumItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Tracer");
            Tooltip.SetDefault("Weakly homes in on foes");
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
            item.value = 1;
            item.rare = ItemRarityID.Blue;
            //item.shoot = ProjectileType<Projectiles.AluminumBulletProjectile>(); TODO: Reimplement
            item.shootSpeed = 0.01f;
            item.ammo = AmmoID.Bullet;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.MusketBall, 70);
            recipe.AddIngredient(ItemType<AluminumBar>(), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this, 70);
            recipe.AddRecipe();
        }
    }
}
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.AstralMeteor
{
    class StarSniper : ModItem
    {
        public override string Texture => AssetDirectory.AluminumItem + "StarSniper";

        public override void SetDefaults()
        {
            item.damage = 140;
            item.useTime = 60;
            item.useAnimation = 60;
            item.useAmmo = AmmoID.FallenStar;
            item.ranged = true;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.crit = 20;
            item.shoot = ProjectileType<StarSniperBolt>();
            item.shootSpeed = 5;
            item.knockBack = 20;
            item.UseSound = SoundID.Item40;
            item.rare = ItemRarityID.Blue;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Boomstick);
            recipe.AddIngredient(ItemType<AluminumBar>(), 20);
            recipe.AddIngredient(ItemID.FallenStar, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    class StarSniperBolt : ModProjectile
    {
        public override string Texture => AssetDirectory.AluminumItem + "StarSniperBolt";

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.width = 8;
            projectile.height = 8;
            projectile.extraUpdates = 20;
            projectile.timeLeft = 600;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + 1.57f;
            Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Stamina>(), projectile.velocity * -Main.rand.NextFloat(), 0, default, 2);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileType<StarSniperAura>(), 20, 0, projectile.owner);
        }
    }

    class StarSniperAura : ModProjectile
    {
        public override string Texture => AssetDirectory.AluminumItem + "StarSniperAura";

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.width = 64;
            projectile.height = 64;
            projectile.timeLeft = 60;
            projectile.penetrate = -1;
        }

        public override void AI() => projectile.rotation += 0.1f;

        public override Color? GetAlpha(Color lightColor) => Color.White * (projectile.timeLeft / 60f);
    }
}

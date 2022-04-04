using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles.AstralMeteor;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.AstralMeteor
{
	class StarSniper : ModItem
    {
        public override void SetStaticDefaults() => Tooltip.SetDefault("Scrapped Item");

        public override string Texture => AssetDirectory.AluminumItem + "StarSniper";

        public override void SetDefaults()
        {
            Item.damage = 140;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useAmmo = AmmoID.FallenStar;
            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.crit = 20;
            Item.shoot = ProjectileType<StarSniperBolt>();
            Item.shootSpeed = 5;
            Item.knockBack = 20;
            Item.UseSound = SoundID.Item40;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 10, 0, 0);
            Item.noMelee = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.Boomstick);
            recipe.AddIngredient(ItemType<AluminumBarItem>(), 20);
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
            Projectile.friendly = true;
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.extraUpdates = 20;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
            Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Stamina>(), Projectile.velocity * -Main.rand.NextFloat(), 0, default, 2);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileType<StarSniperAura>(), 20, 0, Projectile.owner);
        }
    }

    class StarSniperAura : ModProjectile
    {
        public override string Texture => AssetDirectory.AluminumItem + "StarSniperAura";

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.timeLeft = 60;
            Projectile.penetrate = -1;
        }

        public override void AI() => Projectile.rotation += 0.1f;

        public override Color? GetAlpha(Color lightColor) => Color.White * (Projectile.timeLeft / 60f);
    }
}

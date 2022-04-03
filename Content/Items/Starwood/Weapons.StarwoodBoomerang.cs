using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Starwood
{
	public class StarwoodBoomerang : StarwoodItem
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;
        public StarwoodBoomerang() : base(ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodBoomerang_Alt").Value) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Boomerang");
            Tooltip.SetDefault("Hold LMB to channel the boomerang, causing it to release an explosion");
        }

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.melee = true;
            Item.width = 18;
            Item.height = 34;
            Item.useTime = 10;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.shootSpeed = 10f;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item19;
            Item.shoot = ModContent.ProjectileType<StarwoodBoomerangProjectile>();
            Item.useAnimation = 10;
            Item.noMelee = true;
        }

        public override bool CanUseItem(Player Player)
        {
            for (int k = 0; k <= Main.maxProjectiles; k++)
                if (Main.projectile[k].active && Main.projectile[k].owner == Player.whoAmI && Main.projectile[k].type == ModContent.ProjectileType<StarwoodBoomerangProjectile>())
                    return false;
            return base.CanUseItem(Player);
        }
    }
}
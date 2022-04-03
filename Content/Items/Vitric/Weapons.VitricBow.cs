using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricBow : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Bow");
            Tooltip.SetDefault("Could use a rework");
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 34;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 28;
            Item.useTime = 28;
            Item.shootSpeed = 8f;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.knockBack = 2f;
            Item.damage = 25;
            Item.useAmmo = AmmoID.Arrow;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item5;
            Item.noMelee = true;
            Item.ranged = true;
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 aim = Vector2.Normalize(Main.MouseWorld - Player.Center);

            int proj = Projectile.NewProjectile(Player.Center, (aim * 8.5f).RotatedBy(0.1f), type, damage, knockBack, Player.whoAmI);
            Main.projectile[proj].scale = 0.5f;
            Main.projectile[proj].damage /= 2;
            Main.projectile[proj].noDropItem = true;
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
            int proj2 = Projectile.NewProjectile(Player.Center, (aim * 8.5f).RotatedBy(-0.1f), type, damage, knockBack, Player.whoAmI);
            Main.projectile[proj2].scale = 0.5f;
            Main.projectile[proj2].damage /= 2;
            Main.projectile[proj2].noDropItem = true;
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj2);
            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemType<SandstoneChunk>(), 10);
            recipe.AddIngredient(ItemType<VitricOre>(), 20);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
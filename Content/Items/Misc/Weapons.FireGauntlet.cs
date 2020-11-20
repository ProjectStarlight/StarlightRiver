using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Misc
{
    public class FireGauntlet : ModItem
    {
        public override void SetDefaults()
        {
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 24;
            item.useTime = 24;
            item.shootSpeed = 10f;
            item.knockBack = 2f;
            item.width = 60;
            item.height = 72;
            item.damage = 72;
            Item.staff[item.type] = true;
            item.rare = ItemRarityID.LightRed;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
            item.autoReuse = true;
            item.mana = 24;
            item.magic = true;
            item.shoot = ProjectileType<Flamebolt>();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fire gauntlet");
            Tooltip.SetDefault("It go like whoom whoom and you like shoot fire projectiles\nAnd like, if you hit an enemy with th efire projectile and like\nIt will give birth and like, its child will home in on like, not the thing you hit\nIf the target of the child dies it goes back to the one hit by its parent\nThat also happens like when it takes too long to find anyone");
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 muzzleOffset = Vector2.Normalize(new Vector2(speedX, speedY)) * 55f;
            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
            return true;
        }
    }
}
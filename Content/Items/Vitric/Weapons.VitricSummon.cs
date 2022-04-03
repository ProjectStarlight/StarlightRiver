using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricSummon : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.knockBack = 3f;
            Item.mana = 10;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;
            Item.summon = true;
            Item.buffType = Mod.BuffType("VitricSummonBuff");
            Item.shoot = Mod.ProjectileType("VitricSummonOrb");
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Player.AddBuff(Item.buffType, 2);
            position = Player.Center - new Vector2(Player.direction * 64, 16);

            int index = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile currentProjectile = Main.projectile[i];
                if (currentProjectile.active
                && currentProjectile.owner == Player.whoAmI
                && currentProjectile.type == type)
                {
                    if (i == currentProjectile.whoAmI)
                        index += 1;
                    index %= 4;
                }
            }

            Projectile.NewProjectile(position, Vector2.Zero, type, damage, knockBack, Player.whoAmI, index);

            return false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glassweaver's Punty");
            Tooltip.SetDefault("Summons the Glassweaver's arsenal");
        }
    }
}
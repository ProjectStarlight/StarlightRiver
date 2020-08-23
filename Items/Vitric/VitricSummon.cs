using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Vitric
{
    public class VitricSummon : ModItem
    {
		public override void SetDefaults()
		{
			item.damage = 45;
			item.knockBack = 3f;
			item.mana = 10;
			item.width = 32;
			item.height = 32;
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(0, 4, 0, 0);
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item44;

			// These below are needed for a minion weapon
			item.noMelee = true;
			item.summon = true;
			item.buffType = mod.BuffType("VitricSummonBuff");
			item.shoot = mod.ProjectileType("VitricSummonOrb");
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			player.AddBuff(item.buffType, 2);
			return true;
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glassweaver's Staff");
            Tooltip.SetDefault("Summons a glass orb");
        }
    }
}
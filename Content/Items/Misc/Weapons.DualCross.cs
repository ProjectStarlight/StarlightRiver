using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    class DualCross : ModItem
    {
		public override string Texture => AssetDirectory.MiscItem + Name;

		public int ticker;

        private int ammoToUse;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dual Cross");
			Tooltip.SetDefault("Alternates between your top 2 arrows");
		}
		public override void SetDefaults()
		{
			item.damage = 14;
			item.noMelee = true;
			item.ranged = true;
			item.width = 28;
			item.height = 42;
			item.useTime = 1;
			item.useAnimation = 20;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.shoot = ProjectileID.Shuriken;
			item.useAmmo = AmmoID.Arrow;
			item.knockBack = 1;
			item.value = Terraria.Item.sellPrice(0, 0, 10, 0);
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item5;
			item.autoReuse = true;
			item.shootSpeed = 7.8f;
			item.crit = 6;

		}

        public override bool ConsumeAmmo(Player player)
        {
            return false;
        }

        public override bool CanUseItem(Player player)
        {
            bool canShoot = false;
            bool flag1 = false;
            for (int index = 54; index < 58; ++index)
            {
                if (player.inventory[index].ammo == item.useAmmo && player.inventory[index].stack > 0)
                {
                    canShoot = true;
                    flag1 = true;
                    break;
                }
            }
            if (!flag1)
            {
                for (int index = 0; index < 54; ++index)
                {
                    if (player.inventory[index].ammo == item.useAmmo && player.inventory[index].stack > 0)
                    {
                        canShoot = true;
                        break;
                    }
                }
            }
            if (canShoot)
            {
                return true;
            }
            return false;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (ticker % 2 == 1)
            {
                Vector2 offset = Vector2.Normalize(new Vector2(speedX, speedY)).RotatedBy(-1.57f * player.direction) * 10;
                position += offset;
                player.itemTime = 19;
            }
            else
                position -= new Vector2(speedX, speedY);
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }

    public class DualCrossGlobalItem : GlobalItem
    {
        private static Item GetAmmo(int skip, Item weapon, Player player, ref bool canShoot)
        {
            Item obj = new Item();
            bool flag1 = false;
            bool canShootInner = false;

            int innerTicker = 0;

            for (int index = 54; index < 58; ++index)
            {
                if (player.inventory[index].ammo == weapon.useAmmo && player.inventory[index].stack > 0)
                {
                    obj = player.inventory[index];
                    canShootInner = true;
                    innerTicker++;

                    if (innerTicker > skip)
                    {
                        flag1 = true;
                        break;
                    }
                }
            }
            if (!flag1)
            {
                for (int index = 0; index < 54; ++index)
                {
                    if (player.inventory[index].ammo == weapon.useAmmo && player.inventory[index].stack > 0)
                    {
                        obj = player.inventory[index];
                        canShootInner = true;
                        innerTicker++;

                        if (innerTicker > skip)
                            break;
                    }
                }
            }
            if (canShootInner)
            {
                canShoot = true;
                return obj;
            }
            else
            {
                canShoot = false;
                return null;
            }
        }

        private static float GetSpeed(Player player, Item obj, Item weapon, float speed)
        {
            if (player.magicQuiver)
            {
                speed *= 1.1f;
            }
            speed += obj.shootSpeed;
            if (player.archery)
            {
                if ((double)speed < 20.0)
                {
                    speed *= 1.2f;
                    if ((double)speed > 20.0)
                        speed = 20f;
                }
            }
            return speed;
        }

        public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback)
        {
            if (weapon.type != ModContent.ItemType<DualCross>())
            {
                base.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);
                return;
            }

            var mp = weapon.modItem as DualCross;
            int ticker = mp.ticker;

            bool canShoot = false;
            Item obj = new Item();
            Item obj2 = new Item();
            obj = DualCrossGlobalItem.GetAmmo(ticker % 2, weapon, player, ref canShoot);
            obj2 = DualCrossGlobalItem.GetAmmo((ticker + 1) % 2, weapon, player, ref canShoot);

            if (!canShoot)
                return;

            mp.ticker++;

            if (obj.shoot > 0)
                type = obj.shoot;
            if (weapon.type == 3019 && type == 1)
                type = 485;

            if (type == 42)
            {
                if (obj.type == 370)
                {
                    type = 65;
                    damage += 5;
                }
                else if (obj.type == 408)
                {
                    type = 68;
                    damage += 5;
                }
                else if (obj.type == 1246)
                {
                    type = 354;
                    damage += 5;
                }
            }

            if (player.inventory[player.selectedItem].type == 2888 && type == 1)
                type = 469;
            if (player.magicQuiver && (weapon.useAmmo == AmmoID.Arrow || weapon.useAmmo == AmmoID.Stake))
            {
                knockback = (float)(int)((double)knockback * 1.1);
            }
            if (obj.ranged)
            {
                if (obj.damage > 0)
                    damage += (int)((double)obj.damage * (double)player.rangedDamage);
            }
            else
                damage += obj.damage;
            if (weapon.useAmmo == AmmoID.Arrow && player.archery)
            {
                damage = (int)((double)damage * 1.2);
            }
            knockback += obj.knockBack;

            float speed1 = GetSpeed(player, obj, weapon, speed);
            float speed2 = GetSpeed(player, obj2, weapon, speed);
            speed = (speed1 + speed2) / 2;

            bool flag2 = false;

            if (weapon.type == 3475 && Main.rand.Next(3) != 0)
                flag2 = true;
            if (weapon.type == 3540 && Main.rand.Next(3) != 0)
                flag2 = true;
            if (player.magicQuiver && weapon.useAmmo == AmmoID.Arrow && Main.rand.Next(5) == 0)
                flag2 = true;
            if (player.ammoBox && Main.rand.Next(5) == 0)
                flag2 = true;
            if (player.ammoPotion && Main.rand.Next(5) == 0)
                flag2 = true;
            if (player.ammoCost80 && Main.rand.Next(5) == 0)
                flag2 = true;
            if (player.ammoCost75 && Main.rand.Next(4) == 0)
                flag2 = true;
            if (type == 85 && player.itemAnimation < player.itemAnimationMax - 6)
                flag2 = true;
            if ((type == 145 || type == 146 || (type == 147 || type == 148) || type == 149) && player.itemAnimation < player.itemAnimationMax - 5)
                flag2 = true;
            if (flag2 || !obj.consumable)
                return;
            --obj.stack;
            if (obj.stack > 0)
                return;
            obj.active = false;
            obj.TurnToAir();
        }
    }
}
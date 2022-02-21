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
			DisplayName.SetDefault("Double Cross");
			Tooltip.SetDefault("Alternates between your highest two ammo slots");
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
            item.noUseGraphic = true;

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
                Projectile.NewProjectile(position, Vector2.Zero, ModContent.ProjectileType<DualCrossHeld>(), 0, 0, player.whoAmI);
            }
            else
                position -= new Vector2(speedX, speedY);
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }

    public class DualCrossHeld : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Dual Cross");

        Player owner => Main.player[projectile.owner];

        private bool initialized = false;

        private Vector2 currentDirection => projectile.rotation.ToRotationVector2();

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.ranged = true;
            projectile.width = 2;
            projectile.height = 2;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 999999;
            projectile.ignoreWater = true;
            projectile.alpha = 255;
            Main.projFrames[projectile.type] = 4;
        }

        public override void AI()
        {
            owner.heldProj = projectile.whoAmI;
            if (owner.itemTime <= 1)
                projectile.active = false;
            projectile.Center = owner.Center;

            if (!initialized)
            {
                initialized = true;
                projectile.rotation = projectile.DirectionTo(Main.MouseWorld).ToRotation();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Vector2 position = (owner.Center + (currentDirection * 15)) - Main.screenPosition;

            if (owner.direction == 1)
            {
                SpriteEffects effects1 = SpriteEffects.None;
                Main.spriteBatch.Draw(texture, position, null, lightColor, currentDirection.ToRotation(), texture.Size() / 2, projectile.scale, effects1, 0.0f);
            }

            else
            {
                SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
                Main.spriteBatch.Draw(texture, position, null, lightColor * .91f, currentDirection.ToRotation() - 3.14f, texture.Size() / 2, projectile.scale, effects1, 0.0f);

            }
            return false;
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

        private static int GetProjType(Item weapon, Item obj, Player player, int type)
        {
            if (obj.shoot > 0)
                type = obj.shoot;
            if (weapon.type == 3019 && type == 1)
                type = 485;

            if (type == 42)
            {
                if (obj.type == 370)
                {
                    type = 65;
                }
                else if (obj.type == 408)
                {
                    type = 68;
                }
                else if (obj.type == 1246)
                {
                    type = 354;
                }
            }

            if (player.inventory[player.selectedItem].type == 2888 && type == 1)
                type = 469;
            return type;
        }

        public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback)
        {
            if (weapon.type != ModContent.ItemType<DualCross>())
            {
                base.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);
                return;
            }

            int type2 = 0;

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

            type = GetProjType(weapon, obj, player, type);
            type2 = GetProjType(weapon, obj2, player, type2);

            Projectile proj = new Projectile();
            proj.SetDefaults(type);

            Projectile proj2 = new Projectile();
            proj2.SetDefaults(type2);

            float speed1 = GetSpeed(player, obj, weapon, speed) * (proj.extraUpdates + 1);
            float speed2 = GetSpeed(player, obj2, weapon, speed) * (proj2.extraUpdates + 1);


            speed = ((speed1 + speed2) / 2) / (proj.extraUpdates + 1);

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
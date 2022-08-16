using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.Audio;

namespace StarlightRiver.Content.Items.BaseTypes
{
    public abstract class MultiAmmoWeapon : ModItem
    {
        public virtual List<AmmoStruct> ValidAmmos { get; }

        public Item ammoItem;

        public bool hasAmmo;

        public AmmoStruct currentAmmoStruct;

        public override void Load()
        {
            StarlightPlayer.ResetEffectsEvent += ResetAmmos;
        }

        private void ResetAmmos(StarlightPlayer Player)
        {
            ammoItem = null;
            hasAmmo = false;
        }

        public virtual void SafeSetDefaults() { }
        public sealed override void SetDefaults()
        {
            SafeSetDefaults();
            Item.DamageType = DamageClass.Ranged;
            Item.useAmmo = 0;
            Item.shoot = ProjectileID.PurificationPowder;
        }

        public virtual void SafeUpdateInventory() { }
        public sealed override void UpdateInventory(Player player)
        {
            SafeUpdateInventory();

            for (int i = 0; i < player.inventory.Length; i++)
            {
                bool exit = false;

                Item item = player.inventory[i];

                for (int x = 0; x < ValidAmmos.Count; x++)
                {
                    if (ValidAmmos[x].ammoID == item.type)
                    {
                        hasAmmo = true;
                        ammoItem = item;
                        currentAmmoStruct = ValidAmmos[x];
                        exit = true;
                        break;
                    }
                }

                if (exit)
                    break;
            }
        }

        public virtual bool SafeCanUseItem(Player player) { return true; }
        public sealed override bool CanUseItem(Player player)
        {
            if (!hasAmmo)
                return false;

            SafeCanUseItem(player);

            return base.CanUseItem(player) && SafeCanUseItem(player);
        }

        public virtual void SafeModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) { }
        public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            SafeModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);


            float speed = velocity.Length();
            velocity.Normalize();
            velocity *= speed + currentAmmoStruct.ShootSpeed;

            damage += currentAmmoStruct.Damage;
            knockback += currentAmmoStruct.KnockBack;

            type = currentAmmoStruct.projectileID;
        }


        public virtual bool? SafeUseItem(Player player) { return null; }
        public sealed override bool? UseItem(Player player)
        {
            SafeUseItem(player);

            if (Item.ModItem.CanConsumeAmmo(ammoItem, player))
            {
                ammoItem.ModItem.OnConsumedAsAmmo(Item, player);
                Item.ModItem.OnConsumeAmmo(ammoItem, player);

                int type = currentAmmoStruct.projectileID; // this code sucks ass
                bool dontConsumeAmmo = false;
                if (player.magicQuiver && ammoItem.ammo == AmmoID.Arrow && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (player.ammoBox && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (player.ammoPotion && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (player.ammoCost80 && Main.rand.NextBool(5))
                    dontConsumeAmmo = true;
                if (player.ammoCost75 && Main.rand.NextBool(4))
                    dontConsumeAmmo = true;
                if (type == 85 && player.itemAnimation < player.itemAnimationMax - 6)
                    dontConsumeAmmo = true;
                if ((type == 145 || type == 146 || (type == 147 || type == 148) || type == 149) && player.itemAnimation < player.itemAnimationMax - 5)
                    dontConsumeAmmo = true;

                if (!dontConsumeAmmo)
                {
                    ammoItem.stack--;
                    if (ammoItem.stack <= 0)
                        ammoItem.TurnToAir();
                }
            }

            return base.UseItem(player);
        }

        public virtual void SafeModifyTooltips(List<TooltipLine> tooltips) { }
        public sealed override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ammoItem == null)
                return;

            TooltipLine AmmoLine = new TooltipLine(StarlightRiver.Instance, "AmmoLineToolTip", $"Current Ammo: [i:{ammoItem.type}]{ammoItem.stack}");
            var kbLine = tooltips.Find(n => n.Name == "Knockback");
            int index = kbLine is null ? tooltips.Count - 1 : tooltips.IndexOf(kbLine);
            tooltips.Insert(index + 1, AmmoLine);

            SafeModifyTooltips(tooltips);
        }

        public sealed override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.playerInventory || ammoItem == null)
                return;

            Utils.DrawBorderString(spriteBatch, $"{ammoItem.stack}", new Vector2(position.X - 2, position.Y + 8), Color.White, scale + 0.18f, 0, 0, 4);
        }
    }

    public struct AmmoStruct // contains everything needed for the ammo, ID, projectile, etc
    {
        public int Damage;
        public float ShootSpeed;
        public float KnockBack;
        
        public int projectileID;
        public int ammoID;

        public AmmoStruct(int ammoid, int projectileid, int damage = 0, float shootspeed = 0f, float knockback = 0f)
        {
            ammoID = ammoid;
            projectileID = projectileid;
            Damage = damage;
            ShootSpeed = shootspeed;
            KnockBack = knockback;
        }
    }
}

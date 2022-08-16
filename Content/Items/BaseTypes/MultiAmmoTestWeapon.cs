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
    public class MultiAmmoTestWeapon : MultiAmmoWeapon
    {
        public override string Texture => AssetDirectory.MiscItem + "CoachGun";
        public override List<AmmoStruct> ValidAmmos => new List<AmmoStruct>() {new AmmoStruct(ItemID.Hive, ProjectileID.BeeArrow, 100, 15f, 8f),
            new AmmoStruct(ItemID.CursedFlame, ProjectileID.CursedFlameFriendly)};

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("'based'");
        }

        public override void SafeSetDefaults()
        {
            Item.damage = 14;
            Item.width = 60;
            Item.height = 36;
            Item.useAnimation = Item.useTime = 55;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.rare = ItemRarityID.Blue;
            Item.shootSpeed = 16f;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.knockBack = 1f;
        }

        public override void SafeModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            damage = damage * 2;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return false;
        }
    }
}

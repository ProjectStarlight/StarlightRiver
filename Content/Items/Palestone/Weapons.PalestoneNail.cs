using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Linq;
using System;
using System.Collections.Generic;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Buffs.Summon;

namespace StarlightRiver.Content.Items.Palestone
{
    public class PalestoneNail : ModItem
    {
        public override string Texture => AssetDirectory.PalestoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palenail");
        }

        public override void SetDefaults()
        {
            item.damage = 7;
            item.knockBack = 3f;
            item.mana = 10;
            item.width = 32;
            item.height = 32;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = Item.buyPrice(0, 0, 12, 0);
            item.rare = ItemRarityID.White;
            item.UseSound = SoundID.Item44;

            item.noMelee = true;
            item.summon = true;
            item.buffType = ModContent.BuffType<PalestoneSummonBuff>();
            item.shoot = mod.ProjectileType("VitricSummonOrb");
        }
    }

    //public class PaleKnight : ModProjectile
    //{

    //}
}
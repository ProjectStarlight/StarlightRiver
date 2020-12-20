using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Palestone
{
    public class PalestoneHammer : ModItem
    {
        public override string Texture => Directory.PalestoneItemDir + "PalestoneHammer";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palecrusher");
        }

        public override void SetDefaults()
        {
            item.channel = true;
            item.damage = 12;
            item.width = 24;
            item.height = 24;
            item.useTime = 320;
            item.useAnimation = 320;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 8;
            item.useTurn = false;
            item.value = Item.sellPrice(0, 1, 42, 0);
            item.rare = 0;
            item.autoReuse = false;
            item.shoot = mod.ProjectileType("PalecrusherProj");
            item.shootSpeed = 6f;
            item.noUseGraphic = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);
    }
}
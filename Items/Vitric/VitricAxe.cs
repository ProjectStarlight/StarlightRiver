using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Vitric
{
    internal class VitricAxe : ModItem
    {
        public override void SetDefaults()
        {
            item.damage = 18;
            item.melee = true;
            item.width = 36;
            item.height = 32;
            item.useTime = 25;
            item.useAnimation = 25;
            item.axe = 20;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Axe");
            Tooltip.SetDefault("Attracts dropped item to it's user");
        }

        public override bool UseItem(Player player)
        {
            foreach (Item item in Main.item.Where(item => Vector2.Distance(item.Center, player.Center) <= 120 && item.active))
            {
                item.velocity = Vector2.Normalize(item.Center - player.Center) * -2;
                Dust.NewDust(item.Center, 4, 4, DustType<Dusts.Air>());
            }
            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.FossilOre, 10);
            recipe.AddIngredient(ItemType<VitricGem>(), 4);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
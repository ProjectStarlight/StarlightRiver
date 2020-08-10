using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Vitric
{
    internal class VitricPick : ModItem
    {
        public override void SetDefaults()
        {
            item.damage = 10;
            item.melee = true;
            item.width = 38;
            item.height = 38;
            item.useTime = 18;
            item.useAnimation = 18;
            item.pick = 85;
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
            DisplayName.SetDefault("Vitric Pickaxe");
            Tooltip.SetDefault("Attracts dropped item to its user");
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
            recipe.AddIngredient(ItemType<VitricGem>(), 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items
{
    class GrowthWand : ModItem
    {
        public override string Texture => AssetDirectory.HerbologyTile + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Growth Wand");
            Tooltip.SetDefault("Advances crop stage");
        }

        public override void SetDefaults()
        {
            item.damage = 10;
            item.melee = true;
            item.width = 38;
            item.height = 40;
            item.useTime = 18;
            item.useAnimation = 18;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 5f;
            item.value = 1000;
            item.rare = ItemRarityID.LightRed;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;
        }

        public override bool UseItem(Player player)
        {
            //Main.windSpeed += 0.8f;
            //float scale = System.Math.Min(0.03f + (Main.windSpeed * 0.07f), 0.12f);
            //Main.NewText(Main.windSpeed);
            //Main.NewText(scale);
            int x = (int)Main.MouseWorld.X / 16;
            int y = (int)Main.MouseWorld.Y / 16;

            Terraria.ModLoader.TileLoader.GetTile(Main.tile[x, y].type)?.RandomUpdate(x, y);

            return true;
        }
    }
}
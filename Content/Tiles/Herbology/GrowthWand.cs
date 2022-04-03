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
            Item.damage = 10;
            Item.melee = true;
            Item.width = 38;
            Item.height = 40;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.knockBack = 5f;
            Item.value = 1000;
            Item.rare = ItemRarityID.LightRed;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item18;
            Item.useTurn = true;
        }

        public override bool UseItem(Player Player)
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
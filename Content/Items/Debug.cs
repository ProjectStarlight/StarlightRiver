using StarlightRiver.Content.Tiles.CrashTech;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items
{
    class DebugPotion : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + "VitricPick";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Debug Stick");
            Tooltip.SetDefault("How did you get this?");
        }

        public override void SetDefaults()
        {
            item.damage = 10;
            item.melee = true;
            item.width = 38;
            item.height = 38;
            item.useTime = 18;
            item.useAnimation = 18;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;

            item.createTile = ModContent.TileType<CrashPod>();
        }

        public override void UpdateInventory(Player player)
        {
            float rot = Main.rand.NextFloat(6.28f);

            Dust.NewDustPerfect(player.Center + Microsoft.Xna.Framework.Vector2.UnitX.RotatedBy(rot) * 300, ModContent.DustType<Bosses.GlassBoss.LavaSpew>(), Microsoft.Xna.Framework.Vector2.UnitX.RotatedBy(rot));
        }

        public override bool UseItem(Player player)
        {
            for(int x = 0; x < Main.maxTilesX; x++)
                for(int y = 0; y < Main.maxTilesY; y++)
				{
                    var tile = Framing.GetTileSafely(x, y);
                    if (tile.collisionType == 1)
                        tile.type = TileID.Stone;
                    else
					{
                        tile.active(true);
                        tile.type = TileID.Dirt;
                    }
				}
            return true;
        }
    }
}

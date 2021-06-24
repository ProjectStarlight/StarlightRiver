using StarlightRiver.Codex;
using StarlightRiver.Content.Tiles.CrashTech;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Permafrost
{
	class WaterStaff : ModItem
	{
        public override string Texture => AssetDirectory.PermafrostItem + "WaterStaff";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Staff of the Waves");
            Tooltip.SetDefault("Places and removes aurora water\nLeft click to place\nRight click to remove");
        }

        public override void SetDefaults()
        {
            item.width = 38;
            item.height = 38;
            item.useTime = 12;
            item.useAnimation = 12;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = 10000;
            item.rare = ItemRarityID.Orange;
            item.autoReuse = true;
            item.useTurn = true;
        }

        public override bool AltFunctionUse(Player player) => true;

		public override bool UseItem(Player player)
        {
            if (WorldGen.InWorld(Player.tileTargetX, Player.tileTargetY))
            {
                Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

                if (player.altFunctionUse != 2 && (tile.bTileHeader3 & 0b11100000) >> 5 == 0)
                {
                    tile.bTileHeader3 |= 0b00100000;

                    Main.PlaySound(SoundID.Splash);
                    for (int k = 0; k < 5; k++)
                        Dust.NewDust(new Vector2(Player.tileTargetX, Player.tileTargetY) * 16, 16, 16, ModContent.DustType<Dusts.Glow>(), 0, 0, 0, AuroraColor(Main.rand.NextFloat()), 0.6f);
                }

                if(player.altFunctionUse == 2 && (tile.bTileHeader3 & 0b11100000) >> 5 == 1)
				{
                    tile.bTileHeader3 &= 0b00011111;

                    Main.PlaySound(SoundID.Splash);
                    for (int k = 0; k < 10; k++)
                        Dust.NewDust(new Vector2(Player.tileTargetX, Player.tileTargetY) * 16, 16, 16, ModContent.DustType<Dusts.Glow>(), 0, 0, 0, AuroraColor(Main.rand.NextFloat()), 0.6f);
                }
            }

            return true;
        }

        private Color AuroraColor(float seed)
        {
            return new Color(5, 200, 255);
        }
    }
}

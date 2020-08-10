using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dragons;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Dragons
{
    internal class Egg : SoulboundItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dragon Egg");
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
        }

        public override void UpdateInventory(Player player)
        {
            item.color = player.GetModPlayer<DragonHandler>().data.scaleColor;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = GetTexture("StarlightRiver/Items/Dragons/EggOver");
            spriteBatch.Draw(tex, position, tex.Frame(), Main.LocalPlayer.GetModPlayer<DragonHandler>().data.bellyColor, 0, Vector2.Zero, scale, 0, 0);
        }

        public override void SafeModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine nameline = tooltips.FirstOrDefault(n => n.Name == "ItemName" && n.mod == "Terraria");
            nameline.text = Main.LocalPlayer.name + "`s Dragon Egg";
            nameline.overrideColor = new Color(255, 220, 50);

            TooltipLine line = new TooltipLine(mod, "n", "Perhaps it would hatch if it had a nest...")
            {
                overrideColor = new Color(255, 255, 200)
            };
            tooltips.Add(line);
        }
    }
}
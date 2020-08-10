using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    class ForgeActor : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Invisible";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Air>(), SoundID.Shatter, false, Color.Black);

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Vector2 pos = (new Vector2(i + 3, j + 3) + Helper.TileAdj) * 16;
            Texture2D backdrop = GetTexture("StarlightRiver/NPCs/Miniboss/Glassweaver/Backdrop");

            spriteBatch.Draw(backdrop, pos - Terraria.Main.screenPosition, Color.White);
        }
    }
}

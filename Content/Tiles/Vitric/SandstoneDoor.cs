﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class SandstoneDoor : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetStaticDefaults() => (this).QuickSetFurniture(8, 2, DustID.Stone, SoundID.Tink, false, new Color(130, 85, 45));

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (StarlightWorld.HasFlag(WorldFlags.DesertOpen) && !Main.npc.Any(n => n.type == NPCType<Bosses.VitricBoss.VitricBoss>() && n.active)) Main.tileSolid[Type] = false;
            else Main.tileSolid[Type] = true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => false;

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
            {
                Texture2D tex = TextureAssets.Tile[Type].Value;
                Vector2 basepos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition;
                int off = StarlightWorld.HasFlag(WorldFlags.DesertOpen) ? 46 : 0;
                spriteBatch.Draw(tex, basepos + new Vector2(-off, 0), tex.Frame(), drawData.colorTint, 0, Vector2.Zero, 1, 0, 0);
                spriteBatch.Draw(tex, basepos + new Vector2(tex.Width + off, 0), tex.Frame(), drawData.colorTint, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
            }
        }
    }
}
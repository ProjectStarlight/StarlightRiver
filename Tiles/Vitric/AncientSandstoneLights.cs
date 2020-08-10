using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    public class AncientSandstoneTorchItem : Items.QuickTileItem
    {
        public AncientSandstoneTorchItem() : base("Ancient Vitric Illuminator", "It has an entrancing glow", TileType<AncientSandstoneTorch>(), 0)
        {
        }

        public override string Texture => "StarlightRiver/MarioCumming";
    }

    internal class AncientSandstoneTorch : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileID.Sets.FramesOnKillWall[Type] = true;

            drop = mod.ItemType("TorchOvergrowItem");
            dustType = mod.DustType("Air");
            AddMapEntry(new Color(115, 182, 158));
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Lighting.AddLight(new Vector2(i * 16, j * 16), new Vector3(115, 162, 158) * 0.004f);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Texture2D tex = GetTexture("StarlightRiver/RiftCrafting/Glow0");
            Texture2D tex2 = GetTexture("StarlightRiver/RiftCrafting/Glow1");

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive);

            Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One * 8 - Main.screenPosition;
            for (int k = 0; k < 3; k++)
            {
                spriteBatch.Draw(tex, pos, tex.Frame(), new Color(115, 162, 158) * (0.6f + (float)Math.Sin(StarlightWorld.rottime) * 0.05f), 0, tex.Size() / 2, k * 0.4f, 0, 0);
            }
            spriteBatch.Draw(tex2, pos, tex.Frame(), new Color(115, 162, 158) * (0.6f + (float)Math.Sin(StarlightWorld.rottime) * 0.10f), (float)Math.Sin(StarlightWorld.rottime) * 0.1f, tex.Size() / 2, 1f, 0, 0);
            spriteBatch.Draw(tex2, pos, tex.Frame(), new Color(115, 162, 158) * (0.6f - (float)Math.Sin(StarlightWorld.rottime) * 0.10f), 2 + -(float)Math.Sin(StarlightWorld.rottime + 1) * 0.1f, tex.Size() / 2, 1.3f, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin();
        }
    }
}
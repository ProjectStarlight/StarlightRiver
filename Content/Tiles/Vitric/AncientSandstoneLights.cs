using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Tiles.Overgrow;

namespace StarlightRiver.Content.Tiles.Vitric
{
    public class AncientSandstoneTorchItem : QuickTileItem
    {
        public AncientSandstoneTorchItem() : base("Ancient Vitric Illuminator", "It has an entrancing glow", TileType<AncientSandstoneTorch>(), 0, AssetDirectory.Debug, true) { }
    }

    internal class AncientSandstoneTorch : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }
        
        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileID.Sets.FramesOnKillWall[Type] = true;

            drop = ItemType<TorchOvergrowItem>();
            dustType = mod.DustType("Air");
            AddMapEntry(new Color(115, 182, 158));
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            if (!StarlightWorld.HasFlag(WorldFlags.DesertOpen) || !Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneGlassTemple) 
                return;
            Lighting.AddLight(new Vector2(i * 16, j * 16), new Vector3(125, 162, 158) * 0.003f);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (!StarlightWorld.HasFlag(WorldFlags.DesertOpen) || !Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneGlassTemple) 
                return;
            Texture2D tex = GetTexture(AssetDirectory.RiftCrafting + "Glow0");
            Texture2D tex2 = GetTexture(AssetDirectory.RiftCrafting + "Glow1");

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive);

            Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One * 8 - Main.screenPosition;
            for (int k = 0; k < 3; k++)
                spriteBatch.Draw(tex, pos, tex.Frame(), new Color(125, 162, 158) * (0.65f + (float)Math.Sin(StarlightWorld.rottime) * 0.05f), 0, tex.Size() / 2, k * 0.3f, 0, 0);
            spriteBatch.Draw(tex2, pos, tex.Frame(), new Color(125, 162, 158) * (0.65f + (float)Math.Sin(StarlightWorld.rottime) * 0.10f), (float)Math.Sin(StarlightWorld.rottime) * 0.1f, tex.Size() / 2, 0.6f, 0, 0);
            spriteBatch.Draw(tex2, pos, tex.Frame(), new Color(125, 162, 158) * (0.65f - (float)Math.Sin(StarlightWorld.rottime) * 0.10f), 2 + -(float)Math.Sin(StarlightWorld.rottime + 1) * 0.1f, tex.Size() / 2, 0.9f, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin();
        }
    }
}
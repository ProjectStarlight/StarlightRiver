using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core.Loaders;

namespace StarlightRiver.Content.Tiles.Void
{
    internal class VoidTorch : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VoidTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 2, DustType<Dusts.Darkness>(), SoundID.Tink, false, new Color(55, 60, 40));

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VoidTorchItem>());

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(1, 0.5f, 1.2f) * 0.6f);
            if (Main.tile[i, j].frameX == 0 && Main.tile[i, j].frameY == 0)
            {
                for (int k = 0; k <= 2; k++)
                {
                    float dist = Main.rand.NextFloat(8);
                    float dist2 = dist > 4 ? 4 - (dist - 4) : dist;
                    Dust.NewDustPerfect(new Vector2(i, j) * 16 + new Vector2(dist + 5, -6 + dist2), DustType<Dusts.Darkness>(), new Vector2(0, -0.04f * dist2), 0, default, 0.5f);
                }
            }
        }
    }

    class VoidTorchItem : QuickTileItem 
    { 
        public VoidTorchItem() : base("Void Torch", "", TileType<VoidTorch>(), 1, AssetDirectory.VoidTile) { } 
    }
}
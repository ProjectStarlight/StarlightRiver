using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Void
{
    class VoidBrick : ModTile { public override void SetDefaults() => QuickBlock.QuickSet(this, 200, DustType<Dusts.Darkness>(), SoundID.Tink, new Color(45, 50, 30), ItemType<VoidBrickItem>()); }
    class VoidBrickItem : QuickTileItem { public VoidBrickItem() : base("Void Bricks", "", TileType<VoidBrick>(), 1) { } }

    class VoidStone : ModTile { public override void SetDefaults() => QuickBlock.QuickSet(this, 200, DustType<Dusts.Darkness>(), SoundID.Tink, new Color(35, 40, 20), ItemType<VoidStoneItem>()); }
    class VoidStoneItem : QuickTileItem { public VoidStoneItem() : base("Void Stone", "", TileType<VoidStone>(), 1) { } }

    internal class VoidTorch : ModTile
    {
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
    class VoidTorchItem : QuickTileItem { public VoidTorchItem() : base("Void Torch", "", TileType<VoidTorch>(), 1) { } }

    internal class VoidBrazier : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 1, DustType<Dusts.Darkness>(), SoundID.Tink, false, new Color(55, 60, 40));
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VoidBrazierItem>());

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(1, 0.5f, 1.2f) * 0.6f);
            if (Main.tile[i, j].frameX == 0 && Main.tile[i, j].frameY == 0)
            {
                for (int k = 0; k <= 3; k++)
                {
                    float dist = Main.rand.NextFloat(10);
                    float dist2 = dist > 5 ? 5 - (dist - 5) : dist;
                    Dust.NewDustPerfect(new Vector2(i, j) * 16 + new Vector2(dist + 11, -10 + dist2), DustType<Dusts.Darkness>(), new Vector2(0, -0.05f * dist2));
                }
            }
        }
    }
    class VoidBrazierItem : QuickTileItem { public VoidBrazierItem() : base("Void Brazier", "", TileType<VoidBrazier>(), 1) { } }

    internal class VoidPillarBase : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);
            QuickBlock.QuickSetFurniture(this, 3, 2, DustType<Dusts.Darkness>(), SoundID.Tink, false, new Color(55, 60, 40));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VoidPillarBaseItem>());
    }
    class VoidPillarBaseItem : QuickTileItem { public VoidPillarBaseItem() : base("Void Pillar Base", "", TileType<VoidPillarBase>(), 1) { } }

    internal class VoidPillarMiddle : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 3, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileType<VoidPillarBase>(),
                TileType<VoidPillarMiddle>()
            };
            QuickBlock.QuickSetFurniture(this, 3, 2, DustType<Dusts.Darkness>(), SoundID.Tink, false, new Color(55, 60, 40));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VoidPillarMiddleItem>());
    }
    class VoidPillarMiddleItem : QuickTileItem { public VoidPillarMiddleItem() : base("Void Pillar", "", TileType<VoidPillarMiddle>(), 1) { } }

    internal class VoidPillarTop : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 3, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileType<VoidPillarBase>(),
                TileType<VoidPillarMiddle>()
            };
            QuickBlock.QuickSetFurniture(this, 3, 1, DustType<Dusts.Darkness>(), SoundID.Tink, false, new Color(55, 60, 40));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VoidPillarTopItem>());
    }
    class VoidPillarTopItem : QuickTileItem { public VoidPillarTopItem() : base("Void Pillar Support", "", TileType<VoidPillarTop>(), 1) { } }

    internal class VoidPillarTopAlt : ModTile
    {
        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 3, 0);
            TileObjectData.newTile.AnchorAlternateTiles = new int[]
            {
                TileType<VoidPillarBase>(),
                TileType<VoidPillarMiddle>()
            };
            QuickBlock.QuickSetFurniture(this, 3, 1, DustType<Dusts.Darkness>(), SoundID.Tink, false, new Color(55, 60, 40), true);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<VoidPillarTopAltItem>());
    }
    class VoidPillarTopAltItem : QuickTileItem { public VoidPillarTopAltItem() : base("Void Pillar Pedestal", "", TileType<VoidPillarTopAlt>(), 1) { } }
}
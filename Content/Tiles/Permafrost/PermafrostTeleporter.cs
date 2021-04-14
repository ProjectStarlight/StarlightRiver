using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class PermafrostTeleporter : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.PermafrostTile + name;
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 5, 10, DustType<Dusts.Stone>(), SoundID.Tink, false, new Color(100, 200, 200));

        public override bool NewRightClick(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            int offX = tile.frameX / 18;
            int offY = tile.frameY / 18;

            var entity = TileEntity.ByPosition[new Point16(i - offX, j - offY)] as PermafrostTeleporterEntity;
            entity.controlling = Main.LocalPlayer;

            return true;
        }
    }

    class PermafrostTeleporterEntity : ModTileEntity
    {
        public Vector2 target;
        private Vector2 startCache;
        public int timer;
        public Player controlling;

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return tile.type == TileType<PermafrostTeleporter>();
        }

        public override void Update()
        {
            if (controlling != null)
            {
                if (startCache == Vector2.Zero) startCache = controlling.Center;

                var pos = Vector2.SmoothStep(startCache, target, timer / 120f);

                controlling.Center = pos;
                controlling.immune = true;
                controlling.immuneTime = 2;
                controlling.immuneAlpha = 255;
                controlling.noItems = true;
                controlling.GetModPlayer<StarlightPlayer>().trueInvisible = true;

                for (int k = 0; k < 25; k++)
                {
                    float sin = 1 + (float)System.Math.Sin(-StarlightWorld.rottime * 6.28f);
                    float cos = 1 + (float)System.Math.Cos(-StarlightWorld.rottime * 6.28f);
                    Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.1f;

                    var dustPos = Vector2.SmoothStep(startCache, target, (timer * 25 + k) / (120f * 25));
                    Dust.NewDustPerfect(dustPos, DustType<Dusts.Aurora>(), Vector2.Zero, 0, color, 1);
                }

                timer++;

                if (timer >= 120)
                {
                    controlling = null;
                    timer = 0;
                    startCache = Vector2.Zero;
                }
            }
        }

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                ["target"] = target
            };
        }

        public override void Load(TagCompound tag)
        {
            target = tag.Get<Vector2>("target");
        }
    }

    class PermafrostTeleporterItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public PermafrostTeleporterItem() : base("PermafrostTeleporter", "I came", TileType<PermafrostTeleporter>(), 1) { }
    }
}

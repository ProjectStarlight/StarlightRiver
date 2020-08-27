using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric.Blocks
{
    internal class VitricSpike : ModTile
    {
        public override void SetDefaults()
        {
            minPick = int.MaxValue;
            QuickBlock.QuickSet(this, 200, DustType<Dusts.Glass3>(), SoundID.Tink, new Color(95, 162, 138), -1);
            Main.tileMerge[Type][TileType<VitricSand>()] = true;
        }

        public override bool Dangersense(int i, int j, Player player) => true;

        public override void FloorVisuals(Player player)
        {
            damage = 0;
            var points = Collision.GetEntityEdgeTiles(entity);
            foreach (var p in points)
            {
                if (!WorldGen.InWorld(p.X, p.Y)) continue;

                // If any edge tiles are spikes, collide
                var tile = Framing.GetTileSafely(p);
                var vector = new Vector2();
                if (tile.active() && tile.type == TileType<VitricSpike>())
                {
                    // ech, necessary
                    if (p.X * 16 + 16 <= entity.TopLeft.X)
                        vector.X += 1;
                    if (p.X * 16 >= entity.TopRight.X)
                        vector.X -= 1;
                    if (p.Y * 16 + 16 <= entity.TopLeft.Y)
                        vector.Y += 1;
                    if (p.Y * 16 >= entity.TopRight.Y)
                        vector.Y -= 1;
                    // Damage
                    damage = 25;
                }
                if (vector != default)
                {
                    vector.Normalize();
                    vector *= 15;
                    entity.velocity.X = vector.X == 0 ? entity.velocity.X : vector.X;
                    entity.velocity.Y = vector.Y == 0 ? entity.velocity.Y : vector.Y;
                }
            }
        }
    }

    class VitricSpikeItem : QuickTileItem
    {
        public VitricSpikeItem() : base("Vitric Spikes", "Ouch!", TileType<VitricSpike>(), 0) { }
    }
}
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricSpike : ModTile
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetStaticDefaults()
        {
            MinPick = int.MaxValue;
            (this).QuickSet(200, DustType<Dusts.GlassGravity>(), 2, new Color(95, 162, 138), -1, false, false, "", 27);
            TileID.Sets.TouchDamageOther[Type] = 5;//vanilla contact damage
            Main.tileMerge[Type][Mod.Find<ModTile>("VitricSand").Type] = true;
            Main.tileMerge[Type][TileType<VitricGiantCrystal>()] = true;
            Main.tileMerge[Type][TileType<VitricMediumCrystal>()] = true;
            Main.tileMerge[Type][TileType<VitricLargeCrystal>()] = true;
            Main.tileMerge[Type][TileType<VitricSmallCrystal>()] = true;
        }

        public override bool IsTileDangerous(int i, int j, Player player) => true;

		public static void CollideWithSpikes(Entity entity, out int damage)
        {
            if (entity is NPC && ((entity as NPC).dontTakeDamage || (entity as NPC).immortal))
            {
                damage = 0;
                return;
            }

            damage = 0;
            var points = Collision.GetEntityEdgeTiles(entity);
            foreach (var p in points)
            {
                if (!WorldGen.InWorld(p.X, p.Y)) continue;

                // If any edge tiles are spikes, collide
                var tile = Framing.GetTileSafely(p);
                var vector = new Vector2();

                if (tile.HasTile && tile.TileType == TileType<VitricSpike>())
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
                    vector *= 6;
                    entity.velocity.X = vector.X == 0 ? entity.velocity.X : vector.X;
                    entity.velocity.Y = vector.Y == 0 ? entity.velocity.Y : vector.Y;
                }
            }
        }
    }

    class VitricSpikeItem : QuickTileItem
    {
        public VitricSpikeItem() : base("Vitric Spikes", "Ouch!", TileType<VitricSpike>(), 0, AssetDirectory.VitricTile) { }
    }
}
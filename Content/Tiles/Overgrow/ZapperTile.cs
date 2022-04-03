using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class ZapperTile : ModTile
    {
        public override string Texture => AssetDirectory.OvergrowTile + Name;

        public override void SetDefaults() { QuickBlock.QuickSetFurniture(this, 5, 2, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 80)); }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
            {
                if (!(Main.projectile.Any(proj => proj.ModProjectile is Zapper && (proj.ModProjectile as Zapper).parent == Main.tile[i, j] && proj.active)))
                {
                    int proj = Projectile.NewProjectile(new Vector2(i + 2, j + 2) * 16, Vector2.Zero, ProjectileType<Zapper>(), 1, 1);
                    (Main.projectile[proj].ModProjectile as Zapper).parent = Main.tile[i, j];
                }
            }
        }
    }
}
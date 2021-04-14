using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class DoorBombShooter : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/DoorBombShooter";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Ice, SoundID.Tink, false, new Color(200, 255, 255));

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!StarlightWorld.HasFlag(WorldFlags.SquidBossOpen) && !Main.projectile.Any(n => n.active && n.type == ProjectileType<DoorBomb>()))
                Projectile.NewProjectile(new Vector2(i + 1, j + 0.5f) * 16, new Vector2(1, 0), ProjectileType<DoorBomb>(), 0, 0);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (!StarlightWorld.HasFlag(WorldFlags.SquidBossOpen))
            {
                Vector2 pos = (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(18, -42);
                Utils.DrawBorderString(spriteBatch, "Place blocks on", pos, Color.White, 0.7f);
                Utils.DrawBorderString(spriteBatch, "BLUE", pos + new Vector2(90, 0), Color.DeepSkyBlue, 0.7f);
                Utils.DrawBorderString(spriteBatch, "squares", pos + new Vector2(130, 0), Color.White, 0.7f);
            }
        }
    }

    class DoorBombShooterItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public DoorBombShooterItem() : base("Debug Shooter Placer", "", TileType<DoorBombShooter>(), ItemRarityID.White) { }
    }
}

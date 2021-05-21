using Terraria;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Content.Bosses.SquidBoss;

namespace StarlightRiver.Content.CustomHooks
{
    class NoBuild : HookGroup
    {
        //Changes vanilla behavior, but should be highly conditional.
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public override void Load()
        {
            On.Terraria.Player.PlaceThing += PlacementRestriction;
        }

        private void PlacementRestriction(On.Terraria.Player.orig_PlaceThing orig, Player self)
        {
            Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

            if (tile.wall == WallType<AuroraBrickWall>())
            {
                for (int k = 0; k < Main.maxProjectiles; k++) //this is gross. Unfortunate.
                {
                    Projectile proj = Main.projectile[k];

                    if (proj.active && proj.timeLeft > 10 && proj.modProjectile is InteractiveProjectile && (proj.modProjectile as InteractiveProjectile).CheckPoint(Player.tileTargetX, Player.tileTargetY))
                    {
                        orig(self);
                        return;
                    }
                }
                return;
            }

            else orig(self);
        }
    }
}
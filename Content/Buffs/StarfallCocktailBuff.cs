using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class StarfallCocktailBuff : SmartBuff
    {
        public StarfallCocktailBuff() : base("Starcaller", "Attract fallen stars!", false) { }

        public override void Update(Player player, ref int buffIndex)
        {
            if (!Main.dayTime && Main.rand.Next(1000) == 0)
            {
                Projectile.NewProjectile(new Vector2(Main.rand.Next(Main.maxTilesX * 16), 0), -Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(10, 15), ProjectileID.FallingStar, 1000, 10f);
            }
        }
    }
}
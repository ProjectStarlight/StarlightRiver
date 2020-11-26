using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Items;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Tiles.Temple
{
    class DashBarrier : DummyTile
    {
        public override int DummyType => ProjectileType<DashBarrierDummy>();

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 3, DustType<Dusts.Stamina>(), SoundID.Shatter, false, new Color(204, 91, 50), false, false, "Stamina Jar");
    }

    internal class DashBarrierDummy : Dummy
    {
        public DashBarrierDummy() : base(TileType<DashBarrier>(), 32, 48) { }
        public override void Collision(Player player)
        {
            if (AbilityHelper.CheckDash(player, projectile.Hitbox))
            {
                WorldGen.KillTile(ParentX, ParentY);
                Main.PlaySound(SoundID.Tink, projectile.Center);
            }
        }
    }

    public class DashBarrierItem : QuickTileItem
    {
        public override string Texture => Directory.Debug;

        public DashBarrierItem() : base("Dash Barrier", "Cum in my pussy.", TileType<DashBarrier>(), -12) { }
    }
}

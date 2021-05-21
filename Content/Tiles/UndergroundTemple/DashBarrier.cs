using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Tiles.Temple
{
    class DashBarrier : DummyTile
    {
        public override int DummyType => ProjectileType<DashBarrierDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.UndergroundTempleTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 2, 3, DustType<Content.Dusts.Stamina>(), SoundID.Shatter, false, new Color(204, 91, 50), false, false);
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
        public override string Texture => AssetDirectory.Debug;

        public DashBarrierItem() : base("Dash Barrier", "Cum in my pussy.", TileType<DashBarrier>(), -12) { }
    }
}

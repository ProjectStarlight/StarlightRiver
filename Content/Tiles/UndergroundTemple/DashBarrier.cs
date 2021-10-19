using Microsoft.Xna.Framework;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 2, 3, DustType<Content.Dusts.Stamina>(), SoundID.Shatter, false, new Color(204, 91, 50), false, true);
            minPick = int.MaxValue;
        }

        public override void SafeNearbyEffects(int i, int j, bool closer)
        {
            if (Main.rand.Next(300) == 0)
            {
                Vector2 pos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));
                if (Main.rand.NextBool())
                    Dust.NewDustPerfect(pos, ModContent.DustType<CrystalSparkle>(), Vector2.Zero);
                else
                    Dust.NewDustPerfect(pos, ModContent.DustType<CrystalSparkle2>(), Vector2.Zero);
            }
            base.SafeNearbyEffects(i, j, closer);
        }
    }

    internal class DashBarrierDummy : Dummy
    {
        public DashBarrierDummy() : base(TileType<DashBarrier>(), 32, 48) { }

        public override void Collision(Player player)
        {
            if (AbilityHelper.CheckDash(player, projectile.Hitbox))
            {
                WorldGen.KillTile(ParentX, ParentY);
                NetMessage.SendTileRange(player.whoAmI, (int)(projectile.position.X / 16f), (int)(projectile.position.Y / 16f), 2, 3, TileChangeType.None);

                Main.PlaySound(SoundID.Tink, projectile.Center);
            }
        }
    }

    public class DashBarrierItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public DashBarrierItem() : base("Dash Barrier", "Debug item", TileType<DashBarrier>(), -12) { }
    }
}

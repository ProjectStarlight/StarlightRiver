using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class WindsCrystal : DummyTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override int DummyType => ProjectileType<WindsCrystalDummy>();

        public override void SetDefaults()
        {
            //minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(10, 10, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, new Color(100, 200, 255), false, true, "Crystal Containment");
        }
    }

    class WindsCrystalDestroyed : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricTile + name;
            return base.Autoload(ref name, ref texture);
        }


        public override void SetDefaults()
        {
            //minPick = int.MaxValue;
            TileID.Sets.DrawsWalls[Type] = true;
            (this).QuickSetFurniture(10, 4, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, new Color(100, 200, 255), false, false, "Crystal Containment");
        }
    }

    class WindsCrystalDummy : Dummy
    {
        public WindsCrystalDummy() : base(TileType<WindsCrystal>(), 11 * 16, 11 * 16) { }

        public override void Update()
        {
            Rectangle box = projectile.Hitbox;
            box.Inflate(10, 10);

            for (int k = 0; k < Main.maxNPCs; k++)
            {
                var npc = Main.npc[k];
                if (npc.active && npc.type == NPCType<Boulder>() && npc.Hitbox.Intersects(box))
                {
                    WorldGen.KillTile(ParentX - 5, ParentY - 5);
                    Helper.PlaceMultitile(new Point16(ParentX - 5, ParentY + 1), TileType<WindsCrystalDestroyed>());
                    WorldGen.PlaceTile(ParentX, ParentY, TileType<Pickups.ForbiddenWindsPickupTile>());
                }
            }
        }
    }
}

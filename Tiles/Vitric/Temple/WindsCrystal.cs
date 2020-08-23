using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using static Terraria.ModLoader.ModContent;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.Dummies;
using Terraria.DataStructures;
using StarlightRiver.Items;

namespace StarlightRiver.Tiles.Vitric.Temple
{
    class WindsCrystal : DummyTile
    {
        public override int DummyType => ProjectileType<WindsCrystalDummy>();

        public override void SetDefaults()
        {
            //minPick = int.MaxValue;
            QuickBlock.QuickSetFurniture(this, 11, 11, DustType<Dusts.Air>(), SoundID.Shatter, false, new Color(100, 200, 255), false, true, "Crystal Containment");
        }
    }

    class WindsCrystalDestroyed : ModTile
    {
        public override void SetDefaults()
        {
            //minPick = int.MaxValue;
            QuickBlock.QuickSetFurniture(this, 11, 5, DustType<Dusts.Air>(), SoundID.Shatter, false, new Color(100, 200, 255), false, false, "Crystal Containment");
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

    class WindsCrystalItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public WindsCrystalItem() : base("Winds Crystal Item", "", TileType<WindsCrystal>(), 1) { }
    }
}

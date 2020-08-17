using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.Dummies;
using StarlightRiver.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Vitric
{
    class WalkingSentinel : ModNPC
    {
        public override bool Autoload(ref string name) => false;

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 48;
            npc.lifeMax = 150;
            npc.defense = 10;
            npc.damage = 30;
            npc.aiStyle = -1;
        }
    }

    class WalkingSentinelTile : DummyTile
    {
        public override bool Autoload(ref string name, ref string texture) => false;

        public override int DummyType => ProjectileType<WalkingSentinelDummy>();

        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
            QuickBlock.QuickSetFurniture(this, 2, 3, DustID.Sandnado, SoundID.Tink, false, new Color(200, 100, 20), false, false, "???");
        }
    }

    class WalkingSentinelDummy : Dummy
    {
        public override bool Autoload(ref string name) => false;

        public WalkingSentinelDummy() : base(TileType<WalkingSentinelTile>(), 2 * 16, 3 * 16) { }
    }
}

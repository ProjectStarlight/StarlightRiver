using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	class WalkingSentinel : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/WalkingSentinel";

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 48;
            NPC.lifeMax = 150;
            NPC.defense = 10;
            NPC.damage = 30;
            NPC.aiStyle = -1;
        }

        public override void AI()
        {
            NPC.TargetClosest();
            Player Player = Main.player[NPC.target];

            //basic movement
            if (Player.Center.X > NPC.Center.X && NPC.velocity.X < 3) NPC.velocity.X += 0.02f;
            else if (NPC.velocity.X > -3) NPC.velocity.X -= 0.02f;

            if (NPC.collideX) NPC.velocity.Y -= 3;
        }
    }

    class WalkingSentinelTile : DummyTile
    {
        public override int DummyType => ProjectileType<WalkingSentinelDummy>();

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/WalkingSentinelTile";

        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);
            this.QuickSetFurniture(2, 3, DustID.Sandnado, SoundID.Tink, false, new Color(200, 100, 20), false, false, "???");
        }
    }

    class WalkingSentinelDummy : Dummy
    {
        public WalkingSentinelDummy() : base(TileType<WalkingSentinelTile>(), 2 * 16, 3 * 16) { }

        public override void Update()
        {

        }
    }
}

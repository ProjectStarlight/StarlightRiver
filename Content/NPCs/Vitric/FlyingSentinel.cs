using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	class FlyingSentinel : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/FlyingSentinel";

        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 48;
            NPC.lifeMax = 80;
            NPC.defense = 10;
            NPC.damage = 40;
            NPC.noGravity = true;
            NPC.aiStyle = -1;
        }

        public override void AI()
        {
            NPC.ai[0] += 0.02f;

            NPC.TargetClosest();
            NPC.velocity.X = Main.player[NPC.target].Center.X > NPC.Center.X ? 1 : -1;
            NPC.velocity.Y = (float)Math.Sin(NPC.ai[0]) * 0.05f;

        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            base.PostDraw(spriteBatch, drawColor);
        }
    }

    class FlyingSentinelTile : DummyTile
    {
        public override int DummyType => ProjectileType<FlyingSentinelDummy>();

        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/FlyingSentinelTile";

        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorWall = true;
            this.QuickSetFurniture(2, 2, DustID.Sandnado, SoundID.Tink, false, new Color(200, 100, 20), false, false, "???");
        }
    }

    class FlyingSentinelDummy : Dummy
    {
        public FlyingSentinelDummy() : base(TileType<FlyingSentinelTile>(), 2 * 16, 2 * 16) { }

        public override void Update()
        {

        }
    }
}

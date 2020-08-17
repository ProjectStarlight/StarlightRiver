using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Projectiles.Dummies;
using StarlightRiver.Tiles;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Vitric
{
    class FlyingSentinel : ModNPC
    {
        public override bool Autoload(ref string name) => false;

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 48;
            npc.lifeMax = 80;
            npc.defense = 10;
            npc.damage = 40;
            npc.noGravity = true;
            npc.aiStyle = -1;
        }

        public override void AI()
        {
            npc.ai[0] += 0.02f;

            npc.TargetClosest();
            npc.velocity.X = Main.player[npc.target].Center.X > npc.Center.X ? 1 : -1;
            npc.velocity.Y = (float)Math.Sin(npc.ai[0]) * 0.05f;

        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            base.PostDraw(spriteBatch, drawColor);
        }
    }

    class FlyingSentinelTile : DummyTile
    {
        public override bool Autoload(ref string name, ref string texture) => false;

        public override int DummyType => ProjectileType<FlyingSentinelDummy>();

        public override void SetDefaults()
        {
            TileObjectData.newTile.AnchorWall = true;
            QuickBlock.QuickSetFurniture(this, 2, 2, DustID.Sandnado, SoundID.Tink, false, new Color(200, 100, 20), false, false, "???");
        }
    }

    class FlyingSentinelDummy : Dummy
    {
        public override bool Autoload(ref string name) => false;

        public FlyingSentinelDummy() : base(TileType<FlyingSentinelTile>(), 2 * 16, 3 * 16) { }
    }
}

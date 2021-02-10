using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.NPCs.Vitric
{
    class FlyingSentinel : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/FlyingSentinel";

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
        public override int DummyType => ProjectileType<FlyingSentinelDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/NPCs/Vitric/FlyingSentinelTile";
            return true;
        }

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
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile proj = Main.projectile[k];
                if (proj.active && proj.type == ProjectileType<Tiles.Vitric.Temple.LightBeam>() && proj.Hitbox.Intersects(projectile.Hitbox))
                {
                    WorldGen.KillTile(ParentX, ParentY);
                    NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y, NPCType<FlyingSentinel>());
                }
            }
        }
    }
}

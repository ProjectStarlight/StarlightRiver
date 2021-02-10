using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.NPCs.Vitric
{
    class WalkingSentinel : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/WalkingSentinel";

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 48;
            npc.lifeMax = 150;
            npc.defense = 10;
            npc.damage = 30;
            npc.aiStyle = -1;
        }

        public override void AI()
        {
            npc.TargetClosest();
            Player player = Main.player[npc.target];

            //basic movement
            if (player.Center.X > npc.Center.X && npc.velocity.X < 3) npc.velocity.X += 0.02f;
            else if (npc.velocity.X > -3) npc.velocity.X -= 0.02f;

            if (npc.collideX) npc.velocity.Y -= 3;
        }
    }

    class WalkingSentinelTile : DummyTile
    {
        public override int DummyType => ProjectileType<WalkingSentinelDummy>();

        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/NPCs/Vitric/WalkingSentinelTile";
            return true;
        }

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
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile proj = Main.projectile[k];
                if (proj.active && proj.type == ProjectileType<Tiles.Vitric.Temple.LightBeam>() && proj.Hitbox.Intersects(projectile.Hitbox))
                {
                    WorldGen.KillTile(ParentX, ParentY);
                    NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y, NPCType<WalkingSentinel>());
                }
            }
        }
    }
}

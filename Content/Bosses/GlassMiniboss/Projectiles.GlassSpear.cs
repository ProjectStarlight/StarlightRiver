using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassSpear : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.tileCollide = true;
        }

        public ref float Timer => ref Projectile.ai[0];

        public NPC Parent => Main.npc[(int)Projectile.ai[1]];

        public override void AI()
        {
            if (!Parent.active || Parent.type != NPCType<GlassMiniboss>())
                Projectile.Kill();

            Timer++;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}

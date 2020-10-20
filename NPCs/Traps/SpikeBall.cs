using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Traps
{
    internal class SpikeBall : ModNPC
    {
        public Vector2 anchor;
        public int length; 

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Swinger");
        }

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 32;
            npc.immortal = true;
            //npc.dontTakeDamage = true;
            npc.lifeMax = 1;
            npc.dontCountMe = true;
            npc.aiStyle = -1;
            npc.noGravity = true;
            npc.knockBackResist = 0;
            npc.behindTiles = true;
            npc.damage = 20;
        }

        public override void AI()
        {
            if (anchor == Vector2.Zero) anchor = npc.Center;
            if (length == 0)
            {
                length = 100;
                npc.Center += new Vector2(0, 100);
            }

            float g = 0.4f; //yes, g is 0.4 in terraria.
            float m = 1f; //arbitrary mass.

            float angle = (anchor - npc.Center).ToRotation();
            Vector2 force = Vector2.UnitY * m * g + (Vector2.UnitX.RotatedBy(angle) * m * g * (1 + Vector2.Distance(npc.Center, anchor) - length));
            npc.velocity += force / m;

            npc.velocity *= 0.98f;
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            npc.velocity.X += target.velocity.X / 1f;
        }

        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit)
        {
            npc.velocity.X += projectile.velocity.X / 1f;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            for(float k = 0; k < 1; k += 2f / Vector2.Distance(npc.Center, anchor))
            {
                Vector2 pos = Vector2.Lerp(npc.Center, anchor, k) - Main.screenPosition;
                float angle = (npc.Center - anchor).ToRotation();
                spriteBatch.Draw(Main.magicPixel, pos, new Rectangle(0, 0, 2, 2), Color.White, angle, Vector2.One, 1, 0, 0);
            }
        }
    }
}
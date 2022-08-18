using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using StarlightRiver.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
    class GlassweaverDoor : ModProjectile
    {
        public override string Texture => AssetDirectory.Glassweaver + Name;

        private float closeTimer;

        private bool closing = false;

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 80;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            var parent = Main.npc.Where(n => n.active && n.type == ModContent.NPCType<Glassweaver>()).FirstOrDefault();

            if (parent != default && !closing)
            {
                if (closeTimer < 1)
                    closeTimer += 0.025f;
            }
            else
            {
                closing = true;
                closeTimer -= 0.025f;
            }

            Projectile.timeLeft = 2;

            if (closeTimer < 0)
                Projectile.active = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int height = (int)(tex.Height * closeTimer);

            Rectangle frame = new Rectangle(0, 0, tex.Width, height);
            Vector2 origin = new Vector2(tex.Width / 2, 0);

            Main.spriteBatch.Draw(tex, (Projectile.Center - new Vector2(0, height)) - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}

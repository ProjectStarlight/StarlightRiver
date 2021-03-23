using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items.Vitric;
using System.Linq;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    class PlayerShield : ModProjectile
    {
        public VitricBoss parent;

        private Vector2 savedPos;

        public override string Texture => AssetDirectory.GlassBoss + Name;

        public ref float Timer => ref projectile.ai[0];

        public override void SetDefaults()
        {
            projectile.timeLeft = 2;
            projectile.width = 20;
            projectile.height = 20;
            projectile.scale = 0;
            projectile.friendly = true;
        }

        public override void AI()
        {
            Player owner = Main.player[projectile.owner];

            projectile.timeLeft = 2;
            Timer++;

            if (Timer <= 120)
                projectile.scale += 1 / 120f;

            if (Timer == 120)
            {
                foreach (Player player in Main.player.Where(n => n.active && parent.arena.Contains(n.Center.ToPoint())))
                {
                    int i = Projectile.NewProjectile(projectile.Center, Vector2.Zero, projectile.type, 0, 0, player.whoAmI, 120);
                }
                projectile.active = false; //despawn the initial blob, the shields take over from here
            }

            if (Timer == 121)
            {
                savedPos = projectile.Center;
                projectile.scale = 1;
            }

            if(Timer >= 121 && Timer <= 180)
            {
                projectile.Center = Vector2.SmoothStep(savedPos, owner.Center, (Timer - 120) / 60f);
            }

            if(Timer >= 180) //player is holding a shield
            {
                projectile.Center = owner.Center + new Vector2(0, owner.gfxOffY);
                owner.heldProj = projectile.whoAmI;

                if(Main.netMode == NetmodeID.MultiplayerClient || Main.netMode == NetmodeID.SinglePlayer)
                {
                    projectile.rotation = (Main.MouseWorld - owner.Center).ToRotation();
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) => false;

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        { 
            var tex = GetTexture(Texture);
            var color = VitricSummonOrb.MoltenGlow(Timer / 3f);

            if (Timer >= 60)
                spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.Teal, 0, Vector2.One * 16, projectile.scale, 0, 0);

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color, 0, Vector2.One * 16, projectile.scale, 0, 0);

            if(Timer >= 180)
            {
                var tex2 = GetTexture(Texture + "Out");
                var color2 = VitricSummonOrb.MoltenGlow(Timer / 3f);

                spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, Color.Teal, projectile.rotation, new Vector2(-10, 32), 1, 0, 0);
                spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, color2, projectile.rotation, new Vector2(-10, 32), 1, 0, 0);
            }

        }
    }
}

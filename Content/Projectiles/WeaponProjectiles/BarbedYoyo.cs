using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Projectiles.WeaponProjectiles
{
    internal class BarbedYoyo : ModProjectile
    {
        private readonly List<NPC> targets = new List<NPC>();

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 2;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flayer");
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (player.channel) projectile.timeLeft = 2;
            projectile.rotation += 0.2f;

            Vector2 basepos;
            if (targets.Count == 0)
            {
                basepos = player.Center;
            }
            else
            {
                basepos = targets.Last().Center;
            }

            projectile.position = Vector2.Distance(basepos, Main.MouseWorld) < 200 ?
                Main.MouseWorld :
                basepos + new Vector2(-200, 0).RotatedBy((basepos - Main.MouseWorld).ToRotation());

            List<NPC> removals = new List<NPC>();
            foreach (NPC npc in targets)
            {
                //npc.StrikeNPC(projectile.damage / 2, 0, 0);
                if (!npc.active) removals.Add(npc);
            }

            foreach (NPC npc in removals)
            {
                targets.Remove(npc);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (!targets.Contains(target) && targets.Count < 3)
            {
                targets.Add(target);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];
            Texture2D tex = GetTexture("StarlightRiver/Projectiles/WeaponProjectiles/BarbedYoyoChain");

            if (targets.Count == 0) DrawBetween(spriteBatch, tex, player.Center, projectile.Center, lightColor);
            else
            {
                DrawBetween(spriteBatch, tex, player.Center, targets.First().Center, lightColor);
                for (int k = 1; k < targets.Count(); k++)
                {
                    DrawBetween(spriteBatch, tex, targets[k].Center, targets[k - 1].Center, lightColor);
                }
                DrawBetween(spriteBatch, tex, targets.Last().Center, projectile.Center, lightColor);
            }
            return true;
        }

        private void DrawBetween(SpriteBatch spriteBatch, Texture2D texture, Vector2 point1, Vector2 point2, Color color)
        {
            for (int k = 0; k < Vector2.Distance(point1, point2) / texture.Width; k++)
            {
                spriteBatch.Draw(texture, Vector2.Lerp(point1, point2, k / (Vector2.Distance(point1, point2) / texture.Width)) - Main.screenPosition, texture.Frame(), color,
                    (point1 - point2).ToRotation(), texture.Size() / 2, 1, 0, 0);
            }
        }
    }
}
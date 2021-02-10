using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using Terraria.ModLoader;
using StarlightRiver.Content.Abilities.Purify.TransformationHelpers;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StarlightRiver.Content.Abilities.Purify
{
    public class Pure : Ability
    {
        public override string Texture => "StarlightRiver/Assets/Abilities/PureCrown";
        public override bool Available => base.Available && !Main.projectile.Any(proj => proj.owner == Player.whoAmI && proj.active && (proj.type == ProjectileType<Purifier>() || proj.type == ProjectileType<PurifierReturn>()));
        public override Color Color => Color.White;
        public override float ActivationCostDefault => 1;

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            return abilityKeys.Get<Pure>().JustPressed;
        }

        public override void OnActivate()
        {
            Main.PlaySound(SoundID.Item37);
        }

        public override void UpdateActive()
        {
            Projectile.NewProjectile(Player.Center + new Vector2(16, -24), Vector2.Zero, ProjectileType<Purifier>(), 0, 0, Player.whoAmI);
            StarlightWorld.PureTiles.Add((Player.Center + new Vector2(16, -24)) / 16);

            Deactivate();
        }
    }

    internal class Purifier : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Corona of Purity");

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public override void SetDefaults()
        {
            projectile.width = 32; 
            projectile.height = 32;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 900;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft <= 800 && projectile.ai[1] == 0 && StarlightRiver.Instance.AbilityKeys.Get<Pure>().JustPressed) //recall logic
            {
                projectile.ai[1] = 1;
                projectile.timeLeft = 150;
                projectile.extraUpdates = 2;
                projectile.netUpdate = true;
            }

            //if (projectile.timeLeft == 900) //activate shader on start
            //Filters.Scene.Activate("PurityFilter", projectile.position).GetShader().UseDirection(new Vector2(0.1f, 0.1f));

            else if (projectile.timeLeft >= 800) //grow       
                projectile.ai[0] += 3;

            else if (projectile.timeLeft < 150) //shrink          
                projectile.ai[0] -= 2;

            //Filters.Scene["PurityFilter"].GetShader().UseProgress((projectile.ai[0] / 255) * 0.125f).UseIntensity((projectile.ai[0] / 255) * 0.006f);

            Dust.NewDust(projectile.Center - Vector2.One * 32, 32, 32, DustType<Dusts.Purify>()); //dusts around crown

            for (int x = -40; x < 40; x++) //tile transformation
                for (int y = -40; y < 40; y++)
                {
                    Vector2 check = projectile.position / 16 + new Vector2(x, y);

                    if (Vector2.Distance(check * 16, projectile.Center) <= projectile.ai[0] - 2)
                        PurifyTransformation.TransformTile((int)check.X, (int)check.Y);
                    else
                        PurifyTransformation.RevertTile((int)check.X, (int)check.Y);

                    //just in case
                    if (projectile.timeLeft == 1)
                        PurifyTransformation.RevertTile((int)check.X, (int)check.Y);
                }

            if (projectile.timeLeft == 1)
            {
                for (int k = 0; k <= 50; k++)
                    Dust.NewDustPerfect(projectile.Center - Vector2.One * 8, DustType<Dusts.Purify2>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2.4f), 0, default, 1.2f);
                Projectile.NewProjectile(projectile.Center - Vector2.One * 16, Vector2.Normalize(projectile.Center - Vector2.One * 16 - Main.player[projectile.owner].Center).RotatedBy(0.3f) * 6,
                    ProjectileType<PurifierReturn>(), 0, 0, projectile.owner);
            }

            else if (projectile.timeLeft == 60)
                if (Filters.Scene["PurityFilter"].IsActive())
                {
                    //Filters.Scene.Deactivate("PurityFilter");
                }
        }

        private readonly Texture2D cirTex = GetTexture("StarlightRiver/Assets/Abilities/ArcaneCircle");
        private readonly Texture2D cirTex2 = GetTexture("StarlightRiver/Assets/Abilities/ArcaneCircle2");

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(cirTex, projectile.Center - Vector2.One * 16 - Main.screenPosition, cirTex.Frame(), Color.White, -(projectile.ai[0] / 300f), cirTex.Size() / 2, projectile.ai[0] / cirTex.Width * 2.1f, 0, 0);
            spriteBatch.Draw(cirTex2, projectile.Center - Vector2.One * 16 - Main.screenPosition, cirTex2.Frame(), Color.White, projectile.ai[0] / 300f, cirTex2.Size() / 2, projectile.ai[0] / cirTex.Width * 2.1f, 0, 0);

            Texture2D tex = GetTexture("StarlightRiver/Assets/Abilities/PureCrown");
            spriteBatch.Draw(tex, projectile.Center + new Vector2(-16, -16 + (float)Math.Sin(StarlightWorld.rottime) * 2) - Main.screenPosition, tex.Frame(),
                Color.White * (projectile.timeLeft < 500 ? 1 : projectile.ai[0] / 250f), 0, tex.Size() / 2, 1, 0, 0);
        }
    }

    internal class PurifierReturn : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 120;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Returning Crown");
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            if (projectile.timeLeft < 120)
                for (int k = 0; k <= 8; k++)
                    Dust.NewDustPerfect(Vector2.Lerp(projectile.position, projectile.oldPosition, k / 8f), DustType<Dusts.Purify>(), Vector2.Zero, 0, default, 2.4f);

            Vector2 target = player.Center + new Vector2(0, -16);
            projectile.velocity += Vector2.Normalize(projectile.Center - target) * -0.8f;

            if (projectile.velocity.Length() >= 6)
                projectile.velocity = Vector2.Normalize(projectile.velocity) * 6f;

            if (projectile.Hitbox.Intersects(new Rectangle((int)player.Center.X - 2, (int)player.Center.Y - 14, 4, 4)) || projectile.timeLeft == 1)
            {
                for (int k = 0; k <= 50; k++)
                    Dust.NewDustPerfect(player.Center + new Vector2(0, -16), DustType<Dusts.Purify2>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(1.4f));
                for (int k = 0; k <= Vector2.Distance(player.Center + new Vector2(0, -16), projectile.position); k++)
                    Dust.NewDustPerfect(Vector2.Lerp(player.Center + new Vector2(0, -16), projectile.Center, k / Vector2.Distance(player.Center + new Vector2(0, -16), projectile.position))
                        , DustType<Dusts.Purify>());

                projectile.timeLeft = 0;
            }
        }
    }
}
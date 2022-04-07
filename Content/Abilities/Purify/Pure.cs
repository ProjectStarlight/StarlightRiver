using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities.Purify.TransformationHelpers;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item37);
        }

        public override void UpdateActive()
        {
            Projectile.NewProjectile(null, Player.Center + new Vector2(16, -24), Vector2.Zero, ProjectileType<Purifier>(), 0, 0, Player.whoAmI); //TODO: centralize / standardize our source Ids instead of null here
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
            Projectile.width = 32; 
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft <= 800 && Projectile.ai[1] == 0 && StarlightRiver.Instance.AbilityKeys.Get<Pure>().JustPressed) //recall logic
            {
                Projectile.ai[1] = 1;
                Projectile.timeLeft = 150;
                Projectile.extraUpdates = 2;
                Projectile.netUpdate = true;
            }

            //if (Projectile.timeLeft == 900) //activate shader on start
            //Filters.Scene.Activate("PurityFilter", Projectile.position).GetShader().UseDirection(new Vector2(0.1f, 0.1f));

            else if (Projectile.timeLeft >= 800) //grow       
                Projectile.ai[0] += 3;

            else if (Projectile.timeLeft < 150) //shrink          
                Projectile.ai[0] -= 2;

            //Filters.Scene["PurityFilter"].GetShader().UseProgress((Projectile.ai[0] / 255) * 0.125f).UseIntensity((Projectile.ai[0] / 255) * 0.006f);

            Dust.NewDust(Projectile.Center - Vector2.One * 32, 32, 32, DustType<Dusts.Purify>()); //dusts around crown

            for (int x = -40; x < 40; x++) //tile transformation
                for (int y = -40; y < 40; y++)
                {
                    Vector2 check = Projectile.position / 16 + new Vector2(x, y);

                    if (Vector2.Distance(check * 16, Projectile.Center) <= Projectile.ai[0] - 2)
                        PurifyTransformation.TransformTile((int)check.X, (int)check.Y);
                    else
                        PurifyTransformation.RevertTile((int)check.X, (int)check.Y);

                    //just in case
                    if (Projectile.timeLeft == 1)
                        PurifyTransformation.RevertTile((int)check.X, (int)check.Y);
                }

            if (Projectile.timeLeft == 1)
            {
                for (int k = 0; k <= 50; k++)
                    Dust.NewDustPerfect(Projectile.Center - Vector2.One * 8, DustType<Dusts.Purify2>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2.4f), 0, default, 1.2f);
                Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center - Vector2.One * 16, Vector2.Normalize(Projectile.Center - Vector2.One * 16 - Main.player[Projectile.owner].Center).RotatedBy(0.3f) * 6,
                    ProjectileType<PurifierReturn>(), 0, 0, Projectile.owner);
            }

            //else if (Projectile.timeLeft == 60)
                //if (Filters.Scene["PurityFilter"].IsActive())
                //{
                    //Filters.Scene.Deactivate("PurityFilter");
                //}
        }

        private readonly Texture2D cirTex = Request<Texture2D>("StarlightRiver/Assets/Abilities/ArcaneCircle").Value;
        private readonly Texture2D cirTex2 = Request<Texture2D>("StarlightRiver/Assets/Abilities/ArcaneCircle2").Value;

        public override void PostDraw(Color lightColor)
        {
            Main.EntitySpriteDraw(cirTex, Projectile.Center - Vector2.One * 16 - Main.screenPosition, cirTex.Frame(), Color.White, -(Projectile.ai[0] / 300f), cirTex.Size() / 2, Projectile.ai[0] / cirTex.Width * 2.1f, 0, 0);
            Main.EntitySpriteDraw(cirTex2, Projectile.Center - Vector2.One * 16 - Main.screenPosition, cirTex2.Frame(), Color.White, Projectile.ai[0] / 300f, cirTex2.Size() / 2, Projectile.ai[0] / cirTex.Width * 2.1f, 0, 0);

            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Abilities/PureCrown").Value;
            Main.EntitySpriteDraw(tex, Projectile.Center + new Vector2(-16, -16 + (float)Math.Sin(StarlightWorld.rottime) * 2) - Main.screenPosition, tex.Frame(),
                Color.White * (Projectile.timeLeft < 500 ? 1 : Projectile.ai[0] / 250f), 0, tex.Size() / 2, 1, 0, 0);
        }
    }

    internal class PurifierReturn : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Returning Crown");
        }

        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            if (Projectile.timeLeft < 120)
                for (int k = 0; k <= 8; k++)
                    Dust.NewDustPerfect(Vector2.Lerp(Projectile.position, Projectile.oldPosition, k / 8f), DustType<Dusts.Purify>(), Vector2.Zero, 0, default, 2.4f);

            Vector2 target = Player.Center + new Vector2(0, -16);
            Projectile.velocity += Vector2.Normalize(Projectile.Center - target) * -0.8f;

            if (Projectile.velocity.Length() >= 6)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 6f;

            if (Projectile.Hitbox.Intersects(new Rectangle((int)Player.Center.X - 2, (int)Player.Center.Y - 14, 4, 4)) || Projectile.timeLeft == 1)
            {
                for (int k = 0; k <= 50; k++)
                    Dust.NewDustPerfect(Player.Center + new Vector2(0, -16), DustType<Dusts.Purify2>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(1.4f));
                for (int k = 0; k <= Vector2.Distance(Player.Center + new Vector2(0, -16), Projectile.position); k++)
                    Dust.NewDustPerfect(Vector2.Lerp(Player.Center + new Vector2(0, -16), Projectile.Center, k / Vector2.Distance(Player.Center + new Vector2(0, -16), Projectile.position))
                        , DustType<Dusts.Purify>());

                Projectile.timeLeft = 0;
            }
        }
    }
}
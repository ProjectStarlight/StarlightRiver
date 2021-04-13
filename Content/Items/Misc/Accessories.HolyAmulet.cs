using StarlightRiver.Core;
using Terraria;
using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Misc
{
    public class HolyAmulet : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public HolyAmulet() : base("Holy Amulet", "Releases bursts of homing energy for every 25 HP healed") { }

        public override bool Autoload(ref string name)
        {
            On.Terraria.Player.HealEffect += HealEffect;

            return true;
        }

        private void HealEffect(On.Terraria.Player.orig_HealEffect orig, Player self, int healAmount, bool broadcast)
        {
            if (Equipped(self))
            {
                self.GetModPlayer<HolyAmuletHealingTracker>().Healed(healAmount);
            }

            orig(self, healAmount, broadcast);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemID.Ruby, 5);
            recipe.AddIngredient(ItemID.LifeCrystal);
            recipe.AddIngredient(ItemID.GoldBar, 10);
            recipe.AddTile(TileID.Anvils);

            recipe.SetResult(this);

            recipe.AddRecipe();

            recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemID.Ruby, 5);
            recipe.AddIngredient(ItemID.LifeCrystal);
            recipe.AddIngredient(ItemID.PlatinumBar, 10);
            recipe.AddTile(TileID.Anvils);

            recipe.SetResult(this);

            recipe.AddRecipe();
        }
    }

    public class HolyAmuletHealingTracker : ModPlayer
    {
        private int cumulativeAmountHealed;

        public void Healed(int amount)
        {
            cumulativeAmountHealed += amount;
        }

        public override void PreUpdate()
        {
            int amountOfProjectiles = cumulativeAmountHealed / 25;

            float step = MathHelper.TwoPi / amountOfProjectiles;

            float rotation = 0;

            while (cumulativeAmountHealed >= 25)
            {
                Projectile.NewProjectile(player.Center, Vector2.UnitX.RotatedBy(rotation) * 16, ModContent.ProjectileType<HolyAmuletOrb>(), 10, 2.5f, player.whoAmI);

                rotation += step;

                cumulativeAmountHealed -= 25;
            }
        }
    }

    // This is basically a carbon copy of the astroflora bow. Consider making a base "TrailProjectile" to cut down on the boilerplate.
    public class HolyAmuletOrb : ModProjectile, IDrawPrimitive
    {
        // 20 Tiles.
        private const float detectionRange = 320;

        private const int oldPositionCacheLength = 24;

        private const int trailMaxWidth = 4;

        public override string Texture => AssetDirectory.Invisible;

        private Trail trail;

        private List<Vector2> cache;

        private int TargetNPCIndex
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        private bool HitATarget
        {
            get => (int)projectile.ai[1] == 1;
            set => projectile.ai[1] = value ? 1 : 0;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;

            projectile.friendly = true;

            projectile.timeLeft = 300;

            projectile.tileCollide = false;

            projectile.penetrate = -1;
        }

        public override void AI()
        {
            // Sync its target.
            projectile.netUpdate = true;

            ManageCaches();

            ManageTrail();

            if (projectile.timeLeft < 30)
            {
                projectile.alpha += 8;
            }

            if (!HitATarget && TargetNPCIndex == -1)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (npc.CanBeChasedBy() && npc.DistanceSQ(projectile.Center) < detectionRange * detectionRange)
                    {
                        TargetNPCIndex = i;

                        break;
                    }
                }
            }
            else if (TargetNPCIndex != -1)
            {
                if (!Main.npc[TargetNPCIndex].CanBeChasedBy())
                {
                    TargetNPCIndex = -1;

                    return;
                }

                Homing(Main.npc[TargetNPCIndex]);
            }
        }

        private void Homing(NPC target)
        {
            Vector2 move = target.Center - projectile.Center;

            AdjustMagnitude(ref move);

            projectile.velocity = (10 * projectile.velocity + move) / 11f;

            AdjustMagnitude(ref projectile.velocity);
        }

        private void AdjustMagnitude(ref Vector2 vector)
        {
            float adjustment = 24;

            float magnitude = vector.Length();

            if (magnitude > adjustment)
            {
                vector *= adjustment / magnitude;
            }
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < oldPositionCacheLength; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > oldPositionCacheLength)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, oldPositionCacheLength, new TriangularTip(trailMaxWidth * 4), factor => factor * trailMaxWidth, factor =>
            {
                // 1 = full opacity, 0 = transparent.
                float normalisedAlpha = 1 - (projectile.alpha / 255f);

                // Scales opacity with the projectile alpha as well as the distance from the beginning of the trail.
                return Color.Crimson * normalisedAlpha * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["Primitives"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["transformMatrix"].SetValue(world * view * projection);

            trail?.Render(effect);
        }

        public override bool? CanHitNPC(NPC target)
            => TargetNPCIndex != -1 && !HitATarget && Main.npc[TargetNPCIndex] == target;

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            projectile.timeLeft = 30;

            HitATarget = true;

            // This is hacky, but it lets the projectile keep its rotation without having to make an extra variable to cache it after it hits a target and "stops".
            projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 0.0001f;
        }

        public override void Kill(int timeLeft)
        {
            trail?.Dispose();
        }
    }
}
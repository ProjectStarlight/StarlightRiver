using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
	class CeirosExpert : SmartAccessory
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public CeirosExpert() : base("Shattered Aegis", "Releases a burning ring when damaged\n'Meet your foes head-on, and give them a scorching embrace'") { }

        public override void SafeSetDefaults()
        {
            item.rare = ItemRarityID.Orange;
            item.accessory = true;
            item.width = 32;
            item.height = 32;
        }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreHurtEvent += PreHurtKnockback;
            return true;
        }

        private bool PreHurtKnockback(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (Equipped(player))
            {
                Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<FireRing>(), 100, 0, player.whoAmI);
            }

            return true;
        }
    }

    class FireRing : ModProjectile, IDrawPrimitive
	{
        private List<Vector2> cache;
        private Trail trail;

        public float TimeFade => 1 - projectile.timeLeft / 30f;
        public float Radius => (30 - projectile.timeLeft) * 5;

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
		{
            projectile.friendly = true;
            projectile.width = 1;
            projectile.height = 1;
            projectile.tileCollide = false;
            projectile.timeLeft = 30;
		}

		public override void AI()
		{
            ManageCaches(ref cache);
            ManageTrail(ref trail, cache, 80);
        }

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            return Helper.CheckCircularCollision(projectile.Center, (int)Radius, targetHitbox);
		}

		private void ManageCaches(ref List<Vector2> cache)
        {
            if (cache is null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 20; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            for(int k = 0; k < 20; k++)
			{
                cache.Add(projectile.Center + Vector2.One.RotatedBy(k / 20f * 6.28f) * Radius);
			}

            while (cache.Count > 20)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail(ref Trail trail, List<Vector2> cache, int width)
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(40 * 4), factor => width * (float)Math.Sin(TimeFade * 3.14f), factor =>
            {
                return new Color(255, 150 + (int)(factor.X * 70), 135) * (float)Math.Sin(TimeFade * 3.14f);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + Vector2.One * Radius;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/EnergyTrail"));

            trail?.Render(effect);
        }

    }
}

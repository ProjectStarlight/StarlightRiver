using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Dungeon
{
    class Cloudstrike : ModItem
    {
        int charge;

        int counter = 0;
        public override string Texture => AssetDirectory.DungeonItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cloudstrike");

            Tooltip.SetDefault("Update this later");
        }

        public override void SetDefaults()
        {
            item.damage = 32;
            item.useTime = 5;
            item.useAnimation = 5;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.magic = true;
            item.mana = 100;
            item.shoot = ModContent.ProjectileType<CloudstrikeShot>();
            item.shootSpeed = 10;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.Orange;
            item.autoReuse = true;
            item.channel = true;
            item.noMelee = true;
        }
        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            mult = (float)Math.Sqrt(charge / 100f);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 dir = Vector2.Normalize(new Vector2(speedX, speedY));
            Vector2 pos = position + (dir * 75) + (dir.RotatedBy(-player.direction * 1.57f) * 5);
            Projectile.NewProjectile(pos, new Vector2(speedX, speedY).RotatedBy(Main.rand.NextFloat(-0.2f,0.2f)), type, damage, knockBack, player.whoAmI, charge);
            //Dust.NewDustPerfect(pos, DustID.Electric, dir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(5));
            if (charge > 60)
                player.velocity -= dir * (float)Math.Sqrt(charge - 60);
            charge = 1;
            return false;
        }

        public override void HoldItem(Player player)
        {
            counter++;
            if (charge < 100 && !player.channel)
            {
                charge++;
                if (charge == 100)
                {
                    //full charge effects here
                }
            }
            if (Main.rand.NextBool((int)(50 / (float)Math.Sqrt(charge))) && !player.channel)
            {
                Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Vector2 offset = Main.rand.NextBool() ? new Vector2(Main.rand.Next(-10, 10), player.height / 2) : dir * Main.rand.NextFloat(30);
                Projectile proj = Projectile.NewProjectileDirect(player.Center + offset, dir.RotatedBy(Main.rand.NextFloat(-1,1)) * 5, ModContent.ProjectileType<CloudstrikeShot>(), 0, 0, player.whoAmI, 0.01f, 2);
                var mp = proj.modProjectile as CloudstrikeShot;
                mp.velocityMult = Main.rand.Next(1,4);
                mp.baseColor = charge == 100 ? new Color(200, 230, 255) : Color.Violet;
                if (charge == 100 && Main.rand.NextBool(10))
                    mp.baseColor = Color.Cyan;
            }

            item.damage = (int)MathHelper.Lerp(20, 70, charge / 100f);
            base.HoldItem(player);
        }

    }

    public class CloudstrikeShot : ModProjectile, IDrawAdditive, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.DungeonItem + "Cloudstrike";

        private float charge => projectile.ai[0];

        private float chargeSqrt => (float)Math.Sqrt(charge);

        private int reach => ((int)charge * 5) + 100;

        private bool initialized = false;

        private bool reachedMouse = false;

        private int power => (int)(chargeSqrt * 3) + 10;

        private Player player => Main.player[projectile.owner];

        private List<Vector2> cache;
        private List<Vector2> cache2;
        private Trail trail;

        private Trail trail2;

        private List<NPC> hitTargets = new List<NPC>();
        private NPC target = default;

        private Vector2 startPoint = Vector2.Zero;

        private Vector2 mousePos = Vector2.Zero;

        private float curve;

        private bool hitNPC = false;

        private Vector2 oldVel = Vector2.Zero;

        public int velocityMult = 10;

        public Color baseColor = new Color(200, 230, 255);
        public Color endColor = Color.Purple;

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.timeLeft = 60;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.magic = true;
            projectile.extraUpdates = 14;
            projectile.penetrate = -1;
            projectile.hide = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electro Shock");
        }

        public override void AI()
        {
            if (!initialized)
            {
                startPoint = projectile.Center;
                ManageCaches();
                initialized = true;
                if (projectile.ai[1] == 0)
                {
                    projectile.timeLeft = (int)(Math.Sqrt(chargeSqrt) * 30) + (projectile.ai[1] == 2 ? 25 : 45);
                    mousePos = Main.MouseWorld;
                }
                player.GetModPlayer<StarlightPlayer>().Shake += (int)chargeSqrt;
            }

            if (Main.netMode != NetmodeID.Server && (projectile.timeLeft % 4 == 0 || projectile.timeLeft <= 25))
            {
                if (projectile.timeLeft % 2 == 0)
                    ManageCaches();
                ManageTrails();
            }
            if (projectile.timeLeft > 36 && projectile.ai[1] != 2)
                player.itemTime = player.itemAnimation = (int)(chargeSqrt + 1) * 3;

            if (projectile.timeLeft <= 25)
            {
                projectile.velocity = Vector2.Zero;
                projectile.extraUpdates = 0;
                return;
            }

            var temptarget = Main.npc.Where(x => x.active && !x.townNPC /*&& !x.immortal && !x.dontTakeDamage /&& !x.friendly*/ && !hitTargets.Contains(x) && x.Distance(projectile.Center) < reach).OrderBy(x => x.Distance(projectile.Center)).FirstOrDefault();

           /* if (hitNPC && temptarget == default)
            {
                ManageCaches();
                projectile.timeLeft = 25;
                return;
            }*/

            oldVel = projectile.velocity / 6;
            if (Main.rand.NextBool(30))
            {
                target = temptarget;
            }
            if (hitTargets.Contains(target))
                target = default;

            Vector2 rotToBe = Vector2.One;
            float distance = 100;
            if (target != default)
            {
                Vector2 dir = target.Center - projectile.Center;
                distance = dir.Length();
                rotToBe = Vector2.Normalize(dir);
            }
            else
            {
                if (projectile.ai[1] != 0)
                {
                    if (Main.rand.NextBool(4))
                        mousePos = projectile.Center + (projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * 30);
                }
                else if (Main.rand.NextBool(6))
                    curve = Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 dir = Vector2.Zero;
                if (!reachedMouse)
                {
                    dir = mousePos - projectile.Center;
                    if (dir.Length() < 30)
                        reachedMouse = true;
                }
                else
                {
                    dir = mousePos - player.Center;
                }
                distance = dir.Length();
                rotToBe = Vector2.Normalize(dir).RotatedBy(curve);
            }

            if (projectile.ai[1] == 2)
            {
                Vector2 dir2 = (player.Center + Main.rand.NextVector2Circular(12,12)) - projectile.Center;
                distance = dir2.Length();
                rotToBe = Vector2.Normalize(dir2).RotatedBy(curve * 3);
            }

            float rotDifference = ((((rotToBe.ToRotation() - projectile.velocity.ToRotation()) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

            float lerper = MathHelper.Lerp(0.55f, 0.35f, MathHelper.Min(1, distance / 300f));
            if (projectile.ai[1] == 2)
                lerper /= 3;
            float rot = MathHelper.Lerp(projectile.velocity.ToRotation(), projectile.velocity.ToRotation() + rotDifference, lerper);
            projectile.velocity = rot.ToRotationVector2() * velocityMult;

            if (Main.rand.NextBool((int)charge + 50) && projectile.ai[1] == 0)
            {
                Projectile proj = Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)), ModContent.ProjectileType<CloudstrikeShot>(), projectile.damage, projectile.knockBack, player.whoAmI, charge, 1);
                proj.timeLeft = (int)((projectile.timeLeft - 25) * 0.75f) + 25;
                var modProj = proj.modProjectile as CloudstrikeShot;
                modProj.mousePos = proj.Center + (proj.velocity * 30);
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (projectile.timeLeft < 25)
                return false;
            return base.CanHitNPC(target);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //ManageCaches(true);
            hitNPC = true;
            hitTargets.Add(target);

            int dustType = charge > 50 ? ModContent.DustType<CloudstrikeStarDust>() : ModContent.DustType<CloudstrikeCircleDust>();
            Dust.NewDustPerfect(projectile.Center, dustType, Vector2.Zero, 0, default, (float)Math.Pow(chargeSqrt, 0.3f));

            for (int i = 0; i < 20; i++)
                Dust.NewDustPerfect(target.Center + new Vector2(0, 30), ModContent.DustType<Dusts.GlowLine>(), Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat() * 6, 0, new Color(100, 150, 200) * (power / 30f), 0.5f);

            for (int j = 0; j < 6; j++)
                Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CloudstrikeCircleDust>(), Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat() * 3, 0, default, (float)Math.Pow(chargeSqrt, 0.3f) * 0.3f);
            base.OnHitNPC(target, damage, knockback, crit);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.timeLeft > 25)
            {
                ManageCaches();
                projectile.timeLeft = 25;
            }
            return false;
        }

        private void ManageCaches(bool addPoint = false)
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            if (projectile.timeLeft > 25 || addPoint)
                cache.Add(projectile.Center);

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }

            cache2 = new List<Vector2>();
            for (int i = 0; i < cache.Count; i++)
            {
                Vector2 point = cache[i];
                Vector2 nextPoint = i == cache.Count - 1 ? projectile.Center + oldVel : cache[i + 1];
                Vector2 dir = Vector2.Normalize(nextPoint - point).RotatedBy(Main.rand.NextBool() ? -1.57f : 1.57f);
                if (i > cache.Count - 3 || dir == Vector2.Zero)
                    cache2.Add(point);
                else
                    cache2.Add(point + (dir * Main.rand.NextFloat(5) * (float)Math.Sqrt(chargeSqrt)));
            }
        }

        private void ManageTrails()
        {
            int sparkMult = projectile.ai[1] == 2 ? 6 : 1;
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(4), factor => sparkMult * Main.rand.NextFloat(0.75f,1.25f) * 16 * (float)Math.Pow(chargeSqrt, 0.7f), factor =>
            {
                if (factor.X > 0.99f)
                    return Color.Transparent;

                return new Color(160, 220, 255) * (projectile.extraUpdates == 0 ? EaseFunction.EaseCubicOut.Ease(projectile.timeLeft / 25f) : 1) * 0.1f * EaseFunction.EaseCubicOut.Ease(1 - factor.X);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + oldVel;
            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(4), factor => sparkMult * 3 * (float)Math.Pow(chargeSqrt, 0.7f) * Main.rand.NextFloat(0.75f, 1.25f), factor =>
            {
                float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
                return Color.Lerp(baseColor, endColor, EaseFunction.EaseCubicIn.Ease(1 - progress)) * (projectile.extraUpdates == 0 ? EaseFunction.EaseCubicOut.Ease(projectile.timeLeft / 25f) : 1) * progress;
            });

            trail2.Positions = cache2.ToArray();
            trail2.NextPosition = projectile.Center;
        }
        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(1f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));

            trail?.Render(effect);
            trail2?.Render(effect);
        }

        public void DrawAdditive(SpriteBatch sb)
        {
            var point1 = startPoint;

            if (point1 == Vector2.Zero)
                return;

            var tex = ModContent.GetTexture("StarlightRiver/Assets/GlowTrail");

            var tex2 = ModContent.GetTexture(AssetDirectory.Assets + "Keys/GlowSoft");

            var color = new Color(200, 230, 255) * (projectile.extraUpdates == 0 ? EaseFunction.EaseCubicOut.Ease(projectile.timeLeft / 25f) : 1);
            sb.Draw(tex2, startPoint - Main.screenPosition, null, color, 0, tex2.Size() / 2, (float)MathHelper.Lerp(1,12,charge / 100f) * (projectile.ai[1] == 0 ? 0.5f : 0.25f), SpriteEffects.None, 0f);

            for (int k = 1; k < cache2.Count; k++)
            {
                Vector2 prevPos = k == 1 ? point1 : cache2[k - 1];

                var target = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(cache2[k], prevPos) + 2, (int)(power * Main.rand.NextFloat(0.65f,1.35f)));
                var origin = new Vector2(0, tex.Height / 2);
                var rot = (cache2[k] - prevPos).ToRotation();

                //sb.Draw(tex, target, null, color, rot, origin, 0, 0);

                if (Main.rand.Next(40) == 0)
                    Dust.NewDustPerfect(prevPos + new Vector2(0, 30), ModContent.DustType<Dusts.GlowLine>(), Vector2.Normalize(cache2[k] - prevPos) * Main.rand.NextFloat(-3, -2), 0, baseColor * (power / 30f), 0.5f);
            }
        }
    }

    class CloudstrikeStarDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Keys/GlowSoft";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.color = Color.Transparent;
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
           // dust.rotation = Main.rand.NextFloat(6.28f);
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.color == Color.Transparent)
                dust.position -= Vector2.One * 32 * dust.scale;

            //dust.rotation += dust.velocity.Y * 0.1f;
            dust.position += dust.velocity;

            dust.color = new Color(200, 230, 255);
            dust.shader.UseColor(dust.color * (1 - (dust.alpha / 255f)));

            dust.alpha += 15;
            if (dust.velocity == Vector2.Zero)
                dust.alpha += 10;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.4f * dust.scale);

            if (dust.alpha > 255)
                dust.active = false;

            return false;
        }
    }
    class CloudstrikeCircleDust : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Keys/GlowSoft";
            return true;
        }

        public override void OnSpawn(Dust dust)
        {
            dust.color = Color.Transparent;
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            //dust.rotation = Main.rand.NextFloat(6.28f);
            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.color == Color.Transparent)
                dust.position -= Vector2.One * 32 * dust.scale;

            //dust.rotation += dust.velocity.Y * 0.1f;
            dust.position += dust.velocity;

            dust.color = new Color(200, 230, 255);
            dust.shader.UseColor(dust.color * (1 - (dust.alpha / 255f)));

            dust.alpha += 18;
            if (dust.velocity == Vector2.Zero)
                dust.alpha += 7;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.4f * dust.scale);

            if (dust.alpha > 255)
                dust.active = false;

            return false;
        }
    }
}
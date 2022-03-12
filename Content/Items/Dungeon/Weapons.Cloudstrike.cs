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
        public const int MAXCHARGE = 120;

        private int charge; //How much charge the weapon has (out of MAXCHARGE)

        private float chargeRatio => charge / (float)MAXCHARGE;
        public override string Texture => AssetDirectory.DungeonItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cloudstrike");

            Tooltip.SetDefault("Accumulate electrical charge while not firing\n" +
            "Damage and range of your next shot increases with charge\n" +
            "'Meet this storm of sound and fury, till thunder-clashes fade to silence'");
        }

        public override void SetDefaults()
        {
            item.damage = 45;
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
            mult = (float)Math.Sqrt(chargeRatio);
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

            player.GetModPlayer<StarlightPlayer>().Shake += (int)(Math.Sqrt(charge) * 2);

            //Dust.NewDustPerfect(pos, DustID.Electric, dir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(5));
            if (charge > 60)
                player.velocity -= dir * (float)Math.Sqrt(charge - 60);
            charge = 1;
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (charge < MAXCHARGE && !player.channel)
            {
                charge++;
                if (charge == MAXCHARGE)
                {
                    for (int i = 0; i < 12; i++)
                        CreateStatic(charge, player, true);
                }
            }
            if (Main.rand.NextBool((int)(50 / (float)Math.Sqrt(charge))) && !player.channel)
            {
                CreateStatic(charge, player);
            }

            item.damage = (int)MathHelper.Lerp(25, 100, chargeRatio);
            base.HoldItem(player);
        }

        private static void CreateStatic(int charge, Player player, bool fullCharge = false)
        {
            Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2();
            Vector2 offset = Main.rand.NextBool(4) ? dir * Main.rand.NextFloat(30) : new Vector2(Main.rand.Next(-35, 35), player.height / 2);

            float smalLCharge = fullCharge ? 0.5f : 0.01f;

            Projectile proj = Projectile.NewProjectileDirect(player.Center + offset, dir.RotatedBy(Main.rand.NextFloat(-1, 1)) * 5, ModContent.ProjectileType<CloudstrikeShot>(), 0, 0, player.whoAmI, smalLCharge, 2);
            var mp = proj.modProjectile as CloudstrikeShot;
            mp.velocityMult = Main.rand.Next(1, 4);

            if (fullCharge)
                mp.thickness = 0.45f;

            mp.baseColor = charge == MAXCHARGE ? new Color(200, 230, 255) : Color.Violet;
            if (charge == MAXCHARGE && Main.rand.NextBool(10))
                mp.baseColor = Color.Cyan;
        }

    }

    public class CloudstrikeShot : ModProjectile, IDrawAdditive, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.DungeonItem + "Cloudstrike";

        public bool followPlayer = false;

        public float thickness = 1;

        public int velocityMult = 10;

        public Color baseColor = new Color(200, 230, 255);
        public Color endColor = Color.Purple;

        private bool initialized = false;

        private bool reachedMouse = false;

        private List<Vector2> cache;
        private List<Vector2> cache2;
        private Trail trail;
        private Trail trail2;

        private List<NPC> hitTargets = new List<NPC>();
        private NPC target = default;

        private Vector2 startPoint = Vector2.Zero;

        private Vector2 mousePos = Vector2.Zero;

        private float curve;

        private Vector2 oldVel = Vector2.Zero;

        private Vector2 oldPlayerPos = Vector2.Zero;
        private float oldRotation = 0f;

        private float charge => projectile.ai[0];

        private float chargeSqrt => (float)Math.Sqrt(charge);

        private int reach => ((int)charge * 5) + 100;

        private int power => (int)(chargeSqrt * 3) + 10;

        private Player player => Main.player[projectile.owner];

        private float fade => projectile.extraUpdates == 0 ? EaseFunction.EaseCubicOut.Ease(projectile.timeLeft / 25f) : 1;

        private bool miniature => projectile.ai[1] == 2; //If this is true, it's a spark created around the player
        private bool branch => projectile.ai[1] == 1; //If this is true, it's a branch of the main stream. This means it has a smaller starting ball + can't make further branches

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
                if (!branch && !miniature)
                {
                    projectile.timeLeft = (int)(Math.Sqrt(chargeSqrt) * 20) + 45;
                    mousePos = Main.MouseWorld;
                }

                if (charge < 10 && !(branch || miniature))
                    followPlayer = true;

                oldRotation = (Main.MouseWorld - player.Center).ToRotation();
                oldPlayerPos = player.Center;

            }

            if (Main.netMode != NetmodeID.Server && (projectile.timeLeft % 4 == 0 || projectile.timeLeft <= 25))
            {
                if (projectile.timeLeft % 2 == 0)
                    ManageCaches();
                ManageTrails();
            }
            if (projectile.timeLeft > 36 && !miniature)
                player.itemTime = player.itemAnimation = (int)(chargeSqrt + 1) * 3;

            foreach (Vector2 point in cache)
            {
                Lighting.AddLight(point, baseColor.ToVector3() * (float)Math.Sqrt(chargeSqrt) * 0.3f * fade);
            }

            for (int k = 1; k < cache2.Count; k++)
            {
                if (Main.rand.Next(40 * (projectile.extraUpdates + 1)) == 0)
                {
                    Vector2 prevPos = k == 1 ? startPoint : cache2[k - 1];
                    Dust.NewDustPerfect(prevPos + new Vector2(0, 30), ModContent.DustType<CloudstrikeGlowLine >(), Vector2.Normalize(cache2[k] - prevPos) * Main.rand.NextFloat(-3, -2), 0, baseColor * (power / 30f), 0.5f);
                }
            }

            if (projectile.timeLeft <= 25)
            {
                projectile.velocity = Vector2.Zero;
                projectile.extraUpdates = 0;
                return;
            }

            oldVel = projectile.velocity / 6;

            CalculateTarget();

            CalculateVelocity();

            if (Main.rand.NextBool((int)charge + 50) && !(branch || miniature)) //damaging, large branches
            {
                Projectile proj = Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)), ModContent.ProjectileType<CloudstrikeShot>(), projectile.damage, projectile.knockBack, player.whoAmI, charge, 1);
                proj.timeLeft = (int)((projectile.timeLeft - 25) * 0.75f) + 25;

                var modProj = proj.modProjectile as CloudstrikeShot;
                modProj.mousePos = proj.Center + (proj.velocity * 30);
                modProj.followPlayer = followPlayer;
            }

            if (Main.rand.NextBool(10 + (int)Math.Sqrt((Cloudstrike.MAXCHARGE + 2) - charge)) && !(branch || miniature)) //small, non damaging branches
            {
                Projectile proj = Projectile.NewProjectileDirect(projectile.Center, projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)), ModContent.ProjectileType<CloudstrikeShot>(), 0, 0, player.whoAmI, 1, 1);
                proj.timeLeft = Math.Min(Main.rand.Next(40,70), projectile.timeLeft);

                var modProj = proj.modProjectile as CloudstrikeShot;
                modProj.mousePos = proj.Center + (proj.velocity * 30);
                modProj.followPlayer = followPlayer;
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
            hitTargets.Add(target);
            Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CloudstrikeCircleDust>(), Vector2.Zero, 0, default, (float)Math.Pow(chargeSqrt, 0.3f));

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
            int sparkMult = miniature ? 6 : 1;
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(4), factor => thickness * sparkMult * Main.rand.NextFloat(0.75f,1.25f) * 16 * (float)Math.Pow(chargeSqrt, 0.7f), factor =>
            {
                if (factor.X > 0.99f)
                    return Color.Transparent;

                return new Color(160, 220, 255) * fade * 0.1f * EaseFunction.EaseCubicOut.Ease(1 - factor.X);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + oldVel;
            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(4), factor => thickness * sparkMult * 3 * (float)Math.Pow(chargeSqrt, 0.7f) * Main.rand.NextFloat(0.55f, 1.45f), factor =>
            {
                float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
                return Color.Lerp(baseColor, endColor, EaseFunction.EaseCubicIn.Ease(1 - progress)) * fade * progress;
            });

            trail2.Positions = cache2.ToArray();
            trail2.NextPosition = projectile.Center;
        }
        public void DrawPrimitives()
        {
            if (followPlayer)
            {
                UpdatePosition();
                UpdateRotation();
            }
            if (trail != null)
            {
                trail.Positions = cache.ToArray();
                trail.NextPosition = projectile.Center + oldVel;
            }

            if (trail2 != null)
            {
                trail2.Positions = cache2.ToArray();
                trail2.NextPosition = projectile.Center;
            }
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
            if (followPlayer)
            {
                UpdatePosition();
                UpdateRotation();
            }

            var point1 = startPoint;

            if (point1 == Vector2.Zero)
                return;

            if (branch)
                return;

            var tex = ModContent.GetTexture(AssetDirectory.Assets + "Keys/GlowSoft");

            var color = new Color(200, 230, 255) * fade;
            sb.Draw(tex, startPoint - Main.screenPosition, null, color, 0, tex.Size() / 2, (float)MathHelper.Lerp(1, 12, charge / (float)Cloudstrike.MAXCHARGE) * ((branch || miniature) ? 0.25f : 0.5f), SpriteEffects.None, 0f);
        }

        private void CalculateTarget()
        {
            var temptarget = Main.npc.Where(x => x.active && !x.townNPC && !x.immortal && !x.dontTakeDamage && !x.friendly && !hitTargets.Contains(x) && x.Distance(projectile.Center) < reach).OrderBy(x => x.Distance(projectile.Center)).FirstOrDefault();

            if (temptarget != default && Main.rand.NextBool((int)Math.Sqrt(temptarget.Distance(projectile.Center)) + 1))
                target = temptarget;

            if (hitTargets.Contains(target))
                target = default;
        }

        private void CalculateVelocity()
        {
            Vector2 rotToBe;
            float distance;
            if (target != default)
            {
                Vector2 dir = target.Center - projectile.Center;
                distance = dir.Length();
                rotToBe = Vector2.Normalize(dir);
            }
            else
            {
                if (branch || miniature)
                {
                    if (Main.rand.NextBool(4))
                        mousePos = projectile.Center + (projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * 30);
                }
                else if (Main.rand.NextBool(6))
                    curve = Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 dir;
                if (!reachedMouse)
                {
                    dir = mousePos - projectile.Center;

                    Vector2 pToM = mousePos - player.Center;
                    Vector2 pToL = projectile.Center - player.Center;
                    if (pToL.Length() > pToM.Length())
                        reachedMouse = true;
                }
                else
                {
                    dir = mousePos - player.Center;
                }
                distance = dir.Length();
                rotToBe = Vector2.Normalize(dir).RotatedBy(curve);
            }

            if (miniature)
            {
                Vector2 dir2 = (player.Center + Main.rand.NextVector2Circular(12, 12)) - projectile.Center;
                distance = dir2.Length();
                rotToBe = Vector2.Normalize(dir2).RotatedBy(curve * 3);
            }

            float rotDifference = ((((rotToBe.ToRotation() - projectile.velocity.ToRotation()) % 6.28f) + 9.42f) % 6.28f) - 3.14f;

            float lerper = MathHelper.Lerp(0.55f, 0.35f, MathHelper.Min(1, distance / 300f));
            if (miniature)
                lerper /= 3;
            float rot = MathHelper.Lerp(projectile.velocity.ToRotation(), projectile.velocity.ToRotation() + rotDifference, lerper);
            projectile.velocity = rot.ToRotationVector2() * velocityMult;
        }

        private void UpdatePosition()
        {
            if (!followPlayer)
                return;
            Vector2 center = player.Center;
            if (oldPlayerPos != center)
            {
                Vector2 offset = center - oldPlayerPos;
                oldPlayerPos = center;

                startPoint += offset;
                projectile.Center += offset;
                if (cache != null)
                {
                    for (int i = 0; i < cache.Count; i++)
                        cache[i] += offset;
                }

                if (cache2 != null)
                {
                    for (int i = 0; i < cache2.Count; i++)
                        cache2[i] += offset;
                }
            }
        }

        private void UpdateRotation()
        {
            if (!followPlayer)
                return;
            float rot = (Main.MouseWorld - player.Center).ToRotation();
            if (rot != oldRotation)
            {
                float difference = rot - oldRotation;

                startPoint = (startPoint - player.Center).RotatedBy(difference) + player.Center;
                projectile.Center = (projectile.Center - player.Center).RotatedBy(difference) + player.Center;

                if (cache != null)
                {
                    for (int i = 0; i < cache.Count; i++)
                        cache[i] = (cache[i] - player.Center).RotatedBy(difference) + player.Center;
                }

                if (cache2 != null)
                {
                    for (int i = 0; i < cache2.Count; i++)
                        cache2[i] = (cache2[i] - player.Center).RotatedBy(difference) + player.Center;
                }

                oldRotation = rot;
            }
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

    class CloudstrikeGlowLine : ModDust
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.VitricBoss + "RoarLine";
            return true;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * MathHelper.Min(1, dust.fadeIn / 20f);
        }

        public override void OnSpawn(Dust dust)
        {
            dust.fadeIn = 0;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 8, 128);

            dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.GetEffect("Effects/GlowingDust")), "GlowingDustPass");
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is null)
            {
                dust.position -= new Vector2(4, 64) * dust.scale;
                dust.customData = 1;
            }

            dust.rotation = dust.velocity.ToRotation() + 1.57f;
            dust.position += dust.velocity;

            dust.velocity *= 0.98f;
            dust.color *= 0.95f;

            dust.shader.UseColor(dust.color);
            dust.fadeIn+= 2;

            Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

            if (dust.fadeIn > 60)
                dust.active = false;
            return false;
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	public class RefractiveBlade : ModItem
    {
        public int combo;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override bool AltFunctionUse(Player player) => true;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Refractive Blade");
            Tooltip.SetDefault("Hold RMB down to charge a laser\nEnemies struck by the laser are more vulnerable to melee damage");
        }

        public override void SetDefaults()
        {
            item.damage = 34;
            item.width = 60;
            item.height = 60;
            item.useTime = 22;
            item.useAnimation = 22;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.melee = true;
            item.noMelee = true;
            item.knockBack = 7;
            item.useTurn = false;
            item.value = Item.sellPrice(0, 2, 20, 0);
            item.rare = ItemRarityID.Orange;
            item.shoot = ProjectileType<RefractiveBladeProj>();
            item.shootSpeed = 0.1f;
            item.noUseGraphic = true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
                item.useStyle = ItemUseStyleID.HoldingOut;
            else
                item.useStyle = ItemUseStyleID.SwingThrow;
            return true;
        }

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
            if(player.altFunctionUse == 2)
			{
                if(!Main.projectile.Any(n => n.active && n.type == ProjectileType<RefractiveBladeLaser>() && n.owner == player.whoAmI))
                    Projectile.NewProjectile(position, new Vector2(speedX, speedY), ProjectileType<RefractiveBladeLaser>(), (int)(damage * 0.05f), knockBack, player.whoAmI, 0, 120);

                return false;
			}

            if (!Main.projectile.Any(n => n.active && n.type == ProjectileType<RefractiveBladeLaser>() && n.owner == player.whoAmI))
                Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, 0, combo);

            combo++;

            if (combo > 1)
                combo = 0;

            return false;
		}
	}

    public class RefractiveBladeProj : ModProjectile, IDrawPrimitive
    {
        int direction = 0;
        float maxTime = 0;
        float maxAngle = 0;

		private List<Vector2> cache;
        private Trail trail;

        public override string Texture => AssetDirectory.VitricItem + "RefractiveBlade";

        public ref float StoredAngle => ref projectile.ai[0];
        public ref float Combo => ref projectile.ai[1];

        public float Timer => 300 - projectile.timeLeft;
        public Player Owner => Main.player[projectile.owner];
        public float SinProgress => (float)Math.Sin((1 - Timer / maxTime) * 3.14f);

        public sealed override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.melee = true;
            projectile.width = projectile.height = 2;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.alpha = 255;

            projectile.timeLeft = 300;
        }

		public override void AI()
		{
            if (Timer == 0)
            {
                StoredAngle = projectile.velocity.ToRotation();
                projectile.velocity *= 0;

                Helper.PlayPitched("Effects/FancySwoosh", 1, Combo);

                switch(Combo)
				{
                    case 0:
                        direction = 1;
                        maxTime = 22;
                        maxAngle = 4;
                        break;
                    case 1:
                        direction = -1;
                        maxTime = 15;
                        maxAngle = 2;
                        break;
				}
            }

            float targetAngle = StoredAngle + (-(maxAngle / 2) + Helper.BezierEase(Timer / maxTime) * maxAngle) * Owner.direction * direction;

            projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(targetAngle) * (70 + (float)Math.Sin(Helper.BezierEase(Timer / maxTime) * 3.14f) * 40);
            projectile.rotation = targetAngle + 1.57f * 0.5f;

            ManageCaches();
            ManageTrail();

            var color = new Color(255, 140 + (int)(40 * SinProgress), 105);

            Lighting.AddLight(projectile.Center, color.ToVector3() * SinProgress);

            if(Main.rand.Next(2) == 0)
                Dust.NewDustPerfect(projectile.Center, DustType<Glow>(), Vector2.UnitY.RotatedByRandom(0.5f) * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.2f);

            if (Timer >= maxTime)
                projectile.timeLeft = 0;
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.velocity += Vector2.UnitX.RotatedBy((target.Center - Owner.Center).ToRotation()) * 10 * target.knockBackResist;

            Helper.CheckLinearCollision(Owner.Center, projectile.Center, target.Hitbox, out Vector2 hitPoint); //here to get the point of impact, ideally we dont have to do this twice but for some reasno colliding hook dosent have an actual npc ref, soo...

            if (Helper.IsFleshy(target))
            {
                Helper.PlayPitched("Impacts/FireBladeStab", 0.3f, -0.2f, projectile.Center);

                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(hitPoint, DustType<Glow>(), Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.25f) * Main.rand.NextFloat(5), 0, new Color(255, 105, 105), 0.5f);

                    Dust.NewDustPerfect(hitPoint, DustID.Blood, Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(2, 8), 0, default, Main.rand.NextFloat(1, 2));
                    Dust.NewDustPerfect(hitPoint, DustID.Blood, Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(3, 15), 0, default, Main.rand.NextFloat(1, 2));
                }
            }

            else
            {
                Helper.PlayPitched("Impacts/Clink", 0.5f, 0, projectile.Center);

                for (int k = 0; k < 30; k++)
                {
                    Dust.NewDustPerfect(hitPoint, DustType<Glow>(), Vector2.Normalize(hitPoint - Owner.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 8), 0, new Color(255, Main.rand.Next(130, 255), 80), Main.rand.NextFloat(0.3f, 0.7f));
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            if (Helper.CheckLinearCollision(Owner.Center, projectile.Center, targetHitbox, out Vector2 hitPoint))
                return true;

            return false;
        }

        public override bool? CanCutTiles() => true;

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile; //copypasted from example solar eruption with slight changes :trollge:
            Utils.PlotTileLine(Owner.Center, projectile.Center, (projectile.width + projectile.height) * 0.5f * projectile.scale, DelegateMethods.CutTiles);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
            var tex = GetTexture(Texture);
            var texGlow = GetTexture(Texture + "Glow");

            float targetAngle = StoredAngle + (-(maxAngle / 2) + Helper.BezierEase(Timer / maxTime) * maxAngle) * Owner.direction * direction;
            var pos = Owner.Center + Vector2.UnitX.RotatedBy(targetAngle) * ((float)Math.Sin(Helper.BezierEase(Timer / maxTime) * 3.14f) * 20) - Main.screenPosition;

            spriteBatch.Draw(tex, pos, null, lightColor, projectile.rotation, new Vector2(0, tex.Height), 1.1f, 0, 0);
            spriteBatch.Draw(texGlow, pos, null, Color.White, projectile.rotation, new Vector2(0, texGlow.Height), 1.1f, 0, 0);

            return false;
		}

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 10; i++)
                {
                    cache.Add(Vector2.Lerp(projectile.Center, Owner.Center, 0.15f));
                }
            }

            cache.Add(Vector2.Lerp(projectile.Center, Owner.Center, 0.15f));

            while (cache.Count > 10)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => factor * (50 + 40 * Timer / maxTime), factor =>
            {
                if (factor.X >= 0.8f)
                    return Color.White * 0;

                return new Color(255, 120 + (int)(factor.X * 70), 80) * (factor.X * SinProgress );
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Vector2.Lerp(projectile.Center, Owner.Center, 0.15f) + projectile.velocity;
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
            effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/EnergyTrail"));

            trail?.Render(effect);
        }
    }

	public class RefractiveBladeLaser : ModProjectile, IDrawAdditive
	{
        public Vector2 endPoint;
        public float LaserRotation;
        public bool firing;

        public ref float Charge => ref projectile.ai[0];
        public ref float MaxTime => ref projectile.ai[1];

        public int LaserTimer => (int)MaxTime - projectile.timeLeft;
        public Player Owner => Main.player[projectile.owner];

        public override string Texture => AssetDirectory.VitricItem + "RefractiveBlade";

        public override void SetDefaults()
		{
            projectile.timeLeft = 300;
            projectile.width = 2;
            projectile.height = 2;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = -1;

            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 8;
        }

		public override void AI()
		{
            projectile.Center = Owner.Center;
            Owner.itemAnimation = Owner.itemAnimationMax;

            float targetRot = (Main.MouseWorld - Owner.Center).ToRotation();
            float diff = Helper.CompareAngle(LaserRotation, targetRot);
            float maxRot = firing ? 0.02f : 0.08f;
            LaserRotation -= MathHelper.Clamp(diff, -maxRot, maxRot);

            Lighting.AddLight(projectile.Center, new Vector3(0.7f, 0.4f, 0.2f));

            if (Charge == 0)
                LaserRotation = targetRot;

            if (Charge == 1)
                Helper.PlayPitched("Magic/RefractiveCharge", 0.7f, 0, projectile.Center);

            if (Charge >= 12 || firing)
                projectile.rotation = LaserRotation + 1.57f / 2;
            else
            {
                projectile.rotation = LaserRotation + 1.57f / 2 + Helper.BezierEase(Charge / 12f) * 6.28f;
                projectile.scale = 0.5f + (Charge / 12f) * 0.5f;
            }

            if (Main.mouseRight && !firing)
			{
                if (Charge < 35)
                    Charge++;

                projectile.timeLeft = (int)MaxTime + 1;
                return;
			}
            else if (Charge < 30)
			{
                projectile.timeLeft = 0;
                return;
			}

            if (Charge >= 30 && !firing)
                Helper.PlayPitched("Magic/RefractiveLaser", 0.6f, -0.2f, projectile.Center);

            firing = true;

			for(int k = 0; k < 80; k++)
			{
                Vector2 posCheck = projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * k * 8;

                if (Helper.PointInTile(posCheck) || k == 79)
                {
                    endPoint = posCheck;
                    break;
                }
            }
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
            if (LaserTimer > 0 && Helper.CheckLinearCollision(Owner.Center, endPoint, targetHitbox, out Vector2 colissionPoint))
            {
                Dust.NewDustPerfect(colissionPoint, DustType<Glow>(), Vector2.One.RotatedByRandom(6.28f), 0, new Color(255, 150, 100), Main.rand.NextFloat());
                return true;
            }

            return false;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
            target.velocity += Vector2.UnitX.RotatedBy(LaserRotation) * 0.25f * target.knockBackResist;
            target.AddBuff(BuffType<RefractiveBladeBuff>(), 240);
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(GetTexture(Texture), Owner.Center - Main.screenPosition, null, lightColor, projectile.rotation, new Vector2(0, GetTexture(Texture).Height), projectile.scale, 0, 0);
            spriteBatch.Draw(GetTexture(Texture + "Glow"), Owner.Center - Main.screenPosition, null, Color.White, projectile.rotation, new Vector2(0, GetTexture(Texture).Height), projectile.scale, 0, 0);

            float prog1 = Helper.SwoopEase((Charge - 12) / 23f);
            float prog2 = Helper.SwoopEase((Charge - 17) / 18f);

            float pow = 0;

            if (Charge <= 35) 
                pow = (Charge / 35f) * 0.1f;

            if (LaserTimer > 0 && LaserTimer < 20)
                pow = 0.1f + (LaserTimer / 20f) * 0.4f;

            if (LaserTimer >= 20)
                pow = 0.5f;

            prog1 += (float)Math.Sin(Main.GameUpdateCount * 0.2f) * pow;
            prog2 += (float)Math.Sin((Main.GameUpdateCount - 20) * 0.2f) * pow;

            if(LaserTimer > 80)
			{
                prog1 *= (120 - LaserTimer) / 40f;
                prog2 *= (120 - LaserTimer) / 40f;
            }

            DrawRing(spriteBatch, projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * prog1 * 30, 1, 1, Main.GameUpdateCount * 0.01f, prog1, new Color(255, 240, 120));
            DrawRing(spriteBatch, projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * prog2 * 50, 0.5f, 0.5f, Main.GameUpdateCount * -0.015f, prog2, new Color(255, 180, 120));

            return false;
        }

        private void DrawRing(SpriteBatch sb, Vector2 pos, float w, float h, float rotation, float prog, Color color)
        {
            var texRing = GetTexture(AssetDirectory.VitricItem + "BossBowRing");
            var effect = Filters.Scene["BowRing"].GetShader().Shader;

            effect.Parameters["uProgress"].SetValue(rotation);
            effect.Parameters["uColor"].SetValue(color.ToVector3());
            effect.Parameters["uImageSize1"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["uOpacity"].SetValue(prog);

            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            var target = toRect(pos, (int)(10 * (w + prog)), (int)(30 * (h + prog)));
            sb.Draw(texRing, target, null, color * prog, projectile.rotation - 1.57f / 2, texRing.Size() / 2, 0, 0);

            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
        }

        private Rectangle toRect(Vector2 pos, int w, int h)
        {
            return new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), w, h);
        }

        public void DrawAdditive(SpriteBatch spriteBatch) 
		{
            if(LaserTimer <= 0)
                return;		

            int sin = (int)(Math.Sin(StarlightWorld.rottime * 3) * 40f); //Just a copy/paste of the boss laser. Need to tune this later
            var color = new Color(255, 160 + sin, 40 + sin / 2);

            var texBeam = GetTexture(AssetDirectory.MiscTextures + "BeamCore");
            var texBeam2 = GetTexture(AssetDirectory.MiscTextures + "BeamTrail");
            var texDark = GetTexture(AssetDirectory.MiscTextures + "GradientBlack");

            Vector2 origin = new Vector2(0, texBeam.Height / 2);
            Vector2 origin2 = new Vector2(0, texBeam2.Height / 2);

            var effect = StarlightRiver.Instance.GetEffect("Effects/GlowingDust");

            effect.Parameters["uColor"].SetValue(color.ToVector3());

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            float height = texBeam.Height / 4f;
            int width = (int)(projectile.Center - endPoint).Length();

            if (LaserTimer < 20)
                height = texBeam.Height / 4f * LaserTimer / 20f;

            if (LaserTimer > (int)MaxTime - 40)
                height = texBeam.Height / 4f * (1 - (LaserTimer - ((int)MaxTime - 40)) / 40f);


            var pos = projectile.Center - Main.screenPosition;

            var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.2f));
            var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);

            var source = new Rectangle((int)((LaserTimer / 20f) * -texBeam.Width), 0, texBeam.Width, texBeam.Height);
            var source2 = new Rectangle((int)((LaserTimer / 45f) * -texBeam2.Width), 0, texBeam2.Width, texBeam2.Height);

            spriteBatch.Draw(texBeam, target, source, color, LaserRotation, origin, 0, 0);
            spriteBatch.Draw(texBeam2, target2, source2, color * 0.5f, LaserRotation, origin2, 0, 0);

            for (int i = 0; i < width; i += 10)
            {
                Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(LaserRotation) * i + Main.screenPosition, color.ToVector3() * height * 0.010f);

                if (Main.rand.Next(20) == 0)
                    Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * i, DustType<Dusts.Glow>(), Vector2.UnitY * Main.rand.NextFloat(-1.5f, -0.5f), 0, color, 0.35f);
            }

            var opacity = height / (texBeam.Height / 2f) * 0.75f;

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            if (Owner == Main.LocalPlayer)
            {
                spriteBatch.Draw(texDark, projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation + 1.57f) * 80 - Main.screenPosition, null, Color.White * opacity, LaserRotation, new Vector2(texDark.Width / 2, 0), 10, 0, 0);
                spriteBatch.Draw(texDark, projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation - 1.57f) * 80 - Main.screenPosition, null, Color.White * opacity, LaserRotation - 3.14f, new Vector2(texDark.Width / 2, 0), 10, 0, 0);
            }

            spriteBatch.Draw(GetTexture(Texture), Owner.Center - Main.screenPosition, null, Color.White, LaserRotation + 1.57f / 2, new Vector2(0, GetTexture(Texture).Height), 1, 0, 0);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            var impactTex = GetTexture(AssetDirectory.Assets + "Keys/GlowSoft");
            var impactTex2 = GetTexture(AssetDirectory.GUI + "ItemGlow");
            var glowTex = GetTexture(AssetDirectory.Assets + "GlowTrail");

            spriteBatch.Draw(glowTex, target, source, color * 0.95f, LaserRotation, new Vector2(0, glowTex.Height / 2), 0, 0);

            spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color * (height * 0.012f), 0, impactTex.Size() / 2, 3.8f, 0, 0);
            spriteBatch.Draw(impactTex2, endPoint - Main.screenPosition, null, color * (height * 0.05f), StarlightWorld.rottime * 2, impactTex2.Size() / 2, 0.38f, 0, 0);

            for (int k = 0; k < 4; k++)
            {
                float rot = Main.rand.NextFloat(6.28f);
                int variation = Main.rand.Next(30);

                color.G -= (byte)variation;

                Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(LaserRotation) * width + Vector2.One.RotatedBy(rot) * Main.rand.NextFloat(40), DustType<Dusts.Glow>(), Vector2.One.RotatedBy(rot) * 1, 0, color, 0.2f - (variation * 0.02f));
            }
        }
	}

	class RefractiveBladeBuff : SmartBuff
	{
        public RefractiveBladeBuff() : base("Melting", "Taking additional melee damage!", true) { }

		public override bool Autoload(ref string name, ref string texture)
		{
            StarlightNPC.ModifyHitByProjectileEvent += ExtraDamage;
            StarlightNPC.ModifyHitByItemEvent += ExtraMeleeDamage;

            StarlightPlayer.ModifyHitByNPCEvent += PlayerExtraMeleeDamage;

            return base.Autoload(ref name, ref texture);
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
            Dust.NewDust(npc.position, npc.width, npc.height, DustType<Dusts.Glow>(), 0, 0, 0, new Color(255, 150, 50), 0.5f);
		}

		private void PlayerExtraMeleeDamage(Player player, NPC npc, ref int damage, ref bool crit)
		{
            if (Inflicted(player))
            {
                damage = (int)(damage * 1.5f);
            }
        }

		private void ExtraMeleeDamage(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
            if (Inflicted(npc))
            {
                if (item.melee)
                    damage = (int)(damage * 1.25f);
            }
		}

		private void ExtraDamage(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
            if (Inflicted(npc))
            {
                if (projectile.melee)
                    damage = (int)(damage * 1.25f);

                if (projectile.type == ProjectileType<RefractiveBladeProj>())
                    damage = (int)(damage * 1.5f);
            }
        }
	}
}
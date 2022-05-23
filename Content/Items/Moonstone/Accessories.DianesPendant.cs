using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.CustomHooks;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Moonstone
{
	[AutoloadEquip(EquipType.Neck)]
	public class DianesPendant : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Diane's Pendant");
			Tooltip.SetDefault("Something about a crescent idk \n+20 barrier"); //TODO: EGSHELS FIX!!!!!

		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.rare = 3;
			Item.value = Item.buyPrice(0, 5, 0, 0);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			Player.GetModPlayer<BarrierPlayer>().MaxBarrier += 20;
			Player.GetModPlayer<DianePlayer>().Active = true;

			if (Player.ownedProjectileCounts[ModContent.ProjectileType<DianeCrescant>()] < 1 && !Player.dead)
			{
				Projectile.NewProjectile(Player.GetSource_Accessory(Item), Player.Center, new Vector2(7, 7), ModContent.ProjectileType<DianeCrescant>(), 30, 1.5f, Player.whoAmI);
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 4);
			recipe.AddIngredient(ModContent.ItemType<AquaSapphire>(), 1);
			recipe.AddTile(TileID.Anvils);
		}
	}
	internal class DianePlayer : ModPlayer
    {
		public bool Active = false;

        public int charge = 0;

        private int currentMana = -1;
        public override void ResetEffects()
        {
			Active = false;
        }

        public override void OnMissingMana(Item item, int neededMana)
        {
            if (Active)
            {
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<DianeCrescant>())
                    {
                        var mp = proj.ModProjectile as DianeCrescant;
                        if (!mp.attacking)
                            mp.StartAttack();
                        break;
                    }
                }
                charge = 0;
            }
        }

        public override void PostUpdate()
        {
            if (currentMana != Player.statMana && Active)
            {
                if (currentMana > Player.statMana && charge < 500)
                    charge += currentMana - Player.statMana;
                currentMana = Player.statMana;
            }
        }
    }

    public class DianeCrescant : ModProjectile
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        private int charge = 0;

        private float chargeRatio => charge / 500f;

        public bool attacking = false;

        private float speed = 10;

        private List<NPC> alreadyHit = new List<NPC>();

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crescant");
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 68;
            Projectile.height = 68;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 216000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Main.projPet[Projectile.type] = false;
            Player Player = Main.player[Projectile.owner];

            Projectile.rotation += Projectile.velocity.Length() * 0.01f;

            if (Player.dead)
                Projectile.active = false;

            if (Player.GetModPlayer<DianePlayer>().Active)
                Projectile.timeLeft = 2;

            if (attacking)
                AttackMovement(Player);
            else
                IdleMovement(Player);

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail(Main.spriteBatch);
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (alreadyHit.Contains(target))
                return false;
            return base.CanHitNPC(target);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            alreadyHit.Add(target);
            Projectile.velocity *= -2;
        }

        public void StartAttack()
        {
            Projectile.friendly = true;
            Projectile.damage = (int)MathHelper.Lerp(10, 50, chargeRatio);
            attacking = true;
            alreadyHit = new List<NPC>();
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 15; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center - (Projectile.rotation.ToRotationVector2() * 20));

            while (cache.Count > 15)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(40 * 4), factor => (10 + factor * 25) * MathHelper.Lerp(0.4f, 1f, chargeRatio), factor =>
            {
                if (factor.X >= 0.96f)
                    return Color.White * 0;

                return new Color(120, 20 + (int)(100 * factor.X), 255) * (float)Math.Sin(factor.X * 3.14f);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(40 * 4), factor => (80 + 0 + factor * 0) * MathHelper.Lerp(0.4f, 1f, chargeRatio), factor =>
            {
                if (factor.X >= 0.96f)
                    return Color.White * 0;

                return new Color(100, 20 + (int)(60 * factor.X), 255) * 0.15f * (float)Math.Sin(factor.X * 3.14f);
            });

            trail2.Positions = cache.ToArray();
            trail2.NextPosition = Projectile.Center + Projectile.velocity;
        }

        private void AttackMovement(Player Player)
        {
            Projectile.rotation += Projectile.velocity.Length() * 0.01f;
            Projectile.friendly = true;
            speed = MathHelper.Lerp(20, 30, chargeRatio);
            var target = Main.npc.Where(x => x.active && !x.townNPC && !alreadyHit.Contains(x) && Projectile.Distance(x.Center) < 600).OrderBy(x => Projectile.Distance(x.Center)).FirstOrDefault();
            if (target == default)
            {
                Projectile.velocity *= 0.4f;
                attacking = false;
            }
            else
            {
                Vector2 direction = Projectile.DirectionTo(target.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, MathHelper.Lerp(0.1f, 0.2f, chargeRatio));
            }
        }

        private void IdleMovement(Player Player)
        {
            charge = Player.GetModPlayer<DianePlayer>().charge;
            speed = 20;
            Projectile.friendly = false;
            Vector2 direction = Player.Center - Projectile.Center;
            if (direction.Length() > 100)
            {
                direction.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction.RotatedByRandom(1.5f) * speed, 0.01f);
            }
        }

        private void DrawTrail(SpriteBatch spriteBatch)
        {
            spriteBatch.End();
            Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
            effect.Parameters["repeats"].SetValue(8f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
            effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture2"].SetValue(TextureAssets.MagicPixel.Value);

            trail2?.Render(effect);

            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
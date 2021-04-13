using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;
using StarlightRiver.Content.Projectiles;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Content.Items.Vitric
{
    class BossSpear : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public bool buffed = false;
        public int buffPower = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Facet & Lattice");
            Tooltip.SetDefault("Right click to guard\nAttacks are empowered after a guard\nEmpowerment is more effective with better guard timing");
        }

        public override void SetDefaults()
        {
            item.melee = true;
            item.damage = 40;
            item.useTime = 35;
            item.useAnimation = 35;
            item.width = 32;
            item.height = 32;
            item.knockBack = 8;
            item.shoot = ModContent.ProjectileType<BossSpearProjectile>();
            item.shootSpeed = 1;
            item.rare = ItemRarityID.Green;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.UseSound = SoundID.DD2_MonkStaffSwing;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                item.shoot = ModContent.ProjectileType<BossSpearShieldProjectile>();
                item.UseSound = SoundID.DD2_CrystalCartImpact;
                item.useAnimation = 80;
                item.useTime = 80;
                item.knockBack = 12;
            }
            else
            {
                item.shoot = ModContent.ProjectileType<BossSpearProjectile>();
                item.UseSound = SoundID.DD2_MonkStaffSwing;
                item.useAnimation = 35;
                item.useTime = 35;
                item.knockBack = 8;
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var damageLine = tooltips.FirstOrDefault(n => n.Name == "Damage");

            if(damageLine != null)
            {
                tooltips.Insert(tooltips.IndexOf(damageLine) + 1, new TooltipLine(mod, "ShieldLife", "50 shield life"));
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse != 2) ;


            if(buffed && player.altFunctionUse != 2)
            {
                int i = Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
                Main.projectile[i].ai[1] = buffPower;
                buffed = false;

                return false;
            }

            buffed = false;

            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool UseItem(Player player)
        {
            //player.velocity += Vector2.Normalize(player.Center - Main.MouseWorld) * 10;
            return true;
        }
    }

    class BossSpearProjectile : SpearProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public ref float DrawbackTime => ref projectile.ai[0];
        public ref float BuffPower => ref projectile.ai[1];

        public BossSpearProjectile() : base(50, 74, 164) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Facet");
        }

        public override void SafeAI()
        {
            var player = Main.player[projectile.owner];

            DrawbackTime++;

            if (DrawbackTime < 10)
            {
                Min -= 2;

                if (DrawbackTime < 6)
                    projectile.timeLeft += 2;
            }
            
            if (projectile.timeLeft > (int)(20 * player.meleeSpeed) && projectile.timeLeft < (int)(50 * player.meleeSpeed))
                projectile.extraUpdates = 8;
            else  
                projectile.extraUpdates = 0;

            if (projectile.timeLeft == (int)(25 * player.meleeSpeed))
            {
                Dust.NewDustPerfect(projectile.Center + Vector2.UnitX.RotatedBy(projectile.rotation + (float)Math.PI / 4f * 5f) * 124, ModContent.DustType<Dusts.AirSetColorNoGravity>(), Vector2.Zero, 0, default, 2);

                 player.velocity += Vector2.Normalize(player.Center - Main.MouseWorld) * (BuffPower > 0 ? -10 : -4);
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            var player = Main.player[projectile.owner];
            return projectile.timeLeft < (int)(50 * player.meleeSpeed);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            var slot = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Impale");
            Main.PlaySound(slot.SoundId, (int)projectile.Center.X, (int)projectile.Center.Y, slot.Style, 1, Main.rand.NextFloat(0.6f, 0.9f));

            if (BuffPower > 0)
            {
                var slot2 = SoundID.DD2_BetsyFireballImpact;
                Main.PlaySound(slot2.SoundId, (int)projectile.Center.X, (int)projectile.Center.Y, slot2.Style, 1, -3.5f);
                damage = (int)(damage * (1 + BuffPower / 50f));
                knockback *= 3;

                for (int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(projectile.Center, ModContent.DustType<Dusts.Stamina>(), Vector2.One.RotatedBy(projectile.rotation + Main.rand.NextFloat(0.2f)) * Main.rand.NextFloat(12), 0, default, 1.5f);

                BuffPower = 0;
            }
            
            for (int k = 0; k < 20; k++)
                Dust.NewDustPerfect(projectile.Center, DustID.Blood, Vector2.One.RotatedBy(projectile.rotation + Main.rand.NextFloat(0.2f)) * Main.rand.NextFloat(6), 0, default, 1.5f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = Main.projectileTexture[projectile.type];
            var color = lightColor * (DrawbackTime < 10 ? (DrawbackTime / 10f) : 1); //fadein   why did I write it like this? idk lol shoot me
            if (projectile.timeLeft <= 5) color *= projectile.timeLeft / 5f; //fadeout

            spriteBatch.Draw(tex, (projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY),
                tex.Frame(), color, projectile.rotation - (float)Math.PI / 4f, new Vector2(tex.Width / 2, 0), projectile.scale, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (BuffPower <= 0)
                return;

            var tex = ModContent.GetTexture("StarlightRiver/Assets/FireTrail");
            var color = new Color(255, 200, 100);

            var source = new Rectangle((int)(projectile.timeLeft / 50f * tex.Width / 2), 0, tex.Width / 2, tex.Height);
            var target = new Rectangle((int)(projectile.Center.X - Main.screenPosition.X), (int)(projectile.Center.Y - Main.screenPosition.Y), 64, 40);

            spriteBatch.Draw(tex, target, source, color, projectile.rotation - (float)Math.PI * 3/4f, new Vector2(tex.Width / 2, tex.Height / 2), 0, 0);

            Lighting.AddLight(projectile.Center, new Vector3(1, 0.6f, 0.2f) * 0.5f);
        }
    }

    class BossSpearShieldProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public ref float ShieldLife => ref projectile.ai[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lattice");
        }

        public override void SetDefaults()
        {
            projectile.friendly = true;
            projectile.width = 32;
            projectile.height = 32;
            projectile.timeLeft = 60;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.knockBack = 2;
            ShieldLife = 50;
        }

        public override void Kill(int timeLeft)
        {
            if (timeLeft > 0)
            {
                for (int k = 0; k < 20; k++)
                    Dust.NewDust(projectile.position, 16, 16, ModContent.DustType<Dusts.GlassNoGravity>());

                Main.PlaySound(SoundID.Shatter);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            ShieldLife -= target.damage / 2;
            CombatText.NewText(projectile.Hitbox, new Color(100, 255, 255), target.damage / 2);

            var player = Main.player[projectile.owner];
            player.velocity += Vector2.Normalize(player.Center - target.Center) * 3;

            var item = (player.HeldItem.modItem as BossSpear);

            if (item != null && !item.buffed)
            {
                item.buffed = true;
                item.buffPower = (int)ShieldLife;
            }
        }

        public override void AI()
        {
            var player = Main.player[projectile.owner];

            if (projectile.timeLeft > 5 && player == Main.LocalPlayer && !Main.mouseRight)
            {
                projectile.timeLeft = 5;
                player.itemTime = 20;
                player.itemAnimation = 20;
            }

            if (projectile.timeLeft == 60)
            {
                projectile.damage = 10;

                projectile.ai[1] = (Main.MouseWorld - player.Center).ToRotation() - (float)Math.PI;
                ShieldLife = 50;
            }

            var progress = projectile.timeLeft > 50 ? (60 - projectile.timeLeft) / 10f : projectile.timeLeft < 5 ? (projectile.timeLeft / 5f) : 1;

            projectile.Center = player.Center + Vector2.UnitY * player.gfxOffY + Vector2.UnitX.RotatedBy(projectile.ai[1]) * 28 * -progress;
            projectile.scale = progress;
            projectile.rotation = projectile.ai[1] + (float)Math.PI;

            player.heldProj = projectile.whoAmI;

            if(projectile.timeLeft < 40)
                ShieldLife --;

            for(int k = 0; k < Main.maxProjectiles; k++)
            {
                var proj = Main.projectile[k];

                if(proj.active && proj.hostile && proj.damage > 1 && proj.Hitbox.Intersects(projectile.Hitbox))
                {
                    var diff = (proj.damage * 2) - ShieldLife;

                    var item = (player.HeldItem.modItem as BossSpear);

                    if(item != null && !item.buffed)
                    {
                        item.buffed = true;
                        item.buffPower = (int)ShieldLife;
                    }

                    if(diff <= 0)
                    {
                        proj.penetrate -= 1;
                        proj.friendly = true;
                        ShieldLife -= proj.damage * 2;
                        CombatText.NewText(projectile.Hitbox, new Color(100, 255, 255), proj.damage * 2);
                    }
                    else
                    {
                        CombatText.NewText(projectile.Hitbox, new Color(100, 255, 255), "Cant block!");
                        proj.damage -= (int)ShieldLife / 2;
                        projectile.Kill();
                    }

                    player.velocity += Vector2.Normalize(player.Center - proj.Center) * 3;
                }
            }

            if(ShieldLife <= 0)
                projectile.Kill();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = ModContent.GetTexture(Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, new Rectangle(0, 32 * projectile.frame, 22, 32), lightColor, projectile.rotation, new Vector2(11, 16), projectile.scale, 0, 0);

            if (Main.LocalPlayer == Main.player[projectile.owner])
            {
                var barTex = ModContent.GetTexture(AssetDirectory.GUI + "ShieldBar0");
                var barTex2 = ModContent.GetTexture(AssetDirectory.GUI + "ShieldBar1");

                var pos = Main.LocalPlayer.Center - Main.screenPosition + new Vector2(0, -36) - barTex.Size() / 2;
                var target = new Rectangle((int)pos.X + 1, (int)pos.Y - 2, (int)(ShieldLife / 50f * barTex2.Width), barTex2.Height);
                var opacity = projectile.timeLeft > 50 ? 1 - (projectile.timeLeft - 50) / 10f : projectile.timeLeft < 5 ? projectile.timeLeft / 5f : 1;
                var color = Color.Lerp(new Color(50, 100, 200), new Color(100, 255, 255), ShieldLife / 50f);

                spriteBatch.Draw(barTex, pos, color * opacity);
                spriteBatch.Draw(barTex2, target, color * 0.8f * opacity);
            }

            if (projectile.timeLeft % 8 == 0) projectile.frame++;
            projectile.frame %= 3;

            return false;
        }
    }
}

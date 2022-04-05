using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Projectiles;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

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
            Item.DamageType = DamageClass.Melee;
            Item.damage = 35;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.width = 32;
            Item.height = 32;
            Item.knockBack = 8;
            Item.shoot = ModContent.ProjectileType<BossSpearProjectile>();
            Item.shootSpeed = 1;
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.DD2_MonkStaffSwing;
        }

        public override void HoldItem(Player Player)
        {
            if (Player.whoAmI == Main.myPlayer)
                Player.GetModPlayer<ControlsPlayer>().rightClickListener = true;

            if (Player.GetModPlayer<ControlsPlayer>().mouseRight)
            {
                Item.shoot = ModContent.ProjectileType<BossSpearShieldProjectile>();
                Item.UseSound = SoundID.DD2_CrystalCartImpact;
                Item.useAnimation = 80;
                Item.useTime = 80;
                Item.knockBack = 12;
            }
            else
            {
                Item.shoot = ModContent.ProjectileType<BossSpearProjectile>();
                Item.UseSound = SoundID.DD2_MonkStaffSwing;
                Item.useAnimation = 35;
                Item.useTime = 35;
                Item.knockBack = 8;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var damageLine = tooltips.FirstOrDefault(n => n.Name == "Damage");

            if(damageLine != null)
            {
                tooltips.Insert(tooltips.IndexOf(damageLine) + 1, new TooltipLine(Mod, "ShieldLife", "50 shield life"));
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (buffed && player.altFunctionUse != 2)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0, buffPower);
                buffed = false;

                return false;
            }

            buffed = false;

            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        public override bool AltFunctionUse(Player Player)
        {
            return true;
        }

        public override bool? UseItem(Player Player)
        {
            //Player.velocity += Vector2.Normalize(Player.Center - Main.MouseWorld) * 10;
            if (Player.GetModPlayer<ControlsPlayer>().mouseRight)
            {
                Helper.PlayPitched(SoundID.DD2_CrystalCartImpact, 1f, 0, Player.position);
            } else
            {
                Helper.PlayPitched(SoundID.DD2_MonkStaffSwing, 1f, 0, Player.position);
            }

            return true;
        }
    }

    class BossSpearProjectile : SpearProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public ref float DrawbackTime => ref Projectile.ai[0];
        public ref float BuffPower => ref Projectile.ai[1];

        public BossSpearProjectile() : base(50, 74, 164) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Facet");
        }

        public override void SafeAI()
        {
            var Player = Main.player[Projectile.owner];

            DrawbackTime++;

            if (DrawbackTime < 10)
            {
                Min -= 2;

                if (DrawbackTime < 6)
                    Projectile.timeLeft += 2;
            }
            
            if (Projectile.timeLeft > (int)(20 * Player.meleeSpeed) && Projectile.timeLeft < (int)(50 * Player.meleeSpeed))
                Projectile.extraUpdates = 8;
            else  
                Projectile.extraUpdates = 0;
            
            if (Projectile.timeLeft == (int)(25 * Player.meleeSpeed))
            {
                Dust.NewDustPerfect(Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + (float)Math.PI / 4f * 5f) * 124, ModContent.DustType<Dusts.AirSetColorNoGravity>(), Vector2.Zero, 0, default, 2);

                 Player.velocity += Vector2.UnitX.RotatedBy(Projectile.rotation + (float)Math.PI / 4f * 5f + 3.14f) * (BuffPower > 0 ? -10 : -4);
            }


        }

        public override void PostAI()
        {
            if (Main.myPlayer != Projectile.owner)
                findIfHit();
        }

        public override bool? CanHitNPC(NPC target)
        {
            var Player = Main.player[Projectile.owner];
            return (Projectile.timeLeft < (int)(50 * Player.meleeSpeed)) && target.active && !target.dontTakeDamage && !target.townNPC;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            hitDirection = Main.player[Projectile.owner].direction;
            if (BuffPower > 0)
            {
                var slot2 = SoundID.DD2_BetsyFireballImpact;
                Terraria.Audio.SoundEngine.PlaySound(slot2.SoundId, (int)Projectile.Center.X, (int)Projectile.Center.Y, slot2.Style, 1, -3.5f);
                damage = (int)(damage * (1 + BuffPower / 50f));
                knockback *= 3;

                for (int k = 0; k < 20; k++)
                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Stamina>(), Vector2.One.RotatedBy(Projectile.rotation + Main.rand.NextFloat(0.2f)) * Main.rand.NextFloat(12), 0, default, 1.5f);

                BuffPower = 0;
            }

            target.immune[Projectile.owner] = 10; //equivalent to normal pierce iframes but explicit for multiPlayer compatibility


            if (Main.myPlayer == Projectile.owner)
                Projectile.netUpdate = true;

            if (Helpers.Helper.IsFleshy(target))
            {
                Helpers.Helper.PlayPitched("Impale", 1, Main.rand.NextFloat(0.6f, 0.9f), Projectile.Center);

                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Vector2.One.RotatedBy(Projectile.rotation + Main.rand.NextFloat(0.2f)) * Main.rand.NextFloat(6), 0, default, 1.5f);
                }
            }

            else
			{
                Helpers.Helper.PlayPitched("Impacts/Clink", 1, Main.rand.NextFloat(0.1f, 0.3f), Projectile.Center);

                for(int k = 0; k < 15; k++)
				{
                    Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Glow>(), Vector2.One.RotatedBy(Projectile.rotation + Main.rand.NextFloat(0.2f)) * Main.rand.NextFloat(6), 0, new Color(255, Main.rand.Next(130, 255), 80), Main.rand.NextFloat(0.3f, 0.5f));
                }
            }
        }

        private void findIfHit()
        {
            foreach (NPC NPC in Main.npc.Where(n => n.active  && !n.dontTakeDamage && !n.townNPC && n.life > 0 && Projectile.timeLeft < (int)(50 * Main.player[Projectile.owner].meleeSpeed) && n.immune[Projectile.owner] <= 0 &&  n.Hitbox.Intersects(Projectile.Hitbox) ))
            {
                int zero = 0;
                float zerof = 0f;
                bool none = false;
                ModifyHitNPC(NPC, ref zero, ref zerof, ref none, ref zero);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = TextureAssets.Projectile[Projectile.type].Value;
            var color = lightColor * (DrawbackTime < 10 ? (DrawbackTime / 10f) : 1); //fadein   why did I write it like this? idk lol shoot me
            if (Projectile.timeLeft <= 5) color *= Projectile.timeLeft / 5f; //fadeout

            spriteBatch.Draw(tex, (Projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY),
                tex.Frame(), color, Projectile.rotation - (float)Math.PI / 4f, new Vector2(tex.Width / 2, 0), Projectile.scale, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (BuffPower <= 0)
                return;

            var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value;
            var color = new Color(255, 200, 100);

            var source = new Rectangle((int)(Projectile.timeLeft / 50f * tex.Width / 2), 0, tex.Width / 2, tex.Height);
            var target = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X), (int)(Projectile.Center.Y - Main.screenPosition.Y), 64, 40);

            spriteBatch.Draw(tex, target, source, color, Projectile.rotation - (float)Math.PI * 3/4f, new Vector2(tex.Width / 2, tex.Height / 2), 0, 0);

            Lighting.AddLight(Projectile.Center, new Vector3(1, 0.6f, 0.2f) * 0.5f);
        }
    }

    class BossSpearShieldProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public ref float ShieldLife => ref Projectile.ai[0];

        public ref float Rotation => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lattice");
        }

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.knockBack = 2;
            Projectile.DamageType = DamageClass.Melee;
            ShieldLife = 50;
        }

        public override void Kill(int timeLeft)
        {
            if (timeLeft > 0)
            {
                for (int k = 0; k < 20; k++)
                    Dust.NewDust(Projectile.position, 16, 16, ModContent.DustType<Dusts.GlassNoGravity>());

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter);
            }
        }

        private void findIfHit()
        {
            foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.immune[Projectile.owner] <= 0 && n.Hitbox.Intersects(Projectile.Hitbox)))
            {
                OnHitNPC(NPC, 0, 0, false);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            ShieldLife -= target.damage / 2;
            CombatText.NewText(Projectile.Hitbox, new Color(100, 255, 255), target.damage / 2);

            var Player = Main.player[Projectile.owner];
            Player.velocity += Vector2.Normalize(Player.Center - target.Center) * 3;

            var Item = (Player.HeldItem.ModItem as BossSpear);

            target.immune[Projectile.owner] = 10; //equivalent to normal pierce iframes but explicit for multiPlayer compatibility

            if (Item != null && !Item.buffed)
            {
                Item.buffed = true;
                Item.buffPower = (int)ShieldLife;
            }

            if (ShieldLife <= 0)
                Projectile.Kill();

            if (Main.myPlayer == Projectile.owner)
                Projectile.netUpdate = true;
        }

        public override void AI()
        {
            var Player = Main.player[Projectile.owner];
            Projectile.frameCounter++;

            ControlsPlayer cPlayer = Player.GetModPlayer<ControlsPlayer>();

            if (Main.myPlayer == Projectile.owner)
            {
                cPlayer.mouseRotationListener = true;
                cPlayer.rightClickListener = true;
            }
                

            if (Projectile.timeLeft > 5 && !cPlayer.mouseRight)
            {
                Projectile.timeLeft = 5;
                Player.itemTime = 20;
                Player.itemAnimation = 20;
            }
            if (cPlayer.mouseRight && Projectile.timeLeft < 10)
            {
                Projectile.timeLeft = 10;
                Player.itemTime = 20;
                Player.itemAnimation = 20;
            }

            Rotation = (cPlayer.mouseWorld - Player.Center).ToRotation() - (float)Math.PI;

            if (Projectile.timeLeft == 60)
            {
                Projectile.damage = 10;

                ShieldLife = 50;
            }

            var progress = Projectile.timeLeft > 50 ? (60 - Projectile.timeLeft) / 10f : Projectile.timeLeft < 5 ? (Projectile.timeLeft / 5f) : 1;

            Projectile.Center = Player.Center + Vector2.UnitY * Player.gfxOffY + Vector2.UnitX.RotatedBy(Rotation) * 28 * -progress;
            Projectile.scale = progress;
            Projectile.rotation = Rotation + (float)Math.PI;

            if (cPlayer.mouseWorld.X > Player.Center.X)
                Player.direction = 1;
            else
                Player.direction = -1;

            Player.itemRotation = Rotation + (float)Math.PI; //TODO: Wrap properly when facing left

            if (Player.direction != 1)
                Player.itemRotation -= 3.14f;

            Player.heldProj = Projectile.whoAmI;

            if(Projectile.timeLeft < 40 && ShieldLife > 10)
                ShieldLife --;

            for(int k = 0; k < Main.maxProjectiles; k++)
            {
                var proj = Main.projectile[k];

                if(proj.active && proj.hostile && proj.damage > 1 && proj.Hitbox.Intersects(Projectile.Hitbox))
                {
                    var diff = (proj.damage * 2) - ShieldLife;

                    var Item = (Player.HeldItem.ModItem as BossSpear);

                    if(Item != null && !Item.buffed)
                    {
                        Item.buffed = true;
                        Item.buffPower = (int)ShieldLife;
                    }

                    if(diff <= 0)
                    {
                        proj.penetrate -= 1;
                        proj.friendly = true;
                        ShieldLife -= proj.damage * 2;
                        CombatText.NewText(Projectile.Hitbox, new Color(100, 255, 255), proj.damage * 2);
                    }
                    else
                    {
                        CombatText.NewText(Projectile.Hitbox, new Color(100, 255, 255), "Cant block!");
                        proj.damage -= (int)ShieldLife / 2;
                        Projectile.Kill();
                        return;
                    }

                    Player.velocity += Vector2.Normalize(Player.Center - proj.Center) * 3;
                }
            }

            if (ShieldLife <= 0)
                Projectile.Kill();
        }

        public override void PostAI()
        {
            if (Main.myPlayer != Projectile.owner)
                findIfHit();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;

            var tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, 32 * Projectile.frame, 22, 32), lightColor, Projectile.rotation, new Vector2(11, 16), Projectile.scale, 0, 0);

            if (Main.LocalPlayer == Main.player[Projectile.owner])
            {
                var barTex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "SmallBar0").Value;
                var barTex2 = ModContent.Request<Texture2D>(AssetDirectory.GUI + "SmallBar1").Value;

                var pos = Main.LocalPlayer.Center - Main.screenPosition + new Vector2(0, -36) - barTex.Size() / 2;
                var target = new Rectangle((int)pos.X + 1, (int)pos.Y - 2, (int)(ShieldLife / 50f * barTex2.Width), barTex2.Height);
                var opacity = Projectile.timeLeft > 50 ? 1 - (Projectile.timeLeft - 50) / 10f : Projectile.timeLeft < 5 ? Projectile.timeLeft / 5f : 1;
                var color = Color.Lerp(new Color(50, 100, 200), new Color(100, 255, 255), ShieldLife / 50f);

                spriteBatch.Draw(barTex, pos, color * opacity);
                spriteBatch.Draw(barTex2, target, color * 0.8f * opacity);
            }

            if (Projectile.frameCounter % 8 == 0) Projectile.frame++;
            Projectile.frame %= 3;

            return false;
        }
    }
}

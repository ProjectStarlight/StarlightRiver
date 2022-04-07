using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	class VitricHook : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ripshatter");
            Tooltip.SetDefault("Fires a hook, allowing you to grapple to an enemy and bounce off of them\nCan only be used while airborn");
        }

        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.shoot = ProjectileType<VitricHookProjectile>();
            Item.shootSpeed = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
            Item.knockBack = 3;
        }

        public override bool CanUseItem(Player Player)
        {
            if (Player.velocity.Y == 0 || Main.projectile.Any(n => n.active && n.type == ProjectileType<VitricHookProjectile>() && n.owner == Player.whoAmI))
                return false;

            return base.CanUseItem(Player);
        }

        //public override void HoldItem(Player Player)
        //{
        //    if (Main.projectile.Any(n => n.type == ProjectileType<VitricHookProjectile>() && n.owner == Player.whoAmI))
        //        Player.itemAnimation = 5;
        //}

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.GrapplingHook);
            recipe.AddIngredient(ItemType<VitricOre>(), 30);
        }
    }

    class VitricHookProjectile : ModProjectile 
    {
        NPC hooked;
        Vector2 startPos;
        bool struck;

        ref float Progress => ref Projectile.ai[0];
        ref float Distance => ref Projectile.ai[1];
        bool Retracting => Projectile.timeLeft < 30;

        public override string Texture => AssetDirectory.VitricItem + Name;

        Player Player => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.timeLeft = 60;
            Projectile.aiStyle = -1;
            Projectile.penetrate = 2;
        }


        private void findIfHit()
        {
            foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.Hitbox.Intersects(Projectile.Hitbox)))
            {
                hooked = NPC;
                Projectile.velocity *= 0;
                startPos = Player.Center;
                Distance = Vector2.Distance(startPos, NPC.Center);
            }
        }

        public override void AI()
        {

            Projectile.rotation = Projectile.velocity.ToRotation();
            if(Projectile.timeLeft < 40)//slows down the Projectile by 8%, for about 10 ticks before it retracts
                Projectile.velocity *= 0.92f;
            

            if(Projectile.timeLeft == 30)
            {
                startPos = Projectile.Center;
                Projectile.velocity *= 0;
            }

            if(Retracting)
                Projectile.Center = Vector2.Lerp(Player.Center, startPos, Projectile.timeLeft / 30f);

            if (hooked is null && !Retracting && Main.myPlayer != Projectile.owner)
            {
                Projectile.friendly = true; //otherwise it will stop just short of actually intersecting the hitbox
                findIfHit(); //since onhit hooks are client side only, all other clients will manually check for collisions
            }


            if (hooked != null && !struck)
            {
                Projectile.timeLeft = 32;
                Projectile.Center = hooked.Center;
                Player.velocity = Vector2.Zero;//resets wings / double jumps

                Progress += 20f / Distance;
                Player.Center = Vector2.Lerp(startPos, hooked.Center, Progress);

                if(Player.Hitbox.Intersects(hooked.Hitbox))
                {
                    struck = true;
                    Projectile.timeLeft = 20;

                    Player.immune = true;
                    Player.immuneTime = 20;
                    Player.velocity = Vector2.Normalize(startPos - hooked.Center) * 15;
                    Player.GetModPlayer<StarlightPlayer>().Shake += 15;

                    hooked.StrikeNPC(Projectile.damage, Projectile.knockBack, Player.Center.X < hooked.Center.X ? -1 : 1);

                    for(int k = 0; k < 30; k++)
                        Dust.NewDustPerfect(Player.Center, DustType<Dusts.GlassNoGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4f), 0, default, Main.rand.NextFloat(1f, 2f));
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter);
                }
            }

            if(struck)
            {
                Player.fullRotation += (Projectile.timeLeft / 20f) * 3.14f * Player.direction;
                Player.fullRotationOrigin = Player.Size / 2;
                Player.velocity *= 0.95f;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0) return;

            if (!Retracting && hooked is null)
            {
                hooked = target;
                Projectile.velocity *= 0;
                startPos = Player.Center;
                Distance = Vector2.Distance(startPos, target.Center);
                Projectile.friendly = false;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage /= 4;
            knockback /= 4f;
            crit = false;
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override bool PreDraw(ref Color lightColor)
        {           
            if(struck) 
                return false;

            var spriteBatch = Main.spriteBatch;

            Texture2D chainTex1 = Request<Texture2D>(AssetDirectory.VitricItem + "VitricHookChain1").Value;
            Texture2D chainTex2 = Request<Texture2D>(AssetDirectory.VitricItem + "VitricHookChain2").Value;

            float dist = Vector2.Distance(Player.Center, Projectile.Center);
            float rot = (Player.Center - Projectile.Center).ToRotation() + (float)Math.PI / 2f;


            float length = 1f / dist * chainTex1.Height;
            for (int k = 0; k * length < 1; k++)
            {
                var pos = Vector2.Lerp(Projectile.Center, Player.Center, k * length);
                if(k % 2 == 0)
                    spriteBatch.Draw(chainTex1, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
                else
                    spriteBatch.Draw(chainTex2, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
            }

            Texture2D hook = TextureAssets.Projectile[Projectile.type].Value;

            spriteBatch.Draw(hook, Projectile.Center - Main.screenPosition, null, lightColor, rot + ((float)Math.PI * 0.75f), hook.Size() / 2, 1, 0, 0);

            return false;
        }
    }
}

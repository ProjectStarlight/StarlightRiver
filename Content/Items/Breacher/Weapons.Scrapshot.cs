using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Breacher
{
	public class Scrapshot : ModItem
	{
        public ScrapshotHook hook;
        public int timer;

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override bool AltFunctionUse(Player player) => true;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Scrapshot");
			Tooltip.SetDefault("Right click to fire a chain hook\nShooting hooked enemies deals extra damage");
		}

		public override void SetDefaults()
		{
			item.width = 24;
			item.height = 28;
			item.damage = 30;
			item.useAnimation = 30;
			item.useTime = 2;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.knockBack = 2f;
			item.rare = ItemRarityID.Orange;
			item.value = Item.sellPrice(0, 10, 0, 0);
			item.noMelee = true;
			item.useTurn = false;
			item.useAmmo = AmmoID.Bullet;
			item.ranged = true;
			item.shoot = 1;
			item.shootSpeed = 17;
		}

		public override bool CanUseItem(Player player)
		{
            return timer <= 0 || (hook != null && hook.projectile.active && hook.hooked != null);
        }

		public override void UpdateInventory(Player player)
		{
            if (timer > 0)
                timer--;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
            if(!CanUseItem(player))
                    return false;

            timer = 30;

			if (player.altFunctionUse == 2)
			{
                int i = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ModContent.ProjectileType<ScrapshotHook>(), damage, knockBack, player.whoAmI);
                hook = Main.projectile[i].modProjectile as ScrapshotHook;
			}
			else if (hook is null || (hook != null && (!hook.projectile.active || hook.hooked != null)))
			{
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 10;

                float spread = 0.5f;

                if (hook != null && hook.projectile.active && hook.hooked != null)
                {
                    spread = 0.05f;

                    hook.struck = true;
                    hook.projectile.timeLeft = 20;

                    player.velocity = Vector2.Normalize(hook.startPos - hook.hooked.Center) * 15;
                    player.GetModPlayer<StarlightPlayer>().Shake += 15;
                }

				for (int k = 0; k < 8; k++)
				{
					int i = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY).RotatedByRandom(spread), type, damage, knockBack, player.whoAmI);
					Main.projectile[i].timeLeft = 30;
				}

                Helper.PlayPitched("Guns/Scrapshot", 1, 0, player.Center);
			}

			return false;
		}
	}

    public class ScrapshotHook : ModProjectile
    {
        public NPC hooked;
        public Vector2 startPos;
        public bool struck;

        ref float Progress => ref projectile.ai[0];
        ref float Distance => ref projectile.ai[1];
        bool Retracting => projectile.timeLeft < 30;

        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.timeLeft = 60;
            projectile.aiStyle = -1;
            projectile.penetrate = 2;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            projectile.rotation = projectile.velocity.ToRotation();
            if (projectile.timeLeft < 40)//slows down the projectile by 8%, for about 10 ticks before it retracts
                projectile.velocity *= 0.92f;
            
            if (projectile.timeLeft == 30)
            {
                startPos = projectile.Center;
                projectile.velocity *= 0;
            }

            if (Retracting)
                projectile.Center = Vector2.Lerp(player.Center, startPos, projectile.timeLeft / 30f);

            if (hooked != null && !struck)
            {
                projectile.timeLeft = 52;
                projectile.Center = hooked.Center;
                player.velocity = Vector2.Zero;//resets wings / double jumps

                Progress += (10f / Distance) * (1.4f + Progress * 1.5f);
                player.Center = Vector2.Lerp(startPos, hooked.Center, Progress);

                if (player.Hitbox.Intersects(hooked.Hitbox))
                {
                    struck = true;
                    projectile.timeLeft = 20;

                    player.immune = true;
                    player.immuneTime = 20;
                    player.velocity = Vector2.Normalize(startPos - hooked.Center) * 15;
                    player.GetModPlayer<StarlightPlayer>().Shake += 15;

                    hooked.StrikeNPC(projectile.damage, projectile.knockBack, player.Center.X < hooked.Center.X ? -1 : 1);
                }
            }

            if (struck)
            {
                player.fullRotation = (projectile.timeLeft / 20f) * 3.14f * player.direction;
                player.fullRotationOrigin = player.Size / 2;
                player.velocity *= 0.95f;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Player player = Main.player[projectile.owner];

			if (target.life <= 0)
				return;

            if (!Retracting && hooked is null)
            {
                hooked = target;
                projectile.velocity *= 0;
                startPos = player.Center;
                Distance = Vector2.Distance(startPos, target.Center);
                projectile.friendly = false;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage /= 4;
            knockback /= 4f;
            crit = false;
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (struck)
                return false;

            Texture2D chainTex1 = ModContent.GetTexture(AssetDirectory.BreacherItem + "ScrapshotHookChain1");
            Texture2D chainTex2 = ModContent.GetTexture(AssetDirectory.BreacherItem + "ScrapshotHookChain2");
            Player player = Main.player[projectile.owner];

            float dist = Vector2.Distance(player.Center, projectile.Center);
            float rot = (player.Center - projectile.Center).ToRotation() + (float)Math.PI / 2f;


            float length = 1f / dist * chainTex1.Height;
            for (int k = 0; k * length < 1; k++)
            {
                var pos = Vector2.Lerp(projectile.Center, player.Center, k * length);
                if (k % 2 == 0)
                    spriteBatch.Draw(chainTex1, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
                else
                    spriteBatch.Draw(chainTex2, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
            }

            Texture2D hook = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(hook, projectile.Center - Main.screenPosition, null, lightColor, rot + ((float)Math.PI * 0.75f), hook.Size() / 2, 1, 0, 0);

            return false;
        }
    }
}
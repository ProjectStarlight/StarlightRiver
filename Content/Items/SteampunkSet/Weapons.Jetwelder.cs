using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class Jetwelder : ModItem
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
            Tooltip.SetDefault("I shall update this later");
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.knockBack = 3f;
            item.mana = 10;
            item.width = 32;
            item.height = 32;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item44;
            item.channel = true;
            item.noMelee = true;
            item.summon = true;
            item.shoot = ModContent.ProjectileType<JetwelderFlame>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            JetwelderPlayer modPlayer = player.GetModPlayer<JetwelderPlayer>();
            if (player.altFunctionUse == 2)
            {

                switch (modPlayer.scrap / 5)
                {
                    case 1:
                        type = ModContent.ProjectileType<JetwelderCrawler>();
                        modPlayer.scrap -= 5;
                        break;
                    case 2:
                        type = ModContent.ProjectileType<JetwelderCrawler>();
                        modPlayer.scrap -= 10;
                        break;
                    case 3:
                        type = ModContent.ProjectileType<JetwelderGatler>();
                        modPlayer.scrap -= 15;
                        break;
                    case 4:
                        type = ModContent.ProjectileType<JetwelderGatler>();
                        modPlayer.scrap -= 20;
                        break;
                    default:
                        type = ModContent.ProjectileType<JetwelderGatler>();
                        modPlayer.scrap = 0;
                        break;

                }
                if (type == ModContent.ProjectileType<JetwelderCrawler>())
                    position = FindFirstTile(position, type);
                Main.NewText(modPlayer.scrap.ToString(), Color.Orange);
            }
            return true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 0);
        }

        public override bool AltFunctionUse(Player player)
        {
            return player.GetModPlayer<JetwelderPlayer>().scrap >= 5;
        }

        private static Vector2 FindFirstTile(Vector2 position, int type)
        {
            int tries = 0;
            while (tries < 99)
            {
                tries++;
                int posX = (int)position.X / 16;
                int posY = (int)position.Y / 16;
                if (Framing.GetTileSafely(posX,posY).active() && Main.tileSolid[Framing.GetTileSafely(posX, posY).type])
                {
                    break;
                }
                position += new Vector2(0, 16);
            }

            Projectile proj = new Projectile();
            proj.SetDefaults(type);
            return position - new Vector2(0, (proj.height / 2));
        }
    }
    public class JetwelderFlame : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private Player player => Main.player[projectile.owner];

        private Vector2 direction = Vector2.Zero;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Jetwelder");
            Main.projFrames[projectile.type] = 1;

        }
        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.width = 256;
            projectile.height = 64;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 999999;
            projectile.ignoreWater = true;
            projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            projectile.velocity = Vector2.Zero;
            if (player.channel)
            {
                projectile.timeLeft = 2;
                player.itemTime = player.itemAnimation = 2;

                direction = player.DirectionTo(Main.MouseWorld);
                direction.Normalize();
                player.ChangeDir(Math.Sign(direction.X));

                player.itemRotation = direction.ToRotation();

                if (player.direction != 1)
                    player.itemRotation -= 3.14f;

                player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
            }
            projectile.rotation = direction.ToRotation();
            projectile.Center = player.Center + new Vector2(0, player.gfxOffY) +  new Vector2(30, -15 * player.direction).RotatedBy(projectile.rotation);

            for (int i = 0; i < projectile.width * projectile.scale; i++)
                Lighting.AddLight(projectile.Center + (direction * i), Color.Cyan.ToVector3() * 0.6f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, new Vector2(0, tex.Height / 2), projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + (direction * projectile.width), projectile.height, ref collisionPoint); 
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0 && Main.rand.NextBool(5))
                SpawnScrap(target.Center);
            else if (Main.rand.NextBool(30))
                SpawnScrap(target.Center);
        }

        private static void SpawnScrap(Vector2 position)
        {
            int ItemType = ModContent.ItemType<JetwelderScrap1>();
            switch (Main.rand.Next(4))
            {
                case 0:
                    ItemType = ModContent.ItemType<JetwelderScrap1>();
                    break;
                case 1:
                    ItemType = ModContent.ItemType<JetwelderScrap2>();
                    break;
                case 2:
                    ItemType = ModContent.ItemType<JetwelderScrap3>();
                    break;
                case 3:
                    ItemType = ModContent.ItemType<JetwelderScrap4>();
                    break;
            }
            Item.NewItem(position, ItemType);
        }
    }
    public abstract class JetwelderScrap : ModItem
    {

        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrap");
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            SetSize();
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            if (player.GetModPlayer<JetwelderPlayer>().scrap < 20)
                player.GetModPlayer<JetwelderPlayer>().scrap++;

            Main.NewText(player.GetModPlayer<JetwelderPlayer>().scrap.ToString());
            return false;
        }

        protected virtual void SetSize() { }

        public override Color? GetAlpha(Color lightColor) => lightColor;
    }

    public class JetwelderScrap1 : JetwelderScrap
    {
        protected override void SetSize() => item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap2 : JetwelderScrap
    {
        protected override void SetSize() => item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap3 : JetwelderScrap
    {
        protected override void SetSize() => item.Size = new Vector2(32, 32);
    }
    public class JetwelderScrap4 : JetwelderScrap
    {
        protected override void SetSize() => item.Size = new Vector2(32, 32);
    }

    public class JetwelderPlayer : ModPlayer
    {
        public int scrap;
    }
}
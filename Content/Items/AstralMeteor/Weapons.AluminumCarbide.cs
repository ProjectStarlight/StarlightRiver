using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.AstralMeteor
{
    class AluminumCarbide : ModItem, IGlowingItem
    {
        float spinup = 0;

        public override string Texture => AssetDirectory.AluminumItem + "AluminumCarbide";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Carbine");
            Tooltip.SetDefault("Winds up with use\nYour first shot inflicts zapped");
        }

        public override void SetDefaults()
        {
            item.damage = 34;
            item.useTime = 45;
            item.useAnimation = 45;
            item.mana = 15;
            item.magic = true;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.crit = 4;
            item.shoot = ProjectileType<CarbideLaser>();
            item.shootSpeed = 5;
            item.knockBack = 1;
            item.rare = ItemRarityID.Blue;
            item.autoReuse = true;
            item.channel = true;
            item.noMelee = true;
            item.value = Item.sellPrice(0, 0, 40, 0);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Main.PlaySound(SoundID.Item114, player.Center);

            float mult = (6 - (spinup - 3)) / 6f;
            Vector2 velocity = new Vector2(speedX, speedY);
            int i = Projectile.NewProjectile(player.Center + velocity, velocity, type, (int)(damage * mult), knockBack * mult, player.whoAmI, spinup == 0 ? 1 : 0);
            Main.projectile[i].scale = (6 - (spinup - 3)) / 7f;

            if (spinup < 3) spinup += 0.25f;
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (!player.channel && player.itemAnimation == 0) spinup = 0;
            player.GetModPlayer<StarlightPlayer>().itemSpeed += spinup * 0.75f;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            Player player = info.drawPlayer;

            if (player.itemAnimation != 0)
            {
                Texture2D tex = GetTexture(Texture + "Glow");

                float turn = info.spriteEffects == SpriteEffects.None ? 10 : tex.Width - 10;
                Main.playerDrawData.Add(new DrawData(tex, player.Center - Main.screenPosition, tex.Frame(), Color.White, player.itemRotation, new Vector2(turn, tex.Height / 2), 1, info.spriteEffects, 0));
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<AluminumBar>(), 20);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    class CarbideLaser : ModProjectile, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.timeLeft = 300;
            projectile.extraUpdates = 2;

            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.tileCollide = true;
            projectile.ignoreWater = true;
            projectile.aiStyle = -1;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 10; k++)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center, DustID.Electric, Vector2.One.RotatedByRandom(6.28f) * projectile.scale * 6, 0, Color.White, projectile.scale * 0.5f);
                d.noGravity = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (projectile.ai[0] == 1)
            {
                crit = true;
                target.AddBuff(BuffType<Buffs.Overcharge>(), 1200);
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            for (int k = 0; k < projectile.oldPos.Length; k++)
            {
                Color color = new Color(130, 220, 255) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
                float scale = projectile.scale * (projectile.oldPos.Length - k) / projectile.oldPos.Length * 0.2f * projectile.scale;

                Texture2D tex = GetTexture("StarlightRiver/Assets/Keys/Glow");

                Vector2 off = Vector2.Normalize(projectile.velocity).RotatedBy(1.57f) * (float)Math.Sin(-StarlightWorld.rottime * 9 + k * 0.5f) * (k * 0.12f);
                spriteBatch.Draw(tex, projectile.oldPos[k] + off + projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                spriteBatch.Draw(tex, projectile.oldPos[k] + off + projectile.Size / 2 - Main.screenPosition, null, color * 0.4f, 0, tex.Size() / 2, scale * 2.2f, default, default);
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Tiles.AstralMeteor;
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
            Item.damage = 10;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.mana = 15;
            Item.magic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.crit = 4;
            Item.shoot = ProjectileType<CarbideLaser>();
            Item.shootSpeed = 5;
            Item.knockBack = 1;
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item114, Player.Center);

            float mult = (6 - (spinup - 3)) / 6f;
            Vector2 velocity = new Vector2(speedX, speedY);
            int i = Projectile.NewProjectile(Player.Center + velocity, velocity, type, (int)(damage * mult), knockBack * mult, Player.whoAmI, spinup == 0 ? 1 : 0);
            Main.projectile[i].scale = (6 - (spinup - 3)) / 7f;

            if (spinup < 3) spinup += 0.25f;
            return false;
        }

        public override void HoldItem(Player Player)
        {
            if (!Player.channel && Player.ItemAnimation == 0) spinup = 0;
            Player.GetModPlayer<StarlightPlayer>().ItemSpeed += spinup * 0.75f;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            Player Player = info.drawPlayer;

            if (Player.ItemAnimation != 0)
            {
                Texture2D tex = Request<Texture2D>(Texture + "Glow").Value;

                float turn = info.spriteEffects == SpriteEffects.None ? 10 : tex.Width - 10;
                Main.playerDrawData.Add(new DrawData(tex, Player.Center - Main.screenPosition, tex.Frame(), Color.White, Player.ItemRotation, new Vector2(turn, tex.Height / 2), 1, info.spriteEffects, 0));
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemType<AluminumBarItem>(), 20);
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;

            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
        }

        public override void Kill(int timeLeft)
        {
            for (int k = 0; k < 10; k++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Vector2.One.RotatedByRandom(6.28f) * Projectile.scale * 6, 0, Color.White, Projectile.scale * 0.5f);
                d.noGravity = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Projectile.ai[0] == 1)
            {
                crit = true;
                target.AddBuff(BuffType<Buffs.Overcharge>(), 1200);
            }
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Color color = new Color(130, 220, 255) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 0.2f * Projectile.scale;

                Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

                Vector2 off = Vector2.Normalize(Projectile.velocity).RotatedBy(1.57f) * (float)Math.Sin(-StarlightWorld.rottime * 9 + k * 0.5f) * (k * 0.12f);
                spriteBatch.Draw(tex, Projectile.oldPos[k] + off + Projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
                spriteBatch.Draw(tex, Projectile.oldPos[k] + off + Projectile.Size / 2 - Main.screenPosition, null, color * 0.4f, 0, tex.Size() / 2, scale * 2.2f, default, default);
            }
        }
    }
}

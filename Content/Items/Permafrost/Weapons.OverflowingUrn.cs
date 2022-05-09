using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Items.Permafrost
{
    class OverflowingUrn : ModItem
    {
        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Overflowing Urn");
            Tooltip.SetDefault("egshels update this lol");
        }

        public override void SetDefaults()
        {
            Item.damage = 15;
            Item.channel = true;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;
            Item.width = 18;
            Item.height = 34;
            Item.useTime = 2;
            Item.useAnimation = 2;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item43;
            Item.knockBack = 0f;
            Item.shoot = ModContent.ProjectileType<OverflowingUrnProj>();
            Item.shootSpeed = 15f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] == 0)
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
            base.HoldItem(player);
        }
    }
    public class OverflowingUrnProj : ModProjectile
    {
        public override string Texture => AssetDirectory.PermafrostItem + Name;

        private Player owner => Main.player[Projectile.owner];

        private bool attacking = false;

        private int attackCounter = 0;

        private int appearCounter = 0;

        private int capCounter = 0;

        private bool capLeaving;

        private float opacity = 0;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Steamsaw");

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 40;
            Projectile.height = 58;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public override void AI()
        {
            if (owner.HeldItem.type == ModContent.ItemType<OverflowingUrn>())
            {
                Projectile.timeLeft = 20;
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = owner.Center - new Vector2(0, 14); 
            }
            capLeaving = false;
            if (owner.channel)
            {
                if (capCounter < 20)
                {
                    capCounter++;
                    capLeaving = true;
                }
            }
            else
            {
                if (capCounter > 0)
                    capCounter--;
                if (capCounter > 6)
                    capCounter--;
            }
            capCounter = (int)MathHelper.Clamp(capCounter, 0, 20);
            appearCounter++;
            Projectile.scale = opacity = MathHelper.Min(Projectile.timeLeft / 20f, appearCounter / 20f);
            owner.bodyFrame = new Rectangle(0, 56 * 5, 40, 56);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D topTex = ModContent.Request<Texture2D>(Texture + "_Top").Value;
            float capOpacity = owner.channel ? 1 - (capCounter / 20f) : 1 - MathHelper.Clamp(((capCounter - 6) / 14f), 0, 1);

            float rot = 0f;
            Vector2 capPos = (Projectile.Center - Main.screenPosition) + new Vector2(0, owner.gfxOffY - (20 * (capLeaving ? EaseFunction.EaseCubicIn.Ease(1 - capOpacity) : EaseFunction.EaseCubicOut.Ease(1 - capOpacity))));
            Vector2 urnPos = Projectile.Center - Main.screenPosition + new Vector2(0, owner.gfxOffY);
            if (!capLeaving)
            {
                float shake = (float)Math.Sin(3.14f * Math.Clamp(capCounter / 6f, 0, 1)) * 2;
                rot = (float)Math.Sin(6.28f * Math.Clamp(capCounter / 6f, 0, 1)) * 0.03f;
                capPos.Y += shake;
                urnPos.Y += shake;
            }
            Main.spriteBatch.Draw(topTex, capPos, null, lightColor * opacity * capOpacity, owner.fullRotation + rot, new Vector2(topTex.Width / 2, tex.Height + 10), Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, urnPos, null, lightColor * opacity, owner.fullRotation + rot, new Vector2(tex.Width / 2, tex.Height), Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
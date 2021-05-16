using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;

namespace StarlightRiver.Core
{
    public abstract class HeldItemProjectile : ModProjectile
    {
        private readonly float Distance;
        public HeldItemProjectile(float distance = 1) =>
            Distance = distance;

        public sealed override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.friendly = false;
            projectile.width = 10;
            projectile.height = 10;
            projectile.aiStyle = -1;
            projectile.penetrate = 1;
            projectile.tileCollide = false;
            projectile.alpha = 255;
            projectile.timeLeft = 9999;

            SafeSetDefaults();
        }
        public virtual void SafeSetDefaults() { }


        protected Vector2 direction = Vector2.Zero;
        public sealed override bool PreAI()
        {
            AdjustDirection();
            Player player = Main.player[projectile.owner];
            player.ChangeDir(Main.MouseWorld.X > player.position.X ? 1 : -1);
            player.heldProj = projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            projectile.Center = player.Center;

            if (!player.channel)
                projectile.Kill();

            return true;
        }

        public sealed override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects spriteEffect = SpriteEffects.None;
            Vector2 offset = Vector2.Zero;
            Texture2D tex = Main.projectileTexture[projectile.type];

            if (Main.player[projectile.owner].direction != 1)
                spriteEffect = SpriteEffects.FlipVertically;

            Main.spriteBatch.Draw(tex, (Main.player[projectile.owner].Center - Main.screenPosition + new Vector2(0, Main.player[projectile.owner].gfxOffY)).PointAccur() + offset + direction * Distance, null, lightColor, direction.ToRotation(), tex.Size() * 0.5f, projectile.scale, spriteEffect, 0);

            return SafePreDraw(spriteBatch, lightColor);
        }
        public virtual bool SafePreDraw(SpriteBatch spriteBatch, Color lightColor) { return true; }

        private void AdjustDirection(float deviation = 0f)
        {
            Player player = Main.player[projectile.owner];
            direction = Main.MouseWorld - (player.Center - new Vector2(4, 4)) - new Vector2(0, Main.player[projectile.owner].gfxOffY);
            direction.Normalize();
            direction = direction.RotatedBy(deviation);
            player.itemRotation = direction.ToRotation();
            if (player.direction != 1)
                player.itemRotation -= 3.14f;
        }
    }

    public abstract class QuickMaterial : ModItem
    {
        private readonly string Matname;
        private readonly string Mattooltip;
        private readonly int Maxstack;
        private readonly int Value;
        private readonly int Rare;
        private readonly string TexturePath;
        private readonly bool PathHasName;

        protected QuickMaterial(string name, string tooltip, int maxstack, int value, int rare, string texturePath = null, bool pathHasName = false)
        {
            Matname = name;
            Mattooltip = tooltip;
            Maxstack = maxstack;
            Value = value;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
        }

        public override string Texture => string.IsNullOrEmpty(TexturePath) ? base.Texture : TexturePath + (PathHasName ? string.Empty : Name);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(Matname);
            Tooltip.SetDefault(Mattooltip);
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = Maxstack;
            item.value = Value;
            item.rare = Rare;
        }
    }

    public class QuickTileItem : ModItem //this is no longer abstract to facilitate quick tile registration
    {
        public string Itemname;
        public string Itemtooltip;
        private readonly int Tiletype;
        private readonly string Tilename;
        private readonly int Rare;
        private readonly string TexturePath;
        private readonly bool PathHasName;

        public QuickTileItem(string name, string tooltip, int placetype, int rare, string texturePath = null, bool pathHasName = false)
        {
            Itemname = name;
            Itemtooltip = tooltip;
            Tiletype = placetype;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
        }

        public QuickTileItem(string name, string tooltip, string placetype, int rare, string texturePath = null, bool pathHasName = false)
        {
            Itemname = name;
            Itemtooltip = tooltip;
            Tilename = placetype;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
        }

        public override string Texture => string.IsNullOrEmpty(TexturePath) ? AssetDirectory.Debug : TexturePath + (PathHasName ? string.Empty : Name);

        public override bool CloneNewInstances => true;

        public virtual void SafeSetDefaults() { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(Itemname ?? "ERROR");
            Tooltip.SetDefault(Itemtooltip ?? "Report me please!");
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createTile = Tiletype == 0 && Tilename != null ? mod.TileType(Tilename) : Tiletype;
            item.rare = Rare;

            SafeSetDefaults();
        }
    }

    public abstract class QuickWallItem : ModItem
    {
        public string Itemname;
        public string Itemtooltip;
        private readonly int Walltype;
        private readonly int Rare;
        private readonly string TexturePath;
        private readonly bool PathHasName;

        protected QuickWallItem(string name, string tooltip, int placetype, int rare, string texturePath = null, bool pathHasName = false)
        {
            Itemname = name;
            Itemtooltip = tooltip;
            Walltype = placetype;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
        }

        public override string Texture => string.IsNullOrEmpty(TexturePath) ? base.Texture : TexturePath + (PathHasName ? string.Empty : Name);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(Itemname);
            Tooltip.SetDefault(Itemtooltip);
        }
        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.useTurn = true;
            item.autoReuse = true;
            item.useAnimation = 15;
            item.useTime = 10;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.consumable = true;
            item.createWall = Walltype;
            item.rare = Rare;
        }
    }
}
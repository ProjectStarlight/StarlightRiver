using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public abstract class HeldItemProjectile : ModProjectile
    {
        private readonly float Distance;
        public HeldItemProjectile(float distance = 1) =>
            Distance = distance;

        public sealed override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.timeLeft = 9999;

            SafeSetDefaults();
        }
        public virtual void SafeSetDefaults() { }


        protected Vector2 direction = Vector2.Zero;
        public sealed override bool PreAI()
        {
            AdjustDirection();
            Player Player = Main.player[Projectile.owner];
            Player.ChangeDir(Main.MouseWorld.X > Player.position.X ? 1 : -1);
            Player.heldProj = Projectile.whoAmI;
            Player.ItemTime = 2;
            Player.ItemAnimation = 2;
            Projectile.Center = Player.Center;

            if (!Player.channel)
                Projectile.Kill();

            return true;
        }

        public sealed override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects spriteEffect = SpriteEffects.None;
            Vector2 offset = Vector2.Zero;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

            if (Main.player[Projectile.owner].direction != 1)
                spriteEffect = SpriteEffects.FlipVertically;

            Main.spriteBatch.Draw(tex, (Main.player[Projectile.owner].Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur() + offset + direction * Distance, null, lightColor, direction.ToRotation(), tex.Size() * 0.5f, Projectile.scale, spriteEffect, 0);

            return SafePreDraw(spriteBatch, lightColor);
        }
        public virtual bool SafePreDraw(SpriteBatch spriteBatch, Color lightColor) { return true; }

        private void AdjustDirection(float deviation = 0f)
        {
            Player Player = Main.player[Projectile.owner];
            direction = Main.MouseWorld - (Player.Center - new Vector2(4, 4)) - new Vector2(0, Main.player[Projectile.owner].gfxOffY);
            direction.Normalize();
            direction = direction.RotatedBy(deviation);
            Player.ItemRotation = direction.ToRotation();
            if (Player.direction != 1)
                Player.ItemRotation -= 3.14f;
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
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = Maxstack;
            Item.value = Value;
            Item.rare = Rare;
        }
    }

    public class QuickBannerItem : QuickTileItem
    {
        public QuickBannerItem(string placetype, string NPCDisplayName, string texturePath = null, int rare = ItemRarityID.Blue, int ItemValue = 1000) : //todo maybe: bool for tooltip
            base(NPCDisplayName + " Banner", "Nearby Players get a bonus against: " + NPCDisplayName, placetype, rare, texturePath, false, ItemValue) { }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.maxStack = 99;
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
        private readonly int ItemValue;

        public QuickTileItem(string name, string tooltip, int placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
        {
            Itemname = name;
            Itemtooltip = tooltip;
            Tiletype = placetype;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
        }

        public QuickTileItem(string name, string tooltip, string placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
        {
            Itemname = name;
            Itemtooltip = tooltip;
            Tilename = placetype;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
            ItemValue = ItemValue;
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
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = Tiletype == 0 && Tilename != null ? Mod.TileType(Tilename) : Tiletype;
            Item.rare = Rare;
            Item.value = ItemValue;
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
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createWall = Walltype;
            Item.rare = Rare;
        }
    }
}
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
        protected Vector2 direction = Vector2.Zero;

        public HeldItemProjectile(float distance = 1) => Distance = distance;

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
       
        public sealed override bool PreAI()
        {
            AdjustDirection();
            Player Player = Main.player[Projectile.owner];
            Player.ChangeDir(Main.MouseWorld.X > Player.position.X ? 1 : -1);
            Player.heldProj = Projectile.whoAmI;
            Player.itemTime = 2;
            Player.itemAnimation = 2;
            Projectile.Center = Player.Center;

            if (!Player.channel)
                Projectile.Kill();

            return true;
        }

        public sealed override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffect = SpriteEffects.None;
            Vector2 offset = Vector2.Zero;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

            if (Main.player[Projectile.owner].direction != 1)
                spriteEffect = SpriteEffects.FlipVertically;

            var pos = (Main.player[Projectile.owner].Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur() + offset + direction * Distance;
            Main.spriteBatch.Draw(tex, pos, null, lightColor, direction.ToRotation(), tex.Size() * 0.5f, Projectile.scale, spriteEffect, 0);

            return SafePreDraw(Main.spriteBatch, lightColor);
        }

        public virtual bool SafePreDraw(SpriteBatch spriteBatch, Color lightColor) 
        {
            return true; 
        }

        private void AdjustDirection(float deviation = 0f)
        {
            Player Player = Main.player[Projectile.owner];
            direction = Main.MouseWorld - (Player.Center - new Vector2(4, 4)) - new Vector2(0, Main.player[Projectile.owner].gfxOffY);
            direction.Normalize();
            direction = direction.RotatedBy(deviation);
            Player.itemRotation = direction.ToRotation();
            if (Player.direction != 1)
                Player.itemRotation -= 3.14f;
        }
    }

    public abstract class QuickNPCItem : ModItem
    {
        private readonly string ItemName;
        private readonly string ItemTooltip;
        private readonly int Maxstack;
        private readonly int Value;
        private readonly int Rare;
        private readonly int npcID;
        private readonly string TexturePath;
        private readonly bool PathHasName;

        protected QuickNPCItem(string name, string tooltip, int value, int rare, int NPCType, string texturePath = null, bool pathHasName = false, int maxstack = 999)
        {
            ItemName = name;
            ItemTooltip = tooltip;
            Maxstack = maxstack;
            Value = value;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
            npcID = NPCType;
        }

        public override string Texture => string.IsNullOrEmpty(TexturePath) ? base.Texture : TexturePath + (PathHasName ? string.Empty : Name);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(ItemName);
            Tooltip.SetDefault(ItemTooltip);
        }

        public override void SetDefaults()
        {
            Item.consumable = true;

            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useTime = Item.useAnimation = 15;
            Item.makeNPC = npcID;

            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Maxstack;
            Item.value = Value;
            Item.rare = Rare;
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
            base(NPCDisplayName + " BannerItem", NPCDisplayName + " Banner", "Nearby Players get a bonus against: " + NPCDisplayName, placetype, rare, texturePath, false, ItemValue) { }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.maxStack = 99;
        }
    }

    public abstract class QuickTileItem : ModItem 
    {
        public string InternalName = "";
        public string Itemname;
        public string Itemtooltip;
        //private readonly int Tiletype;
        private readonly string Tilename;
        private readonly int Rare;
        private readonly string TexturePath;
        private readonly bool PathHasName;
        private readonly int ItemValue;

        public QuickTileItem() { }

        public QuickTileItem(string name, string tooltip, string placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
        {
            Itemname = name;
            Itemtooltip = tooltip;
            Tilename = placetype;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
            this.ItemValue = ItemValue;
        }

        public QuickTileItem(string internalName, string name, string tooltip, string placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
        {
            InternalName = internalName;
            Itemname = name;
            Itemtooltip = tooltip;
            Tilename = placetype;
            Rare = rare;
            TexturePath = texturePath;
            PathHasName = pathHasName;
            this.ItemValue = ItemValue;
        }

        public override string Name => InternalName != "" ? InternalName : base.Name;

		public override string Texture => string.IsNullOrEmpty(TexturePath) ? AssetDirectory.Debug : TexturePath + (PathHasName ? string.Empty : Name);

        public virtual void SafeSetDefaults() { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(Itemname ?? "ERROR");
            Tooltip.SetDefault(Itemtooltip ?? "Report me please!");
        }

        public override void SetDefaults()
        {
            if (Tilename is null)
                return;

            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = Mod.Find<ModTile>(Tilename).Type;
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
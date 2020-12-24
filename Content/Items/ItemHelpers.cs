using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items
{
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

        public override string Texture => string.IsNullOrEmpty(TexturePath) ? base.Texture : TexturePath + (PathHasName ? string.Empty : Name);

        public virtual void SafeSetDefaults() { }

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
            item.createTile = Tiletype;
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
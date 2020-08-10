using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items
{
    public abstract class QuickMaterial : ModItem
    {
        private readonly string Matname;
        private readonly string Mattooltip;
        private readonly int Maxstack;
        private readonly int Value;
        private readonly int Rare;

        public QuickMaterial(string name, string tooltip, int maxstack, int value, int rare)
        {
            Matname = name;
            Mattooltip = tooltip;
            Maxstack = maxstack;
            Value = value;
            Rare = rare;
        }

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

    public abstract class QuickTileItem : ModItem
    {
        public string Itemname;
        public string Itemtooltip;
        private readonly int Tiletype;
        private readonly int Rare;

        public QuickTileItem(string name, string tooltip, int placetype, int rare)
        {
            Itemname = name;
            Itemtooltip = tooltip;
            Tiletype = placetype;
            Rare = rare;
        }

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

        public QuickWallItem(string name, string tooltip, int placetype, int rare)
        {
            Itemname = name;
            Itemtooltip = tooltip;
            Walltype = placetype;
            Rare = rare;
        }
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
using StarlightRiver.Helpers;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Core
{
	public abstract class HeldItemProjectile : ModProjectile
	{
		private readonly float distance;
		protected Vector2 direction = Vector2.Zero;

		public HeldItemProjectile(float distance = 1)
		{
			this.distance = distance;
		}

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

			Vector2 pos = (Main.player[Projectile.owner].Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur() + offset + direction * distance;
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

	public abstract class QuickCritterItem : ModItem
	{
		private readonly string itemName;
		private readonly string itemTooltip;
		private readonly int maxStack;
		private readonly int value;
		private readonly int rarity;
		private readonly int npcID;
		private readonly string texturePath;
		private readonly bool pathHasName;

		public override string Texture => string.IsNullOrEmpty(texturePath) ? base.Texture : texturePath + (pathHasName ? string.Empty : Name);

		protected QuickCritterItem(string name, string tooltip, int value, int rare, int NPCType, string texturePath = null, bool pathHasName = false, int maxstack = 999)
		{
			itemName = name;
			itemTooltip = tooltip;
			maxStack = maxstack;
			this.value = value;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
			npcID = NPCType;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(itemName);
			Tooltip.SetDefault(itemTooltip);
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
			Item.maxStack = maxStack;
			Item.value = value;
			Item.rare = rarity;
		}
	}

	public abstract class QuickMaterial : ModItem
	{
		private readonly string name;
		private readonly string tooltip;
		private readonly int maxStack;
		private readonly int value;
		private readonly int rarity;
		private readonly string texturePath;
		private readonly bool pathHasName;

		public override string Texture => string.IsNullOrEmpty(texturePath) ? base.Texture : texturePath + (pathHasName ? string.Empty : Name);

		protected QuickMaterial(string name, string tooltip, int maxstack, int value, int rare, string texturePath = null, bool pathHasName = false)
		{
			this.name = name;
			this.tooltip = tooltip;
			maxStack = maxstack;
			this.value = value;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(name);
			Tooltip.SetDefault(tooltip);
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = maxStack;
			Item.value = value;
			Item.rare = rarity;
		}
	}

	public class QuickBannerItem : QuickTileItem
	{
		public QuickBannerItem(string placetype, string NPCDisplayName, string texturePath = null, int rare = ItemRarityID.Blue, int ItemValue = 1000) : //todo maybe: bool for tooltip
			base(NPCDisplayName + " BannerItem", NPCDisplayName + " Banner", "Nearby Players get a bonus against: " + NPCDisplayName, placetype, rare, texturePath, false, ItemValue)
		{ }

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.maxStack = 99;
		}
	}

	public abstract class QuickTileItem : ModItem
	{
		public string internalName = "";
		public string itemName;
		public string itemToolTip;
		//private readonly int Tiletype;
		private readonly string tileName;
		private readonly int rarity;
		private readonly string texturePath;
		private readonly bool pathHasName;
		private readonly int itemValue;

		public override string Name => internalName != "" ? internalName : base.Name;

		public override string Texture => string.IsNullOrEmpty(texturePath) ? AssetDirectory.Debug : texturePath + (pathHasName ? string.Empty : Name);

		public QuickTileItem() { }

		public QuickTileItem(string name, string tooltip, string placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
		{
			itemName = name;
			itemToolTip = tooltip;
			tileName = placetype;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
			itemValue = ItemValue;
		}

		public QuickTileItem(string internalName, string name, string tooltip, string placetype, int rare = ItemRarityID.White, string texturePath = null, bool pathHasName = false, int ItemValue = 0)
		{
			this.internalName = internalName;
			itemName = name;
			itemToolTip = tooltip;
			tileName = placetype;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
			itemValue = ItemValue;
		}

		public virtual void SafeSetDefaults() { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(itemName ?? "ERROR");
			Tooltip.SetDefault(itemToolTip ?? "Report me please!");
		}

		public override void SetDefaults()
		{
			if (tileName is null)
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
			Item.createTile = Mod.Find<ModTile>(tileName).Type;
			Item.rare = rarity;
			Item.value = itemValue;
			SafeSetDefaults();
		}
	}

	public abstract class QuickWallItem : ModItem
	{
		public string itemName;
		public string itemToolTip;
		private readonly int wallType;
		private readonly int rarity;
		private readonly string texturePath;
		private readonly bool pathHasName;

		public override string Texture => string.IsNullOrEmpty(texturePath) ? base.Texture : texturePath + (pathHasName ? string.Empty : Name);

		protected QuickWallItem(string name, string tooltip, int placetype, int rare, string texturePath = null, bool pathHasName = false)
		{
			itemName = name;
			itemToolTip = tooltip;
			wallType = placetype;
			rarity = rare;
			this.texturePath = texturePath;
			this.pathHasName = pathHasName;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(itemName);
			Tooltip.SetDefault(itemToolTip);
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
			Item.createWall = wallType;
			Item.rare = rarity;
		}
	}
}
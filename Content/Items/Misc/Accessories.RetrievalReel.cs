using StarlightRiver.Content.Items.BaseTypes;
using System;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class RetrievalReel : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public RetrievalReel() : base("Retrieval Reel", "wip") { }

		public override void Load()
		{
			//StarlightPlayer.ModifyHurtEvent += PreHurtAccessory;
		}

		public override void Unload()
		{
			//StarlightPlayer.ModifyHurtEvent -= PreHurtAccessory;

		}

		public override void SafeSetDefaults()
		{
			//Item.value = Item.sellPrice(gold: 1);
		}

		public override void AddRecipes()
		{

		}
	}

	public class ReelGlobalProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public int ReturnPlayerIndex = -1;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if(source is EntitySource_ItemUse)
			{
				EntitySource_ItemUse playersource = source as Terraria.DataStructures.EntitySource_ItemUse;
				if (playersource.Item.consumable && !projectile.noDropItem)
					ReturnPlayerIndex = playersource.Player.whoAmI;
			}

			base.OnSpawn(projectile, source);
		}

		public override bool PreDrawExtras(Projectile projectile)
		{
			if (ReturnPlayerIndex >= 0)
			{
				var a = projectile.noDropItem;
				Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.BlackTile.Value, projectile.position - Main.screenPosition, null, Color.Green, 0f, new Vector2(4, 4), 4, default);
			}
			return base.PreDrawExtras(projectile);
		}

	}

	public class ReelGlobalItem : GlobalItem
	{
		public override bool InstancePerEntity => true;

		public override void OnSpawn(Item item, IEntitySource source)
		{
			//todo: check source and if source proj has a return player set
			base.OnSpawn(item, source);
		}
	}
}
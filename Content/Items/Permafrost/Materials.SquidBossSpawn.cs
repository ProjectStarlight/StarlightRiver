using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using StarlightRiver.Content.Bosses.SquidBoss;

namespace StarlightRiver.Content.Items.Permafrost
{
	public class SquidBossSpawn : ModItem
    {
        public bool realItem = true;

        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Suspicious Looking Offering");
            Tooltip.SetDefault("Drop in prismatic waters to summon the one the Squiddites worship");
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
        }

		public override bool? UseItem(Player player)
		{
            Item.stack--;
            int i = Item.NewItem(player.GetSource_ItemUse(Item), player.Center + new Vector2(0, -32), Item.type);
            Main.item[i].velocity = Vector2.UnitX * player.direction * 15;
            return true;
		}

		public override bool CanPickup(Player player)
		{
            return realItem;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
            if (!realItem && Item.timeSinceItemSpawned > 600)
                Item.TurnToAir();
		}
	}

    public class SquidBossSpawnEndless : ModItem
	{
        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Gilded Suspicious Looking Offering");
            Tooltip.SetDefault("Drop in prismatic waters to summon the one the Squiddites worship\nInfinite uses");
        }

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 15;
            Item.useAnimation = 15;
        }

        public override bool? UseItem(Player player)
        {
            var noItems = !Main.item.Any(n => n.active && n.type == ModContent.ItemType<SquidBossSpawn>());
            var noBosses = !Main.npc.Any(n => n.active && n.type == ModContent.NPCType<SquidBoss>());

            if (noItems && noBosses)
            {
                int i = Item.NewItem(player.GetSource_ItemUse(Item), player.Center + new Vector2(0, -32), ModContent.ItemType<SquidBossSpawn>());
                Main.item[i].velocity = Vector2.UnitX * player.direction * 15;
                (Main.item[i].ModItem as SquidBossSpawn).realItem = false;
                return true;
            }

            return false;
        }
    }
}
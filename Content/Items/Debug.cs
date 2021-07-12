using StarlightRiver.Codex;
using StarlightRiver.Content.Tiles.CrashTech;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.Bosses.GlassBoss;

namespace StarlightRiver.Content.Items
{
    class TestRock : InworldItem
	{
		public override int NPCType => ModContent.NPCType<TestRockNPC>();

        public override string Texture => AssetDirectory.GUI + "Book1Closed";

		public override void SetDefaults()
		{
            item.width = 32;
            item.height = 32;
            item.useStyle = 1;
            item.useTime = 16;
            item.useAnimation = 16;
		}

		public override bool UseItem(Player player)
		{
            inWorldNPC.Release(false);
            return true;
		}
	}

    class TestRockNPC : InworldItemNPC
	{
        private int looseTime;

        public override string Texture => AssetDirectory.GUI + "Book1Closed";

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.height = 32;
            npc.lifeMax = 200;
            npc.friendly = false;
            npc.damage = 1;
            npc.dontTakeDamage = true;
            npc.aiStyle = -1;
            npc.noGravity = true;
        }

		public override bool CanPickup(Player player)
		{
            return looseTime > 30;
		}

		protected override void PutDown(Player player)
		{
            npc.velocity += Vector2.Normalize(player.Center - Main.MouseWorld) * -20;
		}

		protected override void PutDownNatural(Player player)
		{
            npc.velocity.X += 5 * player.direction;
		}

		public override void AI()
		{
            npc.noGravity = held;
            npc.noTileCollide = held;

            if (held)
            {
                npc.Center = owner.Center + new Vector2(0, -64);
                looseTime = 0;
            }

            else
                looseTime++;

            npc.velocity.X *= 0.95f;
		}
	}

    class DebugPotion : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + "VitricPick";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Debug Stick");
            Tooltip.SetDefault("Resets codex atm");
        }

        public override void SetDefaults()
        {
            item.damage = 10;
            item.melee = true;
            item.width = 38;
            item.height = 38;
            item.useTime = 18;
            item.useAnimation = 18;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;
            item.accessory = true;

            item.createTile = ModContent.TileType<MonkSpear>();
        }

        public override bool UseItem(Player player)
        {
            InworldItem.CreateItem<PlayerShield>(Main.MouseWorld);

            return true;
        }
    }
}

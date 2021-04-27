using StarlightRiver.Codex;
using StarlightRiver.Content.Tiles.CrashTech;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items
{
    class DebugPotion : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + "VitricPick";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Debug Stick");
            Tooltip.SetDefault("How did you get this?");
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

            item.createTile = ModContent.TileType<CrashPod>();
        }

		public override void UpdateEquip(Player player)
		{
            player.GetModPlayer<ShieldPlayer>().MaxShield += 280;
		}

		public override void UpdateInventory(Player player)
        {
            float rot = Main.rand.NextFloat(6.28f);

            Dust.NewDustPerfect(player.Center + Microsoft.Xna.Framework.Vector2.UnitX.RotatedBy(rot) * 300, ModContent.DustType<Bosses.GlassBoss.LavaSpew>(), Microsoft.Xna.Framework.Vector2.UnitX.RotatedBy(rot));
        }

        public override bool UseItem(Player player)
        {
            player.GetModPlayer<CodexHandler>().CodexState = 2;

            foreach (CodexEntry entry in player.GetModPlayer<CodexHandler>().Entries)
                entry.Locked = false;

            player.GetModPlayer<ShieldPlayer>().Shield = 0;

            foreach(NPC npc in Main.npc)
			{
                if (npc != null && npc.active)
                    npc.GetGlobalNPC<ShieldNPC>().Shield = 500;
			}

            return true;
        }
    }
}

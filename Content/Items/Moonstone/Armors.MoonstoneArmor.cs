using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
    [AutoloadEquip(EquipType.Head)]
    public class MoonstoneHead : ModItem
    {
        public int moonCharge = 0;
        public bool spearOn = false;

        internal static Item dummySpear = new Item();

        private int moonFlash = 0;

        public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override bool Autoload(ref string name)
		{
            On.Terraria.Player.KeyDoubleTap += ActivateSpear;
            On.Terraria.Main.MouseText_DrawItemTooltip += SpoofMouseItem;
            StarlightPlayer.PreDrawEvent += DrawMoonCharge;
            StarlightNPC.ModifyHitByItemEvent += ChargeFromMelee;
            StarlightNPC.ModifyHitByProjectileEvent += ChargeFromProjectile;

            return true;
		}

		private void ChargeFromProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(projectile.melee && projectile.type != ProjectileType<DatsuzeiProjectile>() && IsArmorSet(Main.player[projectile.owner]))
			{
                var head = Main.player[projectile.owner].armor[0].modItem as MoonstoneHead;

                int oldCharge = head.moonCharge;
                head.moonCharge += (int)(damage * 0.45f);

                if ((head.moonCharge >= 180 && oldCharge < 180) || (head.moonCharge >= 720 && oldCharge < 720))
                    head.moonFlash = 30;
			}
		}

		private void ChargeFromMelee(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
            if (item.melee && IsArmorSet(player))
            {
                var head = player.armor[0].modItem as MoonstoneHead;

                int oldCharge = head.moonCharge;
                head.moonCharge += (int)(damage * 0.45f);

                if ((head.moonCharge >= 180 && oldCharge < 180) || (head.moonCharge >= 720 && oldCharge < 720))
                    head.moonFlash = 30;
            }
        }

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonstone Helmet");
            Tooltip.SetDefault("2% increased melee critical strike chance\n+20 Barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 6;
        }

		public override void UpdateEquip(Player player)
		{
            player.meleeCrit += 2;
            player.GetModPlayer<ShieldPlayer>().MaxShield += 20;

            if(!IsArmorSet(player))
			{
                moonCharge = 0;
                spearOn = false;
			}
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = ("Accumulate lunar energy by dealing melee damage\ndouble tap DOWN to summon the legendary spear Datsuzei\nDatsuzei consumes lunar energy and dissapears at zero");

            if (moonCharge > 720)
                moonCharge = 720;

            if (moonFlash > 0)
                moonFlash--;

            Lighting.AddLight(player.Center + new Vector2(0, -16), new Vector3(0.55f, 0.5f, 0.9f) * moonCharge / 720f * 0.5f);

            if (spearOn)
            {
                if (!Main.mouseItem.IsTheSameAs(dummySpear) && !Main.mouseItem.IsAir)
                {
                    Main.LocalPlayer.QuickSpawnClonedItem(Main.mouseItem);
                }
                Main.mouseItem = dummySpear;
                player.inventory[58] = dummySpear;
                player.selectedItem = 58;

                moonCharge--;

                if (moonCharge <= 0)
                {
                    spearOn = false;
                    dummySpear.TurnToAir();
                }
            }
            else if (Main.mouseItem == dummySpear)
                Main.mouseItem = new Item();
        }

        private void ActivateSpear(On.Terraria.Player.orig_KeyDoubleTap orig, Player player, int keyDir)
        {
            if (keyDir == 0 && IsArmorSet(player))
            {
                var helm = player.armor[0].modItem as MoonstoneHead;

                if (helm.spearOn)
                {
                    helm.spearOn = false;
                    dummySpear.TurnToAir();
                }
                else if (helm.moonCharge > 180 && Datsuzei.activationTimer == 0 && !Main.projectile.Any(n => n.active && n.type == ProjectileType<DatsuzeiProjectile>() && n.owner == player.whoAmI))
                {
                    dummySpear.SetDefaults(ItemType<Datsuzei>());                
                    helm.spearOn = true;

                    int i = Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<DatsuzeiProjectile>(), 1, 0, player.whoAmI, -1, 160);
                    Main.projectile[i].timeLeft = 160;
                }
            }

            orig(player, keyDir);
        }

        private void SpoofMouseItem(On.Terraria.Main.orig_MouseText_DrawItemTooltip orig, Main self, int rare, byte diff, int X, int Y)
        {
            var player = Main.LocalPlayer;

            if(dummySpear.IsAir)
                dummySpear.SetDefaults(ItemType<Datsuzei>());

            if (IsMoonstoneArmor(Main.HoverItem) && IsArmorSet(player) && player.controlUp)
            {
                Main.HoverItem = dummySpear.Clone();
                Main.hoverItemName = dummySpear.Name;
            }

            orig(self, rare, diff, X, Y);
        }

        public bool IsMoonstoneArmor(Item item)
		{
            return item.type == ItemType<MoonstoneHead>() ||
                item.type == ItemType<MoonstoneChest>() ||
                item.type == ItemType<MoonstoneLegs>();
        }

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
            return head.type == ItemType<MoonstoneHead>() && body.type == ItemType<MoonstoneChest>() && legs.type == ItemType<MoonstoneLegs>();
        }

        public bool IsArmorSet(Player player)
        {
            return player.armor[0].type == ItemType<MoonstoneHead>() && player.armor[1].type == ItemType<MoonstoneChest>() && player.armor[2].type == ItemType<MoonstoneLegs>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
            var player = Main.LocalPlayer;

            if (IsArmorSet(player))
            {
                if (!player.controlUp)
                {
                    TooltipLine spearQuery = new TooltipLine(mod, "StarlightRiver:ArmorSpearQuery", "hold UP for Datsuzei stats");
                    spearQuery.overrideColor = new Color(200, 200, 200);

                    tooltips.Add(spearQuery);
                }
            }
		}

        private void DrawMoonCharge(Player player, SpriteBatch spriteBatch)
        {
            if (IsArmorSet(player) && !player.dead)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

                var head = player.armor[0].modItem as MoonstoneHead;
                float charge = head.moonCharge / 720f;

                var texRing = GetTexture(AssetDirectory.VitricItem + "BossBowRing");
                var color = new Color(130, 110, 225) * (0.5f + charge * 0.5f);

                if (charge <= 180 / 720f)
                    color = new Color(150, 150, 150) * (0.5f + charge * 0.5f);

                if (charge >= 1 || head.spearOn)
                    color = new Color(150, 150 + (int)(Math.Sin(Main.GameUpdateCount * 0.2f) * 20), 255) * (0.5f + charge * 0.5f);

                color = Color.Lerp(color, Color.White, head.moonFlash / 30f);

                spriteBatch.Draw(texRing, player.MountedCenter + new Vector2(0, -16) + Vector2.UnitY * player.gfxOffY - Main.screenPosition, null, color, Main.GameUpdateCount * 0.01f, texRing.Size() / 2, 0.08f + charge * 0.05f, 0, 0);

                spriteBatch.End();

                SamplerState samplerState = Main.DefaultSamplerState;

                if (player.mount.Active)
                    samplerState = Main.MountedSamplerState;

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, samplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<MoonstoneBar>(), 5);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class MoonstoneChest : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonstone Chestpiece");
            Tooltip.SetDefault("+35 Barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 7;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<MoonstoneHead>() && body.type == ItemType<MoonstoneChest>() && legs.type == ItemType<MoonstoneLegs>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;

            if (IsArmorSet(player.armor[0], player.armor[1], player.armor[2]))
            {
                if (!player.controlUp)
                {
                    TooltipLine spearQuery = new TooltipLine(mod, "StarlightRiver:ArmorSpearQuery", "hold UP for Datsuzei stats");
                    spearQuery.overrideColor = new Color(200, 200, 200);

                    tooltips.Add(spearQuery);
                }
            }
        }

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<ShieldPlayer>().MaxShield += 35;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<MoonstoneBar>(), 15);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class MoonstoneLegs : ModItem
    {
        public override string Texture => AssetDirectory.MoonstoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonstone Greaves");
            Tooltip.SetDefault("Improved acceleration\n +25 Barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 6;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<MoonstoneHead>() && body.type == ItemType<MoonstoneChest>() && legs.type == ItemType<MoonstoneLegs>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;

            if (IsArmorSet(player.armor[0], player.armor[1], player.armor[2]))
            {
                if (!player.controlUp)
                {
                    TooltipLine spearQuery = new TooltipLine(mod, "StarlightRiver:ArmorSpearQuery", "hold UP for Datsuzei stats");
                    spearQuery.overrideColor = new Color(200, 200, 200);

                    tooltips.Add(spearQuery);
                }
            }
        }

        public override void UpdateEquip(Player player)
        {
            player.runAcceleration *= 1.5f;
            player.GetModPlayer<ShieldPlayer>().MaxShield += 25;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<MoonstoneBar>(), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}
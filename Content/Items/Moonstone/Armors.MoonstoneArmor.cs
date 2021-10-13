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

        public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override bool Autoload(ref string name)
		{
            On.Terraria.Player.KeyDoubleTap += ActivateSpear;
            On.Terraria.Main.MouseText_DrawItemTooltip += SpoofMouseItem;
            StarlightPlayer.PostDrawEvent += DrawMoonCharge;
            StarlightNPC.ModifyHitByItemEvent += ChargeFromMelee;
            StarlightNPC.ModifyHitByProjectileEvent += ChargeFromProjectile;

            return true;
		}

		private void ChargeFromProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if(projectile.melee && projectile.type != ProjectileType<DatsuzeiProjectile>() && Main.player[projectile.owner].armor[0].type == ItemType<MoonstoneHead>())
			{
                (Main.player[projectile.owner].armor[0].modItem as MoonstoneHead).moonCharge += (int)(damage * 0.35f);
			}
		}

		private void ChargeFromMelee(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
            if (item.melee && player.armor[0].type == ItemType<MoonstoneHead>())
            {
                (player.armor[0].modItem as MoonstoneHead).moonCharge += (int)(damage * 0.35f);
            }
        }

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Moonstone Helmet");
            Tooltip.SetDefault("2% increased melee critical strike chance\n+20 barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 1;
        }

		public override void UpdateEquip(Player player)
		{
            player.meleeCrit += 2;
            player.GetModPlayer<ShieldPlayer>().MaxShield += 20;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = ("Accumulate lunar energy by dealing melee damage\ndouble tap DOWN to summon the legendary spear Datsuzei\nDatsuzei consumes lunar energy and dissapears at zero");

            if (moonCharge > 720)
                moonCharge = 720;

            if (spearOn)
            {
                player.inventory[58] = dummySpear;
                Main.mouseItem = dummySpear;
                player.selectedItem = 58;

                moonCharge--;

                if (moonCharge <= 0)
                {
                    spearOn = false;
                    dummySpear.TurnToAir();
                }
            }
			else if (Main.mouseItem == dummySpear)
			{
                Main.mouseItem = new Item();
            }

            if (player.HeldItem != dummySpear)
            {
                if (Datsuzei.activationTimer > 0)
                    Datsuzei.activationTimer -= 2;
                else
                {
                    Datsuzei.activationTimer = 0;
                    Datsuzei.sparkles.ClearParticles();
                }
            }
        }

        private void ActivateSpear(On.Terraria.Player.orig_KeyDoubleTap orig, Player player, int keyDir)
        {
            if (keyDir == 0 && player.armor[0].type == ItemType<MoonstoneHead>())
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

            if (IsMoonstoneArmor(Main.HoverItem) && IsArmorSet(player.armor[0], player.armor[1], player.armor[2]) && player.controlUp)
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

        private void DrawMoonCharge(Player player, SpriteBatch spriteBatch)
        {
            if (player.armor[0].type == ItemType<MoonstoneHead>() && !player.dead)
            {
                float charge = (player.armor[0].modItem as MoonstoneHead).moonCharge / 720f;

                //Utils.DrawBorderString(spriteBatch, "charge: " + charge, player.Center - Main.screenPosition + new Vector2(-50, -200), Color.White);
                DrawRing(spriteBatch, player.Center + new Vector2(-32 * player.direction, -20), 0.2f, 0.1f, Main.GameUpdateCount * 0.02f, player.direction == 1 ? 0 : 3.14f, (0.5f + charge * 0.5f), Color.White * (0.5f + charge * 0.5f));
            }
        }

        private void DrawRing(SpriteBatch sb, Vector2 pos, float w, float h, float rotation, float facing, float prog, Color color)
        {
            var texRing = GetTexture(AssetDirectory.MoonstoneItem + "MoonSigilFront");
            var effect = Filters.Scene["BowRing"].GetShader().Shader;

            effect.Parameters["uProgress"].SetValue(rotation);
            effect.Parameters["uColor"].SetValue(color.ToVector3());
            effect.Parameters["uImageSize1"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["uOpacity"].SetValue(prog);

            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            var target = toRect(pos, (int)(16 * (w + prog)), (int)(60 * (h + prog)));
            sb.Draw(texRing, target, null, color * prog, facing, texRing.Size() / 2, 0, 0);

            sb.End();
            sb.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
        }

        private Rectangle toRect(Vector2 pos, int w, int h)
        {
            return new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), w, h);
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
            Tooltip.SetDefault("+35 barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 3;
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
            Tooltip.SetDefault("Improved acceleration\n +25 barrier");
        }

        public override void SetDefaults()
        {
            item.width = 18;
            item.height = 18;
            item.value = 1;
            item.rare = ItemRarityID.Green;
            item.defense = 5;
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
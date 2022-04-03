using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Gravedigger
{
    [AutoloadEquip(EquipType.Head)]
    public class PoltergeistHead : ModItem
    {
        public List<Projectile> minions = new List<Projectile>();
        public int manaRestrictFade = 0;
        public int sleepTimer;

        public override string Texture => AssetDirectory.GravediggerItem + Name;

        public override void Load()
        {
            On.Terraria.Player.KeyDoubleTap += HauntItem;
            On.Terraria.Main.DrawInterface_Resources_Mana += DrawRottenMana;
            StarlightItem.CanUseItemEvent += ControlItemUse;
            
        }

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Haunting Hood");
            Tooltip.SetDefault("15% increased magic critical strike damage");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.GetModPlayer<CritMultiPlayer>().MagicCritMult += 0.15f;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<PoltergeistChest>() && legs.type == ItemType<PoltergeistLegs>();
        }

        public override void UpdateArmorSet(Player Player)
        {
            Player.setBonus = 
                "Double tap DOWN with a magic weapon to haunt or unhaunt it, or with an empty hand to unhaunt all\n" +
                "Haunted weapons float around you and attack automatically, but decrease your max mana\n" +
                "Haunted weapons become disinterested in non-magic users and can't be used while haunted";

            minions.RemoveAll(n => !n.active || n.type != ProjectileType<PoltergeistMinion>());


            for (int k = 0; k < 4; k++) //smooth animation for restricting/unrestricting mana
            {
                if (GetManaRestrict() > manaRestrictFade)
                    manaRestrictFade++;

                if (GetManaRestrict() < manaRestrictFade)
                    manaRestrictFade--;
            }

            if (Player.statMana > Player.statManaMax2 - manaRestrictFade) //restrict mana
                Player.statMana = Player.statManaMax2 - manaRestrictFade;

            if (Player == Main.LocalPlayer && sleepTimer == 1 && minions.Count > 0) //warning message
                Main.NewText("Your haunted weapons seem bored...", new Color(200, 120, 255));

            if (sleepTimer > 0) //decrement sleep timer
                sleepTimer--;
            
        }

        private void HauntItem(On.Terraria.Player.orig_KeyDoubleTap orig, Player Player, int keyDir)
        {
            if (keyDir == 0 && Player.armor[0].type == ItemType<PoltergeistHead>())
            {
                var Item = Player.HeldItem;
                var helm = Player.armor[0].ModItem as PoltergeistHead;

                if(Item.IsAir) //clear from empty hand
				{
                    helm.minions.Clear();
                    orig(Player, keyDir);
                    return;
                }

                if (helm.minions.Any(n => (n.ModProjectile as PoltergeistMinion).Item.type == Item.type)) //removal
                {
                    helm.minions.RemoveAll(n => (n.ModProjectile as PoltergeistMinion).Item.type == Item.type);
                    orig(Player, keyDir);
                    return;
                }

                if (Item.magic && Item.mana > 0 && !Item.channel && Item.shoot > 0 && helm.GetManaRestrict(Item) <= Player.statManaMax2) //addition
                {                  
                    int i = Projectile.NewProjectile(Player.Center, Vector2.Zero, ProjectileType<PoltergeistMinion>(), 0, 0, Player.whoAmI);
                    var proj = Main.projectile[i];
                    (proj.ModProjectile as PoltergeistMinion).Item = Item.Clone();

                    helm.minions.Add(proj);
                    helm.sleepTimer = 1200;
                }
            }

            orig(Player, keyDir);
        }

        private int GetManaRestrict(Item add = null)
		{
            int manaRestrict = 0;
            for (int k = 0; k < minions.Count; k++)
            {
                
                var proj = minions[k].ModProjectile as PoltergeistMinion;
                manaRestrict += (int)(proj.Item.mana * (60f / proj.Item.useTime) * 2);
            }
            return manaRestrict + (add is null ? 0 : ((int)(add.mana * (60f / add.useTime) * 2)));
        }

        private bool ControlItemUse(Item Item, Player Player)
        {
            if (Player.armor[0].type == ItemType<PoltergeistHead>())
            {
                var helm = Player.armor[0].ModItem as PoltergeistHead;

                if (helm.minions.Any(n => (n.ModProjectile as PoltergeistMinion)?.Item?.type == Item.type))
                    return false;

                if (Item.magic)
                    helm.sleepTimer = 1200;
            }

            return true;
        }

        private void DrawRottenMana(On.Terraria.Main.orig_DrawInterface_Resources_Mana orig)
        {
            orig();

            Player Player = Main.LocalPlayer;

            if (Player.armor[0].type != ItemType<PoltergeistHead>())
                return;

            var helm = Player.armor[0].ModItem as PoltergeistHead;

            for (int i = 1; i < Player.statManaMax2 / 20 + 1; i++) //iterate each mana star
            {
                int manaDrawn = i * 20; //the amount of mana drawn by this star and all before it

                float starHeight = MathHelper.Clamp(((Main.player[Main.myPlayer].statMana - (i - 1) * 20) / 20f) / 4f + 0.75f, 0.75f, 1); //height of the current star based on current mana

                if (Player.statMana <= i * 20 && Player.statMana >= (i - 1) * 20) //pulsing star for the "current" star
                    starHeight += Main.cursorScale - 1;

				var rottenManaAmount = Player.statManaMax2 - helm.manaRestrictFade; //amount of mana to draw as rotten

				if (rottenManaAmount < manaDrawn)
				{
                    if(manaDrawn - rottenManaAmount < 20)
					{
                        var tex1 = Request<Texture2D>(AssetDirectory.GravediggerItem + "RottenMana").Value;
                        var pos1 = new Vector2(Main.screenWidth - 25, (30 + Main.manaTexture.Height / 2f) + (Main.manaTexture.Height - Main.manaTexture.Height * starHeight) / 2f + (28 * (i - 1)));

                        int off = (int)(rottenManaAmount % 20 / 20f * tex1.Height);
                        var source = new Rectangle(0, off, tex1.Width, tex1.Height - off);
                        pos1.Y += off;

                        Main.spriteBatch.Draw(tex1, pos1, source, Color.White, 0f, tex1.Size() / 2, starHeight, 0, 0);
                        continue;
                    }

                    var tex = Request<Texture2D>(AssetDirectory.GravediggerItem + "RottenMana").Value;
                    var pos = new Vector2(Main.screenWidth - 25, (30 + Main.manaTexture.Height / 2f) + (Main.manaTexture.Height - Main.manaTexture.Height * starHeight) / 2f + (28 * (i - 1)));

                    Main.spriteBatch.Draw(tex, pos, null, Color.White, 0f, tex.Size() / 2, starHeight, 0, 0);
                }
            }
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class PoltergeistChest : ModItem
    {
        public override string Texture => AssetDirectory.GravediggerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Haunting Breastplate");
            Tooltip.SetDefault("5% increased magic damage\n15% increased DoT resistance");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 6;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.magicDamage += 0.05f;
            Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.15f;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class PoltergeistLegs : ModItem
    {
        public override string Texture => AssetDirectory.GravediggerItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Haunting Robes");
            Tooltip.SetDefault("+40 maximum mana");
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = 1;
            Item.rare = ItemRarityID.Green;
            Item.defense = 5;
        }

        public override void UpdateEquip(Player Player)
        {
            Player.statManaMax2 += 40;
        }
    }
}
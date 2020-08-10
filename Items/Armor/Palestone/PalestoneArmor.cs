using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Armor.Palestone
{
    [AutoloadEquip(EquipType.Head)]
    public class PalestoneHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Helm");
            Tooltip.SetDefault("2% increased melee critial strike chance");
        }

        public override void SetDefaults()
        {
            item.width = 22;
            item.height = 22;
            item.value = 8000;
            item.defense = 2;
        }
        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 2;
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class PalestoneChest : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Chestplate");
            Tooltip.SetDefault("2% increased melee critial strike change");
        }

        public override void SetDefaults()
        {
            item.width = 40;
            item.height = 22;
            item.value = 6000;
            item.defense = 3;
        }
        public override void UpdateEquip(Player player)
        {
            player.meleeCrit += 2;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<PalestoneHead>() && legs.type == ItemType<PalestoneLegs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "anyway palestone set bonus i had in mind was that getting kills forms a big stone tablet to spin around the player (not in a circle, more like an orbit (think the overgrowth enemy that throws boulders)) which would provide damage resistance per tablet with a cap of 3, and taking damage would damage the tablets (a tablet can be damaged 3x before breaking)";
            PalestonePlayer palestonePlayer = player.GetModPlayer<PalestonePlayer>();
            foreach (int i in palestonePlayer.tablets)
            {
                if (i > 0)
                {
                    player.endurance += 0.1f;
                }
            }
        }
    }
    public class PalestonePlayer : ModPlayer
    {
        public float counter = 0;
        public int[] tablets = new int[3];
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            if (item.melee)
            {
                if (target.life <= 0)
                {
                    for (int i = 0; i < tablets.Length; i++)
                    {
                        if (tablets[i] == 0)
                        {
                            tablets[i] = 3;
                            break;
                        }
                    }
                }
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (proj.melee)
            {
                if (target.life <= 0)
                {
                    for (int i = 0; i < tablets.Length; i++)
                    {
                        if (tablets[i] == 0)
                        {
                            tablets[i] = 3;
                            break;
                        }
                    }
                }
            }
        }
        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            for (int i = 0; i < tablets.Length; i++)
            {
                if (tablets[i] > 0)
                {
                    tablets[i]--;
                }
            }
        }
        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            Action<PlayerDrawInfo> backTarget = s => DrawGlowmasks(s, false); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
            PlayerLayer backLayer = new PlayerLayer("PalestoneLayer", "Armor Glowmask", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
            layers.Insert(layers.IndexOf(layers.First()), backLayer); //Insert the layer at the appropriate index. 

            Action<PlayerDrawInfo> frontTarget = s => DrawGlowmasks(s, true); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
            PlayerLayer frontLayer = new PlayerLayer("PalestoneLayer", "Armor Glowmask", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
            layers.Insert(layers.IndexOf(layers.Last()), frontLayer); //Insert the layer at the appropriate index. 

            float getTabletRotation(int currentTablet) => currentTablet / (tablets.FirstOrDefault(x => x == 0) + 1) * 6.28f + (float)player.GetModPlayer<StarlightPlayer>().Timer % 120 / 120 * 6.28f;
            Vector2 getTabletPosition(int currentTablet)
            {
                float dist = 50;
                float rot = getTabletRotation(currentTablet);

                float posX = player.Center.X + (float)(Math.Cos(rot) * dist);
                float posY = player.Center.Y + (float)(Math.Sin(rot) * dist) / 2;
                return new Vector2(posX, posY);
            }
            void DrawGlowmasks(PlayerDrawInfo info, bool back)
            {
                for (int k = 0; k < tablets.Length; k++)
                {
                    float rot = getTabletRotation(k);
                    if ((back && rot % 6.28f < 3.14f || !back && rot % 6.28f >= 3.14f) && tablets[k] > 0)
                    {
                        Vector2 pos = getTabletPosition(k);
                        Texture2D texture = GetTexture("StarlightRiver/Items/Armor/Palestone/Tablet");
                        Main.playerDrawData.Add(new DrawData(
                            texture,
                            pos,  //position
                            new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, 0, texture.Width, texture.Height)), //source
                            Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), //color
                            0, //rotation
                            new Vector2(texture.Width / 2, texture.Height / 2), //origin
                            1f, //scale
                            SpriteEffects.None, 0));
                    }
                }
            }
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class PalestoneLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palestone Leggings");
            Tooltip.SetDefault("Slightly increases movement speed");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
            item.value = 4000;
            item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.1f;
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;


namespace StarlightRiver.Helpers
{
    public static partial class Helper
    {
        public static void BoostAllDamage(this Player player, float damage, int crit = 0)
        {
            player.meleeDamage += damage;
            player.rangedDamage += damage;
            player.magicDamage += damage;
            player.minionDamage += damage;
            player.thrownDamage += damage;

            player.thrownCrit += crit;
            player.rangedCrit += crit;
            player.meleeCrit += crit;
            player.magicCrit += crit;
        }

        public static void NewItemSpecific(Vector2 position, Item item)
        {
            int targetIndex = 400;
            Main.item[400] = new Item(); //Vanilla seems to make sure to set the dummy here, so I will too.

            if (Main.netMode != NetmodeID.MultiplayerClient) //Main.Item index finder from vanilla
            {
                for (int j = 0; j < 400; j++)
                {
                    if (!Main.item[j].active && Main.itemLockoutTime[j] == 0)
                    {
                        targetIndex = j;
                        break;
                    }
                }
            }

            if (targetIndex == 400 && Main.netMode != NetmodeID.MultiplayerClient) //some sort of vanilla failsafe if no safe index is found it seems?
            {
                int num2 = 0;
                for (int k = 0; k < 400; k++)
                {
                    if (Main.item[k].spawnTime - Main.itemLockoutTime[k] > num2)
                    {
                        num2 = Main.item[k].spawnTime - Main.itemLockoutTime[k];
                        targetIndex = k;
                    }
                }
            }

            Main.item[targetIndex] = item;
            Main.item[targetIndex].position = position;

            if (ItemSlot.Options.HighlightNewItems && item.type >= ItemID.None && !ItemID.Sets.NeverShiny[item.type]) //vanilla item highlight system
            {
                item.newAndShiny = true;
            }
            if (Main.netMode == NetmodeID.Server) //syncing
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, targetIndex, 0, 0f, 0f, 0, 0, 0);
                item.FindOwner(item.whoAmI);
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                item.owner = Main.myPlayer;
            }
        }

        public static Item NewItemPerfect(Vector2 position, Vector2 velocity, int type, int stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
        {
            int targetIndex = 400;
            Main.item[400] = new Item(); //Vanilla seems to make sure to set the dummy here, so I will too.

            if (Main.netMode != NetmodeID.MultiplayerClient) //Main.Item index finder from vanilla
            {
                if (reverseLookup)
                {
                    for (int i = 399; i >= 0; i--)
                    {
                        if (!Main.item[i].active && Main.itemLockoutTime[i] == 0)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < 400; j++)
                    {
                        if (!Main.item[j].active && Main.itemLockoutTime[j] == 0)
                        {
                            targetIndex = j;
                            break;
                        }
                    }
                }
            }
            if (targetIndex == 400 && Main.netMode != NetmodeID.MultiplayerClient) //some sort of vanilla failsafe if no safe index is found it seems?
            {
                int num2 = 0;
                for (int k = 0; k < 400; k++)
                {
                    if (Main.item[k].spawnTime - Main.itemLockoutTime[k] > num2)
                    {
                        num2 = Main.item[k].spawnTime - Main.itemLockoutTime[k];
                        targetIndex = k;
                    }
                }
            }
            Main.itemLockoutTime[targetIndex] = 0; //normal stuff
            Item item = Main.item[targetIndex];
            item.SetDefaults(type, false);
            item.Prefix(prefixGiven);
            item.position = position;
            item.velocity = velocity;
            item.active = true;
            item.spawnTime = 0;
            item.stack = stack;

            item.wet = Collision.WetCollision(item.position, item.width, item.height); //not sure what this is, from vanilla

            if (ItemSlot.Options.HighlightNewItems && item.type >= ItemID.None && !ItemID.Sets.NeverShiny[item.type]) //vanilla item highlight system
            {
                item.newAndShiny = true;
            }
            if (Main.netMode == NetmodeID.Server && !noBroadcast) //syncing
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, targetIndex, noGrabDelay ? 1 : 0, 0f, 0f, 0, 0, 0);
                item.FindOwner(item.whoAmI);
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                item.owner = Main.myPlayer;
            }
            return item;
        }

        public static Texture2D GetItemTexture(Item item)
        {
            if (item.type < Main.maxItemTypes) return Main.itemTexture[item.type];
            else return GetTexture(item.modItem.Texture);
        }

        public static Texture2D GetItemTexture(int type)
        {
            if (type < Main.maxItemTypes) return Main.itemTexture[type];
            else
            {
                Item item = new Item();
                item.SetDefaults(type);
                return GetTexture(item.modItem.Texture);
            }
        }
    }
}


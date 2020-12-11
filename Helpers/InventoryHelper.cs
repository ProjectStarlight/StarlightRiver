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
        /// <summary>
        /// Consumes the items specified by a predicate.
        /// </summary>
        /// <param name="inventory">The pool of items to consume items from.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="count">The number of items to consume.</param>
        /// <returns>If successful, true; otherwise, false.</returns>
        public static bool ConsumeItems(this Item[] inventory, Predicate<Item> predicate, int count)
        {
            var items = inventory.GetItems(predicate, count);

            // If the sum of items is less than the required amount, don't bother.
            if (items.Sum(i => inventory[i].stack) < count)
                return false;

            for (int i = 0; i < items.Count; i++)
            {
                Item item = inventory[items[i]];

                // If we're at the last item stack, and we're not going to consume the whole thing, just decrease its count by the amount needed.
                if (i == items.Count - 1 && count < item.stack)
                {
                    item.stack -= count;
                }
                // Otherwise, delete the item and decrement count as needed.
                else
                {
                    count -= item.stack;
                    item.TurnToAir();
                }
            }
            return true;
        }

        /// <summary>
        /// Gets a list of item indeces from an inventory matching a predicate.
        /// </summary>
        /// <param name="inventory">The pool of items to search.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="stopCountingAt">The number of items to search before stopping.</param>
        /// <returns>The items matching the predicate.</returns>
        public static List<int> GetItems(this Item[] inventory, Predicate<Item> predicate, int stopCountingAt = int.MaxValue)
        {
            var indeces = new List<int>();
            for (int i = 0; i < inventory.Length; i++)
            {
                if (stopCountingAt <= 0)
                    break;
                if (predicate(inventory[i]))
                {
                    indeces.Add(i);
                    stopCountingAt -= inventory[i].stack;
                }
            }
            return indeces;
        }
        public static bool HasItem(Player player, int type, int count)
        {
            int items = 0;

            for (int k = 0; k < player.inventory.Length; k++)
            {
                Item item = player.inventory[k];
                if (item.type == type) items += item.stack;
            }

            return items >= count;
        }

        public static bool TryTakeItem(Player player, int type, int count)
        {
            if (HasItem(player, type, count))
            {
                int toTake = count;

                for (int k = 0; k < player.inventory.Length; k++)
                {
                    Item item = player.inventory[k];

                    if (item.type == type)
                    {
                        int stack = item.stack;
                        for (int i = 0; i < stack; i++)
                        {
                            item.stack--;
                            if (item.stack == 0) item.TurnToAir();

                            toTake--;
                            if (toTake <= 0) break;
                        }
                    }
                    if (toTake == 0) break;
                }

                return true;
            }
            else return false;
        }
        public static bool HasEquipped(Player player, int ItemID)
        {
            //This needs to be one more: 8, not 7, or <= instead of < -IDG
            for (int k = 3; k < 8 + player.extraAccessorySlots; k++) if (player.armor[k].type == ItemID) return true;
            return false;
        }
    }
}


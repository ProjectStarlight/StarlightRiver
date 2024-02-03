using System;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Helpers
{
	public static partial class Helper
	{
		/// <summary>
		/// Consumes the Items specified by a predicate.
		/// </summary>
		/// <param name="inventory">The pool of Items to consume Items from.</param>
		/// <param name="predicate">The predicate.</param>
		/// <param name="count">The number of Items to consume.</param>
		/// <returns>If successful, true; otherwise, false.</returns>
		public static bool ConsumeItems(this Item[] inventory, Predicate<Item> predicate, int count)
		{
			List<int> Items = inventory.GetItems(predicate, count);

			// If the sum of Items is less than the required amount, don't bother.
			if (Items.Sum(i => inventory[i].stack) < count)
				return false;

			for (int i = 0; i < Items.Count; i++)
			{
				Item Item = inventory[Items[i]];

				// If we're at the last Item stack, and we're not going to consume the whole thing, just decrease its count by the amount needed.
				if (i == Items.Count - 1 && count < Item.stack)
				{
					Item.stack -= count;
				}
				// Otherwise, delete the Item and decrement count as needed.
				else
				{
					count -= Item.stack;
					Item.TurnToAir();
				}
			}

			return true;
		}

		/// <summary>
		/// Gets a list of Item indeces from an inventory matching a predicate.
		/// </summary>
		/// <param name="inventory">The pool of Items to search.</param>
		/// <param name="predicate">The predicate.</param>
		/// <param name="stopCountingAt">The number of Items to search before stopping.</param>
		/// <returns>The Items matching the predicate.</returns>
		public static List<int> GetItems(this Item[] inventory, Predicate<Item> predicate, int stopCountingAt = int.MaxValue)
		{
			var indicies = new List<int>();

			for (int i = 0; i < inventory.Length; i++)
			{
				if (stopCountingAt <= 0)
					break;

				if (predicate(inventory[i]))
				{
					indicies.Add(i);
					stopCountingAt -= inventory[i].stack;
				}
			}

			return indicies;
		}
		public static bool HasItem(Player Player, int type, int count)
		{
			int items = 0;

			for (int k = 0; k < Player.inventory.Length; k++)
			{
				Item Item = Player.inventory[k];

				if (Item.type == type)
					items += Item.stack;
			}

			return items >= count;
		}

		/// <summary>
		/// returns first open non ammo or coin slot if Player has atleast 1 slot empty otherwise returns -1
		/// </summary>
		public static int getFreeInventorySlot(Player Player)
		{
			for (int k = 0; k < 49; k++)
			{
				Item Item = Player.inventory[k];

				if (Item is null || Item.IsAir)
					return k;
			}

			return -1;
		}

		public static bool TryTakeItem(Player Player, int type, int count)
		{
			if (HasItem(Player, type, count))
			{
				int toTake = count;

				for (int k = 0; k < Player.inventory.Length; k++)
				{
					Item Item = Player.inventory[k];

					if (Item.type == type)
					{
						int stack = Item.stack;

						for (int i = 0; i < stack; i++)
						{
							Item.stack--;

							if (Item.stack == 0)
								Item.TurnToAir();

							toTake--;

							if (toTake <= 0)
								break;
						}
					}

					if (toTake == 0)
						break;
				}

				return true;
			}
			else
			{
				return false;
			}
		}
		public static bool HasEquipped(Player Player, int ItemID)
		{
			//This needs to be one more: 8, not 7, or <= instead of < -IDG
			for (int k = 3; k < 8 + Player.extraAccessorySlots; k++)
			{
				if (Player.armor[k].type == ItemID)
					return true;
			}

			return false;
		}
	}
}
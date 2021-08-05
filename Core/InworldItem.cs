using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace StarlightRiver.Core
{
	public abstract class InworldItem : ModItem
	{
		public InworldItemNPC inWorldNPC;
		public NPC npc => inWorldNPC.npc;

		public virtual int NPCType => 0;

		public static NPC CreateItem<T>(Vector2 pos) where T : InworldItem
		{
			Item item = new Item();
			item.SetDefaults(ModContent.ItemType<T>());
			var inworldItem = item.modItem as InworldItem;

			int index = NPC.NewNPC((int)pos.X, (int)pos.Y, inworldItem.NPCType);
			inworldItem.inWorldNPC = Main.npc[index].modNPC as InworldItemNPC;
			inworldItem.inWorldNPC.inWorldItem = inworldItem;

			return Main.npc[index];
		}

		public override void UpdateInventory(Player player)
		{
			if (npc is null)
				item.TurnToAir();

			if (player.HeldItem.type != item.type)
				inWorldNPC.Release(true);
		}	
	}

	public abstract class InworldItemNPC : ModNPC
	{
		public InworldItem inWorldItem;
		public Player owner;
		public bool held => owner != null;

		public Item item => inWorldItem.item;

		protected virtual void Pickup(Player player) { }

		protected virtual void PutDown(Player player) { }

		protected virtual void PutDownNatural(Player player) { }

		public virtual bool CanPickup(Player player) => false;

		public void Release(bool natural)
		{
			if (natural)
				PutDownNatural(owner);
			else
				PutDown(owner);

			owner.selectedItem = 9;
			owner.inventory[58] = new Item();
			Main.mouseItem = owner.inventory[58];
			owner = null;
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			if (CanPickup(target) && !held && target.Hitbox.Intersects(npc.Hitbox))
			{
				target.inventory[58] = item;
				Main.mouseItem = item;
				target.selectedItem = 58;

				owner = target;
				Pickup(target);
			}

			return false;
		}

		public override void PostAI()
		{
			if (inWorldItem is null)
				npc.active = false;

			if (held && owner.HeldItem.type != item.type)
				Release(true);
		}
	}
}

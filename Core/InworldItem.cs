using Terraria.DataStructures;

namespace StarlightRiver.Core
{
	public abstract class InworldItem : ModItem
	{
		public InworldItemNPC inWorldNPC;

		public NPC NPC => inWorldNPC?.NPC;

		public virtual bool VisibleInUI => true;
		public virtual int NPCType => 0;

		public static NPC CreateItem<T>(Vector2 pos) where T : InworldItem
		{
			var Item = new Item();
			Item.SetDefaults(ModContent.ItemType<T>());
			var inworldItem = Item.ModItem as InworldItem;

			int index = NPC.NewNPC(new EntitySource_SpawnNPC(), (int)pos.X, (int)pos.Y, inworldItem.NPCType);
			inworldItem.inWorldNPC = Main.npc[index].ModNPC as InworldItemNPC;
			inworldItem.inWorldNPC.inWorldItem = inworldItem;

			return Main.npc[index];
		}

		public override void UpdateInventory(Player Player)
		{
			if (NPC is null)
				Item.TurnToAir();

			if (Player.HeldItem.type != Item.type)
				inWorldNPC?.Release(true);
		}
	}

	public abstract class InworldItemNPC : ModNPC
	{
		public InworldItem inWorldItem;
		public Player owner;

		public bool Held => owner != null;

		public Item Item => inWorldItem.Item;

		protected virtual void Pickup(Player Player) { }

		protected virtual void PutDown(Player Player) { }

		protected virtual void PutDownNatural(Player Player) { }

		public virtual bool CanPickup(Player Player)
		{
			return false;
		}

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
			if (CanPickup(target) && !Held && target.Hitbox.Intersects(NPC.Hitbox))
			{
				target.inventory[58] = Item;
				Main.mouseItem = Item;
				target.selectedItem = 58;

				owner = target;
				Pickup(target);
			}

			return false;
		}

		public override void PostAI()
		{
			if (inWorldItem is null)
				NPC.active = false;

			if (Held && owner.HeldItem.type != Item.type)
				Release(true);
		}
	}
}
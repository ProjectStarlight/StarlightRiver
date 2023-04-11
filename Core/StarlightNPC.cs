namespace StarlightRiver.Core
{
	public partial class StarlightNPC : GlobalNPC
	{
		public int Age;
		public int DoT;
		public bool dontDropItems;

		public override bool InstancePerEntity => true;

		//public override bool CloneNewInstances => true;

		public override bool PreKill(NPC npc)
		{
			return !dontDropItems;
		}

		public override bool PreAI(NPC NPC)
		{
			Age++;

			return base.PreAI(NPC);
		}
	}
}
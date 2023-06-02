namespace StarlightRiver.Content.Buffs
{
	public class IvySnare : ModBuff
	{
		public override string Texture => AssetDirectory.Buffs + "IvySnare";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Snared");
			Description.SetDefault("You've been caught!");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = false;
		}

		public override void Update(NPC NPC, ref int buffIndex)
		{
			//NPC.GetGlobalNPC<DebuffHandler>().snared = true;
		}
	}
}
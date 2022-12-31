namespace StarlightRiver.Content.Buffs
{
	public abstract class SmartBuff : ModBuff
	{
		private readonly string ThisName;
		private readonly string ThisTooltip;
		private readonly bool Debuff;
		private readonly bool Summon;

		public bool Inflicted(Player Player)
		{
			return Player.active && Player.HasBuff(Type);
		}

		public bool Inflicted(NPC NPC)
		{
			if (ModContent.GetModBuff(Type) != null && NPC.buffImmune.Length > Type)
				return NPC.active && NPC.HasBuff(Type);

			return false;
		}

		public virtual void SafeSetDetafults() { }
		protected SmartBuff(string name, string tooltip, bool debuff, bool summon = false)
		{
			ThisName = name;
			ThisTooltip = tooltip;
			Debuff = debuff;
			Summon = summon;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(ThisName);
			Description.SetDefault(ThisTooltip);
			Main.debuff[Type] = Debuff;
			if (Summon)
			{
				Main.buffNoSave[Type] = true;
				Main.buffNoTimeDisplay[Type] = true;
			}
		}
	}
}

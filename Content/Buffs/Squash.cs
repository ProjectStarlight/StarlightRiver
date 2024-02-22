namespace StarlightRiver.Content.Buffs
{
	class Squash : ModBuff
	{
		public override string Texture => AssetDirectory.Buffs + "Squash";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pancaked");
			Description.SetDefault("You're flat now.");
			Main.debuff[Type] = true;
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			Player.velocity.X *= 0.9f;
		}
	}
}
namespace StarlightRiver.Content.Buffs
{
	public class FerrofluidDraftBuff : ModBuff
	{
		public override string Texture => AssetDirectory.Buffs + "FerrofluidDraftBuff";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ferrofluid Draft");
			Description.SetDefault("Nearby Items gravitate towards you");
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			for (int k = 0; k < Main.item.Length; k++)
			{
				if (Vector2.Distance(Main.item[k].Center, Player.Center) <= 800)
				{
					Main.item[k].velocity += Vector2.Normalize(Player.Center - Main.item[k].Center) * 3f;
					Main.item[k].velocity = Vector2.Normalize(Main.item[k].velocity) * 12;
				}
			}
		}
	}
}
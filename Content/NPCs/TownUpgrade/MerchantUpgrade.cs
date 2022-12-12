namespace StarlightRiver.Content.NPCs.TownUpgrade
{
	class MerchantUpgrade : TownUpgrade
	{
		public MerchantUpgrade() : base("Merchant", "[PH]Merchant Quest", "No Description", "[PH]Upgraded Merchant Action", "[PH]Upgraded merchant") { }

		public override void ClickButton()
		{
			Main.NewText("No message", Color.Brown);
		}
	}
}

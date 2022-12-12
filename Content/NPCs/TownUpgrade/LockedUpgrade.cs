namespace StarlightRiver.Content.NPCs.TownUpgrade
{
	class LockedUpgrade : TownUpgrade
	{
		public LockedUpgrade() : base("", "", "", "Locked", "") { }

		public override void ClickButton()
		{
			Main.NewText("Seek the voidsmith in hell for more information...", Color.Red);
		}
	}
}

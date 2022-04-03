using Terraria;

namespace StarlightRiver.Content.Buffs
{
	public class MossRegen : SmartBuff
    {
        public MossRegen() : base("Mending Moss", "Regenerating life quickly!", false) { }

        public override string Texture => AssetDirectory.Buffs + "MossRegen";

        public override void Update(Player Player, ref int buffIndex)
        {
            Player.lifeRegen += 10;
        }
    }
}
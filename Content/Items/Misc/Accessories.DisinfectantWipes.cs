using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;

namespace StarlightRiver.Content.Items.Misc
{
	public class DisinfectantWipes : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public DisinfectantWipes() : base("Disinfectant Wipes", "Critical strikes have a 10% chance to reduce all debuff durations by 3 seconds\nDoes not affect potion sickness debuffs") { }

        public override void Load()
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPC;
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProj;
        }

        private void OnHit(Player Player, bool crit)
        {
            if (Equipped(Player) && crit && Main.rand.NextFloat() < 0.1f)
            {
                ReduceDebuffDurations(Player);
            }
        }

        private void OnHitNPC(Player Player, Item Item, NPC target, int damage, float knockback, bool crit) 
            => OnHit(Player, crit);

        private void OnHitNPCWithProj(Player Player, Projectile proj, NPC target, int damage, float knockback, bool crit)
            => OnHit(Player, crit);

        public static void ReduceDebuffDurations(Player Player)
        {
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (Helper.IsValidDebuff(Player, i))
                {
                    Player.buffTime[i] -= 180;
                }
            }
        }
    }
}
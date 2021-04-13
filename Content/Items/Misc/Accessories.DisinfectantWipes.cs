using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Misc
{
    public class DisinfectantWipes : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public DisinfectantWipes() : base("Disinfectant Wipes", "Critical strikes have a 10% chance reduce all debuff durations by 3 seconds\nThis will not affect mana or potion sickness") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPC;
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProj;

            return true;
        }

        private void OnHit(Player player, bool crit)
        {
            if (Equipped(player) && crit && Main.rand.NextFloat() < 0.1f)
            {
                ReduceDebuffDurations(player);
            }
        }

        private void OnHitNPC(Player player, Item item, NPC target, int damage, float knockback, bool crit) 
            => OnHit(player, crit);

        private void OnHitNPCWithProj(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit)
            => OnHit(player, crit);

        public static void ReduceDebuffDurations(Player player)
        {
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (Helper.IsValidDebuff(player, i))
                {
                    player.buffTime[i] -= 180;
                }
            }
        }
    }
}
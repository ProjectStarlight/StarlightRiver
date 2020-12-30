using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Dusts;

namespace StarlightRiver.Content.Items.Misc
{
    public class SanitizerSpray : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public SanitizerSpray() : base("Sanitizer Spray", "Critical strikes have a 25% chance to transfer 5 seconds of a random debuff to all enemies nearby\nDoesn't trigger if there are no vurnerable enemies nearby") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPCAccessory;
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
            return true;
        }
        private void OnHit(Player player, NPC target, int damage, float knockback, bool crit)
        {
            if (Equipped(player) && crit && Main.rand.Next(0, 100) < 25)
                DisinfectantWipes.CleanDebuff(player, 1);
        }
        private void OnHitNPCAccessory(Player player, Item item, NPC target, int damage, float knockback, bool crit) =>                 OnHit(player, target, damage, knockback, crit);
        private void OnHitNPCWithProjAccessory(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit) =>   OnHit(player, target, damage, knockback, crit);
        public static bool SanitizeEnemies(Player player, int bufftype, int time, float range)
        {
            bool triggered = false;
            for (int i = 0; i < Main.maxNPCs; i += 1)
            {
                NPC npc = Main.npc[i];
                if ((npc.Center - player.Center).Length() < range)
                {
                    if (npc.active && npc.buffImmune[bufftype] && !npc.dontTakeDamage)
                        continue;

                    npc.AddBuff(bufftype, time);
                    DisinfectantWipes.MakeDusts(new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height), 2, Color.Red, ModContent.DustType<BioLumen>(), 1.5f);
                    triggered = true;
                }

            }
            return triggered;
        }
    }
}
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Vitric
{
    class VitricKnockbackAccessory : SmartAccessory
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public VitricKnockbackAccessory() : base("Vitric Knockback Bauble", "Knocks enemies back upon taking heavy damage") { }

        public override void SafeSetDefaults()
        {
            item.rare = ItemRarityID.Blue;
        }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PreHurtEvent += PreHurtKnockback;
            return true;
        }

        private bool PreHurtKnockback(Player player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (Equipped(player) && damage > player.statLifeMax2 * 0.1f)
            {
                float magnitude = 1 + player.statLife / damage * 4;
                Array.ForEach(Main.npc, (npc) =>
                {
                    if (npc.active && Helper.IsTargetValid(npc) && !npc.friendly && Vector2.Distance(npc.Center, player.MountedCenter) < 250)
                    {
                        Vector2 vel = Vector2.Normalize(npc.Center - player.position) * magnitude * npc.knockBackResist;
                        if (npc.noGravity)
                            npc.velocity += vel;
                        else
                        {
                            npc.HitEffect(player.Center.X < npc.Center.X ? -1 : 1, 0);
                            player.ApplyDamageToNPC(npc, 0, magnitude * 0.8f * (1 + (1 - npc.knockBackResist)), player.Center.X < npc.Center.X ? 1 : -1, false);
                        }

                        for (int i = 0; i < 6; ++i)
                        {
                            Dust.NewDust(npc.position, 22, 22, ModContent.DustType<Dusts.GlassGravity>(), vel.RotatedByRandom(0.05f).X * 0.5f, vel.RotatedByRandom(0.05f).Y * 0.5f);
                            Dust.NewDust(npc.position, 22, 22, ModContent.DustType<Content.Dusts.Air>());
                        }
                    }
                });
            }
            return true;
        }
    }
}

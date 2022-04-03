using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.OvergrowBoss
{
	internal class OvergrowBossAnchor : ModNPC
    {
        public override string Texture => AssetDirectory.Invisible;

        public override bool CheckActive()
        {
            return false;
        }

        public override void SetDefaults()
        {
            NPC.width = 20;
            NPC.height = 20;
            NPC.lifeMax = 200;
            NPC.noGravity = true;
            NPC.knockBackResist = 0f;
            NPC.HitSound = Mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ChainHit");
        }

        public override void AI()
        {
            if (!StarlightWorld.HasFlag(WorldFlags.OvergrowBossOpen))
                NPC.dontTakeDamage = true;
            else NPC.dontTakeDamage = false;

            NPC boss = Main.npc.FirstOrDefault(n => n.active && n.type == NPCType<OvergrowBoss>());
            if (boss == null)
                return;

            if (NPC.immortal) NPC.ai[0]++;
            if (NPC.ai[0] >= 30) NPC.active = false;
            if (NPC.ai[0] > 0)
            {
                Vector2 pos = Vector2.Lerp(NPC.Center, boss.Center + Vector2.Normalize(NPC.Center - boss.Center) * 80, NPC.ai[0] / 30f);
                for (int k = 0; k < 5; k++)
                {
                    Dust.NewDustPerfect(pos, DustType<Dusts.Stamina>(), Vector2.One.RotatedByRandom(6.28f));
                    if (Main.rand.Next(2) == 0) Dust.NewDustPerfect(pos, DustType<Dusts.Stone>(), Vector2.One.RotatedByRandom(6.28f));
                }
                if (NPC.ai[0] % 3 == 0) Gore.NewGore(pos, new Vector2(0, 1), Mod.GetGoreSlot("Gores/ChainGore"));
                if (NPC.ai[0] % 8 == 0) Terraria.Audio.SoundEngine.PlaySound(Mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/ChainHit").WithPitchVariance(0.4f), pos);
            }
            if (NPC.ai[0] == 1)
            {
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (!Main.npc.Any(n => n.active && n.type == NPCType<OvergrowBoss>())) return;
            NPC boss = Main.npc.FirstOrDefault(n => n.active && n.type == NPCType<OvergrowBoss>());

            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Bosses/OvergrowBoss/Chain").Value;
            for (float k = 0; k < 1; k += tex.Height / Vector2.Distance(NPC.Center, boss.Center + Vector2.Normalize(NPC.Center - boss.Center) * 80))
                if (k > NPC.ai[0] / 30f)
                {
                    Vector2 pos = Vector2.Lerp(NPC.Center, boss.Center + Vector2.Normalize(NPC.Center - boss.Center) * 80, k);
                    spriteBatch.Draw(tex, pos - Main.screenPosition, tex.Frame(), Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16), (NPC.Center - boss.Center).ToRotation() + 1.58f, tex.Frame().Size() / 2, 1, 0, 0);
                }
        }

        public override bool CheckDead()
        {
            NPC.HitSound = null;
            NPC.life = 1;
            NPC.immortal = true;
            return false;
        }
    }
}
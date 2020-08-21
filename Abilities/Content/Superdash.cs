using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dusts;
using System;
using System.Runtime.Serialization;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities.Content
{
    public class Superdash : Ability
    {
        //private Vector2 to;

        public override string Texture => "StarlightRiver/Pickups/Cloak1";
        public override Color Color => new Color(199, 143, 255);
        //public override bool Available => base.Available;

        //public override int CooldownMax => 120;
        //public override int TimeActiveMax => 4;

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            return false;
        }

        //public override void OnActivate()
        //{
        //    Main.PlaySound(SoundID.Item8);

        //    for (int k = 0; k <= 10; k++)
        //    {
        //        Dust.NewDust(Player.Center - new Vector2(100, 100), 200, 200, DustType<Void2>(), 0, 0, 0, default, 1.2f);
        //    }

        //    if (Main.myPlayer == Player.whoAmI)
        //    {
        //        to = Main.MouseWorld - Player.Center;
        //    }
        //}

        //public override void UpdateActive()
        //{
        //    base.UpdateActive();

        //    Player.maxFallSpeed = 999;
        //    Player.velocity = to / TimeActiveMax;
        //    Player.immune = true;
        //    Player.immuneTime = 5;

        //    Main.PlaySound(SoundID.Item24);

        //    timer += ((float)Math.PI * 2 / 12);
        //    if (timer >= (float)Math.PI * 2)
        //    {
        //        timer = 0;
        //    }
        //    if (timer <= 2)
        //    {
        //        timer = 2;
        //    }

        //    float rot = Vector2.Normalize(Player.velocity).ToRotation();
        //    float x = Player.Center.X + (float)Math.Sin(rot) * ((float)Math.Sin(timer) * 20);
        //    float y = Player.Center.Y + (float)Math.Cos(rot) * ((float)Math.Sin(timer) * -20);

        //    Dust.NewDustPerfect(new Vector2(x, y), DustType<Dusts.Void>());

        //    for (int k = 0; k <= 10; k++)
        //    {
        //        Dust.NewDust(new Vector2(x, y), 10, 10, DustType<Dusts.Void>(), Main.rand.Next(-20, 20), Main.rand.Next(-20, 20), 0, default, 0.5f);
        //    }

        //    for (int k = 0; k <= 10; k++)
        //    {
        //        Dust.NewDust(Player.Center - new Vector2(Player.height / 2, Player.height / 2), Player.height, Player.height, DustType<Dusts.Void>(), Main.rand.Next(-50, 50), Main.rand.Next(-50, 50), 0, default, 0.4f);
        //    }
        //}

        //public override void OffCooldownEffects()
        //{
        //    for (float k = 0; k <= 6.28f; k += 0.05f)
        //    {
        //        Dust dus = Dust.NewDustPerfect(Player.Center, DustType<Void4>(), Vector2.One.RotatedBy(k));
        //        dus.customData = Player;
        //    }
        //    Main.PlaySound(SoundID.Item105);
        //}

        //public override void OnExit()
        //{
        //    Player.velocity.X = 0;
        //    Player.velocity.Y = 0;
        //    Player.immune = false;
        //    objective = new Vector2(0, 0);
        //    timer = 0;

        //    Cooldown = 120; //Cooldown placed after since duration is variable

        //    for (int k = 0; k <= 100; k++)
        //    {
        //        Dust.NewDust(Player.Center - new Vector2(Player.height / 2, Player.height / 2), Player.height, Player.height, DustType<Dusts.Void>(), Main.rand.Next(-70, 70), Main.rand.Next(-70, 70), 0, default, 1.2f);
        //    }

        //    Main.PlaySound(SoundID.Item38);
        //}
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Items.CursedAccessories
{
    public abstract class BlessedAccessory : SmartAccessory
    {
        private readonly Texture2D Glow = null;
        private Vector2 drawpos = Vector2.Zero;

        public BlessedAccessory(Texture2D glow) : base("Unnamed Blessed Accessory", "You forgot to set a display name/tooltip dingus!")
        {
            Glow = glow;
        }

        //internal static readonly List<BootlegDust> Bootlegdust = new List<BootlegDust>();
        //internal static readonly List<BootlegDust> Bootlegdust2 = new List<BootlegDust>();

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Color color = Color.White * (float)Math.Sin(StarlightWorld.rottime + 3.14);
            spriteBatch.Draw(Glow, position, new Rectangle(0, 0, 32, 32), color, 0, origin, scale, SpriteEffects.None, 0);
            /*
            Bootlegdust.ForEach(BootlegDust => BootlegDust.Draw(spriteBatch));

            if (Main.rand.Next(4) == 0)
            {
                BootlegDust dus = new BlessDust(ModContent.GetTexture("StarlightRiver/GUI/Assets/Holy"), position + new Vector2(Main.rand.Next(0, frame.Width - 4), Main.rand.Next(0, frame.Height - 4)), new Vector2(0, 0.1f), Color.White * 0.1f, 1.5f, 120);
                Bootlegdust.Add(dus);
            }

            BootlegDust dus2 = new BlessDust2(ModContent.GetTexture("StarlightRiver/GUI/Assets/Holy"), position + new Vector2(-0.4f, -0.4f) + Vector2.One * frame.Width / 2 * scale + Vector2.One.RotatedBy(LegendWorld.rottime) * 13, Vector2.Zero, Color.White * 1f, 0.8f, 60);
            //Bootlegdust.Add(dus2);

            BootlegDust dus3 = new BlessDust2(ModContent.GetTexture("StarlightRiver/GUI/Assets/Holy"), position + new Vector2(-0.4f, -0.4f) + Vector2.One * frame.Width / 2 * scale + new Vector2((float)Math.Cos(LegendWorld.rottime) / 2, (float)Math.Sin(LegendWorld.rottime)) * 16, Vector2.Zero, Color.White * 1f, 0.8f, 60);
            if (LegendWorld.rottime > 3.14 / 2 && LegendWorld.rottime < 3.14 * 3 / 2) { Bootlegdust.Add(dus3); }
            else { Bootlegdust2.Add(dus3); }

            BootlegDust dus4 = new BlessDust2(ModContent.GetTexture("StarlightRiver/GUI/Assets/Holy"), position + new Vector2(-0.4f, -0.4f) + Vector2.One * frame.Width / 2 * scale + new Vector2((float)Math.Cos(LegendWorld.rottime), (float)Math.Sin(LegendWorld.rottime) / 2) * 16, Vector2.Zero, Color.White * 1f, 0.8f, 60);
            if (LegendWorld.rottime > 3.14) { Bootlegdust.Add(dus4); }
            else { Bootlegdust2.Add(dus4); }
            */
            drawpos = position - new Vector2((frame.Width / 2), (frame.Width / 2));
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            //Bootlegdust2.ForEach(BootlegDust => BootlegDust.Draw(spriteBatch));
            return true;
        }

        public override bool CanEquipAccessory(Player player, int slot)
        {
            Main.PlaySound(SoundID.NPCDeath57);
            Main.PlaySound(SoundID.Item30);
            for (int k = 0; k <= 175; k++)
            {
                //BootlegDust dus = new BlessDust2(ModContent.GetTexture("StarlightRiver/GUI/Assets/Holy"), drawpos, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0, 0.5f), Color.White * 0.2f, 3.8f, 180);
                //Bootlegdust.Add(dus);
            }
            return true;
        }

        public virtual bool TestCondition()
        {
            return false;
        }
    }

    /*public class BlessDust : BootlegDust
    {
        public BlessDust(Texture2D texture, Vector2 position, Vector2 velocity, Color color, float scale, int time) :
            base(texture, position, velocity, color, scale, time)
        {
        }

        public override void Update()
        {
            if (time > 80 && col.R < 255)
            {
                col *= 1.1f;
            }
            if (time <= 80)
            {
                col *= 0.78f;
            }

            scl *= 0.97f;
            pos += vel;

            time--;
        }
    }

    public class BlessDust2 : BootlegDust
    {
        public BlessDust2(Texture2D texture, Vector2 position, Vector2 velocity, Color color, float scale, int time) :
            base(texture, position, velocity, color, scale, time)
        {
        }

        public override void Update()
        {
            col *= 0.92f;
            scl *= 0.98f;
            pos += vel;

            time--;
        }
    }
    */
}
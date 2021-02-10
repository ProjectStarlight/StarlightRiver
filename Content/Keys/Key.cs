using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.GUI;

namespace StarlightRiver.Keys
{
    public class Key
    {
        public string Name { get; set; }
        public string Texture { get; set; }
        public virtual bool ShowCondition => true;

        public Vector2 Position = new Vector2(0, 0);
        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public Key(string name, string texture)
        {
            Name = name;
            Texture = texture;
        }

        public virtual void PreDraw(SpriteBatch spriteBatch)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            PreDraw(spriteBatch);

            Texture2D tex = GetTexture(Texture);
            spriteBatch.Draw(tex, Position + new Vector2(0, (float)Math.Sin(StarlightWorld.rottime) * 5) - Main.screenPosition, tex.Frame(), Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16));

            if (Hitbox.Contains(Main.MouseWorld.ToPoint())) Utils.DrawBorderString(spriteBatch, Name, Main.MouseScreen + new Vector2(12, 20), Main.mouseTextColorReal);
        }

        public virtual void OnPickup()
        {
        }

        public virtual void PreUpdate()
        {
        }

        public void Update()
        {
            PreUpdate();

            if (Main.player.Any(p => p.Hitbox.Intersects(Hitbox)))
            {
                StarlightWorld.Keys.Remove(this);
                StarlightWorld.KeyInventory.Add(this);
                if (Main.player.FirstOrDefault(p => p.Hitbox.Intersects(Hitbox)) == Main.LocalPlayer) KeyInventory.keys.Add(new KeyIcon(this, true));
                else KeyInventory.keys.Add(new KeyIcon(this, false));
                OnPickup();

                Main.PlaySound(ModLoader.GetMod("StarlightRiver").GetLegacySoundSlot(SoundType.Custom, "Sounds/KeyGet"));
            }
        }

        public static bool Use<T>()
        {
            if (StarlightWorld.KeyInventory.Any(n => n is T))
            {
                Key key = StarlightWorld.KeyInventory.FirstOrDefault(n => n is T);
                StarlightWorld.KeyInventory.Remove(key);
                KeyIcon icon = KeyInventory.keys.FirstOrDefault(n => n.parent == key);
                KeyInventory.keys.Remove(icon);

                Main.PlaySound(ModLoader.GetMod("StarlightRiver").GetLegacySoundSlot(SoundType.Custom, "Sounds/KeyUse"));
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void Spawn<T>(Vector2 position)
        {
            Key key = (Key)Activator.CreateInstance(typeof(T));
            key.Position = position;
            StarlightWorld.Keys.Add(key);
        }
    }
}
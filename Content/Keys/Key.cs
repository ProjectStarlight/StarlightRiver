using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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

            Texture2D tex = Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Position + new Vector2(0, (float)Math.Sin(StarlightWorld.rottime) * 5) - Main.screenPosition, tex.Frame(), Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16));

            if (Hitbox.Contains(Main.MouseWorld.ToPoint())) Utils.DrawBorderString(spriteBatch, Name, Main.MouseScreen + new Vector2(12, 20), Main.MouseTextColorReal);
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
                KeySystem.Keys.Remove(this);
                KeySystem.KeyInventory.Add(this);
                if (Main.player.FirstOrDefault(p => p.Hitbox.Intersects(Hitbox)) == Main.LocalPlayer) KeyInventory.keys.Add(new KeyIcon(this, true));
                else KeyInventory.keys.Add(new KeyIcon(this, false));
                OnPickup();

                Terraria.Audio.SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/KeyGet"));
            }
        }

        public static bool Use<T>()
        {
            if (KeySystem.KeyInventory.Any(n => n is T))
            {
                Key key = KeySystem.KeyInventory.FirstOrDefault(n => n is T);
                KeySystem.KeyInventory.Remove(key);
                KeyIcon icon = KeyInventory.keys.FirstOrDefault(n => n.parent == key);
                KeyInventory.keys.Remove(icon);

                Terraria.Audio.SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/KeyUse"));
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
            KeySystem.Keys.Add(key);
        }
    }
}
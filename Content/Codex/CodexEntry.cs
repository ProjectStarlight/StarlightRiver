using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Codex
{
    public class CodexEntry : TagSerializable
    {
        public bool Locked = true;
        public bool New = false;
        public bool RequiresUpgradedBook = false;
        public Categories Category;

        public string Title;
        public string Body;
        public string Hint;
        public Texture2D Image;
        public Texture2D Icon;

        public int LinePos;

        public enum Categories
        {
            Abilities = 0,
            Biomes = 1,
            Relics = 2,
            Removed = 3,
            Misc = 4,
            None = 5
        }

        public virtual void Draw(Vector2 pos, SpriteBatch spriteBatch)
        {
            if (LinePos < 0) LinePos = 0;

            spriteBatch.Draw(Image, pos + new Vector2(-50 + (310 - Image.Width) / 2, 36), Color.White);
            spriteBatch.Draw(Icon, pos + new Vector2(-38, -5), Color.White);
            Utils.DrawBorderString(spriteBatch, Title, pos, Color.White, 1.2f);

            List<string> lines = Helper.WrapString(Body, 480, Main.fontDeathText, 0.7f).Split('\n').ToList();
            int maxLines = (342 - (50 + Image.Height)) / 18; //grabs the max amount of lines that could feasibly be displated
            int linePosEnd = LinePos + maxLines;
            int lastLine = lines.Count < maxLines ? lines.Count : linePosEnd;

            if (lines.Count < maxLines) LinePos = 0;
            else if (linePosEnd > lines.Count) LinePos = lines.Count - maxLines;

            for (int k = LinePos; k < lastLine; k++)
            {
                int yRel = (k - LinePos) * 16;
                if (k < lines.Count) Utils.DrawBorderString(spriteBatch, lines[k], pos + new Vector2(-30, 50 + Image.Height + yRel), Color.White, 0.7f);
            }

            if (lines.Count > maxLines)
            {
                spriteBatch.Draw(Main.magicPixel, new Rectangle((int)pos.X + 236, (int)pos.Y + 50 + Image.Height, 8, 300 - (50 + Image.Height)), new Rectangle(0, 0, 1, 1), new Color(30, 30, 70), 0, Vector2.Zero, 0, 0);

                Texture2D arrow = GetTexture("StarlightRiver/Assets/GUI/Arrow");
                float posY = LinePos / (float)(lines.Count - maxLines) * (300 - (50 + Image.Height));
                spriteBatch.Draw(arrow, pos + new Vector2(234, 50 + Image.Height + posY - arrow.Height / 2), arrow.Frame(), Color.White, 0, Vector2.Zero, 1, 0, 0);
            }
        }

        public TagCompound SerializeData()
        {
            return new TagCompound()
            {
                ["Name"] = GetType().FullName,
                ["Locked"] = Locked
            };
        }

        public static CodexEntry DeserializeData(TagCompound tag)
        {
            try
            {
                var t = Type.GetType(tag.GetString("Name"));
                if (t is null) return null;

                CodexEntry entry = (CodexEntry)Activator.CreateInstance(t);
                entry.Locked = tag.GetBool("Locked");
                return entry;
            }
            catch { return null; }
        }
    }
}
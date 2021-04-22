using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    static class Markdown
    {
        public static float GetHeight(string message, float scale, int wrapWidth)
        {
            List<string> messages;
            List<string> mods;

            SplitMessage(WrapMarkdownText(message, wrapWidth), out messages, out mods);
            return (1 + messages.Count(n => n == "\n")) * Main.fontMouseText.MeasureString("A").Y * scale;
        }

        public static float GetWidth(string message, float scale)
        {
            return Main.fontMouseText.MeasureString(NeuterString(message)).X * scale;
        }

        public static void DrawMessage(SpriteBatch sb, Vector2 pos, string message, float scale, int wrapWidth = 0)
        {
			SplitMessage(wrapWidth > 0 ? WrapMarkdownText(message, wrapWidth) : message, out List<string> messages, out List<string> mods);

			float xOff = 0;
            float yOff = 0;
            for (int k = 0; k < messages.Count; k++)
            {
                if (messages[k] == "\n") //special case for linebreak because im layzeee
                {
                    yOff += Main.fontMouseText.MeasureString("A").Y * scale;
                    xOff = 0;
                    continue;
                }

                DrawSubstring(sb, pos + new Vector2(xOff, yOff), scale, messages[k], mods[k]);

                var measure = Main.fontMouseText.MeasureString(messages[k]) * scale;
                xOff += measure.X;
            }
        }

        private static void DrawSubstring(SpriteBatch sb, Vector2 pos, float scale, string message, string mod)
        {
            ParseModifier(mod, out Color color, out Vector2 offset, out Vector3 modScale);
            //sb.DrawString(Main.fontItemStack, message, pos + offset, color, 0, Vector2.Zero, scale, 0, 0);
            Utils.DrawBorderString(sb, message, pos + offset, color, scale * modScale.X, modScale.Y, modScale.Z);
        }

        private static void SplitMessage(string message, out List<string> messages, out List<string> mods)
        {
            List<string> outputMessages = new List<string>();
            List<string> modifierMessages = new List<string>();

            string writeTo = "";

            for (int k = 0; k < message.Length; k++)
            {
                var c = message[k];

                if (k > 0 && c == '[')
                {
                    //writeTo = tempModifier;
                    outputMessages.Add(writeTo);
                    writeTo = "";
                    writeTo += c;
                }

                else if (c == ']')
                {
                    writeTo += c;
                    modifierMessages.Add(writeTo);
                    writeTo = "";
                }

                else writeTo += c;
            }
            outputMessages.Add(writeTo);

            messages = outputMessages;
            mods = modifierMessages;
        }

        private static void ParseModifier(string mod, out Color color, out Vector2 offset, out Vector3 scale)
        {
            color = Color.White;
            offset = Vector2.Zero;
            scale = Vector3.UnitX;

            mod = mod.Replace("[", "");
            mod = mod.Replace("]", "");

            if (mod == "") return;

            string[] mods = mod.Split('<');

            for (int k = 0; k < mods.Length; k++)
            {
                var subMod = mods[k].Replace(">", "");
                if (subMod == "") continue;
                var parts = subMod.Split(':');

                string modName = parts[0];
                string param = parts[1];

                if (modName == "color") color = ParseAsColor(param);
                if (modName == "offset") offset = ParseAsOffset(param);
                if (modName == "scale") scale = ParseAsScale(param);
            }
        }

        private static Color ParseAsColor(string param)
        {
            string[] vars = param.Split(',');
            return new Color(int.Parse(vars[0]), int.Parse(vars[1]), int.Parse(vars[2]));
        }

        private static Vector2 ParseAsOffset(string param)
        {
            string[] vars = param.Split(',');
            return new Vector2(float.Parse(vars[0]), float.Parse(vars[1]));
        }

        private static Vector3 ParseAsScale(string param)
        {
            string[] vars = param.Split(',');
            return new Vector3(float.Parse(vars[0]), float.Parse(vars[1]), float.Parse(vars[1]));
        }

        public static string MakeVibratingText(string message, Color color)
        {
            string output = "";
            for (int k = 0; k < message.Length; k++)
            {
                output += SetCharMarkdown(message[k], color, new Vector2(Main.rand.Next(-2, 2), Main.rand.Next(-2, 2)), new Vector3(Main.rand.NextFloat(0.8f, 1), 0, 0));
            }
            return output;
        }

        public static string MakeSuaveText(string message)
        {
            string output = "";
            for (int k = 0; k < message.Length; k++)
            {
                float sin = 0.8f + (float)Math.Sin(StarlightWorld.rottime + k) * 0.5f;
                float sin2 = 0.8f + (float)Math.Sin(StarlightWorld.rottime + k + (float)Math.PI / 1.5f) * 0.5f;
                float sin3 = 0.8f + (float)Math.Sin(StarlightWorld.rottime + k + (float)Math.PI / 1.5f * 2) * 0.5f;
                Color color = new Color(sin, sin2, sin3);
                Vector2 off = new Vector2(0, 12 + (float)(Math.Sin(StarlightWorld.rottime * 2 + k / 2f) * 4));

                float sin4 = 0.8f + (float)Math.Sin(StarlightWorld.rottime + k * 0.25f) * 0.2f;
                Vector3 scale = new Vector3(sin4, 0.5f, 0.5f);

                output += SetCharMarkdown(message[k], color, off, scale);
            }
            return output;
        }

        public static string SetCharMarkdown(char character, Color color, Vector2 offset, Vector3 scale)
        {
            return
                    "[<offset:" + offset.X + "," + offset.Y + ">" +
                    "<color:" + color.R + "," + color.G + "," + color.B + ">" +
                    "<scale:" + scale.X + "," + scale.Y + "," + scale.Z + ">]" +
                    character;
        }

        public static string NeuterString(string text)
        {
            string output = "";
            string[] parts = text.Replace("]", "[").Split('[');

            for (int k = 0; k < parts.Length; k += 2)
            {
                output += parts[k];
            }

            return output;
        }

        public static string WrapMarkdownText(string text, int width) //this is kinda cancer, sorry folks
        {
            string output = "";
            string lastMod = "";
            string[] parts = text.Replace("]", "[").Split('[');
            float totalWidth = 0;

            float singleCharWidthCache = 0;
            int singleCharIndexCache = 0;
            bool singleCharBroken = false;

            for (int k = 1; k < parts.Length; k++)
            {
                var segment = parts[k];

                if (k % 2 == 1) //modifier
                {
                    output += '[' + segment + ']';
                    lastMod = '[' + segment + ']';
                }
                else //text
                {
                    string[] words = segment.Split(' ');

                    if (words.Length == 1 && words[0].Length == 1) //special case for single characters
                    {
                        if (singleCharIndexCache == 0)
                        {
                            singleCharIndexCache = output.Length;
                            singleCharWidthCache = 0;
                        }

                        float w = Main.fontMouseText.MeasureString(words[0]).X;
                        singleCharWidthCache += w;

                        if (totalWidth + singleCharWidthCache + w > width && !singleCharBroken)
                        {
                            output = output.Insert(singleCharIndexCache, "[]\n" + lastMod);
                            singleCharBroken = true;
                        }

                        output += words[0];
                        continue;
                    }
                    else if (singleCharIndexCache != 0) //done reading single characters! reset for next time
                    {
                        if (singleCharBroken)
                            totalWidth = singleCharWidthCache;
                        else
                            totalWidth += singleCharWidthCache;

                        singleCharIndexCache = 0;
                        singleCharWidthCache = 0;
                        singleCharBroken = false;
                    }

                    for (int n = 0; n < words.Length; n++)
                    {
                        float w = Main.fontMouseText.MeasureString(words[n] + ' ').X; //duplicate the markdown signature if we have to newline, and add the newline as it's own seperate blank markdown so the draw method can identify it
                        if (totalWidth + w > width)
                        {
                            totalWidth = w;
                            output += "[]\n" + lastMod + words[n] + ' ';
                        }
                        else
                        {
                            output += words[n] + ' ';
                            totalWidth += w;
                        }
                    }
                }
            }

            return output;
        }
    }
}

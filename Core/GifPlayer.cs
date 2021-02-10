using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace StarlightRiver.Core
{
    public class GifPlayer
    {
        public List<Texture2D> textureData = new List<Texture2D>();
        public int frame;
        public int framerate;

        private int timer;

        public Texture2D CurrentFrame => textureData.Count > 0 ? textureData[frame] : Terraria.ModLoader.ModContent.GetTexture(AssetDirectory.Debug);

        public void Update()
        {
            timer++;
            if(timer >= framerate)
            {
                timer = 0;
                frame++;
                if (frame >= textureData.Count)
                    frame = 0;
            }
        }

        public void LoadFromPath(string path)
        {
            var stream = File.OpenRead(path);

            //Header

            for (int k = 0; k < 6; k++)
            {
                byte first = (byte)stream.ReadByte();
                Console.WriteLine(Convert.ToChar(first));
            }

            //Screen data

            int screenWidth = BitConverter.ToInt16(ReadBytes(2, stream), 0);
            Console.WriteLine(screenWidth);

            int screenHeight = BitConverter.ToInt16(ReadBytes(2, stream), 0);
            Console.WriteLine(screenHeight);

            byte formatData = (byte)stream.ReadByte();
            bool colorMapPresent = (formatData & 1) == 0;
            int colorResolutionBits = 1 + ((formatData >> 1));
            int pixelBits = 1 + (formatData & 0b111);


            int backgroundColorIndex = stream.ReadByte();

            _ = stream.ReadByte();

            //Color map
            Color[] colorMap = new Color[0];

            if (colorMapPresent)
            {
                int entries = (int)Math.Pow(2, pixelBits);
                colorMap = new Color[entries];

                for (int k = 0; k < entries; k++)
                {
                    colorMap[k] = new Color(stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
                }
            }

            Color[] nextTextureData = new Color[screenWidth * screenHeight];
            string output = "";

            for (; ; )
            {
                var nextByte = stream.ReadByte();
                output += nextByte.ToString("x") + ", ";

                if (nextByte == ';')
                {
                    Main.NewText(output);
                    break; //GIF terminator. stop reading.
                }

                if (nextByte == ',')
                {
                    var image = ParseImageDescriptor(stream);

                    if (!image.useGlobalMap) //Image has a local map which should be used for colors
                    {
                        int entries = (int)Math.Pow(2, image.bitsPerPixel);
                        Color[] localMap = new Color[entries];

                        for (int k = 0; k < entries; k++)
                        {
                            localMap[k] = new Color(stream.ReadByte(), stream.ReadByte(), stream.ReadByte());
                        }
                        image.localMap = localMap;
                    }
                    else image.localMap = colorMap; //colorMap should be defined if any images dont use local maps.

                    //if (image.left == 0 && image.top == 0) //next image is top-left again, time to save a texture for the frame and reset!
                    //{
                        var tex = new Texture2D(Main.graphics.GraphicsDevice, screenWidth, screenHeight);
                        tex.SetData(nextTextureData);

                        textureData.Add(tex);

                        nextTextureData = new Color[screenWidth * screenHeight];
                    //}

                    if (!image.orderFormat) //sequential format
                    {
                        int codeSize = stream.ReadByte();
                        List<byte> rasterOutput = new List<byte>();

                        for (; ; )
                        {
                            int byteCount = stream.ReadByte();

                            if (byteCount == 0) 
                                break;

                            BitArray bits = new BitArray(byteCount * 8);

                            for (int k = 0; k < byteCount; k++)
                            {
                                var nextSet = new BitArray(new byte[] { (byte)stream.ReadByte() } );

                                for(int i = 0; i < 8; i++)
                                {
                                    int index = k * 8 + i;
                                    bits[index] = nextSet[i];
                                }
                            }

                            List<byte[]> LZWTable = new List<byte[]>();

                            for (int k = 0; k < 256; k++)
                            {
                                LZWTable.Add(new byte[] { (byte)k });
                            }

                            byte[] w = new byte[0];

                            for (int k = 0; k < bits.Length / codeSize; k++)
                            {
                                var keyBits = new BitArray(codeSize);

                                for (int i = 0; i < codeSize; i++)
                                {
                                    keyBits[i] = bits[k * codeSize + i];
                                }

                                var a = new int[1];
                                keyBits.CopyTo(a, 0);
                                int key = a[0];

                                byte[] entry;

                                if(key == Math.Pow(2, codeSize))
                                {
                                    LZWTable = new List<byte[]>();

                                    for (int i = 0; i < 256; i++)
                                    {
                                        LZWTable.Add(new byte[] { (byte)k });
                                    }

                                    continue;
                                }

                                if (LZWTable.Count >= key)
                                    entry = LZWTable[key];
                                else
                                {
                                    var synthesized = new byte[w.Length + 1];
                                    w.CopyTo(synthesized, 0);
                                    synthesized[w.Length] = w[0];
                                    entry = synthesized;
                                }

                                rasterOutput.AddRange(entry);

                                if (w.Length > 0)
                                {
                                    var newEntry = new byte[w.Length + 1];
                                    w.CopyTo(newEntry, 0);
                                    newEntry[w.Length] = entry[0];
                                    LZWTable.Add(newEntry);
                                }

                                w = entry;
                            }
                        }

                        for (int k = 0; k < rasterOutput.Count; k++)
                        {
                            nextTextureData[k] = image.localMap[rasterOutput[k]];
                        }
                    }
                    /*else //Interlaced format
                    {
                        var entries = image.width * image.height;
                        Color[] deInterlaced = new Color[image.width * image.height];

                        for (int k = 0; k < entries / 8; k++)
                        {
                            var targetIndex = k * 8;
                            deInterlaced[targetIndex] = image.localMap[stream.ReadByte()];
                        }

                        for (int k = 0; k < entries / 8; k++)
                        {
                            var targetIndex = 4 + k * 8;
                            deInterlaced[targetIndex] = image.localMap[stream.ReadByte()];
                        }

                        for (int k = 1; k < entries / 4; k++)
                        {
                            var targetIndex = k * 4 - 2;
                            deInterlaced[targetIndex] = image.localMap[stream.ReadByte()];
                        }

                        for (int k = 0; k < entries / 2; k++)
                        {
                            var targetIndex = 1 + k * 2;
                            deInterlaced[targetIndex] = image.localMap[stream.ReadByte()];
                        }

                        int index = 0;
                        for (int x = 0; x < image.width; x++)
                            for (int y = 0; y < image.height; y++)
                            {
                                index++;
                                nextTextureData[image.left + x + (screenWidth * (image.top + y))] = deInterlaced[index];
                            }
                    }*/
                }
            }
        }

        private static Image ParseImageDescriptor(FileStream stream)
        {
            int left = BitConverter.ToUInt16(ReadBytes(2, stream), 0);
            int top = BitConverter.ToUInt16(ReadBytes(2, stream), 0);
            int width = BitConverter.ToUInt16(ReadBytes(2, stream), 0);
            int height = BitConverter.ToUInt16(ReadBytes(2, stream), 0);

            byte data = (byte)stream.ReadByte();
            bool useGlobalMap = ((data >> 0) & 1) == 0;
            bool orderFormat = ((data >> 1) & 1) == 0;
            int bitsPerPixel = 1 + (data & 0b111);

            return new Image(left, top, width, height, useGlobalMap, orderFormat, bitsPerPixel);
        }

        public static byte[] ReadBytes(int amount, FileStream stream)
        {
            var output = new byte[amount];
            for (int k = 0; k < amount; k++)
            {
                output[k] = (byte)stream.ReadByte();
            }

            return output;
        }
    }

    public class Image
    {
        public int left;
        public int top;
        public int width;
        public int height;
        public bool useGlobalMap;
        public bool orderFormat; //0 = sequential, 1 = interlaced
        public int bitsPerPixel;

        public Color[] localMap;

        public Image(int left, int top, int width, int height, bool useGlobalMap, bool orderFormat, int bitsPerPixel)
        {
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
            this.useGlobalMap = useGlobalMap;
            this.orderFormat = orderFormat;
            this.bitsPerPixel = bitsPerPixel;
        }
    }
}

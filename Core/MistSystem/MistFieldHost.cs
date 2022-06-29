using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.MistSystem
{
    public class MistFieldHost
    {
        public HashSet<MistField> MistFields = new HashSet<MistField>();
        public void GenerateMistField(int SimSize, int CellSize, Vector2 GlobalSpace) => MistFields.Add(new MistField(SimSize, CellSize, GlobalSpace - new Vector2(SimSize / 2) * CellSize));

        public void Update()
        {
            foreach (MistField mf in MistFields.ToList())
            {
                mf.Update();
                if (mf.TimeAlive > mf.Lifetime)
                {
                    MistFields.Remove(mf);
                    mf.Dispose();
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (MistField mf in MistFields.ToList())
            {
                mf.Draw(sb);
            }
        }
    }
}



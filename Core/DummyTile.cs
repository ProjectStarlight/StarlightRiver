using Microsoft.Xna.Framework;
using StarlightRiver.Packets;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public abstract class DummyTile : ModTile
    {
        public static Dictionary<Point16, Projectile> dummies = new Dictionary<Point16, Projectile>();

        public virtual int DummyType { get; }

        public Projectile Dummy(int i, int j) => GetDummy(i, j, DummyType);

        public static Projectile GetDummy(int i, int j, int type)
		{
            Point16 key = new Point16(i, j);

            if (dummies.TryGetValue(key, out Projectile dummy))
			{
                if (dummy.type == type)
                    return dummy;
			}

            return null;
		}

        public static bool DummyExists(int i, int j, int type)
		{
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                var proj = Main.projectile[k];
                if (proj.active && proj.type == type && (proj.position / 16).ToPoint16() == new Terraria.DataStructures.Point16(i, j))
                    return true;
            }

            return false;
        }

        public virtual void PostSpawnDummy(Projectile dummy) { }

        public virtual void SafeNearbyEffects(int i, int j, bool closer) { }

        public virtual bool SpawnConditions(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.frameX == 0 && tile.frameY == 0;
        }

        public sealed override void NearbyEffects(int i, int j, bool closer)
        {
            if (!Main.tileFrameImportant[Type] || SpawnConditions(i, j))
            {
                int type = DummyType;//cache type here so you dont grab the it from a dict every single iteration
                var dummy = Dummy(i, j);

                if (dummy is null || !dummy.active)
                {
                    if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
                    {
                        SpawnDummy packet = new SpawnDummy(Main.myPlayer, type, i, j);
                        packet.Send(-1, -1, false);
                        return;
                    }

                    Projectile p = new Projectile();
                    p.SetDefaults(type);

                    var spawnPos = new Vector2(i, j) * 16 + p.Size / 2;
                    int n = Projectile.NewProjectile(spawnPos, Vector2.Zero, type, 1, 0);

                    Point16 key = new Point16(i, j);
                    dummies[key] = Main.projectile[n];

                    PostSpawnDummy(Main.projectile[n]);
                }
            }

            SafeNearbyEffects(i, j, closer);
        }
    }
}

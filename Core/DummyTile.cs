using Microsoft.Xna.Framework;
using StarlightRiver.Packets;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	internal abstract class DummyTile : ModTile
    {
        public virtual int DummyType { get; }

        public Projectile Dummy(int i, int j) //TODO: Change this to some sort of dict accessed by 2 coordinate key instead of this horrid iteration
		{
            for(int k = 0; k < Main.maxProjectiles; k++)
			{
                var proj = Main.projectile[k];
                if (proj.active && proj.type == DummyType && (proj.position / 16).ToPoint16() == new Terraria.DataStructures.Point16(i, j))
                    return proj;
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

                if (dummy is null)
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

                    PostSpawnDummy(Main.projectile[n]);
                }
            }

            SafeNearbyEffects(i, j, closer);
        }
    }
}

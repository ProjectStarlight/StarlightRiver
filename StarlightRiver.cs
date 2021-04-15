using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Foregrounds;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using StarlightRiver.RiftCrafting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.UI;

namespace StarlightRiver
{
    public partial class StarlightRiver : Mod
    {
        public AbilityHotkeys AbilityKeys { get; private set; }

        private List<ILoadable> loadCache;

        public static float Rotation;

        public static LightingBuffer LightingBufferInstance = null;

        public static StarlightRiver Instance { get; set; }

        public StarlightRiver() { Instance = this; }

        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active)
            {
                Player player = Main.LocalPlayer;

                if (player.GetModPlayer<BiomeHandler>().ZoneGlass)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/GlassPassive");
                    priority = MusicPriority.BiomeHigh;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneGlassTemple)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/GlassTemple");
                    priority = MusicPriority.BiomeHigh;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneVoidPre)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/VoidPre");
                    priority = MusicPriority.BossLow;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneJungleCorrupt)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleCorrupt");
                    priority = MusicPriority.BiomeMedium;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneJungleBloody)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleBloody");
                    priority = MusicPriority.BiomeMedium;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneJungleHoly)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleHoly");
                    priority = MusicPriority.BiomeMedium;
                }

                if (player.GetModPlayer<BiomeHandler>().zoneAluminum)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/AluminumPassive");
                    priority = MusicPriority.BiomeHigh;
                }

                if (player.GetModPlayer<BiomeHandler>().zonePermafrost)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/PermafrostPassive");
                    priority = MusicPriority.BiomeMedium;
                }

                if (Main.tile[(int)player.Center.X / 16, (int)player.Center.Y / 16].wall == ModContent.WallType<AuroraBrickWall>() && !StarlightWorld.HasFlag(WorldFlags.SquidBossDowned))
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/SquidArena");
                    priority = MusicPriority.BiomeHigh;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneOvergrow)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/Overgrow");
                    priority = MusicPriority.BiomeHigh;
                }
            }
            return;
        }

        public override void Load()
        {
            loadCache = new List<ILoadable>();

            foreach (Type type in Code.GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(ILoadable)))
                {
                    var instance = Activator.CreateInstance(type);
                    loadCache.Add(instance as ILoadable);
                }

                loadCache.Sort((n, t) => n.Priority > t.Priority ? 1 : -1);
            }

            for (int k = 0; k < loadCache.Count; k++)
            {
                loadCache[k].Load();
            }

            if (!Main.dedServ)
            {
                LightingBufferInstance = new LightingBuffer();

                //Hotkeys
                AbilityKeys = new AbilityHotkeys(this);
                AbilityKeys.LoadDefaults();
            }
        }

        //private readonly FieldInfo _transformMatrix = typeof(SpriteViewMatrix).GetField("_transformationMatrix", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            if (false) //ignore this block
            {
                Matrix rotation = Matrix.CreateRotationZ(Rotation);
                Matrix translation = Matrix.CreateTranslation(new Vector3(Main.screenWidth / 2, Main.screenHeight / 2, 0));
                Matrix translation2 = Matrix.CreateTranslation(new Vector3(Main.screenWidth / -2, Main.screenHeight / -2, 0));

                //_transformMatrix.SetValue(Transform, ((translation2 * rotation) * translation));
                //base.ModifyTransformMatrix(ref Transform);
                //Helper.UpdateTilt();
            }

            Transform.Zoom = ZoomHandler.ScaleVector;
            ZoomHandler.UpdateZoom();
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < UILoader.UIStates.Count; k++)
            {
                var state = UILoader.UIStates[k];
                UILoader.AddLayer(layers, UILoader.UserInterfaces[k], state, state.InsertionIndex(layers), state.Visible, state.Scale);
            }
        }

        public override void Unload()
        {
            foreach (var loadable in loadCache)
            {
                loadable.Unload();
            }
            loadCache = null;

            if (!Main.dedServ)
            {
                Instance = null;
                AbilityKeys.Unload();
            }
        }

        #region NetEasy
        public override void PostSetupContent()
        {
            NetEasy.NetEasy.Register(this);

            AutoloadChestItems();

            //CallBossChecklist();

            foreach(var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if(!type.IsAbstract && type.GetInterfaces().Contains(typeof(IPostLoadable)))
                {
                    var toLoad = Activator.CreateInstance(type);

                    ((IPostLoadable)toLoad).PostLoad();
                }
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NetEasy.NetEasy.HandleModule(reader, whoAmI);
        }
        #endregion
    }
}

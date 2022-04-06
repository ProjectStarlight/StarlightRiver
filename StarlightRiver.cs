using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using Terraria.UI;

namespace StarlightRiver
{
	public partial class StarlightRiver : Mod
    {
        public AbilityHotkeys AbilityKeys { get; private set; }

        private List<IOrderedLoadable> loadCache;

        private List<IRecipeGroup> recipeGroupCache;

        public static float Rotation;

        public static LightingBuffer LightingBufferInstance = null;

        public bool HasLoaded;

        //debug hook to view RTs
        //public override void PostDrawInterface(SpriteBatch spriteBatch)
        //{
        //    spriteBatch.Draw(Content.CustomHooks.HotspringMapTarget.hotspringMapTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Purple * 0.5f);
        //    spriteBatch.Draw(Content.CustomHooks.HotspringMapTarget.hotspringShineTarget, new Rectangle(Main.screenWidth - (Main.screenWidth / 4), 0, Main.screenWidth / 4, Main.screenHeight / 4), Color.White * 0.5f);
        //}

        public static StarlightRiver Instance { get; set; }

        public StarlightRiver() => Instance = this;

        public bool useIntenseMusic = false; //TODO: Make some sort of music handler at some point for this

        private Vector2 _lastScreenSize; //Putting these in StarlightRiver incase anything else wants to use them (which is likely)

        private Vector2 _lastViewSize;

        private Viewport _lastViewPort;

        public override void UpdateMusic(ref int music, ref MusicPriority priority) //PORTTODO: Port this to modbiome and related
        {
            if (Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active)
            {
                Player Player = Main.LocalPlayer;

                if(useIntenseMusic)
				{
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/Miniboss");
                    priority = MusicPriority.BossLow;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZoneHotspring)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/HotspringAmbient");
                    priority = MusicPriority.BiomeHigh;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZoneGlass)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/GlassPassive");
                    priority = MusicPriority.BiomeHigh;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZoneGlassTemple)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/GlassTemple");
                    priority = MusicPriority.BiomeHigh;
                }

                if(StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && StarlightWorld.VitricBossArena.Contains((Player.Center / 16).ToPoint()))
				{
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/VitricBossAmbient");
                    priority = MusicPriority.BiomeHigh;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZoneVoidPre)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/VoidPre");
                    priority = MusicPriority.BossLow;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZoneJungleCorrupt)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleCorrupt");
                    priority = MusicPriority.BiomeMedium;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZoneJungleBloody)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleBloody");
                    priority = MusicPriority.BiomeMedium;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZoneJungleHoly)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleHoly");
                    priority = MusicPriority.BiomeMedium;
                }

                if (Player.GetModPlayer<BiomeHandler>().zoneAluminum)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/AluminumPassive");
                    priority = MusicPriority.BiomeHigh;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZonePermafrost)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/PermafrostPassive");
                    priority = MusicPriority.BiomeMedium;
                }

                if (Main.tile[(int)Player.Center.X / 16, (int)Player.Center.Y / 16].wall == ModContent.WallType<AuroraBrickWall>() && !StarlightWorld.HasFlag(WorldFlags.SquidBossDowned))
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/SquidArena");
                    priority = MusicPriority.BiomeHigh;
                }

                if (Player.GetModPlayer<BiomeHandler>().ZoneOvergrow)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/Overgrow");
                    priority = MusicPriority.BiomeHigh;
                }
            }

            useIntenseMusic = false;

            return;
        }

        public override void Load()
        {
            //CopyFile();

            loadCache = new List<IOrderedLoadable>();

            foreach (Type type in Code.GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IOrderedLoadable)))
                {
                    var instance = Activator.CreateInstance(type);
                    loadCache.Add(instance as IOrderedLoadable);
                }

                loadCache.Sort((n, t) => n.Priority > t.Priority ? 1 : -1);
            }

            for (int k = 0; k < loadCache.Count; k++)
            {
                loadCache[k].Load();
            }


            recipeGroupCache = new List<IRecipeGroup>();

            foreach (Type type in Code.GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IRecipeGroup)))
                {
                    var instance = Activator.CreateInstance(type);
                    recipeGroupCache.Add(instance as IRecipeGroup);
                }

                recipeGroupCache.Sort((n, t) => n.Priority > t.Priority ? 1 : -1);
            }


            if (!Main.dedServ)
            {
                _lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                _lastViewSize = Main.ViewSize;
                _lastViewPort = Main.graphics.GraphicsDevice.Viewport;

                LightingBufferInstance = new LightingBuffer();

                //Hotkeys
                AbilityKeys = new AbilityHotkeys(this);
                AbilityKeys.LoadDefaults();
            }

            Compat.BossChecklistCalls.CallBossChecklist();
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

        public override void AddRecipeGroups()
        {
            foreach (var group in recipeGroupCache)
            {
                group.AddRecipeGroups();
            }
        }

        public override void PostAddRecipes()
        {
            HasLoaded = true;
        }

        public void CheckScreenSize()
        {
            if (!Main.dedServ)
            {
                if (_lastScreenSize != new Vector2(Main.screenWidth, Main.screenHeight))
                {
                    if (TileDrawOverLoader.projTarget != null)
                        TileDrawOverLoader.ResizeTarget();
                    if (BreacherArmorHelper.NPCTarget != null)
                        BreacherArmorHelper.ResizeTarget();
                }
                _lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
                _lastViewSize = Main.ViewSize;
                _lastViewPort = Main.graphics.GraphicsDevice.Viewport;
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
                    object toLoad = Activator.CreateInstance(type);

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

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

        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active)
            {
                Player player = Main.LocalPlayer;

                if(useIntenseMusic)
				{
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/Miniboss");
                    priority = MusicPriority.BossLow;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneHotspring)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/HotspringAmbient");
                    priority = MusicPriority.BiomeHigh;
                }

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

                if(StarlightWorld.HasFlag(WorldFlags.VitricBossOpen) && StarlightWorld.VitricBossArena.Contains((player.Center / 16).ToPoint()))
				{
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/VitricBossAmbient");
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

                if (player.GetModPlayer<BiomeHandler>().ZonePermafrost)
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

		public override void PostUpdateEverything()
		{
            ZoomHandler.TickZoom();
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

            //SwapFile();
        }




        //TODO: (important) remove before puplic release
        //private const string tempName = "StarlightRiver.export.rename_to_tmod";
        //private void CopyFile()
        //{
        //    string path = Main.SavePath + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar;
        //    bool doNotCopy = false;
        //    char[] modSig;
        //    long writerStartPos = 0;
        //    string id = Steamworks.SteamUser.GetSteamID().ToString();

        //    using (FileStream stream = new FileStream(path + "StarlightRiver.tmod", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //    {
        //        BinaryReader binaryReader = new BinaryReader(stream);

        //        //advances the position forward to the mod signature
        //        stream.Position += 4; //binaryReader.ReadBytes(4);  //discarded //tmod
        //        binaryReader.ReadString();  //discarded //version
        //        stream.Position += 20; //binaryReader.ReadBytes(20); //discarded //hash

        //        writerStartPos = stream.Position;

        //        //the next 256 bytes are free to use*
        //        modSig = Encoding.ASCII.GetChars(binaryReader.ReadBytes(256));//*unless the mod is off the browser, but this bit of code should never be on a browser version

        //        if (modSig.ToString().Contains(id))
        //            doNotCopy = true;
        //        else
        //        {
        //            int nextIndex = -1;//to store start of next string

        //            for (int i = 0; i < modSig.Length; i++)//find next empty space
        //                if (modSig[i] == '\0')
        //                {
        //                    nextIndex = i;
        //                    break;
        //                }

        //            if (nextIndex == -1 || nextIndex > modSig.Length - (id.Length + 1))//if no empty space or not enough room
        //            {
        //                doNotCopy = true;
        //                return;
        //            }

        //            modSig[nextIndex] = '|';
        //            for (int i = 0; i < id.Length; i++)
        //                modSig[nextIndex + i + 1] = id[i];
        //        }
        //    }

        //    if (doNotCopy)//it does not copy if it cannot find a valid space, or the list is up to date (contains current id)
        //    {
        //        if (File.Exists(path + tempName))//makes sure a leftover copy does not exist if set to not copy
        //            File.Delete(path + tempName);//a leftover copy would overwrite the current tmod on unload
        //    }
        //    else
        //    {
        //        File.Copy(path + "StarlightRiver.tmod", path + tempName, true);

        //        using (FileStream stream = new FileStream(path + tempName, FileMode.Open))
        //        {
        //            BinaryWriter binaryWriter = new BinaryWriter(stream);

        //            stream.Position = writerStartPos;

        //            binaryWriter.Write(modSig);
        //        }
        //    }
        //}

        //private void SwapFile()
        //{
        //    string path = Main.SavePath + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar;
        //    if (File.Exists(path + tempName))
        //    {
        //        File.Copy(path + tempName, path + "StarlightRiver.tmod", true);
        //        File.Delete(path + tempName);
        //    }
        //}

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
                    if (BreacherArmorHelper.npcTarget != null)
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

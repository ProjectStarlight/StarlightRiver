using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace StarlightRiver.Content.CustomHooks
{
	class WorldSelectAddons : HookGroup
    {
        //Questionable becasue of the antiquated reflection caching this, may or may not make the GC want to cry.
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        readonly Dictionary<UIWorldListItem, TagCompound> worldDataCache = new Dictionary<UIWorldListItem, TagCompound>();

        private static FieldInfo iconInfo;

        public override void Load() //funny how the length of these lines line up perfectly. heh.
        {
            if (Main.dedServ)
                return;

            iconInfo = typeof(UIWorldListItem).GetField("_worldIcon", BindingFlags.NonPublic | BindingFlags.Instance);

            On.Terraria.GameContent.UI.Elements.UIWorldListItem.DrawSelf += VoidIcon;
            On.Terraria.GameContent.UI.Elements.UIWorldListItem.ctor += AddWorldData;
            On.Terraria.GameContent.UI.States.UIWorldSelect.ctor += RefreshWorldData;
        }

        private void VoidIcon(On.Terraria.GameContent.UI.Elements.UIWorldListItem.orig_DrawSelf orig, UIWorldListItem self, SpriteBatch spriteBatch)
        {
            orig(self, spriteBatch);
			Vector2 pos = self.GetDimensions().ToRectangle().TopRight();

            float chungosity = 0;
            bool isDemo = false;

            if (worldDataCache.TryGetValue(self, out var tag3) && tag3 != null)
            {
                chungosity = tag3.GetFloat("Chungus");
                isDemo = tag3.GetBool("IsDemoWorld");
            }

			if (isDemo)
			{
				var texDemo = ModContent.GetTexture("StarlightRiver/Assets/GUI/DemoIcon");
				UIImage icon = (UIImage)iconInfo.GetValue(self);
				icon.SetImage(texDemo);

                Rectangle box = new Rectangle((int)(pos.X - 420), (int)(pos.Y + 36), 140, 25);
                spriteBatch.Draw(ModContent.GetTexture("StarlightRiver/Assets/GUI/box"), box, Color.White); //Stamina box

                Utils.DrawBorderString(spriteBatch, "Demo World", self.GetDimensions().Position() + new Vector2(204, 38), Color.White);
            }

            if (chungosity > 0.4f)
            {
                Texture2D tex = ModContent.GetTexture("StarlightRiver/Assets/GUI/ChungusMeter");
                Texture2D tex2 = ModContent.GetTexture("StarlightRiver/Assets/GUI/ChungusMeterFill");
                spriteBatch.Draw(tex, pos + new Vector2(-122, 6), Color.White);
                spriteBatch.Draw(tex2, pos + new Vector2(-108, 10), new Rectangle(0, 0, (int)(tex2.Width * chungosity), tex2.Height), Color.White);
                spriteBatch.Draw(Main.magicPixel, new Rectangle((int)pos.X - 108 + (int)(tex2.Width * chungosity), (int)pos.Y + 10, 2, 10), Color.White);

                Rectangle rect = new Rectangle((int)pos.X - 122, (int)pos.Y + 6, tex.Width, tex.Height);

                if (rect.Contains(Main.MouseScreen.ToPoint()))
                {
                    Utils.DrawBorderString(spriteBatch, "Chungosity: " + (int)(chungosity * 100) + "%", self.GetDimensions().Position() + new Vector2(110, 70), Color.White);
                }
            }
        }

        private void AddWorldData(On.Terraria.GameContent.UI.Elements.UIWorldListItem.orig_ctor orig, UIWorldListItem self, WorldFileData data, int snapPointIndex)
        {
            orig(self, data, snapPointIndex);

            string path = data.Path.Replace(".wld", ".twld");

            TagCompound tag;

            try
            {
                byte[] buf = FileUtilities.ReadAllBytes(path, data.IsCloudSave);
                tag = TagIO.FromStream(new MemoryStream(buf), true);
            }
            catch
            {
                tag = null;
            }

            TagCompound tag2 = tag?.GetList<TagCompound>("modData").FirstOrDefault(k => k.GetString("mod") == "StarlightRiver" && k.GetString("name") == "StarlightWorld");
            TagCompound tag3 = tag2?.Get<TagCompound>("data");

            worldDataCache.Add(self, tag3);
        }

        private void RefreshWorldData(On.Terraria.GameContent.UI.States.UIWorldSelect.orig_ctor orig, UIWorldSelect self)
        {
            orig(self);
            worldDataCache.Clear();
        }
    }
}
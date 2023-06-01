using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Misc
{
	internal class PatreonPlaque : ModTile
	{
		public const string PATRONS = "501warhead, 8-Bit CornDog, A Sponge, AetherBreaker, Alessandro Glienke, Andrew Weber-Brown, Andrey Mogilnikov, Anton Filatov, Asteros TheGreat, Bart Zetten, Benny Baker, Blake Posey, Blub, Boshua, CBRTNK, Chad Webb, Charles Kropp, Charles Pisani, Cole, Cornelius Clay, Cristian Arsene, Dahbin Jung, Dan Finneck, Diegster, Doodled_lynx, Dow Zabolio, Drazden, Ererum, Ethan Harris, Ever Treadway, Garrett Cawley, Grylken, Haidex, Hero, Hyper Jinx, Italo Minussi Franco, Ivan Lopez, Jacob Newton, Jake Ryan, Jas, Jasper Strauss, João Gabriel, JudeEgg, Justin Packwood, Justin Trim, Killerjerick, Kinglue, Komaboi, Kristopher Georgia, Kyle Olaszek, Lightedflare, Lil Rice Paddy, Lukasz Sylwestrowicz, Lynncore, Marcel Wohlgemuth, NULL, Nicholas Korff, Not A Pro64, NotNessFromEarthbound, Ok Sneak, Orgiv, PaladinJackal, Paul Häusler, Phoenix Nemo, QRiosity, Rem Tan, RenkDev, Ryan Alan Fini, SWB, Samuel Gomez, Sande, Scrumlet, Sebastian Hawn, Seongbin Park, Serioustar, Silvis, Spencer Gilbert, SuperMage, ThePigeon, TitaniumNebula, TotallyThePope, Victor Bériau, WH0S, Wolf, Xc Gao, YouSnooze, Zealousmagician, beans, drowsy, edit edits, joshua, karachter, lassyTohno, man eating bear, mengEO, mitchell jury-dawes, mrmeee 123, paradise, 立衣 襟, nyang yu";
		public const string SUPER_PATRONS = "123nick, Aidbutler6424, Chase G, Daniel Yacek, Dylan Goldstein, Ethos (Ali), FBI Webcam Surveillance, Felipe Fresta, Flipolipo, Gehsteigpanzer, GlueThePengu, IgelHD, John Schassen, Kaid Bryce, Leone Nagasama, Leone Nagasama, Liuna, Lodi, LumiEvi, Mike Heng, Pixelatedfireball, Re4p3rKn1ght, Rel Darcae, Robenarcz, Scuzs, Sean Grissom, SleepyChann, Squid_Kid, Stay_Frosty, Stranger, TheBingeGamer, Tommy Huynh, Tristan Hinkins, Tucker, Tyler Krahn, Winteresce, WrathOfOlympus, cullen earl, jaja, solo, tooSlyk, woshkins, zombie wolf, Panic";

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Gold, SoundID.Tink, Color.Transparent);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;
			Vector2 pos = (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition;

			var rect = new Rectangle((int)pos.X - 16, (int)pos.Y - 32, 1130, 1000);

			rect.Inflate(20, 20);
			spriteBatch.Draw(tex, rect, new Color(100, 120, 130));

			rect.Inflate(-20, -20);
			spriteBatch.Draw(tex, rect, new Color(120, 140, 150));

			Utils.DrawBorderStringBig(spriteBatch, "This temple stands by the efforts of the weavers of starlight engraved here:", pos + new Vector2(550, 0), Color.White, 0.6f, 0.5f, 0.5f);

			Utils.DrawBorderStringBig(spriteBatch, "Starlit Travelers", pos + new Vector2(200, 80), Color.White, 1, 0.5f, 0.5f);
			Utils.DrawBorderString(spriteBatch, Helpers.Helper.WrapString(PATRONS, 400, Terraria.GameContent.FontAssets.MouseText.Value, 1), pos + new Vector2(0, 120), Color.White);

			Utils.DrawBorderStringBig(spriteBatch, "Starlit Scholars", pos + new Vector2(800, 80), Color.Gold, 1, 0.5f, 0.5f);
			Utils.DrawBorderString(spriteBatch, Helpers.Helper.WrapString(SUPER_PATRONS, 400, Terraria.GameContent.FontAssets.MouseText.Value, 1), pos + new Vector2(600, 120), Color.Gold);
		}
	}
}

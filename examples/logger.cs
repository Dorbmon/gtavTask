using Newtonsoft.Json.Linq;
using SHVDN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace GTA
{
	public static class Logger
	{
		private static JArray attributes;

		
		static Logger()
		{
			try
			{
				//string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				string jsonFilePath = "scripts/attribute.json";
				Log.Message(Log.Level.Info, $"Loading attribute data from: {jsonFilePath}");

				if (File.Exists(jsonFilePath))
				{
					string jsonText = File.ReadAllText(jsonFilePath);
					attributes = JArray.Parse(jsonText);
					Log.Message(Log.Level.Info, "Attribute data loaded successfully.");
				}
				else
				{
					Log.Message(Log.Level.Error, "Attribute JSON file not found.");
				}
			}
			catch (Exception ex)
			{
				Log.Message(Log.Level.Error, "Error loading attribute data: " + ex.Message);
			}
		}

		public static string GenLog(object entity)
		{
			string genLog = "";
			if (attributes == null)
			{
				Log.Message(Log.Level.Error, "Attribute data is not loaded. Returning empty log.");
				return genLog;
			}
			if (entity is Vehicle vehicle)
			{
				VehicleHash vhash = (VehicleHash)vehicle.Model.GetHashCode();
				string vehicleLog = $"hash_name={vhash.ToString()}, hash_code={vehicle.Model.GetHashCode()}, position={vehicle.Position.ToString()}, " +
									$"primary_color={vehicle.Mods.PrimaryColor.ToString()}, primary_color_simple={ColorSimplifier.GetSimpleColor(vehicle.Mods.PrimaryColor)}, " +
									$"secondary_color={vehicle.Mods.SecondaryColor.ToString()}, secondary_color_simple={ColorSimplifier.GetSimpleColor(vehicle.Mods.SecondaryColor)}";

				genLog = vehicleLog;
			}
			else if (entity is Ped ped)
			{
				PedHash phash = (PedHash)ped.Model.GetHashCode();
				string pedLog = $"hash_name={phash.ToString()}, hash_code={ped.Model.GetHashCode()}, position={ped.Position.ToString()}, ";

				JToken pedAttributes = attributes.SelectToken($"$.[?(@.name == '{phash}')]");
				string attributesLog;
				if (pedAttributes != null)
				{
					
					attributesLog = $"Attributes: gender={pedAttributes["sex"]}, hair_type={pedAttributes["hair"]["type"]}, hair_color={pedAttributes["hair"]["color"]}, " +
									$"top={pedAttributes["clothing"]["top"]}, top_color={pedAttributes["clothing"]["top color"]}, " +
									$"bottom={pedAttributes["clothing"]["bottom"]}, bottom_color={pedAttributes["clothing"]["bottom color"]}";
				}
				else
				{
					attributesLog = $"No attributes found for hash {ped.Model.Hash}";
				}

				genLog = pedLog + attributesLog;
			}
			return genLog;
		}

		public static string MapToSimpleColor(VehicleColor color)
		{
			string colorName = color.ToString();

			if (colorName.Contains("Black"))
				return "Black";
			if (colorName.Contains("White"))
				return "White";
			if (colorName.Contains("Red"))
				return "Red";
			if (colorName.Contains("Orange"))
				return "Orange";
			if (colorName.Contains("Yellow"))
				return "Yellow";
			if (colorName.Contains("Green"))
				return "Green";
			if (colorName.Contains("Blue") || colorName.Contains("sea"))
				return "Blue";
			if (colorName.Contains("Purple"))
				return "Purple";
			if (colorName.Contains("Brown") || colorName.Contains("Bronze"))
				return "Brown";
			if (colorName.Contains("Gray") || colorName.Contains("Grey"))
				return "Gray";
			if (colorName.Contains("Pink"))
				return "Pink";
			if (colorName.Contains("Gold"))
				return "Gold";
			if (colorName.Contains("Silver"))
				return "Silver";
			if (colorName.Contains("Chrome"))
				return "Chrome";

			return "Other"; // Default if no match found
		}
	}


	public class ColorSimplifier
	{
		private static readonly Dictionary<VehicleColor, string> ColorMap = new Dictionary<VehicleColor, string>()
	{
			{ VehicleColor.MetallicBlack, "Black" },
			{ VehicleColor.MetallicGraphiteBlack, "Black" },
			{ VehicleColor.MetallicBlackSteel, "Black" },
			{ VehicleColor.MetallicDarkSilver, "Silver" },
			{ VehicleColor.MetallicSilver, "Silver" },
			{ VehicleColor.MetallicBlueSilver, "Blue" },
			{ VehicleColor.MetallicSteelGray, "Gray" },
			{ VehicleColor.MetallicShadowSilver, "Silver" },
			{ VehicleColor.MetallicStoneSilver, "Silver" },
			{ VehicleColor.MetallicMidnightSilver, "Silver" },
			{ VehicleColor.MetallicGunMetal, "Gray" },
			{ VehicleColor.MetallicAnthraciteGray, "Gray" },
			{ VehicleColor.MatteBlack, "Black" },
			{ VehicleColor.MatteGray, "Gray" },
			{ VehicleColor.MatteLightGray, "Gray" },
			{ VehicleColor.UtilBlack, "Black" },
			{ VehicleColor.UtilBlackPoly, "Black" },
			{ VehicleColor.UtilDarksilver, "Silver" },
			{ VehicleColor.UtilSilver, "Silver" },
			{ VehicleColor.UtilGunMetal, "Gray" },
			{ VehicleColor.UtilShadowSilver, "Silver" },
			{ VehicleColor.WornBlack, "Black" },
			{ VehicleColor.WornGraphite, "Gray" },
			{ VehicleColor.WornSilverGray, "Gray" },
			{ VehicleColor.WornSilver, "Silver" },
			{ VehicleColor.WornBlueSilver, "Blue" },
			{ VehicleColor.WornShadowSilver, "Silver" },
			{ VehicleColor.MetallicRed, "Red" },
			{ VehicleColor.MetallicTorinoRed, "Red" },
			{ VehicleColor.MetallicFormulaRed, "Red" },
			{ VehicleColor.MetallicBlazeRed, "Red" },
			{ VehicleColor.MetallicGracefulRed, "Red" },
			{ VehicleColor.MetallicGarnetRed, "Red" },
			{ VehicleColor.MetallicDesertRed, "Red" },
			{ VehicleColor.MetallicCabernetRed, "Red" },
			{ VehicleColor.MetallicCandyRed, "Red" },
			{ VehicleColor.MetallicSunriseOrange, "Orange" },
			{ VehicleColor.MetallicClassicGold, "Gold" },
			{ VehicleColor.MetallicOrange, "Orange" },
			{ VehicleColor.MatteRed, "Red" },
			{ VehicleColor.MatteDarkRed, "Red" },
			{ VehicleColor.MatteOrange, "Orange" },
			{ VehicleColor.MatteYellow, "Yellow" },
			{ VehicleColor.UtilRed, "Red" },
			{ VehicleColor.UtilBrightRed, "Red" },
			{ VehicleColor.UtilGarnetRed, "Red" },
			{ VehicleColor.WornRed, "Red" },
			{ VehicleColor.WornGoldenRed, "Red" },
			{ VehicleColor.WornDarkRed, "Red" },
			{ VehicleColor.MetallicDarkGreen, "Green" },
			{ VehicleColor.MetallicRacingGreen, "Green" },
			{ VehicleColor.MetallicSeaGreen, "Green" },
			{ VehicleColor.MetallicOliveGreen, "Green" },
			{ VehicleColor.MetallicGreen, "Green" },
			{ VehicleColor.MetallicGasolineBlueGreen, "Green" },
			{ VehicleColor.MatteLimeGreen, "Green" },
			{ VehicleColor.UtilDarkGreen, "Green" },
			{ VehicleColor.UtilGreen, "Green" },
			{ VehicleColor.WornDarkGreen, "Green" },
			{ VehicleColor.WornGreen, "Green" },
			{ VehicleColor.WornSeaWash, "Green" },
			{ VehicleColor.MetallicMidnightBlue, "Blue" },
			{ VehicleColor.MetallicDarkBlue, "Blue" },
			{ VehicleColor.MetallicSaxonyBlue, "Blue" },
			{ VehicleColor.MetallicBlue, "Blue" },
			{ VehicleColor.MetallicMarinerBlue, "Blue" },
			{ VehicleColor.MetallicHarborBlue, "Blue" },
			{ VehicleColor.MetallicDiamondBlue, "Blue" },
			{ VehicleColor.MetallicSurfBlue, "Blue" },
			{ VehicleColor.MetallicNauticalBlue, "Blue" },
			{ VehicleColor.MetallicBrightBlue, "Blue" },
			{ VehicleColor.MetallicPurpleBlue, "Blue" },
			{ VehicleColor.MetallicSpinnakerBlue, "Blue" },
			{ VehicleColor.MetallicUltraBlue, "Blue" },
			{ VehicleColor.MetallicBrightBlue2, "Blue" },
			{ VehicleColor.UtilDarkBlue, "Blue" },
			{ VehicleColor.UtilMidnightBlue, "Blue" },
			{ VehicleColor.UtilBlue, "Blue" },
			{ VehicleColor.UtilSeaFoamBlue, "Blue" },
			{ VehicleColor.UtilLightningBlue, "Blue" },
			{ VehicleColor.UtilMauiBluePoly, "Blue" },
			{ VehicleColor.UtilBrightBlue, "Blue" },
			{ VehicleColor.MatteDarkBlue, "Blue" },
			{ VehicleColor.MatteBlue, "Blue" },
			{ VehicleColor.MatteMidnightBlue, "Blue" },
			{ VehicleColor.WornDarkBlue, "Blue" },
			{ VehicleColor.WornBlue, "Blue" },
			{ VehicleColor.WornLightBlue, "Blue" },
			{ VehicleColor.MetallicTaxiYellow, "Yellow" },
			{ VehicleColor.MetallicRaceYellow, "Yellow" },
			{ VehicleColor.MetallicBronze, "Brown" },
			{ VehicleColor.MetallicYellowBird, "Yellow" },
			{ VehicleColor.MetallicLime, "Green" },
			{ VehicleColor.MetallicChampagne, "Beige" },
			{ VehicleColor.MetallicPuebloBeige, "Beige" },
			{ VehicleColor.MetallicDarkIvory, "Ivory" },
			{ VehicleColor.MetallicChocoBrown, "Brown" },
			{ VehicleColor.MetallicGoldenBrown, "Brown" },
			{ VehicleColor.MetallicLightBrown, "Brown" },
			{ VehicleColor.MetallicStrawBeige, "Beige" },
			{ VehicleColor.MetallicMossBrown, "Brown" },
			{ VehicleColor.MetallicBistonBrown, "Brown" },
			{ VehicleColor.MetallicBeechwood, "Brown" },
			{ VehicleColor.MetallicDarkBeechwood, "Brown" },
			{ VehicleColor.MetallicChocoOrange, "Orange" },
			{ VehicleColor.MetallicBeachSand, "Sand" },
			{ VehicleColor.MetallicSunBleechedSand, "Sand" },
			{ VehicleColor.MetallicCream, "Cream" },
			{ VehicleColor.UtilBrown, "Brown" },
			{ VehicleColor.UtilMediumBrown, "Brown" },
			{ VehicleColor.UtilLightBrown, "Brown" },
			{ VehicleColor.MetallicWhite, "White" },
			{ VehicleColor.MetallicFrostWhite, "White" },
			{ VehicleColor.WornHoneyBeige, "Beige" },
			{ VehicleColor.WornBrown, "Brown" },
			{ VehicleColor.WornDarkBrown, "Brown" },
			{ VehicleColor.WornStrawBeige, "Beige" },
			{ VehicleColor.BrushedSteel, "Steel" },
			{ VehicleColor.BrushedBlackSteel, "Black" },
			{ VehicleColor.BrushedAluminium, "Silver" },
			{ VehicleColor.Chrome, "Chrome" },
			{ VehicleColor.WornOffWhite, "White" },
			{ VehicleColor.UtilOffWhite, "White" },
			{ VehicleColor.WornOrange, "Orange" },
			{ VehicleColor.WornLightOrange, "Orange" },
			{ VehicleColor.MetallicSecuricorGreen, "Green" },
			{ VehicleColor.WornTaxiYellow, "Yellow" },
			{ VehicleColor.PoliceCarBlue, "Blue" },
			{ VehicleColor.MatteGreen, "Green" },
			{ VehicleColor.MatteBrown, "Brown" },
			{ VehicleColor.WornOrange2, "Orange" },
			{ VehicleColor.MatteWhite, "White" },
			{ VehicleColor.WornWhite, "White" },
			{ VehicleColor.WornOliveArmyGreen, "Green" },
			{ VehicleColor.PureWhite, "White" },
			{ VehicleColor.HotPink, "Pink" },
			{ VehicleColor.Salmonpink, "Pink" },
			{ VehicleColor.MetallicVermillionPink, "Red" },
			{ VehicleColor.Orange, "Orange" },
			{ VehicleColor.Green, "Green" },
			{ VehicleColor.Blue, "Blue" },
			{ VehicleColor.MettalicBlackBlue, "Black" },
			{ VehicleColor.MetallicBlackPurple, "Black" },
			{ VehicleColor.MetallicBlackRed, "Black" },
			{ VehicleColor.HunterGreen, "Green" },
			{ VehicleColor.MetallicPurple, "Purple" },
			{ VehicleColor.MetaillicVDarkBlue, "Blue" },
			{ VehicleColor.ModshopBlack1, "Black" },
			{ VehicleColor.MattePurple, "Purple" },
			{ VehicleColor.MatteDarkPurple, "Purple" },
			{ VehicleColor.MetallicLavaRed, "Red" },
			{ VehicleColor.MatteForestGreen, "Green" },
			{ VehicleColor.MatteOliveDrab, "Green" },
			{ VehicleColor.MatteDesertBrown, "Brown" },
			{ VehicleColor.MatteDesertTan, "Tan" },
			{ VehicleColor.MatteFoliageGreen, "Green" },
			{ VehicleColor.DefaultAlloyColor, "Metal" },
			{ VehicleColor.EpsilonBlue, "Blue" },
			{ VehicleColor.PureGold, "Gold" },
			{ VehicleColor.BrushedGold, "Gold" },
			{ VehicleColor.MP100GoldSpecular, "Gold" }
	};

		public static string GetSimpleColor(VehicleColor color)
		{
			if (ColorMap.TryGetValue(color, out string simpleColor))
			{
				return simpleColor;
			}
			return "Other";  // 如果没有找到映射，返回"Other"
		}
	}

}

using Newtonsoft.Json.Linq;
using SHVDN;
using System;
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
				string jsonFilePath = "attribute.json";
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
									$"primary_color={vehicle.Mods.PrimaryColor.ToString()}, secondary_color={vehicle.Mods.SecondaryColor.ToString()}";

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
					attributesLog = $"Attributes: hair_type={pedAttributes["hair"]["type"]}, hair_color={pedAttributes["hair"]["color"]}, " +
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
	}
}

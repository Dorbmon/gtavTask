using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SHVDN;

namespace GTA
{
	internal class missionManager: Script
	{
		List<mission> missions = new List<mission>();
		Random rand = new Random();
		private bool isClearPressed = false;
		private bool isRunningMissions = false;

		private string missionInfoFolderPath = @"D:\GTA\Missions\"; 
		private string missionInfoFileName = "CurrentMissionInfo.txt";
		private string missionInfoFilePath;

		private mission currentMission = null;
		private int currentMissionIndex = -1;
		private DateTime missionStartTime = DateTime.MinValue;
		private int MISSION_MAX_TIME = 600;
		public missionManager()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;

			if (!Directory.Exists(missionInfoFolderPath))
			{
				Directory.CreateDirectory(missionInfoFolderPath);
			}

			missionInfoFilePath = Path.Combine(missionInfoFolderPath, missionInfoFileName);
			File.WriteAllText(missionInfoFilePath, "");
		}

		private void OnTick(object sender, EventArgs e)
		{
			//Log.Message(Log.Level.Info, "OnTick called ", ".");
			if (isRunningMissions && currentMissionIndex < missions.Count)
			{
				var mission = missions[currentMissionIndex];
				TimeSpan timeSpan = DateTime.Now - missionStartTime;
				if (!mission.is_mission_finished() && timeSpan.TotalSeconds < MISSION_MAX_TIME)
				{
					mission.Update();
				}
				else
				{
					mission.destroy();
					currentMissionIndex++;
					if (currentMissionIndex < missions.Count)
					{
						missions[currentMissionIndex].load();
						missionStartTime = DateTime.Now;
					}
					else
					{
						isRunningMissions = false;
					}
				}
			}
			if (isClearPressed)
			{
				clearMissions();
				isClearPressed = false;
			}
		}
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F7) {
				// start to do missions
				clearMissions();
				loadMissions();
				if (missions.Any())
				{
					missions[0].load();
					isRunningMissions = true;
					currentMissionIndex = 0;
					missionStartTime = DateTime.Now;
				}
			}
			if (e.KeyCode == Keys.F8)
			{
				Log.Message(Log.Level.Info, "F8 is pressed ", ".");
				isClearPressed = true;
			}
		}
		private void reset_lm()
		{

		}
		
		private void loadMissions()
		{
			//missions.Add(InstantiateScript<mission_hit1>());
			
			//missions.Add(InstantiateScript<mission_vehicle_loop>());
			//missions.Add(InstantiateScript<event_on_off_vehicle_loop>());
			//missions.Add(InstantiateScript<event_animation>());
			missions.Add(InstantiateScript<event_time_loop>());
			//missions.Add(InstantiateScript<mission_dog_follow2>());
			/*
			missions.Add(InstantiateScript<mission_stop_fighting2>());
			missions.Add(InstantiateScript<mission_stop_fighting3>());
			missions.Add(InstantiateScript<mission_stop_fighting4>());
			missions.Add(InstantiateScript<mission_stop_fighting5>());

			missions.Add(InstantiateScript<mission_npc_follow1>());
			
			missions.Add(InstantiateScript<mission_npc_follow3>());
			missions.Add(InstantiateScript<mission_npc_follow4>());
			missions.Add(InstantiateScript<mission_npc_follow5>());

			missions.Add(InstantiateScript<mission_dog_follow1>());
			missions.Add(InstantiateScript<mission_dog_follow2>());
			missions.Add(InstantiateScript<mission_dog_follow3>());
			missions.Add(InstantiateScript<mission_dog_follow4>());
			missions.Add(InstantiateScript<mission_dog_follow5>());
			
			missions.Add(InstantiateScript<mission_boat1>());
			missions.Add(InstantiateScript<mission_boat2>());
			missions.Add(InstantiateScript<mission_boat3>());
			missions.Add(InstantiateScript<mission_boat4>());
			missions.Add(InstantiateScript<mission_boat5>());
			*/
		}
		private void clearMissions()
		{
			GTA.UI.Notification.Show("clearMissions!");
			Log.Message(Log.Level.Info, " clear missions, mission_count= ", missions.Count.ToString(), ".");
			for (int i = 0; i < missions.Count; i++)
			{
				var mission = missions[i];
				mission.destroy();
			}
			missions.Clear();
			Log.Message(Log.Level.Info, " clear missions done, mission_count= ", missions.Count.ToString(), ".");
			//GTA.UI.Screen.ShowSubtitle($"mission_count: {missions.Count}");
		}

		private void UpdateCurrentMissionInfo(string missionClassName, int missionId)
		{
			string content = $"Current Mission: {missionClassName}\nMission ID: {missionId}";
			File.WriteAllText(missionInfoFilePath, content);
		}

		
	}

	public static class RandomEnumPicker
	{
		private static Random random = new Random();

		// 泛型方法，用于从任何枚举类型中随机选择一个值
		public static T GetRandomEnumValue<T>() where T : Enum
		{
			Array values = Enum.GetValues(typeof(T));
			T randomValue = (T)values.GetValue(random.Next(values.Length));
			return randomValue;
		}
	}
}

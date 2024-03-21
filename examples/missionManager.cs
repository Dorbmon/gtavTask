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

		private string missionInfoFolderPath = @"D:\GTA\Missions\"; 
		private string missionInfoFileName = "CurrentMissionInfo.txt";
		private string missionInfoFilePath;

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
			
		}
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F7) {
				// start to do missions
				clearMissions();
				loadMissions();
				runMissionLoop();
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
		private void runMissionLoop()
		{
			for (int i = 0;i < missions.Count;i ++ )
			{
				int mission_id = rand.Next() % missions.Count;
				var mission = missions[i];
				UpdateCurrentMissionInfo(mission.GetType().Name, i);

				mission.load();
				reset_lm();
				var begin = DateTime.Now;
				bool finished = true;
				while (!mission.is_mission_finished())
				{
					Yield();
					
					if (DateTime.Now.Subtract(begin).TotalSeconds > 600)
					{
						// mission failed
						finished = false;
						break;
					}

					if (isClearPressed)
					{
						Log.Message(Log.Level.Info, "F11 is pressed, mission set finish ", ".");
						finished = false;
						break;
					}
					
				}
				if (finished)
				{
					GTA.UI.Notification.Show("mission done!");
				} else
				{
					GTA.UI.Notification.Show("mission failed!");
				}
				mission.destroy();
				if (isClearPressed)
				{
					Log.Message(Log.Level.Info, "F11 is pressed, call clearMissions ", ".");
					clearMissions();
					isClearPressed = false;
				}
			}
		}
		private void loadMissions()
		{
			//missions.Add(InstantiateScript<mission_hit1>());
			
			missions.Add(InstantiateScript<mission_dog_follow1>());
			missions.Add(InstantiateScript<mission_dog_follow2>());
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
}

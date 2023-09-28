using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
//using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;
using GTA.Native;
using SHVDN;

namespace GTA
{
	internal class mission_stop_fighting1 : mission
	{
		enum MissionState
		{
			NotStarted,
			RunToSpot,
			WalkToPed,
			StopFight,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vector3 npc1Pos = new Vector3(0, 0, 0);
		private Vector3 npc2Pos = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 spotPos = new Vector3(0, 0, 0);
		private Ped npc1, npc2;
		private Vehicle spot;
		private int counter = 0;
		private bool isLoaded = false;
		private bool walkToPedState = false;
		private bool runToSpotState = false;
		private bool driveToShoreState = false;
		private bool playerInBoatState = false;
		private int pause = 150;
		private int endPause = 2400;
		private bool isPaused = false;



		public mission_stop_fighting1()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}

		public override void load()
		{
			GTA.UI.Notification.Show("load mission_stop_fighting...");
			Ped player = Game.Player.Character;
			changePos(ref playerPos, -927, -1074, 3);
			changePos(ref npc1Pos, -875, -1072, 2);
			changePos(ref npc2Pos, -877, -1066, 2);
			changePos(ref spotPos, -897, -1063, 2);

			// 设置游戏时间为下午2点30分
			World.CurrentTimeOfDay = new TimeSpan(14, 30, 0);
			World.Weather = Weather.Clear;  // 设置天气为晴朗

			Game.Player.Character.Position = playerPos;
			foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character, 100.0f))
			{
				if (ped != Game.Player.Character) // 不删除玩家角色
				{
					ped.Delete();
				}
			}
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 100.0f))
			{
				vehicle.Delete();
			}
			npc1 = World.CreatePed(PedHash.Beach01AMM, npc1Pos);
			npc2 = World.CreatePed(PedHash.Genfat01AMM, npc2Pos);

			if (npc1.IsAlive && npc2.IsAlive)
			{
				npc1.Task.FightAgainst(npc2);
				npc2.Task.FightAgainst(npc1);
			}

			
			spot = World.CreateVehicle(VehicleHash.Slamtruck, spotPos);

			isLoaded = true;

		}

		public override void destroy()
		{

			if (npc1 != null)
			{
				npc1.Delete();
			}
			if (npc2 != null)
			{
				npc2.Delete();
			}

		}

		public override bool is_mission_finished()
		{
			return isMissionSucceed;
		}
		public void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F12)
			{
				isPaused = !isPaused;
				GTA.UI.Notification.Show("Mission Paused");
			}
		}
		private void OnTick(object sender, EventArgs e)
		{
			Ped player = Game.Player.Character;
			if (isPaused)
			{
				return;
			}

			switch (curState)
			{

				case MissionState.NotStarted:
					if (!isLoaded)
					{
						return;
					}
					if (counter < pause)
					{
						counter++;
						return;
					}
					curState = MissionState.RunToSpot;
					GTA.UI.Notification.Show("Mission started. Run to spot.");
					counter = 0;
					break;

				case MissionState.RunToSpot:
					if (counter < pause)
					{
						counter++;
						return;
					}
					if (!runToSpotState) runToSpotState = PlayerActions.runTo(spot);
					float run_dist = Vector3.Distance(player.Position, spot.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {run_dist}");
					if (run_dist < 5.0f)
					{
						curState = MissionState.WalkToPed;
						GTA.UI.Notification.Show("Mission started. Walk to the ped.");
					}
					
					counter = 0;

					break;


				case MissionState.WalkToPed:
					if (counter < pause)
					{
						counter++;
						return;
					}

					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (npc1 != null && npc2 != null)
					{
						if (!walkToPedState) walkToPedState = PlayerActions.runTo(npc1);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"npc is null!");
					}
					float distance = Vector3.Distance(player.Position, npc1.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {distance}");
					if (distance < 5.0f)
					{
						curState = MissionState.StopFight;
						GTA.UI.Notification.Show("Walk to ped completed. Stop Fight.");
					}
					counter = 0;
					break;
				case MissionState.StopFight:
					if (counter < pause)
					{
						counter++;
						return;
					}

					PlayerActions.stopFight(npc1, npc2);

					if (!isFighting(npc1, npc2))
					{
						curState = MissionState.Completed;
						GTA.UI.Notification.Show("Stop fight completed. Mission completed.");
					}
					counter = 0;
					break;


				case MissionState.Completed:
					if (counter < pause)
					{
						counter++;
						return;
					}
					if (npc1.IsAlive && npc2.IsAlive && !isFighting(npc1, npc2))
					{
						isMissionSucceed = true;
					}
					counter = 0;
					break;
			}
		}

		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
		}

		bool isFighting(Ped npc1, Ped npc2)
		{
			if (npc1.IsDead || npc2.IsDead)
				return false;

			if (npc1.IsInCombatAgainst(npc2) || npc2.IsInCombatAgainst(npc1))
				return true;

			return false;
		}
	}
}

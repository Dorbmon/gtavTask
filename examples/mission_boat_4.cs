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
	internal class mission_boat4 : mission
	{
		enum MissionState
		{
			NotStarted,
			SwimToBoat,
			EnterBoat,
			DriveToSpot,
			DriveBackToShore,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle boat, spot, endtarget;
		private Vector3 boatPos = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 shorePos = new Vector3(0, 0, 0);
		private Vector3 spotPos = new Vector3(0, 0, 0);
		private int counter = 0, swim_counter = 0;
		private bool isLoaded = false;
		private bool swimToBoatState = false;
		private bool driveToShoreState = false;
		private bool driveToSpotState = false;
		private bool playerInBoatState = false;
		private int pause = 150;
		private int endPause = 2400;
		private bool isPaused = false;



		public mission_boat4()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}

		public override void load()
		{
			GTA.UI.Notification.Show("load mission_boat...");
			Ped player = Game.Player.Character;
			changePos(ref playerPos, 2882, -1386, 2);
			changePos(ref boatPos, 3033, -1370, 0);
			changePos(ref shorePos, 2882, -1388, 0);
			changePos(ref spotPos, 2879, -1174, 0);
			Game.Player.Character.Position = playerPos;
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 1000.0f))
			{
				vehicle.Delete();
			}
			// 设置游戏时间为下午5点30分
			World.CurrentTimeOfDay = new TimeSpan(11, 30, 0);
			World.Weather = Weather.Clear;  // 设置天气为晴朗

			boat = World.CreateVehicle(VehicleHash.Marquis, boatPos);
			boat.Heading = 30;

			spot = World.CreateVehicle(VehicleHash.Tug, spotPos);
			endtarget = World.CreateVehicle(VehicleHash.Phoenix, shorePos);
			//if (vehicle !=  null)
			//{
			//	player.SetIntoVehicle(vehicle, VehicleSeat.Driver);
			//}




			isLoaded = true;

		}

		public override void destroy()
		{

			if (boat != null)
			{
				boat.Delete();
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
					curState = MissionState.SwimToBoat;
					GTA.UI.Notification.Show("Mission started. Swim to boat.");
					counter = 0;

					break;


				case MissionState.SwimToBoat:
					if (counter < pause)
					{
						counter++;
						return;
					}

					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (boat != null)
					{
						if (!swimToBoatState) swimToBoatState = PlayerActions.runTo(boat);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"boat is null!");
					}
					float distance = Vector3.Distance(player.Position, boat.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {distance}");
					if (distance < 5.0f)
					{
						curState = MissionState.EnterBoat;
						GTA.UI.Notification.Show("Swim to boat completed. Enter boat.");
					}

					if (distance > 2.0f)
					{
						//curState = MissionState.EnterBoat;
						swimToBoatState = false;
						GTA.UI.Notification.Show("Swim to boat again. Enter boat.");
					}
					swim_counter++;
					counter = 0;
					break;
				case MissionState.EnterBoat:
					if (counter < pause)
					{
						counter++;
						return;
					}

					if (!playerInBoatState) playerInBoatState = PlayerActions.getOnNearbyVehicle();

					if (player.IsInVehicle())
					{
						curState = MissionState.DriveBackToShore;
						GTA.UI.Notification.Show("Player in boat completed. Drive back to shore.");
					}
					counter = 0;
					break;

				case MissionState.DriveToSpot:
					if (counter < pause)
					{
						counter++;
						return;
					}

					//Console.WriteLine("");
					if (boat != null)
					{
						if (!driveToSpotState) driveToSpotState = PlayerActions.driveTo(boat, spot);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"spot is null!");
					}
					float spot_distance = Vector3.Distance(player.Position, spot.Position);

					if (spot_distance < 10.0f)
					{
						curState = MissionState.DriveBackToShore;
						GTA.UI.Notification.Show("drive to spot completed. Drive back to shore.");
					}
					counter = 0;
					break;
				
				case MissionState.DriveBackToShore:
					if (counter < pause)
					{
						counter++;
						return;
					}
					if (!driveToShoreState) driveToShoreState = PlayerActions.driveTo(boat, endtarget);
					float dist = Vector3.Distance(player.Position, shorePos);
					GTA.UI.Screen.ShowSubtitle($"distance: {dist}");
					if (dist < 10.0f)
					{
						curState = MissionState.Completed;
						GTA.UI.Notification.Show("Drive back to shore completed. Mission completed.");
					}
					counter = 0;
					break;

				case MissionState.Completed:
					if (counter < pause)
					{
						counter++;
						return;
					}
					if (boat.Position.DistanceTo(shorePos) < 10.0f && player.CurrentVehicle == boat)
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

	}
}

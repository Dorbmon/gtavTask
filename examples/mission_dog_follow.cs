using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
//using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;
using GTA.Native;
using SHVDN;

namespace GTA
{
	internal class mission_dog_follow : mission
	{
		enum MissionState
		{
			NotStarted,
			Display,
			ExitVehicle,
			WalkToDog,
			CommandDogToFollow,
			OpenFrontRightDoor,
			DogInVehicle,
			PlayerToDriverSeat,
			PlayerInVehicle,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle;
		private Vector3 carPos = new Vector3(0, 0, 0);
		private Vector3 dogPos1 = new Vector3(0, 0, 0);
		private Vector3 dogPos2 = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 personPos1 = new Vector3(0, 0, 0);
		private Vector3 personPos2 = new Vector3(0, 0, 0);
		private Vector3 personPos3 = new Vector3(0, 0, 0);
		private Model dogModel;
		private Model personModel;
		private Ped dog, dog2, person1, person2, person3;
		private int counter = 0;
		private bool isLoaded = false;
		private bool walkToDogState = false;
		private bool walkToVehicileState = false;
		private bool dogFollowState = false;
		private bool dogOnVehicle = false;
		private bool playerInVehicleState = false;
		private int pause = 150;
		private int endPause = 2400;
		private bool isPaused = false;


		
		public mission_dog_follow()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}
		public override void load()
		{
			GTA.UI.Notification.Show("load mission_dog_follow...");
			Ped player = Game.Player.Character;
			
			changePos(ref playerPos, -1935, 588, 121);
			changePos(ref carPos, -1940, 582, 119.3f);
			changePos(ref dogPos1, -1946, 592, 120);
			//changePos(ref dogPos2, -1939, 584, 119);
			// 按序号增加了3个默认person位置。坐标和面向角度已设置好
			//changePos(ref personPos1, -1932, 593, 121);
			//changePos(ref personPos2, -1946, 590, 119);/*原本设置的目的地坐标：-1938.1f, 582, 119.5f*/
			//changePos(ref personPos3, -1939, 583, 119);
			// 设置游戏时间为下午3点30分
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			// 设置天气为晴朗
			World.Weather = Weather.Clear;

			Game.Player.Character.Position = playerPos;
			foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character, 20.0f))
			{
				if (ped != Game.Player.Character) // 不删除玩家角色
				{
					ped.Delete();
				}
			}
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 20.0f))
			{
				vehicle.Delete();
			}

			vehicle = World.CreateVehicle(VehicleHash.BestiaGTS, carPos);
			vehicle.Heading = 60;
			dogModel = new Model(PedHash.Chop);
			if (dogModel.IsValid)
			{
				dogModel.Request(500);
				dog = World.CreatePed(dogModel, dogPos1);
				dog.Heading = 180;
				/*
				dog2 = World.CreatePed(dogModel, dogPos2);
				dog2.Heading = 180;
				if (dog == null || dog2 == null)
				{
					GTA.UI.Notification.Show("CHOP CREATE FAILED !");
				}
				*/
			}
			/*
			Thread.Sleep(500);
			personModel = new Model(PedHash.Michael);
			if (personModel.IsValid)
			{
				personModel.Request(500);
				person1 = World.CreatePed(personModel, personPos1);
				person1.Heading = 92.2199F;

				person2 = World.CreatePed(personModel, personPos2);
				person2.Heading = 0F;
				//从设置的地方走向狗并交互
				//Function.Call(Hash.TASK_CHAT_TO_PED, person2.Handle, dog.Handle, -1, 0f, 0f, 0f, 0f, 0f);
				
				person3 = World.CreatePed(personModel, personPos3);
				person3.Heading = 162.963F;
			}
			*/
			isLoaded = true;
		}

		/*
		public override void load()
		{
			GTA.UI.Notification.Show("load mission_dog_follow...");
			Ped player = Game.Player.Character;
			changePos(ref playerPos, -1947, -592, 120);
			changePos(ref carPos, -951, 32, 48);
			changePos(ref dogPos, -948, 45, 49);

			// 设置游戏时间为下午2点30分
			World.CurrentTimeOfDay = new TimeSpan(7, 0, 0);
			World.Weather = Weather.Clear;  // 设置天气为晴朗

			vehicle = World.CreateVehicle(VehicleHash.BestiaGTS, carPos);
			vehicle.Heading = 30;

			//if (vehicle !=  null)
			//{
			//	player.SetIntoVehicle(vehicle, VehicleSeat.Driver);
			//}

			Game.Player.Character.Position = playerPos;

			dogModel = new Model(PedHash.Chop);
			if (dogModel.IsValid)
			{
				dogModel.Request(500);
				dog = World.CreatePed(dogModel, dogPos);
				if (dog == null)
				{
					GTA.UI.Notification.Show("CHOP CREATE FAILED !");
				}
			}

			isLoaded = false;

		}
		*/
		public override void destroy()
		{
			
			if (vehicle != null)
			{
				vehicle.Delete();
			}
			if (dog != null)
			{
				dog.Delete();
				dogModel.MarkAsNoLongerNeeded();
			}
			if (dog2 != null)
			{
				dog2.Delete();
				dogModel.MarkAsNoLongerNeeded();
			}
			GTA.UI.Notification.Show("mission_dog_follow destroy!");

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
					curState = MissionState.ExitVehicle;
					GTA.UI.Notification.Show("Mission started. Exit Vehicle.");
					counter = 0;

					break;
				//場景展示
				case MissionState.Display:
					if (counter < 1000)
					{
						counter++;
						return;
					}
					person1.Task.PlayAnimation("amb@world_human_stand_impatient@male@no_sign@base", "base", 1.0f, -1, 0.01f );
					person2.Task.PlayAnimation("amb@world_human_guard_patrol@male@idle_b", "idle_e", 1.0f, -1, 0.01f);
					person3.Task.PlayAnimation("amb@prop_human_parking_meter@male@idle_a", "idle_a", 1.0f, -1, 0.01f);
					counter = 0;
					break;
				case MissionState.ExitVehicle:
					//action
					if (vehicle != null)
					{
						PlayerActions.getOffVehicle();
					}
					if (player.CurrentVehicle == null && !player.IsInVehicle())
					{
						curState = MissionState.WalkToDog;
						GTA.UI.Notification.Show("Exit vehicle completed. Walk to the dog.");
					}
					break;

				case MissionState.WalkToDog:
					if (counter < pause)
					{
						counter++;
						return;
					}
					
					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (dog != null)
					{
						if (!walkToDogState) walkToDogState = PlayerActions.walkToEntity(dog);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"dog is null!");
					}
					

					float distance = Vector3.Distance(player.Position, dog.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {distance}");
					if (distance < 2.0f)
					{
						curState = MissionState.CommandDogToFollow;
						GTA.UI.Notification.Show("Walk to dog completed. Command dog to follow.");
					}
					counter = 0;
					break;

				case MissionState.CommandDogToFollow:
					if (counter < pause)
					{
						counter++;
						return;
					}

					float dist = Vector3.Distance(player.Position, dog.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {dist}");

					if (!dogFollowState)		dogFollowState = PlayerActions.letFollow(dog);
					if (!walkToVehicileState) walkToVehicileState = PlayerActions.walkToEntity(vehicle);
					/**
					if (Vector3.Distance(player.Position, dog.Position) > 5.0f)
					{
						PlayerActions.standStill();
						walk_to_vehicle_state = false;
						dog_follow_state = false;
					}
					*/
					if (Vector3.Distance(dog.Position, vehicle.Position) < 5.0f)
					{
						PlayerActions.letStopFollow(dog);
						curState = MissionState.PlayerInVehicle;
						GTA.UI.Notification.Show("Command dog to follow completed. Player in vehicle.");
					}
					counter = 0;
					break;

				case MissionState.PlayerInVehicle:
					if (counter < pause)
					{
						counter++;
						return;
					}

					if (!playerInVehicleState) playerInVehicleState = PlayerActions.getOnNearbyVehicle();

					if (player.IsInVehicle())
					{
						curState = MissionState.OpenFrontRightDoor;
						GTA.UI.Notification.Show("Player in vehicle completed. Open front right door.");
					}
					counter = 0;
					break;

				case MissionState.OpenFrontRightDoor:
					if (counter < pause)
					{
						counter++;
						return;
					}

					PlayerActions.openVehicleFrontRightDoor();
					bool isRightFrontDoorOpen = vehicle.Doors[VehicleDoorIndex.FrontRightDoor].IsOpen;
					Log.Message(Log.Level.Debug, "OpenFrontRightDoor");

					if (isRightFrontDoorOpen)
					{
						curState = MissionState.DogInVehicle;
						GTA.UI.Notification.Show("Open door is completed. Dog get in car.");
					}
					counter = 0;
					break;

				case MissionState.DogInVehicle:
					if (counter < pause)
					{
						counter++;
						return;
					}
					if (!dogOnVehicle) dogOnVehicle = PlayerActions.letOnCar(dog);

					if (dog.IsInVehicle())
					{
						curState = MissionState.Completed;
						GTA.UI.Notification.Show("Dog in vehicle is completed. Mission completed... Result checking...");
					}
					counter = 0;
					break;

				case MissionState.Completed:
					if (counter < pause)
					{
						counter++;
						return;
					}
					if (player.CurrentVehicle == dog.CurrentVehicle )
					{
						isMissionSucceed = true;
					}
					break;
			}
		}

		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
		}

		private void playerAct()
		{
			
		}
	}
}

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
	internal class mission_npc_follow4 : mission
	{
		enum MissionState
		{
			NotStarted,
			WalkToNpc,
			WalkToSpot,
			CommandNpcToFollow,
			WalkToShelter,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle;
		private Vector3 shelterPos = new Vector3(0, 0, 0);
		private Vector3 npcPos = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 spotPos = new Vector3(0, 0, 0);
		private Ped npc;
		private Ped spot;
		private Vehicle endtarget;
		private int counter = 0;
		private bool isLoaded = false;
		private bool walkToSpotState = false;
		private bool walkToNpcState = false;
		private bool walkToShelterState = false;
		private bool npcFollowState = false;
		private int pause = 150;
		private bool isPaused = false;



		public mission_npc_follow4()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}
		public override void load()
		{
			GTA.UI.Notification.Show("load mission_npc_follow...");
			Ped player = Game.Player.Character;

			changePos(ref playerPos, -708, -974, 20);
			changePos(ref shelterPos, -730, -919, 18);
			changePos(ref npcPos, -700, -918, 19);
			changePos(ref spotPos, -701, -971, 20);
			// 设置游戏时间为下午3点30分
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			// 设置天气为晴朗
			World.Weather = Weather.Clear;

			Game.Player.Character.Position = playerPos;
			foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character, 2000.0f))
			{
				if (ped != Game.Player.Character) // 不删除玩家角色
				{
					ped.Delete();
				}
			}
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 2000.0f))
			{
				vehicle.Delete();
			}

			npc = World.CreatePed(PedHash.Downtown01AFM, npcPos);

			
			if (npc == null)
			{
				GTA.UI.Notification.Show("NPC CREATE FAILED !");
			}

			Model mModel = new Model(PedHash.Cat);
			if (mModel.IsValid )
			{
				mModel.Request(1000);
				if (mModel.IsLoaded)
				{
					spot = World.CreatePed(mModel, spotPos);
				}
			}
			else
			{
				GTA.UI.Notification.Show("sofa model invalid！");
			}
			endtarget = World.CreateVehicle(VehicleHash.Manana, shelterPos);

			isLoaded = true;
		}

		public override void destroy()
		{

			if (vehicle != null)
			{
				vehicle.Delete();
			}
			if (npc != null)
			{
				npc.Delete();
			}
			GTA.UI.Notification.Show("mission_npc_follow destroy!");

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
					if (counter < 10)
					{
						counter++;
						return;
					}
					curState = MissionState.WalkToSpot;
					GTA.UI.Notification.Show("Mission started. Walk to cat");
					counter = 0;

					break;

				case MissionState.WalkToSpot:
					if (counter < pause)
					{
						counter++;
						return;
					}

					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (npc != null)
					{
						if (!walkToSpotState) walkToSpotState = PlayerActions.walkToEntity(spot);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"npc is null!");
					}


					float dist = Vector3.Distance(player.Position, spot.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {dist}");
					if (dist < 2.0f)
					{
						curState = MissionState.WalkToNpc;
						GTA.UI.Notification.Show("Walk to cat completed. walk to npc.");
					}
					counter = 0;
					break;


				case MissionState.WalkToNpc:
					if (counter < pause)
					{
						counter++;
						return;
					}

					//action
					//PlayerActions.walkToModel(dogModel);
					//Log.Message(Log.Level.Debug, "Switch to walktoDog successfully.");
					//Console.WriteLine("");
					if (npc != null)
					{
						if (!walkToNpcState) walkToNpcState = PlayerActions.walkToEntity(npc);
					}
					else
					{
						GTA.UI.Screen.ShowSubtitle($"npc is null!");
					}


					float distance = Vector3.Distance(player.Position, npc.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {distance}");
					if (distance < 2.0f)
					{
						curState = MissionState.CommandNpcToFollow;
						GTA.UI.Notification.Show("Walk to npc completed. Command npc to follow.");
					}
					counter = 0;
					break;

				case MissionState.CommandNpcToFollow:
					if (counter < pause)
					{
						counter++;
						return;
					}

					float dista = Vector3.Distance(player.Position, npc.Position);
					GTA.UI.Screen.ShowSubtitle($"distance: {dista}");

					if (!npcFollowState) npcFollowState = PlayerActions.letFollow(npc);
					if (!walkToShelterState) walkToShelterState = PlayerActions.walkToEntity(endtarget);
					/**
					if (Vector3.Distance(player.Position, dog.Position) > 5.0f)
					{
						PlayerActions.standStill();
						walk_to_vehicle_state = false;
						dog_follow_state = false;
					}
					*/
					if (Vector3.Distance(npc.Position, shelterPos) < 5.0f)
					{
						PlayerActions.letStopFollow(npc);
						curState = MissionState.Completed;
						GTA.UI.Notification.Show("Command dog to follow completed. Mission complete.");
					}
					counter = 0;
					break;


				case MissionState.Completed:
					if (counter < pause)
					{
						counter++;
						return;
					}
					float npc_shlt_dist = Vector3.Distance(shelterPos, npc.Position);
					if (npc_shlt_dist < 5.0f)
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

		private void playerAct()
		{

		}
	}
}

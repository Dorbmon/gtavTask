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
	internal class mission_stop_fighting4 : mission
	{
		enum MissionState
		{
			NotStarted,
			RunToPed,
			StopFight,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vector3 npc1Pos = new Vector3(0, 0, 0);
		private Vector3 npc2Pos = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Ped npc1, npc2;
		private int counter = 0;
		private bool isLoaded = false;
		private bool walkToState = false;
		private bool walkToPedState = false;
		private bool driveToShoreState = false;
		private bool playerInBoatState = false;
		private int pause = 150;
		private int endPause = 2400;
		private bool isPaused = false;



		public mission_stop_fighting4()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}

		public override void load()
		{
			GTA.UI.Notification.Show("load mission_stop_fighting...");
			Ped player = Game.Player.Character;
			changePos(ref playerPos, -106, -1626, 35);
			changePos(ref npc1Pos, -84, -1614, 30);
			changePos(ref npc2Pos, -92, -1620, 29);

			// 设置游戏时间为下午2点30分
			World.CurrentTimeOfDay = new TimeSpan(17, 30, 0);
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
			npc1 = World.CreatePed(PedHash.OgBoss01AMM, npc1Pos);
			npc2 = World.CreatePed(PedHash.Genfat02AMM, npc2Pos);
			npc1.IsInvincible = true;
			npc2.IsInvincible = true;
			if (npc1.IsAlive && npc2.IsAlive)
			{
				npc1.Task.FightAgainst(npc2);
				npc2.Task.FightAgainst(npc1);
			}
			if (npc1 != null && npc2 != null)
			{
				isLoaded = true;
				curState = MissionState.RunToPed;
			}
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
			if (isPaused)
			{
				return;
			}
			runTo(curState, npc1);
			stopFight(curState, npc1, npc2);
			checkResult(curState);
		}

		private void runTo(MissionState state, Entity target)
		{

			Ped player = Game.Player.Character;
			if (state != MissionState.RunToPed)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				return;
			}
			if (!walkToState) walkToState = PlayerActions.runTo(target);

			float distance = Vector3.Distance(player.Position, target.Position);
			//Log.Message(Log.Level.Debug, "walkTo, ", " state: ", state.ToString(), " target: ", target.ToString()," distance: ", distance.ToString());
			GTA.UI.Screen.ShowSubtitle($"state: {state.ToString()}, distance: {distance}");
			if (distance < 3.0f)
			{
				if (state == MissionState.RunToPed)
				{
					curState = MissionState.StopFight;
					GTA.UI.Notification.Show("Run to npc completed. Stop fight.");
					walkToState = false;
				}
			}
			counter = 0;
		}

		private void stopFight(MissionState state, Entity target1, Entity target2)
		{
			if (state != MissionState.StopFight)
			{
				return;
			}
			if (counter < pause)
			{
				counter++;
				//return;
			}
			Ped ped1 = target1 as Ped;
			Ped ped2 = target2 as Ped;
			PlayerActions.stopFight(ped1, ped2);
			if (!isFighting(ped1, ped2))
			{
				curState = MissionState.Completed;
				GTA.UI.Notification.Show("Stop fight completed. Mission completed.");
			}
			counter = 0;
		}

		private void checkResult(MissionState state)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.Completed)
			{
				return;
			}
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
		}
		private void OnTick_case(object sender, EventArgs e)
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
					curState = MissionState.RunToPed;
					GTA.UI.Notification.Show("Mission started. Walk to the ped.");
					counter = 0;

					break;


				case MissionState.RunToPed:
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

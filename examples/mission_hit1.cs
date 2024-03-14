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
using static System.Windows.Forms.AxHost;

namespace GTA
{
	[ScriptAttributes(NoDefaultInstance = true)]
	internal class mission_hit1 : mission
	{
		enum MissionState
		{
			NotStarted,
			AimAtTarget,
			HitTarget,
			Completed
		}

		private MissionState curState = MissionState.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle boat;
		private Prop obj;
		
		//private Vehicle endtarget;
		private Vector3 objPos = new Vector3(0, 0, 0);
		private Vector3 playerPos = new Vector3(0, 0, 0);

		GTA.WeaponHash weaponHash = WeaponHash.Pistol;
		private int counter = 0, swim_counter = 0;
		private bool isLoaded = false;
		private bool aimAtTargetState = false;
		private bool hitTargetState = false;
		private int pause = 50;
		private int endPause = 2400;
		private bool isPaused = false;



		public mission_hit1()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}

		public override void load()
		{
			GTA.UI.Notification.Show("load mission_hit...");
			Ped player = Game.Player.Character;
			changePos(ref playerPos, -14, -1444, 30);
			changePos(ref objPos, -14, -1448, 30);
			//player.Heading = 180;
			Game.Player.Character.Position = playerPos;
			Wait(500);
			
			foreach (Prop obj in World.GetNearbyProps(Game.Player.Character.Position, 20.0f))
			{
				if (obj != Game.Player.Character) // 不删除玩家角色
				{
					obj.Delete();
				}
			}
			
			
			obj = World.CreateProp("prop_fire_hydrant_1", objPos, true, true);


			Vector3 direction = obj.Position - Game.Player.Character.Position;
			//float heading = (float)System.Math.Atan2(direction.Y, direction.X) * (180 / (float)System.Math.PI);
			Game.Player.Character.Heading = 180;

			player.Weapons.Give(weaponHash, 100, true, true);
			// set time and weather
			World.CurrentTimeOfDay = new TimeSpan(17, 30, 0);
			World.Weather = Weather.Clear;  // 设置天气为晴朗
			PlayerActions.changeToFirstPersonView();
			
			//endtarget = World.CreateCheckpoint(CheckpointIcon.CylinderTripleArrow, shorePos, new Vector3(0, 0, 0), 10, System.Drawing.Color.Red);

			if (obj != null)
			{
				isLoaded = true;
				//curState = MissionState.AimAtTarget;
			}


		}

		public override void destroy()
		{

			if (obj != null)
			{
				obj.Delete();
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
			//GTA.UI.Screen.ShowSubtitle($"state: {curState}");
			if (isPaused)
			{
				return;
			}
			aimAtTarget(curState, obj);
			hitTarget(curState, obj);
			checkResult(curState, obj);
		}

		private void aimAtTarget(MissionState state, Entity obj)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.AimAtTarget) { return; }
			if (counter < pause)
			{
				counter++;
				return;
			}
			if (obj == null) { return; }
			if (!aimAtTargetState) aimAtTargetState = PlayerActions.aimAtTarget(obj, 10000);
			//Wait(500);
			if (PlayerActions.checkAimTarget(obj))
			{
				curState = MissionState.HitTarget;
				GTA.UI.Notification.Show("Aim at target completed. Next step hit target.");
			}
			curState = MissionState.HitTarget;
			counter = 0;
		}

		private void hitTarget(MissionState state, Entity obj)
		{
			Ped player = Game.Player.Character;
			if (state != MissionState.HitTarget) { return; }
			if (counter < pause) { counter++; return; }
			if (obj == null) { return; }
			if (!hitTargetState) hitTargetState = PlayerActions.hitTarget(obj);
			if (checkObjDamaged(obj))
			{
				curState = MissionState.Completed;
			}
			counter = 0;
		}

		private void checkResult(MissionState state, Entity obj)
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
			if (checkObjDamaged(obj))
			{
				isMissionSucceed = true;
			}
			counter = 0;
		}
		public bool checkObjDamaged(Entity obj)
		{
			if (obj != null && obj.Exists())
			{
				if (obj.HasBeenDamagedBy(weaponHash))
				{
					return true;
				}
			}
			return false;
		}

		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
		}


	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
//using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA.Math;
using GTA.Native;
using System.IO;
using SHVDN;


namespace GTA
{
	[ScriptAttributes(NoDefaultInstance = true)]
	internal class event_car_race : mission
	{
		enum State
		{
			NotStarted,
			Waiting,
			GenerateCarAndNpc,
			NpcGetOnCar,
			CarRacing,
			CleanupAndRestart
		}

		private State curState = State.NotStarted;
		private bool isMissionSucceed = false;
		private Vehicle vehicle1;
		private Vehicle vehicle2;
		private Ped ped1;
		private Ped ped2;

		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 carGenPos1 = new Vector3(0, 0, 0);
		private Vector3 carGenPos2 = new Vector3(0, 0, 0);
		private Vector3 carEndPos1 = new Vector3(0, 0, 0);
		private Vector3 carEndPos2 = new Vector3(0, 0, 0);

		private bool ped1Finished = false;
		private bool ped2Finished = false;

		private PedHash _pedHash1 = new PedHash();
		private PedHash _pedHash2 = new PedHash();
		private VehicleHash _vehicleHash1 = new VehicleHash();
		private VehicleHash _vehicleHash2 = new VehicleHash();

		private TimeSpan timespan;
		private DateTime startTime;
		private bool isPaused = false;
		private bool timerStarted = false;
		private int loopTime = 0;
		private int executeTime = 0;

		public event_car_race()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}

		private void LoadSettings()
		{
			string configPath = $"scripts\\{this.GetType().Name}.ini";

			if (System.IO.File.Exists(configPath))
			{
				ScriptSettings config = ScriptSettings.Load(configPath);
				loopTime = config.GetValue("Settings", "LoopTime", 1);
			}
			else
			{
				loopTime = 1;  
			}
		}

		public override void load()
		{
			GTA.UI.Notification.Show($"load {this.GetType().Name}...");
			LoadSettings();
			Ped player = Game.Player.Character;
			player.Task.ClearAllImmediately();
			changePos(ref playerPos, -819, 192, 73);
			changePos(ref carGenPos1, -799, 163.5f, 71);
			changePos(ref carGenPos2, -790, 163, 72);
			changePos(ref carEndPos1, -839, 156, 67);
			changePos(ref carEndPos2, -834, 156, 67);
			
			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			World.Weather = Weather.Clear;
			Game.Player.Character.Position = playerPos;
			ped1Finished = false;
			ped2Finished = false;

			foreach (Ped ped in World.GetNearbyPeds(Game.Player.Character, 50.0f))
			{
				if (ped != Game.Player.Character)
				{
					ped.Delete();
				}
			}
			foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character, 50.0f))
			{
				vehicle.Delete();
			}
			startTime = DateTime.Now;
			curState = State.Waiting;
		}



		public override bool is_mission_finished()
		{
			return executeTime >= loopTime;
		}
		public void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F12)
			{
				isPaused = !isPaused;
				GTA.UI.Notification.Show("Mission Paused");
			}
		}


		public override void Update()
		{
			timespan = DateTime.Now - startTime;
			switch (curState)
			{	
				case State.Waiting:
					if (!timerStarted)
					{
						startTime = DateTime.Now;
						timerStarted = true;
						Log.Message(Log.Level.Info, $"{this.GetType().Name}::{curState.ToString()}, Starting wait period...");
					}
					else if ((DateTime.Now - startTime).TotalSeconds > 30)
					{
						curState = State.GenerateCarAndNpc;
						timerStarted = false;
						startTime = DateTime.Now;
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, Wait period completed. Proceeding to generate car and NPC.");
						GTA.UI.Notification.Show("Start filming...");
					}
					break;
				case State.GenerateCarAndNpc:
					Wait(500);

					PedHashSafe randomPed1 = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					PedHashSafe randomPed2 = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
					motorcyclesHash randomVehicle1 = RandomEnumPicker.GetRandomEnumValue<motorcyclesHash>();
					motorcyclesHash randomVehicle2 = RandomEnumPicker.GetRandomEnumValue<motorcyclesHash>();
					_vehicleHash1 = (VehicleHash)randomVehicle1;
					_vehicleHash2 = (VehicleHash)randomVehicle2;
					_pedHash1 = (PedHash)randomPed1;
					_pedHash2 = (PedHash)randomPed2;
					vehicle1 = World.CreateVehicle(_vehicleHash1, carGenPos1);
					vehicle2 = World.CreateVehicle(_vehicleHash2, carGenPos2);
					Log.Message(Log.Level.Info, $"car1:hash_name={_vehicleHash1.ToString()}, car2:hash_name={_vehicleHash2.ToString()} ");
					Wait(500);
					Random random = new Random();
					float offsetX = (float)(random.NextDouble() * 5.0 - 2.5); 
					float offsetY = (float)(random.NextDouble() * 5.0 - 2.5); 
					Vector3 pedPosition1 = new Vector3(carGenPos1.X + offsetX, carGenPos1.Y + offsetY, carGenPos1.Z);
					Vector3 pedPosition2 = new Vector3(carGenPos2.X + offsetX, carGenPos2.Y + offsetY, carGenPos2.Z);
					ped1 = World.CreatePed(_pedHash1, pedPosition1);
					ped2 = World.CreatePed(_pedHash2, pedPosition2);
					Log.Message(Log.Level.Info, $"NPC1:hash_name={_pedHash1.ToString()}, NPC2:hash_name={_pedHash2.ToString()},  ");
					
					Wait(500);
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate car and npc, " +
												$"car1:hash_name={_vehicleHash1.ToString()}, hash_code={vehicle1.Model.GetHashCode().ToString()}, generate_position={carGenPos1.ToString()}, " +
												$"primary_color={vehicle1.Mods.PrimaryColor.ToString()}, secondary_color={vehicle1.Mods.SecondaryColor.ToString()}, " +
												$"car2:hash_name={_vehicleHash2.ToString()}, hash_code={vehicle2.Model.GetHashCode().ToString()}, generate_position={carGenPos2.ToString()}, " +
												$"primary_color={vehicle2.Mods.PrimaryColor.ToString()}, secondary_color={vehicle2.Mods.SecondaryColor.ToString()}, " +
												$"NPC1:hash_name={_pedHash1.ToString()}, hash_code={ped1.Model.GetHashCode()}, generate_position={pedPosition1.ToString()}, " +
												$"NPC2:hash_name={_pedHash2.ToString()}, hash_code={ped2.Model.GetHashCode()}, generate_position={pedPosition2.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					curState = State.NpcGetOnCar;
					break;
				case State.NpcGetOnCar:
					Wait(500);
					ped1.Task.EnterVehicle(vehicle1, VehicleSeat.Driver);
					ped2.Task.EnterVehicle(vehicle2, VehicleSeat.Driver);
					Wait(5000);
					if (ped1.IsInVehicle(vehicle1) && ped2.IsInVehicle(vehicle2))
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc1 get in vehicle1, npc2 get in vehicle2" +
													$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.CarRacing;
					}
					//GTA.UI.Notification.Show("NpcGetOnCar finished");
					break;
				case State.CarRacing:
					//GTA.UI.Notification.Show("CarRacing start");
					Wait(500);
					DriveTo(ped1,vehicle1,carEndPos1);
					DriveTo(ped2,vehicle2,carEndPos2);
					Wait(500);
					if (ped1.Position.DistanceTo(carEndPos1) < 8.0f && ped1Finished == false)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, npc1 reach the end. " +
						                            $"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}.");
						ped1Finished = true;
						
					}
					if (ped2.Position.DistanceTo(carEndPos1) < 8.0f && ped2Finished == false)
					{
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, npc2 reach the end. " +
						                            $"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}.");
						ped2Finished = true;
					}

					if (ped1Finished == true && ped2Finished == true)
					{
						curState = State.CleanupAndRestart;
					}
					break;

				case State.CleanupAndRestart:
					Wait(1000);
					timespan = DateTime.Now - startTime;
					int totalSeconds = (int)timespan.TotalSeconds;
					Log.Message(Log.Level.Info, $"before:seconds={totalSeconds.ToString()}, timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					int secondsToNext30Multiple = 30 - (totalSeconds % 30);
					if (secondsToNext30Multiple == 30)
					{
						secondsToNext30Multiple = 0;
					}
					Wait(secondsToNext30Multiple * 1000);
					timespan = DateTime.Now - startTime;
					int seconds = (int)timespan.TotalSeconds;

					vehicle1.Delete();
					vehicle2.Delete();
					ped1.Delete();
					ped2.Delete();

					Log.Message(Log.Level.Info, $"after:seconds={seconds.ToString()}, timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Log.Message(Log.Level.Info, $"{this.GetType().Name}::CleanupAndRestart");
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, car and npc clean up and restart." +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Wait(500);
					addExecuteTime();
					break;
			}
		}

		private void DriveTo(Ped ped, Vehicle vehicle, Vector3 destination)
		{
			if (!ped.IsInVehicle(vehicle))
			{
				ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver);
			}
			else
			{
				ped.Task.DriveTo(vehicle, destination, 5.0f, 2f);
			}
		}
		private void addExecuteTime()
		{
			executeTime++;
			if (executeTime < loopTime)
			{
				curState = State.GenerateCarAndNpc;
			}
			else
			{
				isMissionSucceed = true;
			}
		}

		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
		}
		

	}
}

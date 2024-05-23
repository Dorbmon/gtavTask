using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SHVDN;
using System.Windows.Forms;
using GTA.Math;
using static SHVDN.NativeMemory;

namespace GTA
{


	internal class event_bus : mission
	{
		enum State
		{
			NotStarted,
			Waiting,
			GenerateBus,
			GenerateNpc,
			NpcMoveIn,
			NpcAnimate,
			NpcMoveOut,
			CleanupAndRestart
		}

		private State curState = State.NotStarted;
		private Vehicle bus;
		private List<Ped> passengers = new List<Ped>();
		private List<Ped> peds = new List<Ped>();
		private Dictionary<VehicleSeat, bool> seat_aval_map = new Dictionary<VehicleSeat, bool>
		{
			{ VehicleSeat.LeftFront, true },
			{ VehicleSeat.RightFront, true },
			{ VehicleSeat.LeftRear, true },
			{ VehicleSeat.RightRear, true },
			{ VehicleSeat.ExtraSeat1, true }
		};
		
		private int maxSimulationTime = 30 * 60 * 1000;
		private int elapsedTime = 0;
		private int npcEventInterval = 15000; // Interval for NPC events in milliseconds (1 minute)
		private int lastNpcEventTime = 0; // Time of the last NPC event
		private Random random = new Random();


		private bool isMissionSucceed = false;
		private static List<Tuple<string, string>> animationList = new List<Tuple<string, string>>();
		private Ped driver;

		private Vector3 playerPos = new Vector3(0, 0, 0);
		private Vector3 pedGenPos = new Vector3(0, 0, 0);
		private Vector3 busGenPos = new Vector3(0, 0, 0);
		private Vector3 pedEndPos = new Vector3(0, 0, 0);
		private PedHash _pedHash = new PedHash();

		private TimeSpan timespan;
		private DateTime startTime;
		private bool isPaused = false;
		private bool timerStarted = false;
		private int loopTime = 0;
		private int lastTime = 0;
		private int executeTime = 0;
		private int index = 0;
		private int seatcount = -1;

		public event_bus()
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
				lastTime = config.GetValue("Settings", "lastTime(min)", 5);
				maxSimulationTime = lastTime * 60 * 1000;
			}
			else
			{
				loopTime = 1;
				lastTime = 5;
				maxSimulationTime = lastTime * 60 * 1000;
			}
		}

		public override void load()
		{
			GTA.UI.Notification.Show($"load {this.GetType().Name}...");
			LoadSettings();
			Ped player = Game.Player.Character;
			player.Task.ClearAllImmediately();
			changePos(ref playerPos, -808, 158, 71);
			changePos(ref pedGenPos, -833, 158, 68);
			changePos(ref busGenPos, -839, 156, 67);
			changePos(ref pedEndPos, -829, 177, 71);

			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			World.Weather = Weather.Clear;
			Game.Player.Character.Position = playerPos;

			//animationList = LoadAnimations($"scripts\\animations.txt"); // 假设你的文件名是 animations.txt
			//Log.Message(Log.Level.Info, $"{this.GetType().Name}::{curState.ToString()}, animationList_size={animationList.Count.ToString()}");

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

		public override void destroy()
		{
			animationList.Clear();
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
						curState = State.GenerateBus;
						timerStarted = false;
						startTime = DateTime.Now;
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, Wait period completed. Proceeding to generate car and NPC.");
						GTA.UI.Notification.Show("Start filming...");
					}
					break;
				case State.GenerateBus:
					Wait(500);
					bus = World.CreateVehicle(VehicleHash.Bus, busGenPos);
					passengers.Clear();
					elapsedTime = 0;
					Wait(500);
					if (bus.Exists())
					{
						driver = World.CreatePed(PedHash.Abigail, busGenPos);
						driver.Task.WarpIntoVehicle(bus, VehicleSeat.Driver);
						int seat_count = GetVehicleSeatCount(bus);
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate bus, " +
												$"vehicle:" + Logger.GenLog(bus) + $", seat_count={seat_count}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						curState = State.GenerateNpc;
					}
					break;
				case State.GenerateNpc:
					// Increase elapsed time by the time of the last frame in milliseconds
					elapsedTime += (int)(Game.LastFrameTime * 1000);

					// Simulate passengers getting on and off the bus every minute
					if (elapsedTime - lastNpcEventTime >= npcEventInterval) // every 1 minute
					{
						int action = random.Next(2);
						//int action = 0;
						if (action == 0)
						{
							// Someone gets on the bus
							PedHashSafe randomPed = RandomEnumPicker.GetRandomEnumValue<PedHashSafe>();
							_pedHash = (PedHash)randomPed;
							Ped passenger = World.CreatePed(_pedHash, pedGenPos);
							passengers.Add(passenger);
							peds.Add(passenger);
							Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, generate npc, " +
												$"NPC:" + Logger.GenLog(passenger) + ", " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
							// Find the next available seat for the passenger
							VehicleSeat seat = FindNextAvailableSeat();
							Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, pick a seat, " +
														$"seat: {seat}, {(int)seat}, " +
														$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
							if (seat != VehicleSeat.None && seat != VehicleSeat.Driver)
							{
								seat_aval_map[seat] = false;
								_ = EnterVehicleAsync(passenger, bus, seat, timespan);
							}
							else
							{
								passenger.Task.GoTo(pedEndPos);
								passengers.Remove(passenger);
								Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc leave for no seat, " +
												$"NPC:" + Logger.GenLog(passenger) + ", " +
												$"seat: {seat}, {(int)seat}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
							}
						}
						else if (action == 1 && passengers.Any())
						{
							// Someone gets off the bus
							Ped passenger = passengers[random.Next(passengers.Count)];
							VehicleSeat seat = passenger.SeatIndex;
							passenger.Task.LeaveVehicle(bus, true);
							seat_aval_map[seat] = true;
							Wait(1000);
							Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, random pick a npc leave bus, " +
												$"NPC:" + Logger.GenLog(passenger) + ", " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
							passenger.Task.GoTo(pedEndPos);
							passengers.Remove(passenger);
						}
						/*
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, " +
												$"elapsedTime={elapsedTime}, maxSimulationTime={maxSimulationTime}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
						*/
						lastNpcEventTime = elapsedTime;
					}
					
					// Check if the simulation time has elapsed
					if (elapsedTime >= maxSimulationTime)
					{
						driver.Task.DriveTo(bus, pedEndPos, 5.0f, 10f);
						curState = State.CleanupAndRestart;
					}
					break;
				case State.CleanupAndRestart:
					Wait(1000);
					timespan = DateTime.Now - startTime;
					int totalSeconds = (int)timespan.TotalSeconds;
					
					int secondsToNext30Multiple = 30 - (totalSeconds % 30);
					if (secondsToNext30Multiple == 30)
					{
						secondsToNext30Multiple = 0;
					}
					Wait(secondsToNext30Multiple * 1000);
					timespan = DateTime.Now - startTime;
					int seconds = (int)timespan.TotalSeconds;

					bus.Delete();
					for (int i = 0; i < peds.Count; i++)
					{
						peds[i].Delete();
					}
					peds.Clear();
					passengers.Clear();
					driver.Delete();

					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc clean up and restart." +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Wait(500);
					addExecuteTime();
					break;
			}
		}

		private async Task EnterVehicleAsync(Ped passenger, Vehicle bus, VehicleSeat seat, TimeSpan timespan)
		{
			passenger.Task.EnterVehicle(bus, seat);
			await Task.Delay(12000);
			
			if (passenger.IsInVehicle(bus) && !bus.IsSeatFree(seat))
			{
				Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, npc enter bus, " +
							$"NPC:" + Logger.GenLog(passenger) + ", " +
							$"seat: {seat}, {(int)seat}, " +
							$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
			}
		}

		private VehicleSeat FindNextAvailableSeat()
		{
			// Define the seats that should be checked for availability
			VehicleSeat[] seats = new VehicleSeat[] {
            VehicleSeat.LeftFront, // First row left
            VehicleSeat.RightFront, // First row right
            VehicleSeat.LeftRear, // Second row left
            VehicleSeat.RightRear, // Second row right
			VehicleSeat.ExtraSeat1,
			};

			// Check each seat in the defined list to find an available one
			
			foreach (VehicleSeat seat in seats)
			{
				if (bus.IsSeatFree(seat) && seat_aval_map[seat] == true)
				{
					return seat;
				}
			}

			return VehicleSeat.None; // No seats available
		}

		private int GetVehicleSeatCount(Vehicle vehicle)
		{
			return vehicle.PassengerCapacity;
		}
		private void addExecuteTime()
		{
			executeTime++;
			if (executeTime < loopTime)
			{
				curState = State.GenerateBus;
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

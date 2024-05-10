using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using SHVDN;
using GTA.Math;
using static System.Windows.Forms.AxHost;
using static SHVDN.NativeMemory;

namespace GTA
{
	internal class event_time_loop : mission
	{
		enum State
		{
			NotStarted,
			Waiting,
			SetInitialTime,
			UpdateTime,
			CleanupAndRestart
		}

		private State curState = State.NotStarted;
		private bool isMissionSucceed = false;

		private TimeSpan timespan;
		private DateTime startTime;
		private DateTime dayStartTime;
		private DateTime lastLogTime;
		private TimeSpan timeToSimulateOneDay = TimeSpan.FromSeconds(25); 
		private TimeSpan totalDayTime = TimeSpan.FromHours(24);
		private Vector3 playerPos = new Vector3(0, 0, 0);
		private int loopTime = 1;
		private int executeTime = 0;
		private bool timerStarted = false;
		public event_time_loop()
		{
			Tick += OnTick;
			Interval = 50;
		}

		public override void load()
		{
			GTA.UI.Notification.Show($"load {this.GetType().Name}...");
			LoadSettings();
			Ped player = Game.Player.Character;
			player.Task.ClearAllImmediately();
			changePos(ref playerPos, -808, 158, 71);

			World.CurrentTimeOfDay = new TimeSpan(15, 30, 0);
			World.Weather = Weather.Clear;
			Game.Player.Character.Position = playerPos;

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
			lastLogTime = DateTime.Now;
			curState = State.Waiting;
		}
		

		public override void destroy()
		{
			
		}

		public override bool is_mission_finished()
		{
			return executeTime >= loopTime;
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
						curState = State.SetInitialTime;
						timerStarted = false;
						startTime = DateTime.Now;
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}::{curState.ToString()}, Wait period completed. Proceeding to generate car and NPC.");
						GTA.UI.Notification.Show("Start filming...");
					}
					break;
				case State.SetInitialTime:
					Wait(500);
					World.CurrentTimeOfDay = new TimeSpan(15, 0, 0);
					dayStartTime = DateTime.Now;
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, init time to 15:00, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					curState = State.UpdateTime;
					break;
				case State.UpdateTime:
					var currentTime = DateTime.Now;
					var elapsed = currentTime - dayStartTime;
					if (elapsed > timeToSimulateOneDay)
					{
						curState = State.CleanupAndRestart;	
					}
					double progress = elapsed.TotalSeconds / timeToSimulateOneDay.TotalSeconds;
					TimeSpan simulatedTime = TimeSpan.FromTicks((long)(totalDayTime.Ticks * progress));
					TimeSpan startingTime = TimeSpan.FromHours(15);
					TimeSpan newTimeOfDay = startingTime.Add(simulatedTime);
					if (newTimeOfDay.TotalHours >= 24) // 超过一天，从头开始
					{
						newTimeOfDay = newTimeOfDay.Subtract(TimeSpan.FromHours(24));
					}
					World.CurrentTimeOfDay = new TimeSpan(newTimeOfDay.Hours, newTimeOfDay.Minutes, 0);
					if (currentTime - lastLogTime > TimeSpan.FromSeconds(10))
					{
						lastLogTime = DateTime.Now;
						Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, time update, " +
												$"currentTimeofDay={World.CurrentTimeOfDay.ToString()}, " +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
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
					Log.Message(Log.Level.Info, $"{DateTime.Now}: {this.GetType().Name}:{curState.ToString()}, restart." +
												$"timespan={timespan.Hours}:{timespan.Minutes}:{timespan.Seconds}");
					Wait(500);
					addExecuteTime();
					break;
			}
		}
		private void addExecuteTime()
		{
			executeTime++;
			if (executeTime < loopTime)
			{
				curState = State.SetInitialTime;
			}
			else
			{
				isMissionSucceed = true;
			}
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

		private void changePos(ref Vector3 pos, float x, float y, float z)
		{
			pos = new Vector3(x, y, z);
		}
	}

	
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTA
{
	internal class missionManager: Script
	{
		List<mission> missions = new List<mission>();
		Random rand = new Random();
		missionManager()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
		}
		private void OnTick(object sender, EventArgs e)
		{
			
		}
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F7) {
				// start to do missions
				loadMissions();
				runMissionLoop();
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
				mission.load();
				reset_lm();
				var begin = DateTime.Now;
				bool finished = true;
				while (!mission.is_mission_finished())
				{
					Yield();
					if (DateTime.Now.Subtract(begin).TotalSeconds > 30)
					{
						// mission failed
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
			}
		}
		private void loadMissions()
		{
			missions.Add(InstantiateScript<mission01>());
		}
	}
}

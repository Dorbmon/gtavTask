using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GTA
{
	internal class missionManager: Script
	{
		List<mission> missions = new List<mission>();
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
			}
		}
		private void loadMissions()
		{
			missions.Add(InstantiateScript<mission01>())
		}
	}
}

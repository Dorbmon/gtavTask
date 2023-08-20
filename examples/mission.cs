using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA
{
	internal class mission: Script
	{
		public virtual void load()
		{


		}
		public virtual void destroy()
		{

		}
		public mission()
		{
			Tick += OnTick;
		}
		public virtual bool is_mission_finished()
		{
			return false;
		}
		public virtual String mission_actions()
		{
			return "";
		}
		public virtual String mission_task()
		{
			return "";
		}
		public virtual  void OnTick(object sender, EventArgs e)
		{

		}
	}
}

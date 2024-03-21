using System;
using System.Collections.Generic;
using System.IO;
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
		public virtual void CreateFile()
		{
			string className = GetType().Name;
			string baseFolderPath = @"D:\GTA\Missions\";
			string folderPath = Path.Combine(baseFolderPath, className);

			Directory.CreateDirectory(folderPath);
			string filePath = Path.Combine(folderPath, "mission_info.txt");

			string content = GetMissionInfo();
			File.WriteAllText(filePath, content);
		}

		protected virtual string GetMissionInfo()
		{
			return "Default Mission Information";
		}
	}
}

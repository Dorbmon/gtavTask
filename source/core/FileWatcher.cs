using System;
using System.IO;
using System.Windows.Forms;
namespace SHVDN
{
	public sealed class FileWatcher : MarshalByRefObject
	{
		private FileSystemWatcher watcher;
		private SHVDN.Console console;
		private DateTime lastFileWriteTime = DateTime.MinValue;
		private int lastCommandCount = 0;
		private bool isOpen = false;
		private string filePath = @"c";

		public FileWatcher()
		{
			watcher = new FileSystemWatcher();
			watcher.Path = @"C:\Program Files\Epic Games\GTAV\command";
			watcher.Filter = "player_action_command.txt";

			watcher.Changed += OnFileChanged;
			watcher.Created += OnFileChanged;
			watcher.Deleted += OnFileChanged;
			watcher.Renamed += OnFileRenamed;

			watcher.EnableRaisingEvents = false;
			isOpen = true;
		}

		public void startWatch()
		{
			watcher.EnableRaisingEvents = true;
		}

		public void endWatch()
		{
			watcher.EnableRaisingEvents = false;
		}

		private void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(e.FullPath);
				DateTime currentWriteTime = fileInfo.LastWriteTime;
				int executedCommandCount = 0;
				// check if file has changed
				if (currentWriteTime > lastFileWriteTime)
				{
					string[] lines = File.ReadAllLines(e.FullPath);
					int currentCommandCount = lines.Length;

					// only execute new command
					for (int i = lastCommandCount; i < currentCommandCount; i++)
					{
						// 执行命令
						console.ExecuteCommandString(lines[i]);
						executedCommandCount++;
					}

					// update file state
					lastFileWriteTime = currentWriteTime;
					lastCommandCount = currentCommandCount;
				}
				string logMessage = string.Format("OnFileChanged - LastWriteTime: {0}, CurrentWriteTime: {1}, ExecutedCommands: {2}",
										  lastFileWriteTime.ToString(), currentWriteTime.ToString(), executedCommandCount);
				Log.Message(Log.Level.Info, logMessage);
			}
			catch (Exception ex)
			{
				Log.Message(Log.Level.Error, "Error processing file change:", ex.ToString());
				//console.WriteLine($"Error processing file change: {ex.Message}");
			}
		}

		private void OnFileRenamed(object sender, RenamedEventArgs e)
		{
			// handle rename file
			Log.Message(Log.Level.Error, "Error processing file rename");
		}

		public void  SetConsole(SHVDN.Console console)
		{
			this.console = console;
		}

		internal void DoKeyEvent(Keys keys, bool status)
		{
			if (!status || !isOpen)
			{
				return; // Only interested in key down events and do not need to handle events when the console is not open
			}
			var e = new KeyEventArgs(keys);
			switch (e.KeyCode)
			{
				case Keys.F5:
					startWatch();
					Log.Message(Log.Level.Info, "FileWatcher: F5 is pressed, start watch.");
					break;
				case Keys.F6:
					endWatch();
					Log.Message(Log.Level.Info, "FileWatcher: F6 is pressed, stop watch.");
					break;
				
				default:
					//var buf = new StringBuilder(256);
					//byte[] keyboardState = new byte[256];
					//keyboardState[(int)Keys.Menu] = e.Alt ? (byte)0xff : (byte)0;
					//keyboardState[(int)Keys.ShiftKey] = e.Shift ? (byte)0xff : (byte)0;
					//keyboardState[(int)Keys.ControlKey] = e.Control ? (byte)0xff : (byte)0;

					// Translate key event to character for text input
					//ToUnicode((uint)e.KeyCode, 0, keyboardState, buf, 256, 0);
					//AddToInput(buf.ToString());
					break;
			}
		}
	}
}

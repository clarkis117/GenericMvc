using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Services
{
	public enum TransferType { NewFolder, MoveFolder}

	public struct TransferRequest
	{
		public string SourcePath;

		public string DestPath;

		public TransferType type;
	}

	public class FileTransferService
	{
		private static ObservableCollection<TransferRequest> Requests = new ObservableCollection<TransferRequest>();

		private static bool IsTransferInProgress = false;

		public FileTransferService()
		{
			Requests.CollectionChanged += RequestExecutionFilter;
			
		}

		public void transferCompletedEvent();

		/// <summary>
		/// Filter for executing request
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static async void RequestExecutionFilter(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			//eval if anything is executing
			if (FileTransferService.IsTransferInProgress == false)
			{
				if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
				{
					TransferRequest request;

					try
					{
						request = Requests.Take(1).Single();
					}
					catch (Exception)
					{
						return;
					}

					if (request.type == TransferType.NewFolder)
					{
						await Task.Run(() => CopyFolder.CopyDirectory(request.SourcePath, request.DestPath));
					}
					else if (request.type == TransferType.MoveFolder)
					{
						 Task.Run(() => CopyFolder.MoveDirectory(request.SourcePath, request.DestPath));
					}
					else
					{
						return;
					}
				}
				else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
				{
					throw new Exception("List cannot be replaced");
				}
			}
			//else do nothing
		}

		/// <summary>
		/// To do validate request here
		/// Filter for weather request should be added to request pool
		/// </summary>
		/// <param name="request"></param>
		public void RequestFileTransfer(TransferRequest request)
		{
			FileTransferService.Requests.Add(request);
		}


		public static class CopyFolder
		{
			public static void CopyDirectory(string source, string target)
			{
				var stack = new Stack<Folders>();
				stack.Push(new Folders(source, target));

				while (stack.Count > 0)
				{
					var folders = stack.Pop();
					Directory.CreateDirectory(folders.Target);
					foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
					{
						string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
						if (File.Exists(targetFile)) File.Delete(targetFile);
						File.Copy(file, targetFile);
					}

					foreach (var folder in Directory.GetDirectories(folders.Source))
					{
						stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
					}
				}
			}

			public static void MoveDirectory(string source, string target)
			{
				var stack = new Stack<Folders>();
				stack.Push(new Folders(source, target));

				while (stack.Count > 0)
				{
					var folders = stack.Pop();
					Directory.CreateDirectory(folders.Target);
					foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
					{
						string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
						if (File.Exists(targetFile)) File.Delete(targetFile);
						File.Move(file, targetFile);
					}

					foreach (var folder in Directory.GetDirectories(folders.Source))
					{
						stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
					}
				}
				Directory.Delete(source, true);
			}
			public class Folders
			{
				public string Source { get; private set; }
				public string Target { get; private set; }

				public Folders(string source, string target)
				{
					Source = source;
					Target = target;
				}
			}
		}
	}
}

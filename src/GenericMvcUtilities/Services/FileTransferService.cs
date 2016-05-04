using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Services
{
	public enum TransferType { NewCopyFolder, MoveFolder }

	public struct TransferRequest
	{
		public string SourcePath;

		public string DestinationPath;

		public TransferType Type;

		public TransferRequest(string sourcePath, string destinationPath, TransferType type)
		{
			if (Directory.Exists(sourcePath))//&& Directory.Exists(destPath)
			{
				SourcePath = sourcePath;

				DestinationPath = destinationPath;
			}
			else
			{
				throw new DirectoryNotFoundException(sourcePath + " " + destinationPath);
			}

			Type = type;
		}
	}

	public class FileTransferService
	{
		private static ObservableCollection<TransferRequest> Requests = new ObservableCollection<TransferRequest>();

		private static bool IsTransferInProgress = false;

		public FileTransferService()
		{
			Requests.CollectionChanged += CollectionRequestExecutionFilter;
		}

<<<<<<< HEAD
		public void transferCompletedEvent()
		{

		}
=======
>>>>>>> 284072fbb6b3511b4e13a3b2744ab37f707c96c0

		public static async Task RequestExecutionFilter()
		{
			if (IsTransferInProgress == false)
			{
				TransferRequest request;


				if (Requests.Count > 0)
				{
					request = Requests.Take(1).Single();
				}
				else
				{
					return;
				}

				try
				{
					if (request.Type == TransferType.NewCopyFolder)
					{
						await Task.Run(() => CopyDirectory(request));
					}
					else if (request.Type == TransferType.MoveFolder)
					{
						await Task.Run(() => MoveDirectory(request));
					}
					else
					{
						return;
					}
				}
				catch (Exception)
				{
					return;
				}
			}
		}

		/// <summary>
		/// Filter for executing request
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static async void CollectionRequestExecutionFilter(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
			{
				await RequestExecutionFilter();
			}
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
			{
				throw new Exception("Illegal operation, List cannot be replaced");
			}
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

		public static void CopyDirectory(TransferRequest request)
		{
			if (IsTransferInProgress == false)
			{
				try
				{
					IsTransferInProgress = true;

					var stack = new Stack<Folders>();
					stack.Push(new Folders(request.SourcePath, request.DestinationPath));

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
				finally
				{
					IsTransferInProgress = false;

					Requests.Remove(request);
				}
			}
		}

		public static void MoveDirectory(TransferRequest request)
		{
			if (IsTransferInProgress == false)
			{
				try
				{
					IsTransferInProgress = true;

					var stack = new Stack<Folders>();
					stack.Push(new Folders(request.SourcePath, request.DestinationPath));

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
					Directory.Delete(request.SourcePath, true);
				}
				finally
				{
					IsTransferInProgress = false;

					Requests.Remove(request);
				}
			}
		}

		public struct Folders
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
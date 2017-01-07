using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenericMvc.Services
{
	public enum TransferType : byte { NewCopyFolder, MoveFolder }

	public struct TransferRequest
	{
		public readonly string SourcePath;

		public readonly bool DeleteDestIfExists;

		public readonly string DestinationPath;

		public readonly TransferType Type;

		public TransferRequest(string sourcePath, string destinationPath, TransferType type, bool deleteDestIfExists = false)
		{
			SourcePath = sourcePath;

			DestinationPath = destinationPath;

			DeleteDestIfExists = deleteDestIfExists;

			Type = type;
		}
	}

	public struct Folders
	{
		public readonly string Source;

		public readonly string Target;

		public Folders(string source, string target)
		{
			Source = source;
			Target = target;
		}
	}

	public class FileTransferService
	{
		private readonly ObservableCollection<TransferRequest> Requests = new ObservableCollection<TransferRequest>();

		private bool IsTransferInProgress = false;

		public FileTransferService()
		{
			Requests.CollectionChanged += CollectionRequestExecutionFilter;
		}

		//todo is needed?
		public void transferCompletedEvent()
		{
		}

		public async Task RequestExecutionFilter()
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
		private async void CollectionRequestExecutionFilter(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
			Requests.Add(request);
		}

		public void CopyDirectory(TransferRequest request)
		{
			if (!IsTransferInProgress)
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

		public void MoveDirectory(TransferRequest request)
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
	}
}
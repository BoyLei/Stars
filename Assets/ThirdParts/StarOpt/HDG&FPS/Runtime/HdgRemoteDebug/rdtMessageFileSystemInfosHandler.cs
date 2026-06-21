using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameDLL.Hdg
{
    public class rdtMessageFileSystemInfosHandler
    {
        private RemoteDebugServer m_server;

        private List<rdtTcpMessageFileSystemInfos.Gob> m_allGobs;

		private int m_rootHash;

		private string m_uploadFile;

		private int m_uploadHash;

		private static string m_tempUploadFile;

		private const string TempUploadFileKey = "TempUploadFile";


        public rdtMessageFileSystemInfosHandler(RemoteDebugServer server)
		{
			m_allGobs = new List<rdtTcpMessageFileSystemInfos.Gob>(2048);
			m_server = server;
            m_server.AddCallback(typeof(rdtTcpMessageGetFileSystemInfos), OnRequestFileSystemInfos);
            m_server.AddCallback(typeof(rdtTcpMessageRenameFile), OnRenameFileName);
			m_server.AddCallback(typeof(rdtTcpMessageCreateDirectory), OnCreateDirectory);
			m_server.AddCallback(typeof(rdtTcpMessageDeleteFiles), OnDeleteFiles);
			m_server.AddCallback(typeof(rdtTcpMessageMoveFiles), OnMoveFiles);
            m_server.AddCallback(typeof(rdtTcpMessageDownloadFile), OnRequestDownloadFile);
			m_server.AddCallback(typeof(rdtTcpMessageUploadFile), OnRequestUploadFile);
			m_server.AddCallback(typeof(rdtTcpMessageTransferFile), OnRequestTramsferFile);
			m_server.onDisConnectAction += ClearUploadFile;
		}
		
		private void OnRenameFileName(rdtTcpMessage message)
		{
			rdtTcpMessageRenameFile msg = (rdtTcpMessageRenameFile)message;
			string path = FindFilePath(msg.m_hash);
			if (!string.IsNullOrEmpty(path))
			{
				if (File.Exists(path))
                {
					string newPath = Path.GetDirectoryName(path) + "\\" + msg.m_name;
					if (!File.Exists(newPath) && !Directory.Exists(newPath))
						File.Move(path, newPath);
					else
						rdtDebug.Error(this, "Can not rename file {0}, file or directory {1} already exist", path, newPath);
				}
				else if(Directory.Exists(path))
				{
					string newPath = Path.GetDirectoryName(path) + "\\" + msg.m_name;
					if (!File.Exists(newPath) && !Directory.Exists(newPath))
						Directory.Move(path, newPath);
					else
						rdtDebug.Error(this, "Can not rename directory {0}, file or directory {1} already exist", path, newPath);
				}
				else
					rdtDebug.Error(this, "Can not find file name = {0}", path);
			}
			else
				rdtDebug.Error(this, "Can not find file hash = {0}", msg.m_hash);
		}

		private void OnCreateDirectory(rdtTcpMessage message)
		{
			rdtTcpMessageCreateDirectory msg = (rdtTcpMessageCreateDirectory)message;
			string path = FindFilePath(msg.m_hash);
			if (!string.IsNullOrEmpty(path))
			{
				if (Directory.Exists(path))
                {
					path += "/" + msg.m_name;
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
				}
			}
		}

		private void OnDeleteFiles(rdtTcpMessage message)
        {
			rdtTcpMessageDeleteFiles msg = (rdtTcpMessageDeleteFiles)message;
			string[] paths = FindFilesPath(msg.m_hashs);
			for (int i = 0; i < paths.Length; i++)
			{
				string path = paths[i];
				if (File.Exists(path))
					File.Delete(path);
				else if (Directory.Exists(path))
					Directory.Delete(path, true);
				else
					rdtDebug.Error(this, "Can not find file name {0}", path);
			}
		}

		private void OnMoveFiles(rdtTcpMessage message)
        {
			rdtTcpMessageMoveFiles msg = (rdtTcpMessageMoveFiles)message;
			List<int> hashs = new List<int>();
			hashs.Add(msg.m_directorHash);
			hashs.AddRange(msg.m_childrenHashs);
			string[] paths = FindFilesPath(hashs);
			string rootPath = paths[0];
			DirectoryInfo directoryInfo = new DirectoryInfo(rootPath);
			if (directoryInfo.Exists)
			{
				for (int i = 1; i < paths.Length; i++)
				{
					string path = paths[i];
					if (File.Exists(path))
					{
						FileInfo file = new FileInfo(path);
						File.Move(path, rootPath + "/" + file.Name);
					}
					else if(Directory.Exists(path))
                    {
						DirectoryInfo directory = new DirectoryInfo(path);
						Directory.Move(path, rootPath + "/" + directory.Name);
					}
				}
			}
		}

		private void OnRequestDownloadFile(rdtTcpMessage message)
        {
			rdtTcpMessageDownloadFile msg = (rdtTcpMessageDownloadFile)message;
			string path = FindFilePath(msg.m_hash);
			if (!string.IsNullOrEmpty(path))
			{
				if (File.Exists(path))
					rdtTcpMessageTransferFile.StartTransferFile(m_server, path, msg.m_hash);
				else
					rdtDebug.Error(this, "Can not find file name {0}", path);
			}
			else
				rdtDebug.Error(this, "Can not find file hash {0}", msg.m_hash);
		}

		
		private void OnRequestUploadFile(rdtTcpMessage message)
		{
			m_tempUploadFile = PlayerPrefs.GetString(TempUploadFileKey);
			if (!string.IsNullOrEmpty(m_tempUploadFile)) 
			{
				if (File.Exists(m_tempUploadFile))
					File.Delete(m_tempUploadFile);
				m_tempUploadFile = null;
			}
            if (!string.IsNullOrEmpty(m_uploadFile) && m_uploadHash != 0)
            {
                rdtDebug.Error(this, "Now uploading file {0}", m_uploadFile);
                return;
            }	
			rdtTcpMessageUploadFile msg = (rdtTcpMessageUploadFile)message;
			string path = FindFilePath(msg.m_hash);
			if (!string.IsNullOrEmpty(path))
			{
				if (Directory.Exists(path))
				{
					m_tempUploadFile=(path+"/"+GetTempFileName(msg.m_name)).Replace('\\','/');
					PlayerPrefs.SetString(TempUploadFileKey, m_tempUploadFile);
                    m_uploadFile = (path + "/" + msg.m_name).Replace('\\', '/');
					if ((File.Exists(m_uploadFile) || Directory.Exists(m_uploadFile)) && !msg.m_overwrite)
					{
						m_uploadFile = null;
						m_tempUploadFile = null;
						m_uploadHash = 0;
						rdtDebug.Error(this, "File {0}/{1} already exist", path, msg.m_name);
					}
					else
					{
                        m_uploadHash = m_tempUploadFile.GetHashCode();
                        FileInfo file = new FileInfo(m_tempUploadFile);
                        if (!file.Directory.Exists)
							file.Directory.Create();
					}
					rdtTcpMessageDownloadFile m = default(rdtTcpMessageDownloadFile);
					m.m_hash = m_uploadHash;
					m_server.EnqueueMessage(m);
				}
				else
					rdtDebug.Error(this, "Can not find firectory name {0}", path);
			}
			else
				rdtDebug.Error(this, "Can not find firectory hash {0}", msg.m_hash);  
		}

		private void OnRequestTramsferFile(rdtTcpMessage message)
        {
			rdtTcpMessageTransferFile msg = (rdtTcpMessageTransferFile)message;
            if (!string.IsNullOrEmpty(m_tempUploadFile) && m_uploadHash == msg.m_hash)
            {
                using (FileStream fileStream = new FileStream(m_tempUploadFile, msg.m_offset == 0 ? FileMode.Create : FileMode.Append))
                {
					fileStream.Position = msg.m_offset;
					fileStream.Write(msg.m_bytes, 0, msg.m_length);
					fileStream.Flush();
					if (fileStream.Length == msg.m_fileLength)
					{
						if (File.Exists(m_uploadFile))
							File.Delete(m_uploadFile);
						fileStream.Close();
						File.Move(m_tempUploadFile, m_uploadFile);
						m_uploadFile = null;
						m_uploadHash = 0;
					}
				}
			}
		}

		private void OnRequestFileSystemInfos(rdtTcpMessage message)
		{
			rdtTcpMessageFileSystemInfos msg = default(rdtTcpMessageFileSystemInfos);
			m_allGobs.Clear();

			string fullPath = Application.persistentDataPath;
			DirectoryInfo directory = new DirectoryInfo(fullPath);
			m_rootHash = directory.FullName.Replace('\\', '/').GetHashCode();
			AddFileSystemInfo(directory, 0, m_allGobs);

			msg.m_allGobs = m_allGobs;
            m_server.EnqueueMessage(msg);
			//m_gameObjects.Clear();
        }

		private void AddFileInfo(FileInfo file, int parentHash, List<rdtTcpMessageFileSystemInfos.Gob> list)
		{
			rdtTcpMessageFileSystemInfos.Gob gob = default(rdtTcpMessageFileSystemInfos.Gob);
			gob.m_name = file.Name;
			gob.m_attributes = (int)file.Attributes;
			gob.m_hash = file.FullName.Replace('\\', '/').GetHashCode();
			gob.m_parentHash = parentHash;
			list.Add(gob);
		}

		private void AddFileSystemInfo(DirectoryInfo directory, int parentHash, List<rdtTcpMessageFileSystemInfos.Gob> list)
		{
			string fullpath = directory.FullName;
			rdtTcpMessageFileSystemInfos.Gob gob = default(rdtTcpMessageFileSystemInfos.Gob);
			gob.m_name = directory.Name;
			gob.m_attributes = (int)directory.Attributes;
			gob.m_hash = fullpath.Replace('\\', '/').GetHashCode();
			gob.m_parentHash = parentHash;
			list.Add(gob);
			string[] directories = Directory.GetDirectories(fullpath);
			for (int i = 0; i < directories.Length; i++)
				AddFileSystemInfo(new DirectoryInfo(directories[i]), gob.m_hash, list);
			string[] files = Directory.GetFiles(fullpath);
			for (int i = 0; i < files.Length; i++)
				AddFileInfo(new FileInfo(files[i]), gob.m_hash, list);
		}

		public string FindFilePath(int hash)
		{
			string rootPath = Application.persistentDataPath;
			if (m_rootHash == hash)
				return rootPath;
			string[] paths = Directory.GetFileSystemEntries(rootPath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < paths.Length; i++)
			{
				if (paths[i].Replace('\\', '/').GetHashCode() == hash)
				{
                    return paths[i];
                }
			}
			return null;
		}

		public string[] FindFilesPath(List<int> hashs)
        {
			List<string> p = new List<string>();
			string rootPath = Application.persistentDataPath;
			if (hashs.Contains(m_rootHash))
            {
				hashs.Remove(m_rootHash);
				p.Add(rootPath);
			}
			string[] paths = Directory.GetFileSystemEntries(rootPath, "*.*", SearchOption.AllDirectories);
			for (int i = 0; i < paths.Length; i++)
			{
				if (hashs.Count == 0)
					break;
				int hash = paths[i].Replace('\\', '/').GetHashCode();
				if (hashs.Contains(hash))
				{
					p.Add(paths[i]);
					hashs.Remove(hash);
				}
			}
			return p.ToArray(); ;
		}

        private string GetTempFileName(string sourceFile)
        {
			FileInfo fileInfo = new FileInfo(sourceFile);
			string tempFileName=sourceFile.Remove(sourceFile.LastIndexOf('/') + 1)+fileInfo.GetHashCode()+".temp";
            return tempFileName;
        }

		private void ClearUploadFile()
		{
			m_uploadFile = null;
			m_uploadHash = 0;
		    m_rootHash = 0;
		}
    } 
}

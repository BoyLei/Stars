using GameDLL.Hdg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace GameEditor.Hdg
{
    public partial class ConnectionWindow
    {

        private GUIContent m_fileNameContent;

        private string m_fileName;

        private GUIContent m_fileRenameContent;

        private GUIContent m_fileDirectoryContent;

        private string m_fileDirectory;

        private string m_createDirectory;

        private GUIContent m_fileCreateDirectoryContent;

        private GUIContent m_fileDownloadContent;

        private GUIContent m_fileUploadContent;

        private GUIContent m_uploadLuaContent;

        private GUIContent m_clearLuasContent;

        private bool m_overwriteFile = true;

        private GUIContent m_overwriteFileContent;

        private Vector2 m_fileSystemInfoScrollPos;

        private bool m_forceRefreshFile;

        private double m_fileRefreshTimer;

        [NonSerialized]
        private bool m_waitingForFiles;

        private string m_progressFile;

        private int m_progressHash;

        private float m_progressValue;

        private rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob> m_filesTree;


        [NonSerialized]
        private rdtTcpMessageFileSystemInfos.Gob? m_fileSystemInfo;

        [NonSerialized]
        private List<rdtTcpMessageFileSystemInfos.Gob> m_files;

        private Texture2D m_normalDirectoryIcon;

        private Texture2D m_emptyDirectoryIcon;

        private Texture2D m_opendDirectoryIcon;

        private Texture2D m_fileIcon;

        private bool transferFileOver;

        private Queue<WaitForUploadFile> waitForUploadFiles;
    
        struct WaitForUploadFile
        {
            rdtTcpMessageUploadFile rdtTcpMessage;
            string filePath;
            public WaitForUploadFile(rdtTcpMessageUploadFile rdtTcpMessage, string filePath)
            {
                this.rdtTcpMessage = rdtTcpMessage;
                this.filePath = filePath;
            }
            public rdtTcpMessageUploadFile RdtTcpMessage { get => rdtTcpMessage; set => rdtTcpMessage = value; }
            public string FilePath { get => filePath; set => filePath = value; }
        }
        private void ConnectFiles()
        {
            m_fileSystemInfo = null;
            waitForUploadFiles = new Queue<WaitForUploadFile>();
            m_client.AddCallback(typeof(rdtTcpMessageFileSystemInfos), OnMessageFileSystemInfos);
            m_client.AddCallback(typeof(rdtTcpMessageDownloadFile), OnMessageDownloadFile);
            m_client.AddCallback(typeof(rdtTcpMessageTransferFile), OnMessageTramsferFile);
        }

        private void CheckUnSendMessage()
        {
           if (waitForUploadFiles.Count>0)
            {
                WaitForUploadFile waitUploadFile = waitForUploadFiles.Dequeue();
                m_progressFile = waitUploadFile.FilePath;
                m_client.EnqueueMessage(waitUploadFile.RdtTcpMessage);
            }
        }
        private void DisconnectFiles()
        {
            m_filesTree.Clear();

            m_waitingForFiles = false;

            m_progressFile = null;
            m_progressHash = 0;
            m_progressValue = 0;
            m_forceRepaint = false;
           
        }

        private void ConnectionStatusChangedFiles()
        {
            m_fileSystemInfo = null;
            m_filesTree.Clear();

        }

        private void OnEnableFiles()
        {
            m_filesTree = new rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>(PAGE_TYPE_FILES);
            m_filesTree.SelectedNodesChanged += OnFilesTreeSelectionChanged;
            m_filesTree.SelectedNodesDeleted += OnFilesTreeSelectionDeleted;
            m_filesTree.DrapNodesEnded += OnFilesTreeDragEnded;
            m_filesTree.CheckDrop += OnFilesTreeCheckDrop;
            m_filesTree.CheckAndUpdateDrop += OnFilesTreeCheckAndUpdateDrop;
        }

        private void OnDisableFiles()
        {

        }

        private void InitFilesStylesAndContent()
        {
            m_fileNameContent = EditorGUIUtility.TrTextContent("Name");
            m_fileRenameContent = EditorGUIUtility.TrTextContent("Rename");
            m_fileDirectoryContent = EditorGUIUtility.TrTextContent("Directory");
            m_fileCreateDirectoryContent = EditorGUIUtility.TrTextContent("Create");
            m_fileDownloadContent = EditorGUIUtility.TrTextContent("Download");
            m_fileUploadContent = EditorGUIUtility.TrTextContent("Upload");
            m_overwriteFileContent = EditorGUIUtility.TrTextContent("Overwrite");
            m_uploadLuaContent = EditorGUIUtility.TrTextContent("Upload Lua");
            m_clearLuasContent = EditorGUIUtility.TrTextContent("Clear Lua(s)");
        }

        private void UpdateFiles(double delta)
        {
            if (m_fileRefreshTimer > 0)
            {
                m_fileRefreshTimer -= delta;
                if (m_fileRefreshTimer <= 0.0)
                {
                    m_forceRefreshFile = false;
                    RefreshFiles();
                    m_fileRefreshTimer = 0.0;
                }
            }
        }

        private void DrawFiles(bool windowHasFocus)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.Width(m_split.SeparatorPosition));
            m_filesTree.Draw(rect, windowHasFocus);
            m_split.Draw();
            m_fileSystemInfoScrollPos = EditorGUILayout.BeginScrollView(m_fileSystemInfoScrollPos);
            float inspectorWidth = position.width - m_split.SeparatorPosition;
            //EditorGUIUtility.wideMode = (inspectorWidth >= WIDE_MODE_SIZE_THRESHOLD);
            EditorGUIUtility.labelWidth = 80f;
            EditorGUIUtility.fieldWidth = 0f;
            //if (inspectorWidth > LABEL_ADJUST_SIZE_THRESHOLD)
            //    EditorGUIUtility.labelWidth = (inspectorWidth - LABEL_ADJUST_SIZE_THRESHOLD) * 0.5f + EditorGUIUtility.labelWidth;
            DrawFileSystemInfo();
            EditorGUILayout.EndScrollView();
        }

        private void DrawFileSystemInfo()
        {
            if (m_fileSystemInfo == null)
                return;
            if (m_fileSystemInfo.Value.m_hash == 0)
            {
                EditorGUILayout.LabelField("File was not found.");
                return;
            }

            DrawFileSystemBaseInfo();
            rdtGuiLine.DrawHorizontalSplitLine();
            GUILayout.FlexibleSpace();
            DrawFileProgressBar();
        }
        private void DrawFileSystemBaseInfo()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(m_fileDirectoryContent);
            EditorGUILayout.SelectableLabel(m_fileDirectory, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
            GUI.enabled = m_fileSystemInfo.Value.m_parentHash != 0;
            EditorGUILayout.BeginHorizontal();
            m_fileName = EditorGUILayout.TextField(m_fileNameContent, m_fileName);
            if (GUILayout.Button(m_fileRenameContent, GUILayout.Width(80)))
            {
                if (!string.IsNullOrEmpty(m_fileName) && m_fileName != m_fileSystemInfo.Value.m_name)
                    RenameFile();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            if (m_fileSystemInfo.Value.isDirectory)
            {
                EditorGUILayout.BeginHorizontal();
                m_createDirectory = EditorGUILayout.TextField(m_createDirectory);
                if (GUILayout.Button(m_fileCreateDirectoryContent, GUILayout.Width(80)))
                {
                    CreateDirectory();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(m_overwriteFileContent, GUILayout.Width(64));
                m_overwriteFile = EditorGUILayout.Toggle(m_overwriteFile, GUILayout.Width(16));
                if (GUILayout.Button(m_fileUploadContent, GUILayout.Width(80)))
                {
                    UploadFile();
                }
                EditorGUILayout.EndHorizontal();
                if (m_fileSystemInfo.Value.m_parentHash == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    Rect uploadRect = EditorGUILayout.GetControlRect(GUILayout.Width(120));
                    if (GUI.Button(uploadRect, m_uploadLuaContent))
                    {
                        UploadLua();
                    }
                    if (GUILayout.Button(m_clearLuasContent, GUILayout.Width(120)))
                    {
                        ClearLuas();
                    }
                    EditorGUILayout.EndHorizontal();
                    if (uploadRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.DragUpdated)
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        if (Event.current.type == EventType.DragPerform)
                        {
                            UploadLuaAsset();
                            DragAndDrop.AcceptDrag();
                            Event.current.Use();
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button(m_fileDownloadContent, GUILayout.Width(80)))
                {
                    DownloadFile();
                }
                EditorGUILayout.EndHorizontal();
            }


        }

        private void DrawFileProgressBar()
        {
            if (!string.IsNullOrEmpty(m_progressFile))
            {
                Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight));
                
                EditorGUI.ProgressBar(rect, m_progressValue, string.Format("Transfer File {0:F1}%", m_progressValue * 100));
            }
        }

        private void BuildFileTree()
        {
            m_filesTree.Clear();
            rdtTcpMessageFileSystemInfos.Gob rootGob = m_files.FirstOrDefault((x) => x.m_parentHash == 0);
            if (rootGob.m_hash != 0)
            {
                if (m_normalDirectoryIcon == null)
                    m_normalDirectoryIcon = EditorGUIUtility.TrIconContent("d_Folder Icon").image as Texture2D;
                if (m_opendDirectoryIcon == null)
                    m_opendDirectoryIcon = EditorGUIUtility.TrIconContent("d_FolderOpened Icon").image as Texture2D;
                if (m_emptyDirectoryIcon == null)
                    m_emptyDirectoryIcon = EditorGUIUtility.TrIconContent("d_FolderEmpty Icon").image as Texture2D;
                if (m_fileIcon == null)
                    m_fileIcon = EditorGUIUtility.TrIconContent("d_UnityEditor.ConsoleWindow").image as Texture2D;
                List<rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node> fileRoots = new List<rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node>();

                rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node rootNode = m_filesTree.AddNode(rootGob);
                SetNode(rootNode, rootGob);
                fileRoots.Add(rootNode);
                m_files.Remove(rootGob);

                List<rdtTcpMessageFileSystemInfos.Gob> list = (from x in m_files
                                                               where x.m_parentHash == rootGob.m_hash
                                                               select x).ToList();
                Dictionary<int, rdtTcpMessageFileSystemInfos.Gob> existing = new Dictionary<int, rdtTcpMessageFileSystemInfos.Gob>();
                List<rdtTcpMessageFileSystemInfos.Gob> nonRoots = (from x in (from x in m_files
                                                                              where x.m_parentHash != rootGob.m_hash
                                                                              select x).Where(delegate (rdtTcpMessageFileSystemInfos.Gob x)
                                                                              {
                                                                                  if (existing.ContainsKey(x.m_hash))
                                                                                      return false;
                                                                                  existing.Add(x.m_hash, x);
                                                                                  return true;
                                                                              })
                                                                   orderby x.m_parentHash
                                                                   select x).ToList();
                foreach (rdtTcpMessageFileSystemInfos.Gob r in list)
                {
                    rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node root = rootNode.AddNode(r);
                    SetNode(root, r);
                    fileRoots.Add(root);
                    AddFileChildren(root, nonRoots);
                }
            }
        }

        private void AddFileChildren(rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node parentNode, List<rdtTcpMessageFileSystemInfos.Gob> nonRoots)
        {
            int firstChildIndex = 0;
            while (firstChildIndex < nonRoots.Count && nonRoots[firstChildIndex].m_parentHash != parentNode.Data.m_hash)
                firstChildIndex++;
            if (firstChildIndex >= nonRoots.Count || nonRoots.Count == 0)
                return;
            List<rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node> children = new List<rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node>();
            int count = 0;
            int i = firstChildIndex;
            while (i < nonRoots.Count)
            {
                rdtTcpMessageFileSystemInfos.Gob g = nonRoots[i];
                if (g.m_parentHash != parentNode.Data.m_hash)
                    break;
                rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node node = parentNode.AddNode(g, true);
                SetNode(node, g);
                children.Add(node);
                i++;
                count++;
            }
            nonRoots.RemoveRange(firstChildIndex, count);
            for (int j = 0; j < children.Count; j++)
            {
                rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node node2 = children[j];
                AddFileChildren(node2, nonRoots);
            }
        }

        void SetNode(rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node node, rdtTcpMessageFileSystemInfos.Gob data)
        {
            if (data.isDirectory)
            {
                node.NormalIcon = m_normalDirectoryIcon;
                node.ExpandedIcon = m_opendDirectoryIcon;
                node.EmptyIcon = m_emptyDirectoryIcon;
                node.IsBold = true;
            }
            else
                node.NormalIcon = m_fileIcon;
        }

        private void OnMessageFileSystemInfos(rdtTcpMessage message)
        {
            m_waitingForFiles = false;
            rdtTcpMessageFileSystemInfos msg = (rdtTcpMessageFileSystemInfos)message;
            m_files = msg.m_allGobs;
            m_updatingTree = true;
            List<rdtTcpMessageFileSystemInfos.Gob> selectionData = (from x in m_filesTree.SelectedNodes
                                                                where x.HasData
                                                                select x.Data).ToList();
            List<string> selectionNoData = (from x in m_filesTree.SelectedNodes
                                            where !x.HasData
                                            select x.Name).ToList();
            BuildFileTree();
            if (selectionData.Count > 0 || selectionNoData.Count > 0)
            {
                List<rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node> selectionNodesData = (from x in selectionData
                                                                                          select m_filesTree.FindNode(x) into x
                                                                                          where x != null
                                                                                          select x).ToList();
                List<rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node> selectionNodesNoData = (from x in selectionNoData
                                                                                            select m_filesTree.FindNode(x) into x
                                                                                            where x != null
                                                                                            select x).ToList();
                m_filesTree.SelectedNodes.AddRange(selectionNodesData);
                m_filesTree.SelectedNodes.AddRange(selectionNodesNoData);
            }
            m_updatingTree = false;
            Repaint();
        }

        private void OnMessageDownloadFile(rdtTcpMessage message)
        {
            rdtTcpMessageDownloadFile msg = (rdtTcpMessageDownloadFile)message;
            if (msg.m_hash == 0)
            {
                m_progressFile = null;
                m_progressHash = 0;
                m_progressValue = 0;
                m_forceRepaint = false;
            }
            else
            {
                m_progressHash = msg.m_hash;
               
                int result = rdtTcpMessageTransferFile.StartTransferFile(m_client, m_progressFile, m_progressHash, (f) =>
                {
                    if (f < 0)
                    {
                        CheckUnSendMessage();
                        return;
                    }
                    m_progressValue = f;
                    m_forceRepaint = true;
                    if (m_progressValue >= 1f)
                    {
                        m_progressFile = null;
                        m_progressHash = 0;
                        m_progressValue = 1f;
                        m_fileRefreshTimer = 0.10000000149011612;//=0.1f
                        m_forceRefreshFile = true; 
                    }
                });
                if (result != 0)
                {
                    m_progressFile = null;
                    m_progressHash = 0;
                    m_progressValue = 0;
                    transferFileOver = true;
                    if(result == 1)
                    {
                        m_fileRefreshTimer = 0.10000000149011612;//=0.1f
                        m_forceRefreshFile = true;
                        CheckUnSendMessage();
                    }
                    else
                        m_forceRepaint = true;
                }
            }
        }

        private void OnMessageTramsferFile(rdtTcpMessage message)
        {
            rdtTcpMessageTransferFile msg = (rdtTcpMessageTransferFile)message;
            if (!string.IsNullOrEmpty(m_progressFile) && m_progressHash == msg.m_hash)
            {
                using (FileStream fileStream = new FileStream(m_progressFile, msg.m_offset == 0 ? FileMode.Create : FileMode.Append))
                {
                    fileStream.Position = msg.m_offset;
                    fileStream.Write(msg.m_bytes, 0, msg.m_length);
                    fileStream.Flush();
                    m_progressValue = (float)(msg.m_offset + msg.m_length) / msg.m_fileLength;
                    Repaint();
                    if (fileStream.Length == msg.m_fileLength)
                    {
                        m_progressFile = null;
                        m_progressHash = 0;
                        m_progressValue = 1f;
                        rdtDebug.Debug(this, "Tramsfer file finish");
                    }
                }
            }
        }

        private void OnFilesTreeSelectionChanged()
        {
            if (m_updatingTree)
                return;
            m_clearFocus = true;
            rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node selected = m_filesTree.SelectedNodes.FirstOrDefault((x) => x.GetHashCode() != 0);
            if (selected != null)
            {
                if (!selected.Data.Equals(m_fileSystemInfo))
                {
                    m_fileSystemInfo = selected.Data;
                    m_fileName = m_fileSystemInfo.Value.m_name;
                    m_fileDirectory = "";
                    do
                    {
                        m_fileDirectory = selected.Parent.Name + "/" + m_fileDirectory;
                        selected = selected.Parent;
                    } while (selected.Parent != null && selected.Parent.ID != 0);
                    m_createDirectory = "";
                }
            }
            else
            {
                m_fileSystemInfo = null;
            }
            Repaint();
        }

        private void OnFilesTreeSelectionDeleted()
        {
            rdtTcpMessageDeleteFiles msg = default(rdtTcpMessageDeleteFiles);
            IEnumerable<int> selectedHashs = from x in m_filesTree.SelectedNodes
                                           select x.Data.m_hash;
            msg.m_hashs = selectedHashs.ToList<int>();
            m_client.EnqueueMessage(msg);
            m_filesTree.SelectedNodes.Clear();
            m_fileRefreshTimer = 0.10000000149011612;//=0.1f
            m_forceRefreshFile = true;
        }

        private void OnFilesTreeDragEnded(int parentId, int startIndex, List<int> childrenIds)
        {
            rdtTcpMessageMoveFiles msg = default(rdtTcpMessageMoveFiles);
            msg.m_directorHash = parentId;
            msg.m_childrenHashs = childrenIds;
            m_client.EnqueueMessage(msg);
            m_fileRefreshTimer = 0.10000000149011612;//=0.1f
            m_forceRefreshFile = true;
        }

        private bool OnFilesTreeCheckDrop(rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node dropTarget, rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.HierarchyDropMode dropMode)
        {
            if (dropTarget.CanBeParent)
            {
                if (!dropMode.HasFlag(rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.HierarchyDropMode.kHierarchyDropUpon))
                    return true;
                return false;
            }
            else
                return true;
        }

        private void OnFilesTreeCheckAndUpdateDrop(rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.DropResult result, List<rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node> selection)
        {
            List<rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node> selected = selection.FindAll((node) => node != null && node.Parent != result.parent && !result.parent.IsSelfOrChild(node));
            for (int i = 0; i < selected.Count; i++)
            {
                rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node node = selected[i];
                int idx = 0;
                for (; idx < selected.Count; idx++)
                {
                    rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node n = selected[idx];
                    if (n != node)
                    {
                        if (node.IsSelfOrChild(n))
                            break;
                    }
                }
                if (idx == selected.Count)
                    result.children.Add(node);
            }
        }    

        private void RenameFile()
        {
            if (m_client == null || m_fileSystemInfo == null || m_fileSystemInfo.Value.m_hash == 0)
                return;
            rdtDebug.Debug(this, "Rename from the server");
            rdtTcpMessageRenameFile msg = default(rdtTcpMessageRenameFile);
            msg.m_hash = m_fileSystemInfo.Value.m_hash;
            msg.m_name = m_fileName;
            m_client.EnqueueMessage(msg);
            m_fileRefreshTimer = 0.10000000149011612;//=0.1f
            m_forceRefreshFile = true;
        }

        private void CreateDirectory()
        {
            if (m_client == null || m_fileSystemInfo == null || !m_fileSystemInfo.Value.isDirectory)
                return;
            rdtDebug.Debug(this, "Create directory from the server");
            if (!string.IsNullOrEmpty(m_createDirectory))
            {
                rdtTcpMessageCreateDirectory msg = default(rdtTcpMessageCreateDirectory);
                msg.m_hash = m_fileSystemInfo.Value.m_hash;
                msg.m_name = m_createDirectory;
                m_client.EnqueueMessage(msg);
                m_fileRefreshTimer = 0.10000000149011612;//=0.1f
                m_forceRefreshFile = true;
            }
        }

        private void DownloadFile()
        {
            if (m_client == null || m_fileSystemInfo == null || m_fileSystemInfo.Value.m_hash == 0)
                return;
            rdtDebug.Debug(this, "Download from the server");
            m_progressFile = EditorUtility.SaveFilePanel("Download File", "", m_fileName, "");
            if (!string.IsNullOrEmpty(m_progressFile))
            {
                m_progressHash = m_fileSystemInfo.Value.m_hash;
                m_progressValue = 0f;
                rdtTcpMessageDownloadFile msg = default(rdtTcpMessageDownloadFile);
                msg.m_hash = m_progressHash;
                m_client.EnqueueMessage(msg);
            }
            else
                m_progressHash = 0;
        }

        private void UploadFile()
        {
            if (m_client == null || m_fileSystemInfo == null || m_fileSystemInfo.Value.m_hash == 0)
                return;
            string temp_progressFile =EditorUtility.OpenFilePanel("Upload File", "", "");
            if (!string.IsNullOrEmpty(temp_progressFile))
            {
                FileInfo file = new FileInfo(temp_progressFile);
                UploadFile(file.Name,temp_progressFile, m_fileSystemInfo.Value.m_hash, (int)file.Length);
            }
            else
                m_progressHash = 0;
        }

        private void UploadFile(string name,string sourceFilePath, int hash, int lenth)
        {
            rdtDebug.Debug(this, "Upload from the server");
            if (waitForUploadFiles == null)
                waitForUploadFiles = new Queue<WaitForUploadFile>();
            m_progressHash = hash;
            m_progressValue = 0f;
            rdtTcpMessageUploadFile msg = default(rdtTcpMessageUploadFile);
            msg.m_hash = hash;
            msg.m_name = name;
            msg.m_fileLength = lenth;
            msg.m_overwrite = m_overwriteFile;
            waitForUploadFiles.Enqueue(new WaitForUploadFile(msg, sourceFilePath));
            if (m_progressFile == null)
                CheckUnSendMessage();
        }
        private void UploadLua()
        {
            if (m_client == null || m_fileSystemInfo == null || m_fileSystemInfo.Value.m_hash == 0)
                return;
            string filePath = EditorUtility.OpenFilePanelWithFilters("Upload File", Application.dataPath + "/Assets/Scripts/Lua", new string[]{ "Lua", "lua"});
            if (!string.IsNullOrEmpty(filePath))
                UploadLua(filePath);
            else
                m_progressHash = 0;
        }

        private void UploadLuaAsset()
        {
           
            transferFileOver = true;
            if (m_client == null || m_fileSystemInfo == null || m_fileSystemInfo.Value.m_hash == 0)
                return;
            string[] filePaths = DragAndDrop.paths;
            //将多个文件的上传请求,封装,并加入待上传队列
            for (int i = 0; i < filePaths.Count(); i++)
            {
               if (filePaths[i].EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                    UploadLua(filePaths[i]);
            }
        }

        private void UploadLua(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            filePath = file.FullName.Replace('\\', '/');
            if (filePath.StartsWith(Application.dataPath.Replace('\\', '/') + "/Scripts/Lua/") && file.Extension.Equals(".lua", StringComparison.OrdinalIgnoreCase))
            {
                string temp_progressFile = file.FullName;
                int offset = (Application.dataPath + "/Scripts/").Length;
                int length = temp_progressFile.Length - offset - file.Name.Length;
                string path = temp_progressFile.Substring(offset, length).Replace('\\', '/').ToLower() + file.Name;
                UploadFile(path, temp_progressFile, m_fileSystemInfo.Value.m_hash, (int)file.Length);
            }
        }
        private void ClearLuas()
        {
            rdtGuiTree<rdtTcpMessageFileSystemInfos.Gob>.Node node = m_filesTree.GetItem(0).Children.FirstOrDefault((r) => r.Name == "lua");
            if (node != null && node.HasChildren)
            {
                if (EditorUtility.DisplayDialog("Delete Confirm", "Do you want to delete the file(s) ?", "OK", "Cancel"))
                {
                    rdtTcpMessageDeleteFiles msg = default(rdtTcpMessageDeleteFiles);
                    msg.m_hashs = new List<int>()
                    {
                        node.ID,
                    };
                    m_client.EnqueueMessage(msg);
                    m_filesTree.SelectedNodes.Clear();
                    m_fileRefreshTimer = 0.10000000149011612;//=0.1f
                    m_forceRefreshFile = true;
                }
            }
        }

        private void RefreshFiles()
        {
            if (m_client == null || !m_client.IsConnected || m_waitingForFiles)
                return;
            rdtDebug.Debug(this, "Refreshing File list from the server");
            rdtTcpMessageGetFileSystemInfos msg = default(rdtTcpMessageGetFileSystemInfos);
            m_client.EnqueueMessage(msg);
            m_waitingForFiles = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using GameDLL.Hdg;
using UnityEditor;

namespace GameEditor.Hdg
{
	public class rdtClientEnumerateServers
	{
		private object m_lock = new object();

		private List<rdtServerAddress> m_servers = new List<rdtServerAddress>();

		private UdpClient m_udpHello;

		private IPEndPoint m_endPoint;

		private bool m_alreadyInUse;

		public bool Stopped { get; private set; }

		public List<rdtServerAddress> Servers
		{
			get
			{
				List<rdtServerAddress> result;
				lock (m_lock)
				{
					result = new List<rdtServerAddress>(this.m_servers);
				}
				return result;
			}
		}

		public rdtClientEnumerateServers()
		{
			Start();
		}

		public void Stop()
		{
			try
			{
				if (m_udpHello != null)
					m_udpHello.Close();
			}
			catch (Exception e)
			{
				rdtDebug.Error(this, e, "Tried to stop the client enumerator but we got an exception");
			}
			Stopped = true;
		}

		public void Reset()
		{
			lock (m_lock)
			{
				m_servers.Clear();
			}
		}

		public void Update(double delta)
		{
			if (Stopped)
				Start();
			lock (m_lock)
			{
				List<rdtServerAddress> removeList = new List<rdtServerAddress>();
				foreach (rdtServerAddress addr in m_servers)
				{
					addr.m_timer += delta;
					if (addr.m_timer > Settings.BROADCAST_SECOND * 2f)
						removeList.Add(addr);
				}
				foreach (rdtServerAddress addr2 in removeList)
				{
					rdtDebug.Debug(this, "Removing {0} because we haven't heard from it  timestamp={1}", addr2.FormattedName, DateTime.Now.TimeOfDay.TotalSeconds.ToString());
					m_servers.Remove(addr2);
				}
			}
		}

		private void Start()
		{
			rdtDebug.Debug(this, "Starting hello listener");
			Stopped = false;
			try
			{
				int port = EditorPrefs.GetInt("Hdg.RemoteDebug.BroadcastPort", 12000);
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
				m_udpHello = new UdpClient();
				m_udpHello.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				m_udpHello.Client.Bind(endPoint);
				m_udpHello.BeginReceive(OnReceiveHelloCallback, null);
				m_alreadyInUse = false;
			}
			catch (SocketException e)
			{
				if (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
				{
					if (!m_alreadyInUse)
					{
						m_alreadyInUse = true;
						rdtDebug.Error("Remote Debug: Another instance of the server enumerator is already running!");
					}
				}
				else
				{
					rdtDebug.Error(this, e, "Exception");
				}
				Stop();
			}
			catch (Exception e2)
			{
				rdtDebug.Error(this, e2, "Exception");
				Stop();
			}
		}

		private void OnReceiveHelloCallback(IAsyncResult result)
		{
			bool restart = true;
			try
			{
				rdtUdpMessageHello i;
				using (MemoryStream ms = new MemoryStream(m_udpHello.EndReceive(result, ref m_endPoint)))
				{
					using (BinaryReader br = new BinaryReader(ms))
					{
						i = new rdtUdpMessageHello();
						i.Read(br);
					}
				}
				if (i == null)
					rdtDebug.Error(this, "Ignoring invalid message");
				else
				{
					lock (m_lock)
					{
						rdtServerAddress addr = new rdtServerAddress(m_endPoint.Address, i.m_serverPort, i.m_deviceName, i.m_deviceType, i.m_devicePlatform, i.m_serverVersion);
						int idx = m_servers.IndexOf(addr);
						if (idx >= 0)
							m_servers[idx].m_timer = 0.0;
						else
						{
							rdtDebug.Debug(this, string.Concat(new object[]
							{
								"Found a new server ",
								addr.IPAddress,
								" called ",
								addr.FormattedName,
								" timestamp=",
								DateTime.Now.TimeOfDay.TotalSeconds.ToString()
							}));
							rdtDebug.Debug(this, "Server has version {0}", addr.m_serverVersion);
							this.m_servers.Add(addr);
						}
					}
				}
			}
			catch (ObjectDisposedException)
			{
				restart = false;
				rdtDebug.Debug(this, "Hello listener disposed");
				Stop();
			}
			catch (Exception e)
			{
				rdtDebug.Error(this, "Error {0}", e.Message);
			}
			if (restart)
				m_udpHello.BeginReceive(OnReceiveHelloCallback, null);
		}
	}
}

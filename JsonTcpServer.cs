using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;
public class JsonTcpServer
{
    public event Action<InputMsg> OnMessageReceived;

    private TcpListener listener;
    private Thread listenerThread;
    private bool isRunning;
    private readonly int port;
    public bool clientRunning = false;
    public ConcurrentQueue<OutputMsg> outboundMessageQueue;
    
    public JsonTcpServer(int port, ConcurrentQueue<OutputMsg> obmq)
    {
        this.port = port;
        outboundMessageQueue = obmq;
    }

    public void Start()
    {
        if (isRunning) return;

        isRunning = true;
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        listenerThread = new Thread(ListenLoop);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    public void Update()
    {
        // Why not output the outboundMessageQueue here?
    }

    public void Stop()
    {
        isRunning = false;
        try
        {
            listener?.Stop();
        }
        catch { }
    }

    private void ListenLoop()
    {
        while (isRunning)
        {
            try
            {
                var client = listener.AcceptTcpClient();
                UnityEngine.Debug.Log("Accepted Client.");
                Thread t = new Thread(() => HandleClient(client));
                t.IsBackground = true;
                t.Start();
            }
            catch
            {
                if (!isRunning) return;
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        clientRunning = true;
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            // Start reader and write threads
            Thread read_t = new Thread(() => Reader(client, reader, () => clientRunning = false));
            read_t.IsBackground = true;
            read_t.Start();
            Thread write_t = new Thread(() => Writer(client, writer, () => clientRunning));
            write_t.IsBackground = true;
            write_t.Start();
            
            // keep client, etc. open
            UnityEngine.Debug.Log($"Threads started.");
            read_t.Join();   // reader detected disconnect
            clientRunning = false;

            client.Close();  // force writer stream to break
            write_t.Join();
            
        }
    }

    private void Writer(TcpClient client, StreamWriter writer, Func<bool> running)
    {
        try
        {
            while (running())
            {
                if (outboundMessageQueue.TryDequeue(out var response))
                {
                    string jsonResponse = JsonUtility.ToJson(response);
                    writer.WriteLine(jsonResponse);
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log($"Writer ended: {e}");
        }

    }

    private void Reader(TcpClient client, StreamReader reader, Action stop)
    {
        try
        {
            while (true)
            {
                string line;
                try
                {
                    line = reader.ReadLine();
                    if (line == null) continue;
                    if (line == "") continue;
                }
                catch
                {
                    continue;
                }
                //UnityEngine.Debug.Log($"Got {line}");
                InputMsg msg;
                try
                {
                    msg = UnityEngine.JsonUtility.FromJson<InputMsg>(line);
                }
                catch
                {
                    continue;
                }

                //UnityEngine.Debug.Log($"Reader sees {msg.method}({msg.arg}).");
                OnMessageReceived?.Invoke(msg);
            }
        }
        catch (Exception e)
        {
            
            UnityEngine.Debug.Log($"Reader ended: {e}");
        }
        stop();
        
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using devDept.Serialization;
using DevExpress.DataAccess.Native.Data;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using NetMQ;
using NetMQ.Sockets;
using Pinokio.Simulation.Models.IK;
using ProtoBuf;
using Tutorial;
using System.Data;
using Pinokio.Simulation;
using Pinokio.Core;
using Google.Protobuf;

namespace Pinokio.IKDT
{
    public class ZMQManager
    {
        private ResponseSocket _serverSocket;
        private bool _isRunning;
        private MessageType _messageType;
        private const int _portNumber = 55555;
        private SimDocument _simDocument;
        public int PortNumber { get => _portNumber; }

        public ZMQManager()
        {
            Initialize();
        }

        public void StartServer()
        {
            _serverSocket = new ResponseSocket();
            _serverSocket.Bind($"tcp://localhost:{_portNumber}");
            LogHandler.AddLog(LogLevel.Info, $"IKDT Server Started through TCP://localhost: {_portNumber}");

            Thread serverThread = new Thread(new ThreadStart(this.Listen));
            serverThread.IsBackground = true;
            serverThread.Start();
            LogHandler.AddLog(LogLevel.Info, $"Ready To Listen from Client...");
        }

        public void ServerClose()
        {
            Destroy();
        }

        private void Initialize()
        {
            AsyncIO.ForceDotNet.Force();
        }

        public void Listen()
        {
            if (_serverSocket != null)
            {
                bool _readyToDeserialize = false;
                while (true)
                {
                    string message;
                    byte[] buffer;

                    if (_readyToDeserialize == false)
                    {
                        if (_serverSocket.TryReceiveFrameString(out message))
                        {
                            switch (message)
                            {
                                case "SEND_LAYOUT_MESSAGE":
                                    _messageType = MessageType.SEND_LAYOUT_MESSAGE;
                                    _readyToDeserialize = true;
                                    _serverSocket.SendFrame("Confirmed");
                                    break;

                                case "SEND_MPT_MESSAGE":
                                    _messageType = MessageType.SEND_MPT_MESSAGE;
                                    _readyToDeserialize = true;
                                    _serverSocket.SendFrame("Confirmed");
                                    break;

                                case "REQUEST_LAYOUT_RESULT":
                                    _messageType = MessageType.REQUEST_LAOUT_RESULT;
                                    if (MainForm.Instance.Simdocument.Engine.State is EngineState.End)
                                        Reply();
                                    else
                                        _serverSocket.SendFrame("Denied");
                                    break;

                                case "REQUEST_MPT_RESULT":
                                    _messageType = MessageType.REQUEST_MPT_RESULT;
                                    if (MainForm.Instance.Simdocument.Engine.State is EngineState.End)
                                        Reply();
                                    else
                                        _serverSocket.SendFrame("Denied");
                                    break;
                            }
                            LogHandler.AddLog(LogLevel.Info, $"Successfully Received Message : {message}");
                            LogHandler.AddLog(LogLevel.Info, $"Ready To Listen from Client...");
                        }
                    }
                    else if (_serverSocket.TryReceiveFrameBytes(out buffer))
                    {
                        LogHandler.AddLog(LogLevel.Info, $"Initial Input Data Successfully Received from Client!!");
                        Deserialize(buffer);
                        _readyToDeserialize = false;
                        _serverSocket.SendFrame("Confirmed");
                    }
                }
            }
        }

        private void Deserialize(byte[] buffer)
        {
            if (buffer == null) return;

            switch (_messageType)
            {
                case MessageType.SEND_LAYOUT_MESSAGE:
                    RequestLayoutData layoutData = RequestLayoutData.Parser.ParseFrom(buffer);
                    var data = FormattingData(layoutData);
                    MainForm.Instance.LoadModels(data);
                    MainForm.Instance.RunSimulationAutomation();
                    break;

                case MessageType.SEND_MPT_MESSAGE:
                    RequestMptData mptData = RequestMptData.Parser.ParseFrom(buffer);
                    data = FormattingData(mptData);
                    MainForm.Instance.LoadModels(data);
                    MainForm.Instance.RunSimulationAutomation();
                    break;
            }
        }

        private System.Data.DataTable FormattingData(RequestLayoutData layoutData)
        {
            List<Machine> eqpList = new List<Machine>();
            var dt = new System.Data.DataTable("LAYOUT");

            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("PositionX", typeof(double));
            dt.Columns.Add("PositionY", typeof(double));
            dt.Columns.Add("Width", typeof(double));
            dt.Columns.Add("Depth", typeof(double));
            dt.Columns.Add("TpToResource", typeof(double));
            dt.Columns.Add("TpFromResource", typeof(double));

            eqpList.Add(layoutData.HANIL40T1);
            eqpList.Add(layoutData.HANIL110T1);
            eqpList.Add(layoutData.HANIL150T1);
            eqpList.Add(layoutData.HANIL80T1);
            eqpList.Add(layoutData.KYORI40T1);
            eqpList.Add(layoutData.KYORI30T1);
            eqpList.Add(layoutData.KYORI30T2);
            eqpList.Add(layoutData.MITSUI60T1);

            foreach (var eqp in eqpList)
            {
                dt.Rows.Add(eqp.Name, eqp.PositionX, eqp.PositionY, eqp.Width, eqp.Depth, eqp.TransportationTimeToResource, eqp.TransportationTimeFromResource);
            }

            return dt;
        }

        private System.Data.DataTable FormattingData(RequestMptData mptData)
        {
            var dt = new System.Data.DataTable("MPT");

            dt.Columns.Add("WorkingTimeMin", typeof(int));
            dt.Columns.Add("ProcessWorkerCount", typeof(int));
            dt.Columns.Add("PackagingWorkerCount", typeof(int));

            var workingTimeMin = mptData.ProductionTime;
            var processWorkerCount = mptData.WorkerProcessing;
            var packagingWorkerCount = mptData.WorkerPackaging;

            dt.Rows.Add(workingTimeMin, processWorkerCount, packagingWorkerCount);

            return dt;
        }

        private byte[] Serialize()
        {
            ReplyLayoutResult replyLayoutResult;
            ReplyMptResult replyMptResult;

            using (MemoryStream ms = new MemoryStream())
            {
                switch (_messageType)
                {
                    case MessageType.REQUEST_LAOUT_RESULT:
                        replyLayoutResult = new ReplyLayoutResult();
                        replyLayoutResult.DueDate = Complete.Ratio;
                        replyLayoutResult.TotalProcessingTime = Complete.TotalCycleTime;
                        replyLayoutResult.WriteTo(ms);
                        break;

                    case MessageType.REQUEST_MPT_RESULT:
                        replyMptResult = new ReplyMptResult();
                        replyMptResult.DueDate = Complete.Ratio;
                        replyMptResult.ProcessingUtilizationRate = Complete.ProcessingUtils;
                        replyMptResult.PackagingUtilizationRate = Complete.PackagingUtils;
                        replyMptResult.WriteTo(ms);
                        break;
                }
                return ms.ToArray();
            }
        }

        private void Reply()
        {
            var buffer = Serialize();
            _serverSocket.SendFrame(buffer);
            LogHandler.AddLog(LogLevel.Info, $"Successfully Send Simulation Results to Client!!");
        }

        public void Destroy()
        {
            if (_serverSocket != null)
            {
                _serverSocket.Close();
                NetMQConfig.Cleanup();
            }
        }
    }
}

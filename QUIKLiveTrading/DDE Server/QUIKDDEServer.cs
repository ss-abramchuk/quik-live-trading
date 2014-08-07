using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using NDde.Server;
using log4net;

using WLDSolutions.QUIKLiveTrading.Helpers;

namespace WLDSolutions.QUIKLiveTrading.DDEServer
{
    internal sealed class QUIKDDEServer : DdeServer
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QUIKDDEServer));

        public event Action<string, List<List<object>>> NewRawData;
        public event Action<string> DisconnectTable;

        private enum DataTypes : short
        {
            Table = 0x0010,
            Float = 0x0001,
            String = 0x0002,
            Bool = 0x0003,
            Error = 0x0004,
            Blank = 0x0005,
            Int = 0x0006,
            Skip = 0x0007,
        }

        public QUIKDDEServer() : base("QUIKLiveTrading")
        {

        }

        protected override void OnDisconnect(DdeConversation conversation)
        {
            base.OnDisconnect(conversation);

            if (DisconnectTable != null)
                DisconnectTable(conversation.Topic);
        }

        protected override PokeResult OnPoke(DdeConversation conversation, string item, byte[] data, int format)
        {
            List<List<object>> rawData = Parse(data);

            if (NewRawData != null && rawData != null)
                NewRawData(conversation.Topic, rawData);

            return PokeResult.Processed;
        }

        private List<List<object>> Parse(byte[] data)
        {
            try
            {
                using (Stream stream = new MemoryStream(data))
                {
                    DataTypes type = (DataTypes)BitConverter.ToInt16(stream.Read(typeof(DataTypes)), 0);
                    short size = BitConverter.ToInt16(stream.Read(typeof(short)), 0);
                    short rowCount = BitConverter.ToInt16(stream.Read(typeof(short)), 0);
                    short columnCount = BitConverter.ToInt16(stream.Read(typeof(short)), 0);

                    var rows = new List<List<object>>();

                    for (var row = 0; row < rowCount; row++)
                    {
                        var cells = new List<object>();

                        do
                        {
                            DataTypes cellType = (DataTypes)BitConverter.ToInt16(stream.Read(typeof(DataTypes)), 0);
                            short cellSize = BitConverter.ToInt16(stream.Read(typeof(short)), 0);

                            if (cellType != DataTypes.Skip)
                            {
                                Type blockType = GetBlockType(cellType);
                                int blockSize = blockType == typeof(string) ? cellSize : blockType.GetSize();

                                var cellColumnCount = cellSize / blockSize;

                                for (var column = 0; column < cellColumnCount; column++)
                                {
                                    if (blockType == typeof(string))
                                    {
                                        byte[] buffer = new byte[blockSize];
                                        stream.Read(buffer, 0, blockSize);

                                        int offset = 0;

                                        while (offset < buffer.Length)
                                        {
                                            byte length = buffer[offset];
                                            byte[] byteString = new byte[length];

                                            Buffer.BlockCopy(buffer, offset + 1, byteString, 0, length);

                                            cells.Add(Encoding.Default.GetString(byteString));

                                            offset += length + 1;
                                        }
                                    }
                                    else
                                    {
                                        if (blockType == typeof(double))
                                        {
                                            cells.Add(BitConverter.ToDouble(stream.Read(blockType), 0));
                                        }
                                        else if (blockType == typeof(int))
                                        {
                                            cells.Add(BitConverter.ToInt32(stream.Read(blockType), 0));
                                        }
                                        else if (blockType == typeof(bool))
                                        {
                                            cells.Add(BitConverter.ToBoolean(stream.Read(blockType), 0));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                byte[] buffer = new byte[cellSize];
                                stream.Read(buffer, 0, cellSize);
                            }
                        }
                        while (cells.Count < columnCount);

                        rows.Add(cells);
                    }

                    return rows;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }

        private Type GetBlockType(DataTypes dataType)
        {
            switch (dataType)
            {
                case DataTypes.Float:
                    return typeof(double);
                case DataTypes.String:
                    return typeof(string);
                case DataTypes.Bool:
                    return typeof(bool);
                case DataTypes.Int:
                    return typeof(int);
                case DataTypes.Skip:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException("dataType");
            }
        }
    }
}

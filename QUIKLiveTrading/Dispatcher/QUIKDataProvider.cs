using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using WealthLab;
using log4net;

using WLDSolutions.QUIKLiveTrading.Abstract;
using WLDSolutions.QUIKLiveTrading.Helpers;
using WLDSolutions.LiveTradingManager.Abstract;

namespace WLDSolutions.QUIKLiveTrading.Dispatcher
{
    internal class QUIKDataProvider : IQUIKStaticProvider, IQUIKStreamingProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QUIKDataProvider));

        ILTSettingsProvider _settingsProvider;

        #region Константы потоков QUIK

        private const short protocolVersion = 2;
        private const short symbolNamePointer = 2;
        private const short dataScalePointer = 6;
        private const short candlesCountPointer = 7;
        private const short candlesDataPointer = 8;
        private const short candlesLeftPointer = 9;
        private const short unknownPointer = 5;

        #endregion

        public QUIKDataProvider(ILTSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        #region Получение исторических данных

        public List<Candle> GetStaticData(BarDataScale dataScale, string symbol, string suffix, out string securityName)
        {
            string pipeName = GetPipeName(dataScale, symbol, suffix, false);

            List<Candle> candles = new List<Candle>();

            using (NamedPipeClientStream quikPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None))
            {
                try
                {
                    quikPipe.Connect(1000);
                }
                catch (TimeoutException)
                {
                    throw new Exception("Не удалось подключиться к каналу передачи данных.");
                }
                catch (IOException)
                {
                    throw new Exception("Канал передачи данных занят.");
                }

                // Проверка версии протокола
                CheckProtocolVersion(quikPipe);

                // Получение полного имени инструмента
                securityName = GetSymbolName(quikPipe);

                // Получение таймфрейма для инструмента
                BarDataScale pipeScale = GetDataScale(quikPipe);

                if(!pipeScale.Equals(dataScale))
                    throw new Exception("Таймфрем запроса не совпадает с таймфреймом полученных котировок.");

                bool update = true;

                while (update)
                {
                    byte[] buffer;

                    // Получение указателя
                    //
                    short pointer = 0;
                    bool checkPointer = ReadPipeData(quikPipe, 2, out buffer);
                    if (checkPointer)
                        pointer = BitConverter.ToInt16(buffer, 0);
                    else
                        throw new Exception("Не удалось получить указатель");

                    switch (pointer)
                    {
                        // Неизвестный указатель:
                        // не удалось установить назначение
                        // в комментах была следующая строка "Stop Session for %s"
                        //
                        case unknownPointer:
                            _logger.Debug(string.Format("[{0}.{1}] Неизвестный указатель в последовательности данных.", symbol, suffix));
                            break;

                        // Получение количества свечей
                        //
                        case candlesCountPointer:
                            bool checkCandlesCount = ReadPipeData(quikPipe, 4, out buffer);
                            int candlesCount = 0;
                            if (checkCandlesCount)
                                candlesCount = BitConverter.ToInt32(buffer, 0);
                            else
                                throw new Exception("Не удалось получить количество свечей.");
                            break;

                        // Получение данных свечи
                        //
                        case candlesDataPointer:
                            try
                            {
                                Candle candle = GetCandle(quikPipe);

                                if (pipeScale.IsIntraday)
                                    candle.Date = candle.Date.AddMinutes(pipeScale.BarInterval);

                                candles.Add(candle);
                            }
                            catch (Exception ex)
                            {
                                _logger.Debug(string.Format("[{0}.{1}] {2}", symbol, suffix, ex));
                                update = false;
                            }

                            break;

                        // Получение количества оставшихся свечей
                        //
                        case candlesLeftPointer:
                            bool chekCandlesLeft = ReadPipeData(quikPipe, 4, out buffer);
                            int candlesLeft = 0;
                            if (chekCandlesLeft)
                                candlesLeft = BitConverter.ToInt32(buffer, 0);
                            if (candlesLeft == 0)
                                update = false;
                            break;

                        // Неверная последовательность
                        //
                        default:
                            throw new Exception(string.Format("Неверная последовательность данных. Последняя метка: {0}", pointer));
                    }
                }
            }

            return candles;
        }

        private string GetPipeName(BarDataScale dataScale, string symbol, string suffix, bool staticPipe)
        {
            string pipeName;

            string timeFrameSuffix = GetTimeFrameSuffix(dataScale);

            List<SymbolDescription> symbols = (List<SymbolDescription>)_settingsProvider.GetObject("ImportSymbols", typeof(List<SymbolDescription>)) ?? new List<SymbolDescription>();

            string appropriateSymbol = (from symbolDescription in symbols
                                        where symbolDescription.FullCode == symbol
                                        select symbolDescription.ExportName).DefaultIfEmpty(symbol).First();

            if (staticPipe)
                pipeName = string.Format("QUIK_{0}{1}_{2}_EXISTING_DATA", appropriateSymbol, suffix, timeFrameSuffix);
            else
                pipeName = string.Format("QUIK_{0}{1}_{2}", appropriateSymbol, suffix, timeFrameSuffix);

            return pipeName;
        }

        private string GetTimeFrameSuffix(BarDataScale dataScale)
        {
            string suffix;

            switch (dataScale.Scale)
            {
                case BarScale.Tick:
                    suffix = "TICKS";
                    break;

                case BarScale.Minute:
                    suffix = string.Format("{0}MINUTES", dataScale.BarInterval);
                    break;

                case BarScale.Daily:
                    suffix = "DAY";
                    break;

                case BarScale.Weekly:
                    suffix = "WEEK";
                    break;

                case BarScale.Monthly:
                    suffix = "MONTH";
                    break;

                default:
                    suffix = string.Empty;
                    break;
            }

            return suffix;
        }

        private Candle GetCandle(NamedPipeClientStream pipe)
        {
            Candle candle = new Candle();

            byte[] buffer;

            // Получение DateTime
            //
            bool checkDateTime = ReadPipeData(pipe, 8, out buffer);
            if (checkDateTime)
            {
                long fileTime = BitConverter.ToInt64(buffer, 0);
                candle.Date = DateTime.FromFileTimeUtc(fileTime);
            }
            else
                throw new Exception("Ошибка: не удалось получить Date.");

            // Получение Open
            //
            bool checkOpen = ReadPipeData(pipe, 8, out buffer);
            if (checkOpen)
                candle.Open = BitConverter.ToDouble(buffer, 0);
            else
                throw new Exception("Ошибка: не удалось получить Open.");

            // Получение High
            //
            bool checkHigh = ReadPipeData(pipe, 8, out buffer);
            if (checkHigh)
                candle.High = BitConverter.ToDouble(buffer, 0);
            else
                throw new Exception("Ошибка: не удалось получить High.");

            // Получение Low
            //
            bool checkLow = ReadPipeData(pipe, 8, out buffer);
            if (checkLow)
                candle.Low = BitConverter.ToDouble(buffer, 0);
            else
                throw new Exception("Ошибка: не удалось получить Low.");

            // Получение Close
            //
            bool checkClose = ReadPipeData(pipe, 8, out buffer);
            if (checkClose)
                candle.Close = BitConverter.ToDouble(buffer, 0);
            else
                throw new Exception("Ошибка: не удалось получить Close.");

            // Получение Volume
            //
            bool checkVolume = ReadPipeData(pipe, 8, out buffer);
            if (checkVolume)
                candle.Volume = BitConverter.ToDouble(buffer, 0);
            else
                throw new Exception("Ошибка: не удалось получить Volume.");

            return candle;
        }

        private BarDataScale GetDataScale(NamedPipeClientStream pipe)
        {
            byte[] buffer;

            // Проверка указателя
            //
            bool checkPointer = ReadPipeData(pipe, 2, out buffer);
            if (!checkPointer || BitConverter.ToInt16(buffer, 0) != dataScalePointer)
                throw new Exception("Ошибка: не удалось получить метку таймфрейма");

            // Получение таймфрейма
            //
            int timeFrame = 0;
            bool checkTimeFrame = ReadPipeData(pipe, 4, out buffer);
            if (checkTimeFrame)
                timeFrame = BitConverter.ToInt32(buffer, 0);
            else
                throw new Exception("Ошибка: не удалось получить таймфрейм.");

            BarDataScale dataScale = new BarDataScale();

            switch (timeFrame)
            {
                case -3:
                    dataScale.Scale = BarScale.Monthly;
                    break;

                case -2:
                    dataScale.Scale = BarScale.Weekly;
                    break;

                case -1:
                    dataScale.Scale = BarScale.Daily;
                    break;

                case 0:
                    dataScale.Scale = BarScale.Tick;
                    dataScale.BarInterval = 1;
                    break;

                default:
                    dataScale.Scale = BarScale.Minute;
                    dataScale.BarInterval = timeFrame;
                    break;
            }

            return dataScale;
        }

        private string GetSymbolName(NamedPipeClientStream pipe)
        {
            byte[] buffer;

            // Проверка указателя
            //
            bool checkPointer = ReadPipeData(pipe, 2, out buffer);
            if (!checkPointer || BitConverter.ToInt16(buffer, 0) != symbolNamePointer)
                throw new Exception("Ошибка: не удалось получить метку имени инструмента.");

            // Получение длины строки с именем инструмента
            //
            short securityNameLength = 0;
            bool checkLength = ReadPipeData(pipe, 2, out buffer);
            if (checkLength)
                securityNameLength = BitConverter.ToInt16(buffer, 0);
            else
                throw new Exception("Ошибка: не удалось получить метку длины имени инструмента.");

            // Получение имени инструмента
            //
            string securityName = null;
            bool checkName = ReadPipeData(pipe, securityNameLength, out buffer);
            if (checkName)
                securityName = Encoding.Default.GetString(buffer);
            else
                throw new Exception("Ошибка: не удалось получить имя инструмента.");

            return securityName;
        }

        private void CheckProtocolVersion(NamedPipeClientStream pipe)
        {
            byte[] buffer;

            // Первичная проверка потока
            //
            bool checkPipe = ReadPipeData(pipe, 2, out buffer);
            if (!checkPipe || BitConverter.ToInt16(buffer, 0) != 1)
                throw new Exception("Ошибка: не удалось получить метку протокола.");

            // Проверка версии протокола
            //
            bool checkProtocol = ReadPipeData(pipe, 2, out buffer);
            short currProtocolVersion = BitConverter.ToInt16(buffer, 0);
            if (!checkProtocol || currProtocolVersion != protocolVersion)
                throw new Exception(string.Format("Версии протоколов отличаются. Ожидаемый: {0}; полученный: {1}.", protocolVersion, currProtocolVersion));
        }

        private bool ReadPipeData(NamedPipeClientStream pipe, int bytesToRead, out byte[] buffer)
        {
            bool result = false;

            buffer = new byte[bytesToRead];
            int length = 0;

            try
            {
                length = pipe.Read(buffer, 0, bytesToRead);
            }
            finally
            {
                result = length == bytesToRead;
            }

            return result;
        }

        #endregion

        #region Получение данных в реальном времени

        public event Action<Quote, Candle> NewQuote;

        public void Stream(string symbol, CancellationToken token)
        {
            BarDataScale dataScale = new BarDataScale(BarScale.Minute, 1);

            string pipeName = GetPipeName(dataScale, symbol, string.Empty, false);

            using (NamedPipeClientStream quikPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None))
            {
                try
                {
                    quikPipe.Connect(1000);

                    // Проверка версии протокола
                    CheckProtocolVersion(quikPipe);

                    // Получение полного имени инструмента
                    string securityName = GetSymbolName(quikPipe);

                    // Получение таймфрейма для инструмента
                    BarDataScale pipeScale = GetDataScale(quikPipe);

                    bool sendQuote = false;

                    int candlesCount = -1;

                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        byte[] buffer;

                        // Получение указателя
                        //
                        short pointer = 0;
                        bool checkPointer = ReadPipeData(quikPipe, 2, out buffer);
                        if (checkPointer)
                            pointer = BitConverter.ToInt16(buffer, 0);
                        else
                            throw new Exception("Не удалось получить указатель");

                        switch (pointer)
                        {
                            // Неизвестный указатель:
                            // не удалось установить назначение
                            // в комментах была следующая строка "Stop Session for %s"
                            //
                            case unknownPointer:
                                _logger.Debug(string.Format("[{0}] Неизвестный указатель в последовательности данных", symbol));
                                break;

                            // Получение количества свечей
                            //
                            case candlesCountPointer:
                                bool checkCandlesCount = ReadPipeData(quikPipe, 4, out buffer);
                                if (!checkCandlesCount)
                                    throw new Exception("Не удалось получить количество свечей");
                                if (!sendQuote)
                                    candlesCount = BitConverter.ToInt32(buffer, 0);
                                break;

                            // Получение данных свечи
                            //
                            case candlesDataPointer:
                                try
                                {
                                    DateTime timeStamp = DateTime.Now;

                                    Candle candle = GetCandle(quikPipe);

                                    if (sendQuote && NewQuote != null)
                                    {
                                        Quote quote = new Quote()
                                        {
                                            Symbol = symbol,
                                            TimeStamp = timeStamp.Minute > candle.Date.Minute ? candle.Date.AddSeconds(59) : timeStamp,
                                            Price = candle.Close,
                                            Size = candle.Volume,
                                        };

                                        NewQuote(quote, candle);
                                    }

                                    candlesCount--;

                                    if (!sendQuote && candlesCount == 0)
                                        sendQuote = true;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Debug(string.Format("[{0}] {1}", symbol, ex));
                                }

                                break;

                            // Получение количества оставшихся свечей
                            //
                            case candlesLeftPointer:
                                ReadPipeData(quikPipe, 4, out buffer);
                                break;

                            // Неверная последовательность
                            //
                            default:
                                break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    _logger.Error("[STREAM] " + ex);
                }               
            }
        }

        #endregion        
    }
}

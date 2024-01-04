using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

/*
Name: TurtleTrendFollow_cBot
Description: An automated bot for folowing trends.
Author: GeorgeFreelanceDeveloper
Updated by: LucyFreelanceDeveloper
CreateDate: 3.1.2023
UpdateDate: 4.1.2023
Version: 0.0.1
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class TurtleTrendFollow_cBot : Robot
    {
        // User defined properties
        [Parameter("Type", DefaultValue = TradingStyle.SWING)]
        public TradingStyle SelectedTradingStyle { get; set; }
        
        [Parameter(DefaultValue = 20)]
        public int CountPeriodForEntry { get; set; }
        
        [Parameter(DefaultValue = 10)]
        public int CountPeriodForStop { get; set; }
        
        [Parameter(DefaultValue = 5)]
        public double RiskPercentage {get; set;}
        
        [Parameter(DefaultValue = 1)]
        public int MaxOpenPositions { get; set; }
       
        // Constants
        private readonly bool enableDebug = false;
        private readonly int ATR_Period = 20;
        private readonly string LogFolderPath = "c:/Logs/cBots/TurtleTrendFollow/";
        private readonly string LogSendersAddress = "senderaddress@email.com";
        private readonly string LogRecipientAddress = "recipientaddress@email.com";
        
        //Ids
        private String TradeId {get; set;}

        public enum TradingStyle{
            INTRADAY,
            SWING,
            POSITION
        }

        protected override void OnStart()
        {
            Log("Start TurtleTrendFollow_cBot");

            Log("User defined properties:");
            Log($"SelectedTradingStyle: {SelectedTradingStyle}");
            Log($"CountPeriodForEntry: {CountPeriodForEntry}");
            Log($"CountPeriodForStop: {CountPeriodForStop}");
            Log($"RiskPercentage: {RiskPercentage}");
            Log($"MaxOpenPositions: {MaxOpenPositions}");

            Log("Validation of User defined properties ...");
            List<String> inputErrorMessages = ValidateInputs();
            inputErrorMessages.ForEach(m => Log(m));
            if (inputErrorMessages.Any()){
                Log("App contains input validation errors and will be stop.");
                Stop();
                return;
            }
            
            Log("Register listeners");
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;
        }
        
        protected override void OnTick()
        {
            Log("Start OnTick");
            
            double actualPrice = MarketData.GetTicks().Last().Ask;
            //double actualPrice = MarketData.GetBars(TimeFrame.Minute).Last().Close; // For backtest on m1 bars
            
            TimeFrame timeFrame = SelectTimeFrame(SelectedTradingStyle);
            
            var barsForEntry = MarketData.GetBars(timeFrame).SkipLast(1).ToList().TakeLast(CountPeriodForEntry);
            double maxPriceLastDaysForEntry =  barsForEntry.Max(b=>b.High);
            double minPriceLastDaysForEntry = barsForEntry.Min(b=>b.Low);
            
            var barsForStop = MarketData.GetBars(timeFrame).SkipLast(1).ToList().TakeLast(CountPeriodForStop);
            double maxPriceLastDaysForStop = barsForStop.Max(b=>b.High);
            double minPriceLastDaysForStop = barsForStop.Min(b=>b.Low);
            
            var positions = Positions.ToList();

            TradeId = System.Guid.NewGuid().ToString();
            string label = TradeId;
            
            foreach(Position position in positions)
            {
                if(position.TradeType == TradeType.Buy && actualPrice < minPriceLastDaysForStop)
                {
                    Log($"Long position reach stop level (actualPrice < minPriceLastDaysForStop), position will be close [actualPrice: {actualPrice}, minPriceLastDaysForStop: {minPriceLastDaysForStop}].");
                    position.Close();
                }
                if(position.TradeType == TradeType.Sell && actualPrice > maxPriceLastDaysForStop)
                {
                     Log($"Short position reach stop level (actualPrice > maxPriceLastDaysForStop), position will be close [actualPrice: {actualPrice}, maxPriceLastDaysForStop: {maxPriceLastDaysForStop}].");
                    position.Close();
                }
            }
            
            if(Positions.Count < MaxOpenPositions)
            { 
                if(actualPrice > maxPriceLastDaysForEntry)
                {
                    Log($"Price reach breakout zone for long (actualPrice > maxPriceLastDaysForEntry), bot will execute market long order. [actualPrice: {actualPrice}, maxPriceLastDaysForEntry: {maxPriceLastDaysForEntry}]");
                    ExecuteMarketOrder(TradeType.Buy, Symbol.Name, ComputeTradeAmount(actualPrice, timeFrame), label); 
                }
                else if (actualPrice < minPriceLastDaysForEntry)
                {
                    Log($"Price reach breakout zone for short (actualPrice < minPriceLastDaysForEntry), bot will execute market short order. [actualPrice: {actualPrice}, minPriceLastDaysForEntry: {minPriceLastDaysForEntry}]");
                    ExecuteMarketOrder(TradeType.Sell, Symbol.Name, ComputeTradeAmount(actualPrice, timeFrame), label); 
                }
            }
            
            if(enableDebug)
            {
                Log($"ActualPrice: {actualPrice}", "DEBUG");
                Log($"MaxPriceLastDaysForEntry: {maxPriceLastDaysForEntry}", "DEBUG");
                Log($"MinPriceLastDaysForEntry: {minPriceLastDaysForEntry}", "DEBUG");
                Log("Generated properties ... ");
                Log($"TradeId: {TradeId}", "DEBUG");
            }
            
            Log("Finished OnTick");  
        }

        protected override void OnStop()
        {
            Log("Finished TurtleTrendFollow_cBot");
        }

        protected override void OnException(Exception exception)
        {
            Log(exception.ToString(), "ERROR");
        }

        private void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            var pos = args.Position;
            if (TradeId.SequenceEqual(pos.Label)){
                 Log("Order was converted to position.");
                 Log($"Position opened at {pos.EntryPrice}");
            }

        }
        
        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            var pos = args.Position;
            if(TradeId.SequenceEqual(pos.Label)){
                string profitLossMessage = pos.GrossProfit >= 0 ? "profit" : "loss";   
                Log($"Position closed with {pos.GrossProfit} {profitLossMessage}");
                Stop();
            }
        }
        
        private TimeFrame SelectTimeFrame(TradingStyle tradingStyle){
            switch(tradingStyle){
                case TradingStyle.INTRADAY: return TimeFrame.Hour;
                case TradingStyle.SWING: return TimeFrame.Daily;
                case TradingStyle.POSITION: return TimeFrame.Weekly; 
                default: return TimeFrame.Daily;
            }
        }
        
        private double ComputeTradeAmount(double actualPrice, TimeFrame timeFrame)
        {
           AverageTrueRange ATR = Indicators.AverageTrueRange(MarketData.GetBars(timeFrame), ATR_Period, MovingAverageType.Simple);
                
           double amount = ((RiskPercentage/100) * Account.Balance) / (2*ATR.Result.Last());
           double amountInLotRaw = amount / (actualPrice * Symbol.LotSize);
           double amountInLot = ((int)(amountInLotRaw / Symbol.VolumeInUnitsStep)) * Symbol.VolumeInUnitsStep;
           
           return amountInLot;
        }

        private List<String> ValidateInputs()
        {
            var errMessages = new List<String>();
            
            if (CountPeriodForEntry <= 0)
            {
                errMessages.Add($"WARNING: CountPeriodForEntry must be greater than 0. [CountPeriodForEntry: {CountPeriodForEntry}]");
            }
            
            if (CountPeriodForStop <= 0)
            {
                errMessages.Add($"WARNING: CountPeriodForStop must be greater than 0. [CountPeriodForStop: {CountPeriodForStop}]");
            }
            
            if (RiskPercentage <= 0)
            {
                 errMessages.Add($"WARNING: RiskPercentage must be greater than 0. [RiskPercentage: {RiskPercentage}]");
            }
            
            if (MaxOpenPositions <= 0)
            {
                errMessages.Add($"WARNING: MaxOpenPositions must be greater than 0. [MaxOpenPositions: {MaxOpenPositions}]");
            }
            
            return errMessages;
        }
        
        private void Log(String message, String level = "INFO")
        {
            string logMessage = $"[{DateTime.Now} - {Symbol.ToString()}] {level}: {message}";

            String dy = DateTime.Now.Day.ToString();
            String mn = DateTime.Now.Month.ToString();
            String yy = DateTime.Now.Year.ToString();

            string logFileName = $"TurtleTrendFollow_{Symbol.ToString()}_{yy}{mn}{dy}.log";
            string logPath = LogFolderPath + logFileName;

            if(!Directory.Exists(LogFolderPath))
            {
                Directory.CreateDirectory(LogFolderPath);
            }
            
            Print(logMessage); // Log to terminal
            File.AppendAllText(logPath,logMessage + Environment.NewLine); // Log to log file
            
            if (level.SequenceEqual("ERROR")){
                Notifications.SendEmail(LogSendersAddress, LogRecipientAddress, "Error in TurtleTrendFollow cBot", logMessage);
            }
        }
    }
}
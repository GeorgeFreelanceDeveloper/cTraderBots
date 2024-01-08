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
Description: An automated bot for trend following strategy Turtle.
Author: GeorgeFreelanceDeveloper
Updated by: LucyFreelanceDeveloper, GeorgeFreelanceDeveloper
CreateDate: 3.1.2023
UpdateDate: 7.1.2023
Version: 0.0.2
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class TurtleTrendFollow_cBot : Robot
    {
        // User defined properties
        [Parameter(DefaultValue = 20)]
        public int CountPeriodForEntry1 { get; set; }
        
        [Parameter(DefaultValue = 10)]
        public int CountPeriodForStop1 { get; set; }
        
        [Parameter(DefaultValue = 55)]
        public int CountPeriodForEntry2 { get; set; }
        
        [Parameter(DefaultValue = 20)]
        public int CountPeriodForStop2 { get; set; }
        
        [Parameter(DefaultValue = 2.5)]
        public double RiskPercentage { get; set; }
        
        [Parameter(DefaultValue = true)]
        public bool LongOnly { get; set; }

        [Parameter(DefaultValue = -1)]
        public int Trade1_Id { get; set; }

        [Parameter(DefaultValue = -1)]
        public int Trade2_Id { get; set; }
       
        // Constants
        private readonly int ATR_Period = 20;
        private readonly string LogFolderPath = "c:/Logs/cBots/TurtleTrendFollow/";
        private readonly string LogSendersAddress = "senderaddress@email.com";
        private readonly string LogRecipientAddress = "recipientaddress@email.com";

        public enum Level { L1, L2 }
        
        protected override void OnStart()
        {
            Log("Start TurtleTrendFollow_cBot");

            Log("User defined properties:");
            Log($"CountPeriodForEntry1: {CountPeriodForEntry1}");
            Log($"CountPeriodForStop1: {CountPeriodForStop1}");
            Log($"CountPeriodForEntry2: {CountPeriodForEntry2}");
            Log($"CountPeriodForStop2: {CountPeriodForStop2}");
            Log($"Trade1_Id: {Trade1_Id}");
            Log($"Trade2_Id: {Trade2_Id}");
            Log($"RiskPercentage: {RiskPercentage}");

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

        /*
        protected override void OnTick()
        {
            Log("Start OnTick");
            ExecuteStrategyPerLevel(Level.L1);
            ExecuteStrategyPerLevel(Level.L2);
            Log("Finished OnTick");  
        }
        */
        
        protected override void OnBar()
        {
            Log("Start OnBar");
            ExecuteStrategyPerLevel(Level.L1);
            ExecuteStrategyPerLevel(Level.L2);
            Log("Finished OnBar"); 
        }

        protected override void OnStop()
        {
            Log("Finished TurtleTrendFollow_cBot");
        }

        protected override void OnException(Exception exception)
        {
            Log(exception.ToString(), "ERROR");
        }
        
        private void ExecuteStrategyPerLevel(Level level)
        {
            Log($"ExecuteStrategyPerLevel: {level}");
            int CountPeriodForEntry = level == Level.L1 ? CountPeriodForEntry1 : CountPeriodForEntry2;
            int CountPeriodForStop = level == Level.L1 ? CountPeriodForStop1 : CountPeriodForStop2;
            int tradeId = level == Level.L1 ? Trade1_Id : Trade2_Id;
            
            double actualPrice = MarketData.GetTicks().Last().Ask;
            //double actualPrice = MarketData.GetBars(TimeFrame.Minute).Last().Close; // For backtest on m1 bars
            
            var barsForEntry = MarketData.GetBars(TimeFrame.Daily).SkipLast(1).ToList().TakeLast(CountPeriodForEntry);
            double maxPriceLastDaysForEntry =  barsForEntry.Max(b=>b.High);
            double minPriceLastDaysForEntry = barsForEntry.Min(b=>b.Low);
            
            var barsForStop = MarketData.GetBars(TimeFrame.Daily).SkipLast(1).ToList().TakeLast(CountPeriodForStop);
            double maxPriceLastDaysForStop = barsForStop.Max(b=>b.High);
            double minPriceLastDaysForStop = barsForStop.Min(b=>b.Low);
            
            Position position = Positions.ToList().Where(p=>p.Id == tradeId).FirstOrDefault();
            
            if(position == null)
            {
                if(actualPrice > maxPriceLastDaysForEntry)
                {
                    Log($"Price reach breakout zone for long (actualPrice > maxPriceLastDaysForEntry), bot will execute market long order. [actualPrice: {actualPrice}, maxPriceLastDaysForEntry: {maxPriceLastDaysForEntry}]");
                    TradeResult result = ExecuteMarketOrder(TradeType.Buy, Symbol.Name, ComputeTradeAmount(level)); 
                    int id = result.Position.Id;
                    switch(level)
                    {
                        case Level.L1: Trade1_Id = id; break;
                        case Level.L2: Trade2_Id = id; break;
                    }
                }
                else if (actualPrice < minPriceLastDaysForEntry && !LongOnly)
                {
                    Log($"Price reach breakout zone for short (actualPrice < minPriceLastDaysForEntry), bot will execute market short order. [actualPrice: {actualPrice}, minPriceLastDaysForEntry: {minPriceLastDaysForEntry}]");
                    TradeResult result = ExecuteMarketOrder(TradeType.Sell, Symbol.Name, ComputeTradeAmount(level)); 
                    var id = result.Position.Id;
                    switch(level)
                    {
                        case Level.L1: Trade1_Id = id; break;
                        case Level.L2: Trade2_Id = id; break;
                    }
                }
            }
            else
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
        }

        private void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            var pos = args.Position;
            if (pos.Symbol.ToString().SequenceEqual(Symbol.Name)){
                 Log("Order was converted to position.");
                 Log($"Position opened at {pos.EntryPrice}");
            }

        }
        
        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            var pos = args.Position;
            if(pos.Symbol.ToString().SequenceEqual(Symbol.Name)){
                string profitLossMessage = pos.GrossProfit >= 0 ? "profit" : "loss";   
                Log($"Position closed with {pos.GrossProfit} {profitLossMessage}");
            }
        }
        
        private double ComputeTradeAmount(Level level)
        {
            AverageTrueRange ATR = Indicators.AverageTrueRange(MarketData.GetBars(TimeFrame.Daily), ATR_Period, MovingAverageType.Simple); 
            int atrMultiplier = level == Level.L1 ? 2 : 4;
            double amount = ((RiskPercentage/100) * Account.Balance) / (atrMultiplier*ATR.Result.Last());
            double amountNormalized = Symbol.NormalizeVolumeInUnits(amount);
            return amountNormalized;
        }

        private List<String> ValidateInputs()
        {
            var errMessages = new List<String>();
            
            if (CountPeriodForEntry1 <= 0)
            {
                errMessages.Add($"WARNING: CountPeriodForEntry1 must be greater than 0. [CountPeriodForEntry1: {CountPeriodForEntry1}]");
            }
            
            if (CountPeriodForStop1 <= 0)
            {
                errMessages.Add($"WARNING: CountPeriodForStop1 must be greater than 0. [CountPeriodForStop1: {CountPeriodForStop1}]");
            }
            
            if (CountPeriodForEntry2 <= 0)
            {
                errMessages.Add($"WARNING: CountPeriodForEntry2 must be greater than 0. [CountPeriodForEntry2: {CountPeriodForEntry2}]");
            }
            
            if (CountPeriodForStop2 <= 0)
            {
                errMessages.Add($"WARNING: CountPeriodForStop2 must be greater than 0. [CountPeriodForStop2: {CountPeriodForStop2}]");
            }
            
            if (RiskPercentage <= 0)
            {
                 errMessages.Add($"WARNING: RiskPercentage must be greater than 0. [RiskPercentage: {RiskPercentage}]");
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

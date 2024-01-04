using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class TurtleTrendFollow_cBot : Robot
    {
        // User defined properties
        [Parameter("Type", DefaultValue = TradingStyle.SWING)]
        public TradingStyle SelectedTradingStyle { get; set; }
        
        [Parameter(DefaultValue = 20)]
        public int CountPeriodForEntry { get; set; }
        
        [Parameter(DefaultValue = 10)]
        public int CountPeriodForStop { get; set; }
        
        [Parameter(DefaultValue = 10)]
        public double PercentageAllocation { get; set; }
        
        [Parameter(DefaultValue = 5)]
        public double RiskPercentage {get; set;}
        
        [Parameter(DefaultValue = 1)]
        public int MaxOpenPositions { get; set; }
       
        // Constants
        private readonly bool enableDebug = false;
        private readonly int ATR_Period = 20;
        
        public enum TradingStyle{
            INTRADAY,
            SWING,
            POSITION
        }
        

        protected override void OnStart()
        {
            //TODO: convert print to log INFO
            Print("Start LevelTrader_cBot")

            Print("User defined properties:");

            //TODO: print all user defined properties

            //TODO: validate user defined properties (create private method for it)

            //TODO: registrer listener onOpenPosition, onClosePosition and implement it
        }
        
        protected override void OnTick()
        {
            Print("Start OnTick");
            
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
            
            foreach(Position position in positions)
            {
                if(position.TradeType == TradeType.Buy && actualPrice < minPriceLastDaysForStop)
                {
                    //TODO: Log INFO: Position reach stop level (actualPrice < minPriceLastDaysForStop), position will be close [actualPrice:{actualPrice}, minPriceLastDaysForStop:{minPriceLastDaysForStop}].
                    position.Close();
                }
                if(position.TradeType == TradeType.Sell && actualPrice > maxPriceLastDaysForStop)
                {
                    //TODO: Log INFO: Short Position reach stop level (actualPrice > maxPriceLastDaysForStop), position will be close [actualPrice:{actualPrice}, maxPriceLastDaysForStop:{maxPriceLastDaysForStop}].
                    position.Close();
                }
            }
            
            if(Positions.Count < MaxOpenPositions)
            { 
                if(actualPrice > maxPriceLastDaysForEntry)
                {
                    //TODO: Log INFO: Price reach breakout zone for long (actualPrice > maxPriceLastDaysForEntry), bot will execute market long order. [actualPrice:${actualPrice}, maxPriceLastDaysForEntry, ${maxPriceLastDaysForEntry}]
                    ExecuteMarketOrder(TradeType.Buy,Symbol.Name, ComputeTradeAmount(actualPrice, TimeFrame)); 
                }
                else if (actualPrice < minPriceLastDaysForEntry)
                {
                    //TODO: Log INFO: Price reach breakout zone for short (actualPrice < minPriceLastDaysForEntry), bot will execute market short order. [actualPrice:${actualPrice}, minPriceLastDaysForEntry, ${minPriceLastDaysForEntry}]
                    ExecuteMarketOrder(TradeType.Sell,Symbol.Name, ComputeTradeAmount(actualPrice, TimeFrame)); 
                }
            }
            
            if(enableDebug)
            {
                //TODO: convert to log (DEBUG)
                Print($"ActualPrice: {actualPrice}");
                Print(String.Format("maxPriceLastDaysForEntry: {0}", maxPriceLastDaysForEntry));
                Print(String.Format("minPriceLastDaysForEntry: {0}", minPriceLastDaysForEntry));
            }
            
            Print("Finished OnTick");  
        }

        protected override void OnException(Exception exception)
        {
            Log(exception.ToString(), "ERROR");
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

            //TODO: validate user inputs

            return errMessages;
        }

        private void Log(String message, String level = "INFO")
        {
            //TODO: implement loging logic
        }
    }
}
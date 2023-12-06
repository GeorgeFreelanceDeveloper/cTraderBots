using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.Text.RegularExpressions;

/*
Name: LevelTrader_cBot
Description: An automated bot for controlling trades. The bot helps reduce risk by adjusting positions when prices move favorably, cancel pending order when trade early reaction and eliminates open positions during sudden price spikes.
Author: GeorgeFreelanceDeveloper alias in old version GeorgeQuantAnalyst
Updated by: LucyFreelanceDeveloper alias in old version LucyQuantAnalyst
CreateDate: 1.8.2023
UpdateDate: 6.12.2023
Version: 1.2.0
*/
namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class LevelTrader_cBot : Robot
    {
        
        // User defined properties
        
        [Parameter(DefaultValue = 0)]
        public double EntryPrice {get; set;}
        
        [Parameter(DefaultValue = 0)]
        public double StopLossPrice {get; set;}
        
        [Parameter("Type", DefaultValue = TradeDirectionType.LONG)]
        public TradeDirectionType Direction {get; set;}
        
        [Parameter(DefaultValue = 1.5)]
        public double RiskRevardRatio {get; set;}
        
        [Parameter(DefaultValue = 5)]
        public double RiskPercentage {get; set;}
        
        [Parameter(DefaultValue = false)]
        public Boolean IsEnableTrailingStop {get; set;}

        [Parameter(DefaultValue = 0.5)]
        public double TrailingStopLossLevel1Percentage {get; set;}

        [Parameter(DefaultValue = 0.7)]
        public double TrailingStopLossLevel2Percentage {get; set;}
        
        [Parameter(DefaultValue = 60)]
        public int PlaceTradeDelayInMinutes {get; set;}
        
        [Parameter(DefaultValue = 1)]
        public int MaxAllowedOpenTrades { get; set; }
        
        // Example 2023/01/15
        [Parameter(DefaultValue = "")]
        public string ExpirationDateString {get; set;}
        
        
        // Constants
        private Regex ExpirationDatePattern = new Regex(@"^\d{4}/\d{2}/\d{2}$");
        private readonly double PercentageBeforeEntry = 0.33;
        private readonly bool enableTrace = false;

        // Ids
        private int PendingOrderId {get; set;}
        private String TradeId {get; set;}
        
        // Computed properties
        private double Move {get; set;}
        private double Amount {get; set;}
        private double BeforeEntryPrice {get; set;}
        private double TakeProfitPrice {get; set;}
        private double RiskPerTrade {get; set;}
        private double TrailingStopLossLevel1Price {get; set;}
        private double TrailingStopLossLevel2Price {get; set;}
        private double StopLossPips {get; set;}
        private double TakeProfitPips {get; set;}
        private double StopLossLevel1Price {get; set;}
        private double StopLossLevel2Price {get; set; }
        
        // Timestamps
        private DateTime? ExpirationDate {get; set;}
        private DateTime ReachProfitTargetTimestamp {get; set;}
        private DateTime ReachBeforeEntryPriceTimestamp {get; set;}
        
        // States
        private bool ReachProfitTarget {get; set;}
        private bool ReachBeforeEntryPrice {get; set;}
        private bool ReachTrailingStopLossLevel1Price {get; set;}
        private bool ReachTrailingStopLossLevel2Price {get; set;}
        private bool IsActivePosition {get; set;}
              
        public enum TradeDirectionType
        {
            LONG,
            SHORT
        }

        protected override void OnStart()
        {
            Print("Start CommoditiesLevelTrader_cBot");

            Print("User defined properties:");
            Print(String.Format("EntryPrice: {0}", EntryPrice));
            Print(String.Format("StopLossPrice: {0}", StopLossPrice));
            Print(String.Format("Direction: {0}", Direction));
            Print(String.Format("RiskRevardRatio: {0}", RiskRevardRatio));
            Print(String.Format("RiskPercentage: {0}", RiskPercentage));
            Print(String.Format("IsEnableTrailingStop: {0}", IsEnableTrailingStop));
            Print(String.Format("TrailingStopLossLevel1Percentage: {0}", TrailingStopLossLevel1Percentage));
            Print(String.Format("TrailingStopLossLevel2Percentage: {0}", TrailingStopLossLevel2Percentage));
            Print(String.Format("PlaceTradeDelayInMinutes: {0}", PlaceTradeDelayInMinutes));
            Print(String.Format("MaxAllowedOpenTrades: {0}", MaxAllowedOpenTrades));
            Print(String.Format("ExpirationDateString: {0}", ExpirationDateString));
            
            Print("Validation of User defined properties ...");
            List<String> inputErrorMessages = ValidateInputs();
            inputErrorMessages.ForEach(m => Print(m));
            if (inputErrorMessages.Any()){
                Print("App contains input validation errors and will be stop.");
                Stop();
                return;
            }

            Print("Compute properties ... ");
            TradeId = System.Guid.NewGuid().ToString();
            Move = EntryPrice - StopLossPrice;
            TakeProfitPrice = EntryPrice + Move; // RRR 1:1
            RiskPerTrade = (RiskPercentage / 100) * Account.Balance;
            double AmountRaw = RiskPerTrade / ((Math.Abs(Move) / Symbol.PipSize) * Symbol.PipValue);
            Amount = ((int)(AmountRaw / Symbol.VolumeInUnitsStep)) * Symbol.VolumeInUnitsStep;
            BeforeEntryPrice = EntryPrice + (Move * PercentageBeforeEntry);
            
            StopLossPips = (Math.Abs(Move)/Symbol.PipSize);
            StopLossLevel1Price = EntryPrice - (Move * 0.8);
            StopLossLevel2Price = EntryPrice;
            
            TakeProfitPips = ((Math.Abs(Move)/Symbol.PipSize)) * RiskRevardRatio;
            
            TrailingStopLossLevel1Price = EntryPrice + (Move * TrailingStopLossLevel1Percentage);
            TrailingStopLossLevel2Price = EntryPrice + (Move * TrailingStopLossLevel2Percentage);
            ExpirationDate = ExpirationDateString == String.Empty ? null : DateTime.Parse(ExpirationDateString);
            
            Print("Computed properties:");
            Print(String.Format("TradeId: {0}", TradeId));
            Print(String.Format("Move: {0}", Move));
            Print(String.Format("Take profit price: {0}", TakeProfitPrice)); // RRR 1:1
            Print(String.Format("Account.Balance: {0}", Account.Balance));
            Print(String.Format("RiskPerTrade: {0}", RiskPerTrade));
            Print(String.Format("Amount raw: {0}", AmountRaw));
            Print(String.Format("Min step volume: {0}", Symbol.VolumeInUnitsMin));
            Print(String.Format("Amount: {0}", Amount));
            Print(String.Format("Amount: {0} lots", Symbol.VolumeInUnitsToQuantity(Amount)));
            Print(String.Format("BeforeEntryPrice: {0}", BeforeEntryPrice));
            Print(String.Format("TrailingStopLossLevel1Price: {0}", TrailingStopLossLevel1Price));
            Print(String.Format("TrailingStopLossLevel2Price: {0}", TrailingStopLossLevel2Price));
            Print(String.Format("StopLossPips: {0}", StopLossPips));
            Print(String.Format("StopLossLevel1Price: {0}", StopLossLevel1Price));
            Print(String.Format("StopLossLevel2Price: {0}", StopLossLevel2Price));
            Print(String.Format("TakeProfitPips: {0}", TakeProfitPips));
            Print(String.Format("ExpirationDate: {0}", ExpirationDate));

            Print("Validate of computed properties");
            var errMessages = ValidateComputeValues();
            errMessages.ForEach(m=>Print(m));
            if (errMessages.Any())
            {
                Print("App contains compute values validation errors and will be stop.");
                Stop();
                return;
            }
            
            Print("Register listeners");
            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;
        }

        protected override void OnBar()
        {
            Print("Start onBar step");
            
            if (ExpirationDate != null && DateTime.Now > ExpirationDate)
            {
                Print("Time of trade expired, bot will stop.");
                Stop();
                return;
            }
            
            
            Bar lastBar = MarketData.GetBars(TimeFrame.Minute, Symbol.Name).Last();
            
            if (!ReachProfitTarget && 
                WasReachPriceLevel(lastBar, TakeProfitPrice, Direction==TradeDirectionType.SHORT))
            {
                Print("Price reach ProfitTargetPrice.");
                ReachProfitTarget = true;
                ReachProfitTargetTimestamp = DateTime.Now;
            }
            
            if (ReachProfitTarget && 
                !ReachBeforeEntryPrice &&
                WasReachPriceLevel(lastBar, BeforeEntryPrice, Direction==TradeDirectionType.SHORT))
            {
                Print("Price reach beforeEntryPrice.");
                ReachBeforeEntryPrice = true;
                ReachBeforeEntryPriceTimestamp = DateTime.Now;

                if(CountOpenTrades() >= MaxAllowedOpenTrades){
                    Print("On exchange is open max allowed trades, order do not place on exchange.");
                    Stop();
                    return;
                }
                
                if(ReachBeforeEntryPriceTimestamp.Subtract(ReachProfitTargetTimestamp).TotalMinutes < PlaceTradeDelayInMinutes)
                {
                    Print("Most fast movement to level, order do not place on exchange.");
                    Stop();
                    return;
                }

                Print("Place limit order");
                TradeResult result = PlaceLimitOrder();
                Print(String.Format("Response PlaceLimitOrder: {0}",result));
                PendingOrderId = result.PendingOrder.Id;
            }
            
            if (ReachBeforeEntryPrice &&
                WasReachPriceLevel(lastBar, TakeProfitPrice, Direction == TradeDirectionType.LONG))
            {
                Print("Price reach profit target after hit beforeEntryPrice.");
                Print("Cancel pending order if exist.");
                CancelLimitOrder();
                Stop();
                return;
            }

            if (IsActivePosition && IsEnableTrailingStop &&
                !ReachTrailingStopLossLevel1Price && 
                WasReachPriceLevel(lastBar, TrailingStopLossLevel1Price, Direction == TradeDirectionType.LONG ))
            {
                Print("Price reach TrailingStopLossLevel1Price.");
                ReachTrailingStopLossLevel1Price = true;
                SetStopLoss(StopLossLevel1Price);
                return;
            }

             if (IsActivePosition && IsEnableTrailingStop &&
                 !ReachTrailingStopLossLevel2Price && 
                  ReachTrailingStopLossLevel1Price && 
                  WasReachPriceLevel(lastBar, TrailingStopLossLevel2Price, Direction == TradeDirectionType.LONG))
            {
                Print("Price reach TrailingStopLossLevel2Price.");
                ReachTrailingStopLossLevel2Price = true;
                SetStopLoss(StopLossLevel2Price);
                return;
            }
            
            if (!IsActivePosition){
                foreach (Position pos in Positions)
                {
                    if (TradeId.SequenceEqual(pos.Comment)){
                        Print("Pending order was converted to position.");
                        Print("Position opened at {0}", pos.EntryPrice);
                        IsActivePosition = true;
                    }
                 }
            }
            
            
            if(enableTrace)
            {
                Print(String.Format("ReachProfitTarget: {0}", ReachProfitTarget));
                Print(String.Format("ReachBeforeEntryPrice: {0}", ReachBeforeEntryPrice));
                Print(String.Format("IsActivePosition: {0}",IsActivePosition));
                Print(String.Format("ReachTrailingStopLossLevel1Price: {0}", ReachTrailingStopLossLevel1Price));
                Print(String.Format("ReachTrailingStopLossLevel2Price: {0}", ReachTrailingStopLossLevel2Price));
            }
            
            Print(String.Format("Finished onBar step at time: {0}", DateTime.Now));
        }
        
        private void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            var pos = args.Position;
            if (TradeId.SequenceEqual(pos.Comment)){
                 Print("Pending order was converted to position.");
                 Print("Position opened at {0}", pos.EntryPrice);
                 IsActivePosition = true;
            }

        }
        
        private void PositionsOnClosed(PositionClosedEventArgs args)
        {
            var pos = args.Position;
            if(TradeId.SequenceEqual(pos.Comment)){
                string profitLossMessage = pos.GrossProfit >= 0 ? "profit" : "loss";   
                Print("Position closed with {0} {1}", pos.GrossProfit, profitLossMessage);
                Stop();
            }
        }

        protected override void OnStop()
        {
            Print("Finished CommoditiesLevelTrader_cBot");
        }
        
        private bool WasReachPriceLevel(Bar lastBar, double priceLevel, bool up){
            return up ? lastBar.High >= priceLevel : lastBar.Low <= priceLevel;
        }
        
        private void SetStopLoss(double price){
            var position = Positions.FirstOrDefault(pos => TradeId.SequenceEqual(pos.Comment));
            
            if (position == null)
            {
                Print("Error: Position with TradeId: {0} does not exists.", TradeId);
                return;
            }
            
            position.ModifyStopLossPrice(price);
        }
        
        private TradeResult PlaceLimitOrder()
        {
           TradeType orderTradeType = Direction == TradeDirectionType.LONG ? TradeType.Buy : TradeType.Sell;
           string symbolName = Symbol.Name;
           double volumeInUnits = Amount;
           double limitPrice = EntryPrice;
           string label = "";
           double stopLossPips = StopLossPips;
           double takeProfitPips = TakeProfitPips;
           DateTime? expiryTime = null;
           string comment = TradeId;
           bool hasTrailingStop = false;
           StopTriggerMethod stopLossTriggerMethod = StopTriggerMethod.Trade;

           return  PlaceLimitOrder(orderTradeType, symbolName, volumeInUnits, limitPrice, label, stopLossPips, takeProfitPips,
           expiryTime, comment, hasTrailingStop, stopLossTriggerMethod);
        }
        
        private void CancelLimitOrder()
        {     
            var orderToCancel = PendingOrders.FirstOrDefault(order => order.Id == PendingOrderId);
            if (orderToCancel == null)
            {
                Print("Error: Pending order with id {0} does not exists.", PendingOrderId);
                return;
            }
            
            CancelPendingOrder(orderToCancel);
        }
        
        private int CountOpenTrades()
        {
            return Positions.Count + PendingOrders.Count;
        }
        
        
        private List<String> ValidateInputs()
        {
            var errMessages = new List<String>();
            
            if (EntryPrice <= 0)
            {
                errMessages.Add(String.Format("WARNING: EntryPrice must be greater than 0. [EntryPrice: {0}]", EntryPrice));
            }
            
            if (StopLossPrice <= 0)
            {
                errMessages.Add(String.Format("WARNING: StopLossPrice must be greater than 0. [StopLossPrice: {0}]", StopLossPrice));
            }
            
            if (RiskRevardRatio < 1)
            {
                errMessages.Add(String.Format("WARNING: RiskRevardRatio must be greater or equal 1. [RiskRevardRatio: {0}]", RiskRevardRatio));
            }
            
            if (RiskPercentage <= 0)
            {
                 errMessages.Add(String.Format("WARNING: RiskPercentage must be greater than 0. [RiskPercentage: {0}]", RiskPercentage));
            }
            
            if (PlaceTradeDelayInMinutes < 0)
            {
                errMessages.Add(String.Format("WARNING: PlaceTradeDelayInMinutes must be greater than 0. [PlaceTradeDelayInMinutes: {0}]", PlaceTradeDelayInMinutes));
            }
            
            if (MaxAllowedOpenTrades <= 0)
            {
                errMessages.Add(String.Format("WARNING: MaxAllowedOpenTrades must be greater than 0. [MaxAllowedOpenTrades: {0}]", PlaceTradeDelayInMinutes));
            }
            
            if (Direction == TradeDirectionType.LONG && EntryPrice < StopLossPrice)
            {
                errMessages.Add(String.Format("WARNING: EntryPrice must be greater than stopLossPrice for LONG direction. [EntryPrice: {0}, StopLossPrice{1}]", EntryPrice, StopLossPrice));
            }
            
            if (Direction == TradeDirectionType.SHORT && EntryPrice > StopLossPrice)
            {
                errMessages.Add(String.Format("WARNING: EntryPrice must be lower than stopLossPrice for SHORT direction. [EntryPrice: {0}, StopLossPrice{1}]", EntryPrice, StopLossPrice));
            }
            
            if (ExpirationDateString != String.Empty && !ExpirationDatePattern.IsMatch(ExpirationDateString))
            {
                errMessages.Add(String.Format("WARNING: ExpirationDateString must contains valid date in format YYYY/MM/DD example 2000/01/01: [ExpirationDateString: {0}]", ExpirationDateString));
            }

            if (TrailingStopLossLevel1Percentage <= 0.0 && TrailingStopLossLevel1Percentage >= 1.0)
            {
                errMessages.Add(String.Format("WARNING: TrailingStopLossLevel1Percentage must be between 0.0 and 1.0 (0 => 0%, 1 => 100%). [TrailingStopLossLevel1Percentage: {0}]", TrailingStopLossLevel1Percentage));
            }

            if (TrailingStopLossLevel2Percentage <= 0.0 && TrailingStopLossLevel2Percentage >= 1.0)
            {
                errMessages.Add(String.Format("WARNING: TrailingStopLossLevel2Percentage must be between 0.0 and 1.0 (0 => 0%, 1 => 100%). [TrailingStopLossLevel2Percentage: {0}]", TrailingStopLossLevel2Percentage));
            }
            return errMessages;
        }
        
        private List<String> ValidateComputeValues()
        {
            var errMessages = new List<String>();
             
            if (Amount < Symbol.VolumeInUnitsMin)
            {
                errMessages.Add(String.Format("WARNING: Trade volume is less than minimum tradable amount: [Amount: {0}, MinTradableAmount: {1}]", Amount, Symbol.VolumeInUnitsMin));
            }
            
            if (Amount > Symbol.VolumeInUnitsMax)
            {
                errMessages.Add(String.Format("WARNING: Trade volume is greater than maximum tradable amount: [Amount: {0}, MaxTradableAmount: {1}]", Amount, Symbol.VolumeInUnitsMax));
            }
            
            var firstTier = Symbol.DynamicLeverage[0];
            double amountInAccountCurrency = Amount * EntryPrice;
            double expectedMargin = amountInAccountCurrency/firstTier.Leverage;
            if (expectedMargin > Account.Balance)
            {
                errMessages.Add(String.Format("WARNING: Expected margin is greater that account balance: [ExpectedMargin: {0}, AccountBalance: {1}]", expectedMargin, Account.Balance));
            }
             
            return errMessages;
        }
    }
}
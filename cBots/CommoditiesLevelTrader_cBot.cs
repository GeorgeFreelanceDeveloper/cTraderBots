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
Name: CommoditiesLevelTrader_cBot
Description: An automated bot for controlling trades on commodities. The bot helps reduce risk by adjusting positions when prices move favorably, cancel pending order when trade early reaction and eliminates open orders during sudden price spikes.
Author: GeorgeQuantAnalyst
Date: 1.8.2023
Version: 1.0.0
*/
namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class CommoditiesLevelTrader_cBot : Robot
    {
        
        [Parameter(DefaultValue = 0)]
        public double EntryPrice {get; set;}
        
        [Parameter(DefaultValue = 0)]
        public double StopLossPrice {get; set;}
        
        [Parameter("Type", DefaultValue = TradeDirectionType.LONG)]
        public TradeDirectionType Direction {get; set;}
        
        [Parameter(DefaultValue = 10)]
        public double RiskPerTradeInUsd {get; set;}
        
        [Parameter(DefaultValue = 60)]
        public int PlaceTradeDelayInMinutes {get; set;}
        
        [Parameter(DefaultValue = 1)]
        public int MaxAllowedOpenTrades { get; set; }
        
        // Example 2023/01/15
        [Parameter(DefaultValue = "")]
        public string ExpirationDateString {get; set;}
        
        private Regex ExpirationDatePattern = new Regex(@"^\d{4}/\d{2}/\d{2}$");
        
        private readonly double PercentageBeforeEntry = 0.33;
        
        private double BeforeEntryPrice {get; set;}
        
        private double TakeProfitPrice {get; set;}
        
        private double Move {get; set;}
        
        private double Amount {get; set;}
        
        private DateTime? ExpirationDate {get; set;}
        
        private bool ReachProfitTarget {get; set;}
        
        private DateTime ReachProfitTargetTimestamp {get; set;}
        
        private bool ReachBeforeEntryPrice {get; set;}
        
        private DateTime ReachBeforeEntryPriceTimestamp {get; set;}
        
        private bool ReachProfitTargetAfterBeforeEntryPrice {get; set;}
        
        private int PendingOrderId {get; set;}
        

        
        public enum TradeDirectionType
        {
            LONG,
            SHORT
        }

        protected override void OnStart()
        {
            Print("Start CommoditiesLevelTrader_cBot");
            
            var inputErrorMessages = ValidateInputs();
            if(inputErrorMessages.Count > 0)
            {
                Print("Validation input parameter errors:");
                foreach (var message in inputErrorMessages)
                {
                    Print(message);
                }
                Stop();
                return;
            }
            
            Move = EntryPrice - StopLossPrice;
            TakeProfitPrice = EntryPrice + Move;
            double AmountRaw = RiskPerTradeInUsd/Math.Abs(Move);
            Amount = ((int)(AmountRaw / Symbol.VolumeInUnitsStep)) * Symbol.VolumeInUnitsStep;
            BeforeEntryPrice = EntryPrice + ((EntryPrice - StopLossPrice) * PercentageBeforeEntry);
            ExpirationDate = ExpirationDateString == String.Empty ? null : DateTime.Parse(ExpirationDateString);
            
            Print("Compute properties:");
            Print(String.Format("Move: {0}", Move));
            Print(String.Format("Take profit price: {0}", TakeProfitPrice));
            Print(String.Format("Amount raw: {0}", AmountRaw));
            Print(String.Format("Min step volume: {0}", Symbol.VolumeInUnitsMin));
            Print(String.Format("Amount: {0}", Amount));
            Print(String.Format("Amount: {0} lots", Symbol.VolumeInUnitsToQuantity(Amount)));
            Print(String.Format("BeforeEntryPrice: {0}", BeforeEntryPrice));
            Print(String.Format("ExpirationDate: {0}", ExpirationDate));

            var errMessages = ValidateComputeValues();
            if(errMessages.Count > 0)
            {
                Print("Validation compute values errors:");
                foreach (var message in errMessages)
                {
                    Print(message);
                }
                Stop();
                return;
            }
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
            
            if (!ReachProfitTarget && WasReachProfitTarget(lastBar))
            {
                Print("Price reach ProfitTargetPrice.");
                ReachProfitTarget = true;
                ReachProfitTargetTimestamp = DateTime.Now;
            }
            
            if (ReachProfitTarget && !ReachBeforeEntryPrice && WasReachBeforeEntryPrice(lastBar))
            {
                Print("Price reach beforeEntryPrice.");
                ReachBeforeEntryPrice = true;
                ReachBeforeEntryPriceTimestamp = DateTime.Now;

                if(CountOpenTrades() >= MaxAllowedOpenTrades){
                    Print("On exchange is open max allowed trades, order do not place on exchange.");
                    Stop();
                    return;
                }
                
                if(ReachBeforeEntryPriceTimestamp.Subtract(ReachBeforeEntryPriceTimestamp).TotalMinutes < PlaceTradeDelayInMinutes)
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
            
            if (ReachBeforeEntryPrice && WasReachProfitTargetAfterBeforeEntryPrice(lastBar))
            {
                Print("Price reach profit target after hit beforeEntryPrice.");
                Print("Cancel pending order if exist.");
                CancelLimitOrder();
                Stop();
                return;
            }
            
            if (ReachBeforeEntryPrice && !IsPendingOrderActive())
            {
                Print("Pending order was activate.");
                Stop();
                return;
            }

            Print("Finished onBar step");
        }

        protected override void OnStop()
        {
            Print("Finished CommoditiesLevelTrader_cBot");
        }
        
        private bool WasReachProfitTarget(Bar lastBar)
        {
            return (Direction == TradeDirectionType.LONG && TakeProfitPrice >= lastBar.Low) ||
            (Direction == TradeDirectionType.SHORT && TakeProfitPrice <= lastBar.High);
        }
        
        private bool WasReachBeforeEntryPrice(Bar lastBar)
        {
            return (Direction == TradeDirectionType.LONG && BeforeEntryPrice >= lastBar.Low) ||
            (Direction == TradeDirectionType.SHORT && BeforeEntryPrice <= lastBar.High);
        }
        
        private bool WasReachProfitTargetAfterBeforeEntryPrice(Bar lastBar)
        {
            return (Direction == TradeDirectionType.LONG && TakeProfitPrice <= lastBar.High) ||
            (Direction == TradeDirectionType.SHORT && TakeProfitPrice >= lastBar.Low);
        }
        
        private TradeResult PlaceLimitOrder()
        {
           TradeType orderTradeType = Direction == TradeDirectionType.LONG ? TradeType.Buy : TradeType.Sell;
           string symbolName = Symbol.Name;
           double volumeInUnits = Amount;
           double limitPrice = EntryPrice;
           string label = "";
           double stopLossPips = (Math.Abs(Move)/Symbol.PipSize);
           double takeProfitPips = (Math.Abs(Move)/Symbol.PipSize);
           DateTime? expiryTime = null;
           string comment = "";
           bool hasTrailingStop = true;
           StopTriggerMethod stopLossTriggerMethod = StopTriggerMethod.Trade;
           
           
           return  PlaceLimitOrder(orderTradeType, symbolName, volumeInUnits, limitPrice, label, stopLossPips, takeProfitPips,
           expiryTime, comment, hasTrailingStop, stopLossTriggerMethod);
        }
        
        private void CancelLimitOrder()
        {
            foreach (var order in PendingOrders)
            {
                if (order.Id == PendingOrderId)
                {
                    CancelPendingOrder(order);
                }
            }
        }
        
        private int CountOpenTrades()
        {
            return Positions.Count + PendingOrders.Count;
        }
        
        private bool IsPendingOrderActive()
        {
            foreach(var order in PendingOrders)
            {
                if (order.Id == PendingOrderId)
                {
                    return true;
                }
            }
            return false;
        }
        
        private ArrayList ValidateInputs()
        {
            var errMessages = new ArrayList();
            
            if (EntryPrice <= 0)
            {
                errMessages.Add(String.Format("WARNING: EntryPrice must be greater than 0. [EntryPrice: {0}]", EntryPrice));
            }
            
            if (StopLossPrice <= 0)
            {
                errMessages.Add(String.Format("WARNING: StopLossPrice must be greater than 0. [StopLossPrice: {0}]", StopLossPrice));
            }
            
            if (RiskPerTradeInUsd <= 0)
            {
                 errMessages.Add(String.Format("WARNING: RiskPerTradeInUsd must be greater than 0. [RiskPerTradeInUsd: {0}]", RiskPerTradeInUsd));
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
            
            return errMessages;
        }
        
        private ArrayList ValidateComputeValues()
        {
             var errMessages = new ArrayList();
             
             
            if (Amount < Symbol.VolumeInUnitsMin)
            {
                errMessages.Add(String.Format("WARNING: Trade volume is less than minimum tradable amount: [Amount: {0}, MinTradableAmount: {1}]", Amount, Symbol.VolumeInUnitsMin));
            }
            
            if (Amount > Symbol.VolumeInUnitsMax)
            {
                errMessages.Add(String.Format("WARNING: Trade volume is greater than maximum tradable amount: [Amount: {0}, MaxTradableAmount: {1}]", Amount, Symbol.VolumeInUnitsMax));
            }
             
             return errMessages;
        }
    }
}

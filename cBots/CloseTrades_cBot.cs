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
Name: CloseTrades_cBot
Description: Bot closing pending orders and opened positions at defined date and time. You can set close for all trades or specific market.
Author: LucyQuantAnalyst
CreateDate: 29.10.2023
UpdateDate: 30.12.2023
Version: 1.0.1
*/

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class CloseTrades_cBot : Robot
    {   
        // User defined properties
        [Parameter(DefaultValue = DayOfWeek.Friday)]
        public DayOfWeek Day {get; set;}

        [Parameter(DefaultValue = 20)]
        public int Hours {get; set;}

        [Parameter(DefaultValue = 0)]
        public int Minutes {get; set;}

        [Parameter(DefaultValue = true)]
        public bool All {get; set;}

        [Parameter(DefaultValue = "Copper")]
        public string Market {get; set;}

        // Constants
        private TimeOnly CloseTime {get; set;}
        private readonly string LogFolderPath = "c:/Logs/cBots/CloseTrades/";
        private readonly string LogSendersAddress = "senderaddress@email.com";
        private readonly string LogRecipientAddress = "recipientaddress@email.com";
        
        protected override void OnStart()
        {
            Log("Start CloseTrades_cBot");

            Log("User defined properties:");
            Log(String.Format("Day: {0}", Day));
            Log(String.Format("Hours: {0}", Hours));
            Log(String.Format("Minutes: {0}", Minutes));
            Log(String.Format("All: {0}", All));
            Log(String.Format("Market: {0}", Market));
            
            Log("Validation of User defined properties ...");
            List<String> inputErrorMessages = ValidateInputs();
            inputErrorMessages.ForEach(m => Log(m));
            if (inputErrorMessages.Any()){
                Log("App contains input validation errors and will be stop.");
                Stop();
                return;
            }
            Log("Finished validation of User defined properties ...");
            
            Log("Compute properties ...");
            CloseTime = new TimeOnly(Hours, Minutes);

            Log("Computed properties:");
            Log(String.Format("CloseTime {0}", CloseTime));
        }

        protected override void OnBar()
        {
            Log(String.Format("{0}: Start onBar step", DateTime.Now));
             
            DateTime now = DateTime.Now;
             
            if(now.DayOfWeek == Day && TimeOnly.FromDateTime(now) >= CloseTime)
            {
               CancelOrdersAndPositions(All, Market);
            }
            
            Log(String.Format("{0}: Finished onBar step", DateTime.Now));
        }

        protected override void OnException(Exception exception)
        {
            Log(exception.ToString(), "ERROR");
        }
        
        private void CancelOrdersAndPositions(bool all, string market)
        {
            Log("Start closing orders and positions");
            
            PendingOrders.Where(order => all || order.SymbolName == market)
                .ToList()
                .ForEach(order => CancelPendingOrder(order));
                
            Positions.Where(position => all || position.SymbolName == market)
                .ToList()
                .ForEach(position => ClosePosition(position));
                
            Log("Finished closing orders and positions");     
        }
        
        private List<String> ValidateInputs()
        {
            var errMessages = new List<String>();

            if (Hours < 0 || Hours > 24)
            {
                errMessages.Add(String.Format("WARNING: Hours must be defined and greater or equal to 0 and less or equal to 24. [Hours: {0}]", Hours));
            }
            
            if (Minutes < 0 || Minutes > 60)
            {
                 errMessages.Add(String.Format("WARNING: Minutes must be defined and greater or equal to 0 and less or equal to 60. [Minutes: {0}]", Minutes));
            }
            
            if (!All && (Market == null || Market == ""))
            {
                errMessages.Add(String.Format("WARNING: Market must be defined. [Market: {0}]", Market));
            }
            
            return errMessages;
        }

        private void Log(string message, string level = "INFO")
        {
            
            string logMessage = string.Format("[{0}] {1}: {2}", 
                    DateTime.Now, 
                    level,
                    message);

            String dy = DateTime.Now.Day.ToString();
            String mn = DateTime.Now.Month.ToString();
            String yy = DateTime.Now.Year.ToString();
            string logFileName = String.Format("CloseTrades_{0}{1}{2}.log", yy, mn, dy);
            string logPath = LogFolderPath + logFileName;
            if(!Directory.Exists(LogFolderPath))
            {
                Directory.CreateDirectory(LogFolderPath);
            }
            
            Print(logMessage); // Log to terminal
            File.AppendAllText(logPath,logMessage + Environment.NewLine); // Log to log file

            if (level.SequenceEqual("ERROR")){
                Notifications.SendEmail(LogSendersAddress, LogRecipientAddress, "Error in CloseTrades cBot", logMessage);
            }
        }
    }
}

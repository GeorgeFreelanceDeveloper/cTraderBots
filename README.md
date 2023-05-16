# cTraderBots
cTrader bots for automation tasks in trading (place pending orders and manage risk).

## What Is cTrader Automate
cTrader Automate is a feature of cTrader desktop. It allows for developing and operating custom trading indicators and cBots.

## What Is a cBot
Think of cBots as programs that run inside cTrader desktop and autonomously execute and manage trading operations. cBots can be designed to perform almost any trading activity such as opening positions or placing orders depending on certain conditions.

## Developed cBots
* **EarlyReaction_cBot**: Bot checks whether there was a reaction to the trading level earlier, if so, it cancels the given trade.
* **MaxOpenPositions_cBot**: Bot controlling the maximum number of open positions, if more than allowed limit, the newer positions will be closed.
* **StopOut_cBot**: Bot checking for sufficient equity for trading, if the equity falls below the set limit, all positions and pending orders will be terminated.
* **CloseTradesAtTime_cBot**: Bot closing pending orders and position at defined time. You can set close for all or specific currency pairs.
* **PlacePendingOrders_cBot (In-development)**: Bot compute profit targes, amount orders and place pending ordes to trading platform from CSV file.

## Prerequisites
* [Windows Server 2022 download link](https://www.microsoft.com/en-us/evalcenter/download-windows-server-2022)
* [.NET Framework download link](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
* [cTrader download link](https://ctrader.com/download/)

## Ctrader cBot documentation
* [ctrader.com - documentation](https://help.ctrader.com/ctrader-automate/)
* [ctrader.com - references](https://help.ctrader.com/ctrader-automate/references/)

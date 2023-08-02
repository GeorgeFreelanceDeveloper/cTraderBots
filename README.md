# cTraderBots
cTrader bots for automation tasks in trading (place pending orders and manage risk) on commmodities market.

## What is cTrader Automate
cTrader Automate is a feature of cTrader desktop. It allows for developing and operating custom trading indicators and cBots.

## What is a cBot
Think of cBots as programs that run inside cTrader desktop and autonomously execute and manage trading operations. cBots can be designed to perform almost any trading activity such as opening positions or placing orders depending on certain conditions.

## Developed cBots
**Actively supported**
* **CommoditiesLevelTrader_cBot**: An automated bot for controlling trades on commodities. The bot helps reduce risk by adjusting positions when prices move favorably, cancel pending order when trade early reaction and eliminates open orders during sudden price spikes.
* **StopOut_cBot**: Bot checking for sufficient equity for trading, if the equity falls below the set limit, all positions and pending orders will be terminated.

**Old not supported**
* **EarlyReaction_cBot**: Bot checks whether there was a reaction to the trading level earlier, if so, it cancels the given trade.
* **MaxOpenPositions_cBot**: Bot controlling the maximum number of open positions, if more than allowed limit, the newer positions will be closed.
* **CloseTradesAtTime_cBot**: Bot closing pending orders and position at defined time. You can set close for all or specific currency pairs.
* **PlacePendingOrders_cBot (In-development)**: Bot compute profit targes, amount orders and place pending ordes to trading platform from CSV file.

## Development
Application is actively maintenance and develop.

## Prerequisites
* [Windows Server 2022 download link](https://www.microsoft.com/en-us/evalcenter/download-windows-server-2022)
* [.NET Framework download link](https://dotnet.microsoft.com/en-us/download/dotnet-framework)
* [cTrader download link](https://ctrader.com/download/)

## C# base tutorials
* [C# introduction webinar - freeCodeCamp.org](https://youtu.be/GhQdlIFylQ8)
* [C# programming quide - microsoft.com](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/)
* [C# - learnxinyminutes.com](https://learnxinyminutes.com/docs/csharp/)

## Ctrader cBot documentation
* [ctrader.com - documentation](https://help.ctrader.com/ctrader-automate/)
* [ctrader.com - references](https://help.ctrader.com/ctrader-automate/references/)

## Contributors
<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tr>
     <td align="center"><a href="https://github.com/GeorgeQuantAnalyst"><img src="https://avatars.githubusercontent.com/u/112611533?v=4" width="100px;" alt=""/><br /><sub><b>GeorgeQuantAnalyst</b></sub></a><br /><a href="https://github.com/GeorgeQuantAnalyst" title="Ideas">🤔</a></td>
    <td align="center"><a href="https://github.com/LucyQuantAnalyst"><img src="https://avatars.githubusercontent.com/u/115091833?v=4" width="100px;" alt=""/><br /><sub><b>LucyQuantAnalyst</b></sub></a><br /><a href="https://github.com/LucyQuantAnalyst" title="Code">💻</a></td>
  </tr>
</table>

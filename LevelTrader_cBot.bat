::Parameters
set ctid="YOUR_EMAIL_ADDRESS"
set pwd-file="ctrader.pwd"
set account="YOUR_ACCOUNT_ID"
set symbol="XAUUSD"
set period="m1"

:: Environment variables
set EntryPrice=2024
set StopLossPrice=2000
:: Direction values: 0 LONG, 1 SHORT
set Direction=0
set RiskPercentage=5

set RiskRevardRatio=1.5
set PlaceTradeDelayInMinutes=0
set MaxAllowedOpenTrades=2
set IsEnableTrailingStop=False
set TrailingStopLossLevel1Percentage=0.5
set TrailingStopLossLevel2Percentage=0.7

:: ctrader bot
ctrader-cli.exe run C:\Users\Administrator\Documents\cAlgo\Sources\Robots\LevelTrader_cBot.algo --ctid=%ctid% --account=%account% --pwd-file=%pwd-file% --symbol=%symbol% --period=%period% --environment-variables --EntryPrice=%EntryPrice% --PlaceTradeDelayInMinutes=%PlaceTradeDelayInMinutes% --MaxAllowedOpenTrades=%MaxAllowedOpenTrades% --StopLossPrice=%StopLossPrice% --Direction=%Direction% --RiskRevardRatio=%RiskRevardRatio% --RiskPercentage=%RiskPercentage% --IsEnableTrailingStop=%IsEnableTrailingStop% --TrailingStopLossLevel1Percentage=%TrailingStopLossLevel1Percentage% --TrailingStopLossLevel2Percentage=%TrailingStopLossLevel2Percentage%

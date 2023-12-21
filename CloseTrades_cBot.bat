::Parameters
set ctid="YOUR_EMAIL_ADDRESS"
set pwd-file="ctrader.pwd"
set account="YOUR_ACCOUNT_ID"
set symbol="EURUSD"
set period="m1"

::Environment-variables
set Day="5"
set Hours="20"
set Minutes="0"
set All="True"
set Market="Copper"

::ctrader bot
ctrader-cli.exe run C:\Users\Administrator\Documents\cAlgo\Sources\Robots\CloseTrades_cBot.algo --ctid=%ctid% --pwd-file=%pwd-file% --account=%account% --symbol=%symbol% --period=%period% --environment-variables --Day=%Day% --Hours=%Hours% --Minutes=%Minutes% --All=%All% --Market=%Market%
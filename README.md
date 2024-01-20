
$env:AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE = "127.0.0.1"
# Now run your command to start the Cosmos DB Emulator

docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

docker run --publish 8081:8081 --publish 10250-10255:10250-10255 --interactive --tty -e AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE=127.0.0.1 mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

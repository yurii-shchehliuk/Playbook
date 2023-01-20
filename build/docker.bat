cd ..
docker-compose -f .\docker-compose.yml -f .\docker-compose.override.yml up -d
:: to stop container
:: docker-compose -f .\docker-compose.yml -f .\docker-compose.override.yml down
:: to stop all images
:: docker stop $(docker ps -aq)
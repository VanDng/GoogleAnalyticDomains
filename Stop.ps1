taskkill /im wireshark.exe /f
taskkill /im WiresharkFeeder.exe /f

docker kill tcpdump
docker rm tcpdump

docker kill sample-app-container
docker rm sample-app-container
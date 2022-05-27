#
# Build Wireshark Feeder
#
dotnet restore ".\WiresharkFeeder\WiresharkFeeder.csproj"
dotnet publish ".\WiresharkFeeder\WiresharkFeeder.csproj" -c Release -o ".\WiresharkFeeder_Bin"

#
# Start up Wireshark Feeder
#

Start-Process -FilePath ".\WiresharkFeeder_Bin\WiresharkFeeder.exe" -WorkingDirectory ".\WiresharkFeeder_Bin"

#
# Build & run docker container
#

docker network rm network-analysis
docker network create network-analysis

docker build --progress=plain --no-cache -t sample-app -f Dockerfile .

docker kill sample-app-container
docker run -it -d --name sample-app-container --network network-analysis --rm sample-app  

docker kill tcpdump
docker run --name tcpdump -d --rm --net container:sample-app-container -v $PWD/Docker_TCPDump:/tcpdump kaazing/tcpdump:latest -vv -i any -w /tcpdump/tcpdump.pcap -U --immediate-mode

#
# Start up Wireshark
#

Start-Process -FilePath "C:\Program Files\Wireshark\Wireshark.exe" -ArgumentList "-i \\.\pipe\networkanalysis -k"
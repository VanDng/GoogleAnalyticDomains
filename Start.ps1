if (-not(Test-Path -Path ".\WiresharkFeeder_Bin\WiresharkFeeder.exe" -PathType Leaf)) {
    dotnet restore ".\WiresharkFeeder\WiresharkFeeder.csproj"
    dotnet publish ".\WiresharkFeeder\WiresharkFeeder.csproj" -c Release -o ".\WiresharkFeeder_Bin"
}
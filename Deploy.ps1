[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]
    $ServerName,
    [Parameter()]
    [string]
    $User = "pi"
)

$RemoteDirectory = "/home/pi/home.pi.lightserver"

Write-Host "Building...."
dotnet publish ./src/Home.Pi.LightServer --os linux --arch arm /t:PublishContainer -c Release
if (-not $?) {
    exit
}

docker push jesseryoung/home.pi.lightserver:latest
if (-not $?) {
    exit
}

$serverAddress = [System.Net.Dns]::GetHostByName($ServerName).AddressList[0].IPAddressToString

foreach($service in Get-ChildItem ./systemd)
{
    $serviceName = $service.Name
    # Check if service exists and is running
    ssh $User@$serverAddress sudo systemctl is-active --quiet $serviceName
    if ($?) {
        Write-Host "Stopping $serviceName on $serverAddress...."
        ssh $User@$serverAddress sudo systemctl stop $serviceName
        if (-not $?) {
            throw "Failed to stop $serviceName on $serverAddress."
        }
    }
}


Write-Host "Deploying...."
ssh $User@$serverAddress rm -rf $RemoteDirectory `
    && ssh $User@$serverAddress mkdir $RemoteDirectory `
    && scp -r -o user=$User ./configs/* $serverAddress`:$RemoteDirectory/ `
    && scp -r -o user=$User ./systemd/* $serverAddress`:$RemoteDirectory/ `
    && ssh $User@$serverAddress chmod +x $RemoteDirectory/*.service `
    && ssh $User@$serverAddress sudo ln -sf $RemoteDirectory/*.service /etc/systemd/system/

if (-not $?) {
    exit
}


ssh $User@$serverAddress sudo systemctl daemon-reload
if (-not $?) {
    throw "Failed to reload services $serverAddress."
}
foreach($service in Get-ChildItem ./systemd)
{
    $serviceName = $service.Name
    Write-Host "Restarting $serviceName...."
    ssh $User@$serverAddress sudo systemctl start $serviceName
    if (-not $?) {
        throw "Failed to restart $serviceName on $serverAddress."
    }
}

#move one level up in directory
Push-Location -Path "..\"

$lb = ".\publish\LoadBalancer.dll"
$ws = ".\publish\WebServer.dll"
$logdir = ".\log"

#check the file exists
if (!(Test-Path $lb)) {
    Write-Host
    Write-Host "Publishing files...."
    Start-Process dotnet -PassThru -NoNewWindow -Wait -ArgumentList "publish .\SLB.sln -o ..\..\publish"
}

Write-Host "Starting Load balancer"
$pid_lb = Start-Process dotnet -PassThru -RedirectStandardOutput $logdir\lb.out -RedirectStandardError $logdir\lb.err -ArgumentList $lb

Write-Host
Write-Host "Starting Web API servers"

1 .. 3 | ForEach-Object {
    Start-Process dotnet -NoNewWindow -PassThru -RedirectStandardOutput .\webapiserver$_".out" -RedirectStandardError .\webapiserver$_".err" -ArgumentList $ws" --server.urls http://localhost:6000"$_
}

Write-Host
#list the services
Write-Host "List of running dotnet process"
Get-Process dotnet

#Start-Process dotnet -PassThru -WorkingDirectory "..\bin\LoadBalancer" -RedirectStandardOutput .\lb.out -RedirectStandardError .\lb.err -ArgumentList "netcoreapp2.0\LoadBalancer.dll"

Pop-Location
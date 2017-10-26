1..9 | % {

    # Define what each job does
    $scriptBlock = {
        param($file)
        Invoke-RestMethod -Method Get http://localhost:60000/blog -PassThru -OutFile .\web$file.out
    }

    # Execute the jobs in parallel
    start-job $scriptBlock -ArgumentList $_
}

Get-Job

# Wait for it all to complete
While (Get-Job -State "Running") {
    Start-Sleep 10
}

# Getting the information back from the jobs
Get-Job | Receive-Job
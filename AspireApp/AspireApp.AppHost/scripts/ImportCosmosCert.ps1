# Prompt the user for the port number
$port = Read-Host -Prompt "Enter the port number for the CosmosDB Emulator (e.g., 60553)"

# Define the URL using the user-provided port
$certUrl = "https://localhost:$port"

# Define the path where you want to save the certificate
$certPath = "C:\Temp\localhost.cer"

# Create the directory if it doesn't exist
$certDir = Split-Path -Path $certPath
if (!(Test-Path -Path $certDir)) {
    New-Item -ItemType Directory -Path $certDir -Force
}

# Download the SSL certificate from the endpoint and save it as a .cer file
try {
    $tcpClient = New-Object Net.Sockets.TcpClient
    $tcpClient.Connect("localhost", [int]$port)
    $stream = $tcpClient.GetStream()
    $sslStream = New-Object System.Net.Security.SslStream($stream, $false, ({$true}))
    $sslStream.AuthenticateAsClient("localhost")
    $cert = $sslStream.RemoteCertificate
    $certBytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    [System.IO.File]::WriteAllBytes($certPath, $certBytes)
    $tcpClient.Close()

    Write-Output "Certificate saved to $certPath"
} catch {
    Write-Error "Failed to download the certificate: $_"
    exit
}

# Remove any existing Cosmos Emulator certificates
gci cert:\LocalMachine\My | ? { $_.FriendlyName -eq 'CosmosEmulatorContainerCertificate' } | foreach { Remove-Item $_.PSPath }
gci cert:\LocalMachine\Root | ? { $_.FriendlyName -eq 'CosmosEmulatorContainerCertificate' } | foreach { Remove-Item $_.PSPath }

# Import the downloaded certificate to the LocalMachine Root store
Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\Root -Verbose

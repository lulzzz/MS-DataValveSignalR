#
# CreateEnvVars.ps1
#

$eventHubName = "fusethrudemo"
$eventHubNamespace = "fusethrudemo-ns"
$eventHubKeyName = "FuseThruPOC"
$eventHubKey = "GfvUiioXtnspcwGTn7uT06fx7pF3D6WzaPQgN+pnLHY="
$deviceDataOrigin = "eventHub"
$storageName = "fusethrudemo"
$storageKey = "oft/h53OTIio07ZTUTu67fXY0/1Ip6Aku7mdMcRhm+z4T1v1UARfBBIFGWnqPVhGzdQLLNLYjCYcOSHcBEu3mA=="

[Environment]::SetEnvironmentVariable("eventHubName", $eventHubName, "User")
[Environment]::SetEnvironmentVariable("eventHubNamespace", $eventHubNamespace, "User")
[Environment]::SetEnvironmentVariable("eventHubKeyName", $eventHubKeyName, "User")
[Environment]::SetEnvironmentVariable("eventHubKey", $eventHubKey, "User")
[Environment]::SetEnvironmentVariable("storageName", $storageName, "User")
[Environment]::SetEnvironmentVariable("storageKey", $storageKey, "User")
[Environment]::SetEnvironmentVariable("deviceDataOrigin", $deviceDataOrigin, "User")

# Delete ENV VAR if needed
# [Environment]::SetEnvironmentVariable("TestVariable", $null, "User")
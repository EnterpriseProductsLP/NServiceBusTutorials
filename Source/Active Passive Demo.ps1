# Ready workspace ------------------------------------------------------------------------------------------------------
Clear-Host
& git clean -xdf
& .\.nuget\nuget.exe restore ActivePassive.sln
# ----------------------------------------------------------------------------------------------------------------------



# Compile solution -----------------------------------------------------------------------------------------------------
& 'C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe' ActivePassive.sln /t:Build /p:Configuration=Release /p:TargetFramework=v4.6.1
# ----------------------------------------------------------------------------------------------------------------------



# Create demo directory ------------------------------------------------------------------------------------------------
& Robocopy.exe .\NServiceBusTutorials.ActivePassive.Publisher\bin\Release\ "ActivePassiveDemo\Publisher"
& Robocopy.exe .\NServiceBusTutorials.ActivePassive.Consumer\bin\Release\ "ActivePassiveDemo\Consumer1"
& Robocopy.exe .\NServiceBusTutorials.ActivePassive.Consumer\bin\Release\ "ActivePassiveDemo\Consumer2"
& Robocopy.exe .\NServiceBusTutorials.ActivePassive.Consumer\bin\Release\ "ActivePassiveDemo\Consumer3"

(Get-Content .\ActivePassiveDemo\Consumer1\NServiceBusTutorials.ActivePassive.Consumer.exe.config).Replace('Machine1', 'Consumer1') | Set-Content .\ActivePassiveDemo\Consumer1\NServiceBusTutorials.ActivePassive.Consumer.exe.config
(Get-Content .\ActivePassiveDemo\Consumer2\NServiceBusTutorials.ActivePassive.Consumer.exe.config).Replace('Machine1', 'Consumer2') | Set-Content .\ActivePassiveDemo\Consumer2\NServiceBusTutorials.ActivePassive.Consumer.exe.config
(Get-Content .\ActivePassiveDemo\Consumer3\NServiceBusTutorials.ActivePassive.Consumer.exe.config).Replace('Machine1', 'Consumer3') | Set-Content .\ActivePassiveDemo\Consumer3\NServiceBusTutorials.ActivePassive.Consumer.exe.config
# ----------------------------------------------------------------------------------------------------------------------



# Initialize RabbitMQ --------------------------------------------------------------------------------------------------
# Create the NServiceBusTutorials user
rabbitmqctl delete_user NServiceBusTutorials
rabbitmqctl add_user NServiceBusTutorials 1breakthings!
rabbitmqctl set_user_tags NServiceBusTutorials administrator

# Create the NServiceBusTutorials virtual host 
rabbitmqctl delete_vhost NServiceBusTutorials
rabbitmqctl add_vhost NServiceBusTutorials

# Grant user permissions on the virtual host
rabbitmqctl set_permissions -p NServiceBusTutorials admin ".*" ".*" ".*"
rabbitmqctl set_permissions -p NServiceBusTutorials NServiceBusTutorials ".*" ".*" ".*"

# Add policies for high availablity on exchnages and high availability + federation on queues
rabbitmqctl set_policy -p NServiceBusTutorials --priority 0 --apply-to "exchanges" ha-exchanges ".*" '{""ha-mode"": ""all""}'
rabbitmqctl set_policy -p NServiceBusTutorials --priority 0 --apply-to "queues" ha-queues ".*" '{""ha-mode"": ""all""}'
# ----------------------------------------------------------------------------------------------------------------------



# Run the damned thing! ------------------------------------------------------------------------------------------------
# Create some publishers
Start-Process .\ActivePassiveDemo\Publisher\NServiceBusTutorials.ActivePassive.Publisher.exe
Start-Process .\ActivePassiveDemo\Publisher\NServiceBusTutorials.ActivePassive.Publisher.exe
Start-Process .\ActivePassiveDemo\Publisher\NServiceBusTutorials.ActivePassive.Publisher.exe

# Create some consumers.
Start-Process .\ActivePassiveDemo\Consumer1\NServiceBusTutorials.ActivePassive.Consumer.exe
Start-Process .\ActivePassiveDemo\Consumer2\NServiceBusTutorials.ActivePassive.Consumer.exe
Start-Process .\ActivePassiveDemo\Consumer3\NServiceBusTutorials.ActivePassive.Consumer.exe
# ----------------------------------------------------------------------------------------------------------------------

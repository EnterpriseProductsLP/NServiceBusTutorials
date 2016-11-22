# Create the NServiceBusTutorials user
rabbitmqctl add_user NServiceBusTutorials 1breakthings!
rabbitmqctl set_user_tags NServiceBusTutorials administrator

# Create the NServiceBusTutorials virtual host
rabbitmqctl add_vhost NServiceBusTutorials

# Grant user permissions on the virtual host
rabbitmqctl set_permissions -p NServiceBusTutorials NServiceBusTutorials ".*" ".*" ".*"
rabbitmqctl set_permissions -p NServiceBusTutorials eburcham ".*" ".*" ".*"

# Add policies for high availablity on exchnages and high availability + federation on queues
rabbitmqctl set_policy -p NServiceBusTutorials --priority 0 --apply-to "exchanges" ha-exchanges ".*" '{""ha-mode"": ""all""}'
rabbitmqctl set_policy -p NServiceBusTutorials --priority 0 --apply-to "queues" ha-queues ".*" '{""ha-mode"": ""all""}'

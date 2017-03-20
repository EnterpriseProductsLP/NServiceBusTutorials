# Create the admin user
rabbitmqctl delete_user admin
rabbitmqctl add_user admin 1breakthings!
rabbitmqctl set_user_tags admin administrator

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

#!/bin/bash

echo "Starting ScyllaDB..."

sleep 15 

cqlsh scylla-node1 9042 -f /init.cql
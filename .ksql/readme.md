## How to execute per cluster
- put the all script `ALL_01_BUILDING_SNAPSHOT_OSLO_STREAM` & `ALL_02_BUILDINGUNIT_SNAPSHOT_OSLO_STREAM` into ksqlDB
- Set auto.offset.reset = earliest
- execute
- now do the same for the specific script(s)

## How to set up the connectors
- put the connectors script into ksqlDB
- replace *** with the secrets from last pass
- run the script
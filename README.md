# Aqualab
## Logging Events
Each log is sent with a number of fields required by [SimpleLog](https://github.com/fielddaylab/simplelog). Simple log allows for a custom field called event_data_complex along with its category enumerator:
  event_custom: category enumerator
  event_data_complex: JSON.stringify(log_data)
Each log_data is a JSON object for that specific category as defined below.

#### Bugs
1. When a behavior is changed, the previous sync is logged rather than the updated sync.

#### Version Log
Versions:
1. Alpha

### Event Categories
1. [modelingbehaviorchange](#modelingbehaviorchange)
2. [modelingtickchange](#modelingtickchange)
3. [modelingsyncchange](#modelingsyncchange)

### Data Structures
1. [Rule Data](#RuleData)

<a name="modelingbehaviorchange"/>

## Event Categories
#### modelingbehaviorchange (index=1)
| Key | Value | Description |
| --- | --- | --- |
| scenario_id | scenario_id | id of the current scenario being modeled |
| curr_tick | curr_tick | the current tick of the model being viewed |
| curr_sync | curr_sync | the current sync value between the model and the data |
| prev_value | prev_value | the previous value for this behavior |
| rule_data | rule_data | the organism affected by the change, the type of behavior being changed, and the new value for that behavior |

<a name="modelingtickchange"/>

#### modelingtickchange (index=2)
| Key | Value | Description |
| --- | --- | --- |
| scenario_id | scenario_id | id of the current scenario being modeled |
| prev_tick | prev_tick | the previous tick of the model being viewed |
| new_tick | new_tick | the new tick of the model being viewed |

<a name="modelingsyncchange"/>

#### modelingsyncchange (index=3)
| Key | Value | Description |
| --- | --- | --- |
| scenario_id | scenario_id | id of the current scenario being modeled |
| prev_sync | prev_sync | the previous sync value between the model and the data |
| new_sync | new_sync | the new sync value between the model and the data |

## Data Structures
<a name="RuleData"/>

#### Rule Data
| Key | Value | Description |
| --- | --- | --- |
| Organism | Organism | the organism that this rule data describes |
| ValueType | ValueType | the type of behavior that this value describes |
| CurrValue | CurrValue | numeric value describing this behavior |
# Assignment
## User Stories

### As a User, given a csv, I can produce a json file of valid csv records
* csv files will be placed into _`input-directory`_
    * files may not be present at application start
    * file names will end in `.csv`
* csv columns and validation
    1. `INTERNAL_ID` : 8 digit positive integer. Cannot be empty.
    * `FIRST_NAME` : 15 character max string. Cannot be empty.
    * `MIDDLE_NAME` : 15 character max string. Can be empty.
    * `LAST_NAME` : 15 character max string. Cannot be empty.
    * `PHONE_NUM` : string that matches this pattern ###-###-####. Cannot be empty.
* json files should be output to _`output-directory`_
    * file name should be the same name as the csv file with a `.json` extension
* json format:
    ```js
[
    {
        "id": <INTERNAL_ID>,
        "name": {
            "first": "<FIRST_NAME>",
            "middle": "<MIDDLE_NAME>",
            "last": "<LAST_NAME>"
        },
        "phone": "<PHONE_NUM>"
    }
]
    ```

#### Example

input of:

```
INTERNAL_ID,FIRST_NAME,MIDDLE_NAME,LAST_NAME,PHONE_NUM
12345678,Bobby,,Tables,555-555-5555
```

would produce:

```json
[
    {
        "id": 12345678,
        "name": {
            "first": "Bobby",
            "last": "Tables"
        },
        "phone": "555-555-5555"
    }
]
```
---

### As a User, I can produce a csv file containing validation errors
* if a `error-directory` is not configured, do not produce an error file
* error records should be written to a csv file in _`error-directory`_
* an error record should contain:
    1. `LINE_NUM` : the number of the record which was invalid
    * `ERROR_MSG` : a human readable error message about what validation failed

---

### As a User, I can configure input, output, and error directories

---

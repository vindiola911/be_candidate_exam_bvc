# Assignment
## The Business Case
An expensive consultancy has convinced our business that keeping metrics on our user's ice cream preferences is the key to our success. They request the construction of an internal dashboard which will list all of our users and their associated ice cream flavor preferences in a ranked order.

Your task is to build the API for this system.

## The Requirements
### User List File CSV
Our user list is maintained by a separate system which has been deemed "too scary" to make changes to. Luckily, long ago its designer hardcoded a boolean that will output the entire user list as a CSV file once per day. The file adheres to the following format:
```
INTERNAL_ID,FIRST_NAME,MIDDLE_NAME,LAST_NAME,PHONE_NUM
```
#### Field Formats
* INTERNAL_ID : 8 digit positive integer. Cannot be empty.
* FIRST_NAME : 15 character max string. Cannot be empty.
* MIDDLE_NAME : 15 character max string. Can be empty.
* LAST_NAME : 15 character max string. Cannot be empty.
* PHONE_NUM : string that matches this pattern ###-###-####. Cannot be empty.

### Ice Cream Flavor Preference CSV
The expensive consultancy has pledged to sell us ice cream preferences data as part of their premium service level. As a result, they will be providing data mined ice cream preferences to us in the form a daily CSV file. The file adheres to the following format:
```
TRACKING_ID,FULL_NAME,PHONE_NUM,ICE_CREAM_FLAVOR,ICE_CREAM_FLAVOR_RANK
```
The data in this file is gathered through a dubious beacon tracking method and isn't always reliable. As a result, the following fields can potentially be empty:
* FULL_NAME
* PHONE_NUM
* ICE_CREAM_FLAVOR_RANK

The following fields are guaranteed to be populated:
* TRACKING_ID
* ICE_CREAM_FLAVOR

#### Field Formats
* TRACKING_ID : 10 digit positive integer
* FULL_NAME : 100 character max string
* PHONE_NUM : string that matches this pattern ###-###-####
* ICE_CREAM_FLAVOR : 40 character max string
* ICE_CREAM_FLAVOR_RANK : positive integer from 1-10

### Matching Criteria
Each record in the Ice Cream Flavor Preference File should match to a single user in our user list. If it does not match, the record should be ignored. The order of significance for matching is as followed:
1. FULL_NAME and PHONE_NUM both match
1. If PHONE_NUM is empty, then match on FULL_NAME
1. If FULL_NAME is empty, then match on PHONE_NUM

### API
The front end team has agreed to follow your lead on how to interface with the API you create. Write a short description explaining how to use your API.

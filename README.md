## Credit Score Service
#### InputJson
`{  
   "SSN":"XXXXXX-XXXX",
   "Months":12,
   "Amount":100000
}`

#### OutputJson
`{  
   "SSN":"XXXXXX-XXXX",
   "Months":12,
   "Amount":100000,
   "CreditScore": 350
}`

## Bank Router Service
#### InputJson
`{  
   "SSN":"XXXXXX-XXXX",
   "Months":12,
   "Amount":100000,
   "CreditScore": 350,
   "Banks" : ["BankJSON", "BankSOAP"]
}`

#### OutputJson
`{  
   "SSN":"XXXXXX-XXXX",
   "Months":12,
   "Amount":100000,
   "CreditScore": 350
}`


## BankSOAP Service
URL: /BankService.asmx

## BankSOAP Translator Service
#### InputJson
`{  
   "SSN":"XXXXXX-XXXX",
   "Months":12,
   "Amount":100000,
   "CreditScore": 350
}`

#### OutputJson
`{  
   "SSN":"XXXXXX-XXXX",
   "IntrestRate": 4.1
}`



# Paypal Adaptive Payments API integration module
Paypal Adaptive Payments API payment gateway integration module provides integration with <a href="https://developer.paypal.com/docs/classic/adaptive-payments/integration-guide/APIntro/" target="_blank">Paypal Adaptive Payments</a> API.

# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Paypal Adaptive Payments -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-Paypal-AdaptivePayments/releases. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# Module management and settings UI
![image](https://cloud.githubusercontent.com/assets/5801549/16539365/7a3b5344-4049-11e6-9913-edfc7f5aacff.png)

# Settings
* **Receiver Account Id** - Account Id of the money receiver PayPal account holder
* **API Username** - Merchant API Username credential
* **API password** - Merchant API password
* **API signature** - Merchant API signature credential
* **Working mode** - Type of working mode (Sandbox or Live)
* **Relative callback path** - Relative URL on Storefront to redirect after approving a payment on paypal.com (cart/externalpaymentcallback by default)
* **Paypal AppID** - Merchant application ID. Use APP-80W284485P519543T for testing.


# License
Copyright (c) Virtosoftware Ltd.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.

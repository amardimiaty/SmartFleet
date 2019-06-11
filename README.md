# SmartFleet
Gps Tracker yet supports FMXXX teltonika and Tk103 protocols.  
how to test the listners:

for devices that use Tk103 protocol like Gt02a  and thier chinese clones :

1- send the server config to your GPS tracker.

2- run the two projects inside Test Directory : GT02AServer  and EdgeService (service for data storing).

if you want to test web project  :

1- sign in using admin@smartFleet/123456  account .

2- create a customer account (its aleardy exist two accounts from the initial seed ).

3- add a vehicle with the mountend  Gps device and the owner customer (once gps device sends data to the server it'll be stored on the database ).

4 - sign in by the customer account and go th the home to see the list of vehicles with thier positions on the map.
![](https://github.com/pentest30/SmartFleet/blob/master/src0.png)
![](https://github.com/pentest30/SmartFleet/blob/master/src1.png)
![](https://github.com/pentest30/SmartFleet/blob/master/src2.png)
![](https://github.com/pentest30/SmartFleet/blob/master/src3.png)
![](https://github.com/pentest30/SmartFleet/blob/master/src6.png)
![](https://github.com/pentest30/SmartFleet/blob/master/src4.png)
![](https://github.com/pentest30/SmartFleet/blob/master/src5.png)

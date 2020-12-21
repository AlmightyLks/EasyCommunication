# EasyCommunication

## Description

EasyCommunication is there to offer you a modular & easy way of making apps communicate with a Host-Client structure.  
This library is based on a BinaryFormatter, so either your transmited data's type has to be tagged with `[Serializable]`, or alternatively it has to be convertable with Newtonsoft.Json.  
A get-started example you can find [here](https://github.com/AlmightyLks/EasyCommunication/tree/Example).

---
## Dependencies

- .NET Standard             2.0  
- Newtonsoft.Json           12.0.3  
- Serilog                   2.10.0  
- Serilog.Sinks.Console     2.10.0  

---

# [Complete Documentation](https://almightylks.gitbook.io/easycommunication/home)

## Library includes:

### [EasyHost](https://almightylks.gitbook.io/easycommunication/home/easyhost/class-easyhost)
  - Listens for incoming EasyClient connections & send and receive data from connected EasyClients
### [EasyClient](https://almightylks.gitbook.io/easycommunication/home/easyclient/class-easyclient)
  - Establish a connection and communicate with an EasyHost & send and receive data from a connected EasyHost  

### [Events](https://almightylks.gitbook.io/easycommunication/home/events)  
  - #### [Host](https://almightylks.gitbook.io/easycommunication/home/events/host)  
    - ##### [ClientConnected](https://almightylks.gitbook.io/easycommunication/home/events/host/eventhandler/class-hosteventhandler#EasyCommunication_Events_Host_EventHandler_HostEventHandler_ClientConnected)
          - Event fired when an EasyClient connects  
    - ##### [ClientDisconnected](https://almightylks.gitbook.io/easycommunication/home/events/host/eventhandler/untitled#EasyCommunication_Events_Host_EventHandler_HostEventHandler_ClientDisconnected)
          - Event fired when an EasyClient disconnects  
    - ##### [ReceivedData](https://almightylks.gitbook.io/easycommunication/home/events/host/eventhandler/class-hosteventhandler#EasyCommunication_Events_Host_EventHandler_HostEventHandler_ReceivedData)
          - Event fired when data is received from an EasyClient  
    - ##### [SendingData](https://almightylks.gitbook.io/easycommunication/home/events/host/eventhandler/class-hosteventhandler#EasyCommunication_Events_Host_EventHandler_HostEventHandler_ReceivedData)
          - Event fired when an EasyHost is sending data to an EasyClient  
          
  - #### [Client](https://almightylks.gitbook.io/easycommunication/home/events/host)  
    - ##### [ConnectedToHost](https://almightylks.gitbook.io/easycommunication/home/events/client/eventhandler/class-clienteventhandler#EasyCommunication_Events_Client_EventHandler_ClientEventHandler_ConnectedToHost)
          - Event fired when an EasyClient connected to an EasyHost
    - ##### [DisconnectedFromHost](https://almightylks.gitbook.io/easycommunication/home/events/client/eventhandler/untitled#EasyCommunication_Events_Client_EventHandler_ClientEventHandler_DisconnectedFromHost)
          - Event fired when an EasyClient loses connection from an EasyHost
    - ##### [ReceivedData](https://almightylks.gitbook.io/easycommunication/home/events/client/eventhandler/class-clienteventhandler#EasyCommunication_Events_Client_EventHandler_ClientEventHandler_ReceivedData)
          - Event fired when data is received from an EasyHost
    - ##### [SendingData](https://almightylks.gitbook.io/easycommunication/home/events/client/eventhandler/class-clienteventhandler#EasyCommunication_Events_Client_EventHandler_ClientEventHandler_SendingData)
          - Event fired when an EasyClient is sending data to an EasyHost


# ISComm V3 #
---

ISComm is responsible for the Server to Server communication between CellAO Server instances (Zones) and Chat/Login servers.

It uses [msgpack](https://github.com/msgpack/msgpack-cli) to to serialize the data objects in a dynamic fashion, so **nearly every** type of object can be sent.

The [TinyMessenger](https://github.com/grumpydev/TinyMessenger) ([TinyIoC](https://github.com/grumpydev/TinyIoC)) hub then will distribute the object to the corresponding handler classes.

ISComm V3 is receiving fully asynchronous, compresses the datastream by default and therefore is able to handle a great load of traffic.

There are some exceptions to the objects that can be sent, such as *Exception* and others with public properties without a public setter.
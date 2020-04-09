# Overview
This sample demonstrates a basic implementation of the identity translation pattern between an MQTT broker and IoT Hub devices. 

# Setup

## deviceidlist.template.json
This file contains credentials for the upstream IoT Hub device and the downstream MQTT broker.
- Add values for each device whose identity is mapped to a device in IoT Hub
- Rename the file to `deviceidlist.json`
- Upload to a blob storage account

## Edge Device Twin 
- Add a desired property named `deviceListFile` to the `IdentityTranslationModule`
- Set the value for this propert to the blobstorage url for the uploaded `deviceidlist.json' file


# Limitations
This is only a sample demonstrating the identity translation concept, production ready implementations will need to consider specific performance, error handling and payload transformation requirements. Performance in particular has not been tested at scale, low level networking is managed by the MQTTNet package.  


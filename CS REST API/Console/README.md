# Demo CS REST API

Console application demonstrating use of CS REST API from C#.

## Requirements

- A freja ID with a swedish social security number 
 
or

- A swedish bankid

## Terminology

- CS - Certificate Server

## Setup

To run the demo program the `apiHost` configuration value must be set to a server URL that runs the Certificate Server. Other parameters are optional and, if not set, the program will prompt for the values in the console. 

**Default config:**

```json
{
    "apiHost": "",
    "system": "",
    "provider": "",
    "certificateIssuer": ""
}
```

**NOTE**

Please contact sales at ciceron@twoday.com for apiHost and system keys.

### Configuration

**provider**

May be either `bankid` or `freja`. If the configuration value is invalid the program will prompt the user for a new value.

**system**

The target system. Identified and ordered from Twoday.

**apiHost**

The host of the CS REST API.

**certificateIssuer** (optional)

A client certificate to be used when communicating with a production server. 

## Run the application

Run in Visual Studio or with `dotnet run`.
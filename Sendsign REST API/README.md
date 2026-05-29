# Demo Sendsign REST API application(s)

A console and web application demonstrating use of Sendsign REST API using C# and dotnet 8.0.

## Requirements

- A `customer key` provided by twoday.
- A `sender ID` (personal number) provided by twoday.

**NOTE**

Please contact sales at ciceron@twoday.com for `ApiUrl`, `customer key` and `Sender`.

## Configuration

To run the demo program the `ApiUrl` configuration value must be set to a server URL that runs the Sendsign service.

**Default config (appsettings.json):**

```json
{
    "ApiUrl": "",
}
```

## Run the application

Run in Visual Studio or with `dotnet run` in the corresponding project folder. Either in the [Console](Console/) or in the [Web](Web/) folder.

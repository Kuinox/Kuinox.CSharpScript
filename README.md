<h1 align="center">
	Kuinox.CSharpScript
</h1>
<p align="center">
  A simple C# scripting dotnet tool.
</p>

## Getting Started
Install it with `dotnet tool install -g Kuinox.CSharpScript`  
Execute a C# file with `c# Program.cs` in your favorite terminal.

## .csproj configuration
This is where this lib do things differently.  
You can put your own csproj configuration at the top of the file to override the projects defaults.  
It allow you to import nuget package for example, or configure the project how you want.

```csharp
<Project>
  <ItemGroup>
    <PackageReference Include="CK.MQTT.Client" Version="0.8.2" />
  </ItemGroup>
</Project>
IMqtt3Client client = MqttClient.Factory.CreateMQTT3Client( new MqttClientConfiguration( "test.mosquitto.org:1883" ),
( IActivityMonitor? m, ApplicationMessage msg, CancellationToken token ) =>
{
    Console.WriteLine( msg.Topic );
    return new ValueTask();
} );
await Task.Delay( 10000 );
```
(This program will listen to the mosquitto public mqtt server for 10 seconds.)

# SecurityDroneTest

Drone communication idea for OU Computer Security class

How to use:

- Clone the code
- You will need to perform a nuget restore
` nuget restore SecurityDroneTest.sln `
You can also do this in Visual Studio through the Nuget Package Manager
- Build the code
- You'll need to run two instances
- Open two terminals and make your way to the /bin/ directory Ex: `SecurityDroneTest\bin\Debug\netcoreapp3.1`
- The first is the Drone `.\SecurityDroneTest.exe -m Drone `
- The Second is the Controller `.\SecurityDroneTest.exe -m Controller -i [ip from Drone] -p [port from Drone]`
- In the Controller terminal type a number and it will be sent to the Drone
- Drone should display decrypted number 

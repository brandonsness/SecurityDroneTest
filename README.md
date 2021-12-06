# SecurityDroneTest

Drone communication idea for OU Computer Security class

How to use:

- Clone the code
- You will need to perform a nuget restore
` nuget restore SecurityDroneTest.sln `
You can also do this in Visual Studio through the Nuget Package Manager
- Build the code
- You'll need to run two instances
- The first terminal is used to generate the data. `.\SecurityDroneTest.exe -m DataGenerator -f [output file path]`
- This simple tool will allow you to generate test data
- Open three terminals and make your way to the /bin/ directory Ex: `SecurityDroneTest\bin\Debug\netcoreapp3.1`
- The second is the Drone `.\SecurityDroneTest.exe -m [Drone mode] `
- Drone modes are `Drone` for our implementation and `DroneAES` and `DroneRSA` for AES and RSA respectively
- The third is the Controller `.\SecurityDroneTest.exe -m [Controller Mode] -i [ip from Drone] -p [port from Drone] -f [input file path]`
- Controller modes are `Controller` for our implementation and `ControllerAES` and `ControllerRSA` for AES and RSA respectively
- Drone and Controller should each display the time it took to decrypt/encrypt instructions
- you can play around with the commented out Console.WriteLine() statements if you want to check that the output is correctly decrypted


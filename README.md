# UEnv

A lightweight framework for Python programming, using Unity as a graphical environment.

## Concept

Unity runs a specified python script as subprocess. 
Unity gets input and output of python process through STDIN / STDOUT redirection.
They send messages in base64 encoded bytearray.

## Requirements

1. Unity
2. Your own C# IDE

## How To Use

1. Clone this repo
2. Edit UEnvConfig scriptable object
3. Run Unity

Once UEnvConfig is completed, then You can see example code.

## Functionality : Auto Packet Gneration

Unity, Python interact each other through packet.
Packet is a simple class instance, which has integer value key-code and bytearray data.


This framework offers auto-packet generation.
You can see test-packet definition in "PacketDef.xml".
Once you specified your own packet definition, You can generate packets through "UIEnv/Generate Packet" button in Unity Menu item.


Be warned that the auto packet generation will change contents of script files packet_factory.py and PacketFactory.cs.

## UEnvConfig

UEvnConfig scriptable object contains every informations for how framework should work.

1. Python Path : Specifying your python binary path
2. Script Path : Path for python script which will be run.
3. Packet Def : Field for PacketDef.xml asset. Auto Packet Generation will reference this file.
4. PyPacket Factory Path : Path for python script to which packet definitions will be generated.
5. CsPacket Factory : Field for PacketFactory.cs asset. Packet class will be generated into this files.

## Debugging

This framework contains test code, and in main.py, it will open debugpy server.
So if you use VSCode, then you can simply attach VSCode debugger to python for debbuging.
Of course, if you don't use VSCode, then just remove it.

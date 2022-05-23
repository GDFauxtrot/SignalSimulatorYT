# SignalSimulatorYT
A Signal Simulator mod that adds YouTube support to the in-game VHS.

SignalSimulatorYT is a first attempt at modifying the Unity game "Signal Simulator". It adds support for watching YouTube embedded videos in the in-game VHS screen.

Made with a collection of tools:

  * HarmonyX v2.10.0
  * ZenFulcrum Embedded Browser (Unity store asset, license acquired)
  * Visual Studio 2017
  * A lot of time after work and hair pulled from YouTube embedding

I do not plan on updating this often! There will be some fixes here and there, but for the most part I want to focus on other modding efforts around Signal Simulator. I also do not expect there to be many updates for Signal Simulator in general, so if something updates and breaks this mod, I may come back to provide a fix.

## Download

Check the sidebar on the right for releases! Be sure to follow the instructions included carefully.

## How to modify

Included with each release is a compiled binary for the Windows version of ZFBrowser 3.0.0, which is required in order to build any changes. It is the exact same version included with the Windows demo. However, If you do wish to make your own changes, make sure to follow the steps below.

(A license was purchased from the Unity asset store in order to use the Embedded Browser for this mod, as well as the creator's permission for its use here. You do not need a license if you wish to make your own changes, but it is highly recommended! It's also a great asset to have in general.)

  * Clone (or download) the repo. Make sure to follow the initial setup guide provided by BepInEx: https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/1_setup.html (you only need to follow this first page, but be sure to read them all to get up to speed)
  * Open the provided .csproj files in your IDE of choice (probably gonna need Visual Studio for these, the free Community edition works fine).
    * SignalSimulatorYT.csproj builds all of the main mod components into a dll of the same name - this is where most of your changes will be done.
    * SignalSimulatorYT.Shared.csproj is a separate library and namespace to use for writing Unity components. The idea is that you can build this project into a dll, add it to a Unity scene, and then any MonoBehaviours or data structures can be used in order to create prefabs and other assets (via AssetBundles) and also be accessible in your mod!
      * If you wish to add new files to the Shared namespace, be sure they are excluded from the main project's csproj file, and vice versa!
      * Making changes to Shared also means copying the dll back into the project's lib folder, replacing the one placed there for you by me. I recommend using batch scripts to make building and copying a one-click solution.
  * Make code changes and then build using the "dotnet build [name of csproj file]" command.
  * Take the new dll's from your bin/Debug/net46 folder that just got built, and drop them into the Signal Simulator/BepInEx/plugins folder, replacing the originals.
    * Also recommend batch scripting this operation as well!
  * Be sure to utilize the BepInEx log outputs to do any debugging work! There is also this handy page for dev tools: https://docs.bepinex.dev/articles/dev_guide/dev_tools.html

After all these steps, you should have a working modification to this mod up and running!

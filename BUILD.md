# Build / Download Notes

## Source download
Once this folder is pushed to GitHub, use **Code > Download ZIP** or clone the repository and open it with Unity Hub.

## Windows executable
1. Open the project in Unity 6.x.
2. Run **Engine Writer > Create / Refresh Demo Scene**.
3. Open **File > Build Profiles**.
4. Select Windows and add/switch the profile.
5. Confirm `Assets/EngineWriter/Scenes/EngineWriter.unity` is the enabled scene.
6. Build to a folder such as `Builds/Windows`.

## Linux / macOS
Use the same scene and choose the matching desktop build profile/module in Unity Hub.

## GitHub Releases later
A compiled desktop build can be attached to a GitHub Release. Automated Unity builds are intentionally not enabled in this MVP because CI builds require Unity licensing credentials/secrets; source download works without any secret configuration.

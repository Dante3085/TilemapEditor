# TilemapEditor
TilemapEditor made with MonoGame. Original implementation is from Dante3085/EVCMonoGame project.

# TODO / Ideas
1. Polish
  1.1 Code-Cleanup: Understand TilemapEditor code again and make it clearer and simpler wherever possible.
  
2. New Features
  2.1 Infotext: Gives information about current state of TilemapEditor (Examples: 123 Tiles copied; Unsaved Changes; ...)
  2.2 Hotkeys and general Actions: Hotkeys and Mouse-Buttons for Actions need to make sense. Left-Mouse-Button for most
      stuff instead of Right-Mouse-Button (For some reason we have RectangleSelection on Right-Mouse-Button; ...)
  2.3 Context-Menu: Right-Mouse-Button to open a List-Widget that offers several options depending on Selection and
                    other stuff.

3. Standalone Software
  3.1 Take TilemapEditor out of EVCMonoGame project and realize it as an independent program.
  3.2 Inputs and Outputs of Tilemap-Editor need to have a general format (Example: JSON). Currently the Tilemap-Editor
      uses a custom file-format from the EVCMonoGame project. That wouldn't make sense anymore here.
  3.3 Find out how Exe-files from MonoGame projects can be launched. We don't want to start the Editor out of
      Visual Studio all the time.

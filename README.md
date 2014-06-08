PaletteKit
==========

Helpful editor classes for importing, managing and displaying color palettes. This was originally designed to be used with ProBuilder to replace it's vertex coloring system with something a bit nicer. That being said, there are only 2 lines of code that reference ProBuilder (search the source for "// PROBUILDER REFERENCE" to find them) so it could be of use for a variety of situations where importing and managing colors palettes is important.


Usage is pretty basic though if you are using ProBuilder you will need to uncomment the 2 lines mentioned above. You can open the PaletteKit window by choosing the Tools/ProBuilder/Vertex Colors/Color Palette Window menu item. By default when clicking a color in the scene view window it will just log the color: "chosen color: THE_COLOR". If you are not using ProBuilder that is your hook point for adding any code that you want to run in your project with the selected color. If you are using ProBuilder and you uncommented the two previously mentioned lines clicking the color will set the vertex colors of the selected faces.
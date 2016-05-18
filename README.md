# file-toucher

File toucher is an application to update the various timestamps on files in a Windows environment, similarly to the 'touch' function in Linux.

The original purpose of the application was/is to convince the Google Music application to re-upload music, though it can't be guaranteed that Google Musics approach will continue to use these flags. Also, the Google Music app seems to be extremely fickle, if not totally unpredictable. 

# To do

When refreshing timestamps after touching any FileNotFound files show a date of 1601. Highlight errored files and show no date.

Style the app in some way.

Add an "Add Directory with File Mask" function, or at least by file extension.

Add progress windows (with Cancel function) and some sort of threading in case people "Add Directory -> C:/*.*" etc.

Add option to save file list that can be imported later to save people finding and adding files again.

Automatically save last date time options (to registry?).

# Acknowledgements

This application makes use of the WPF Extension toolkit, and the Ookii Dialogs collection; so thank you to the WPF Extension community and Sven Groot at ookii.org.
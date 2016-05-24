# file-toucher

File toucher is an application to update the various timestamps on files in a Windows environment, similarly to the 'touch' function in Linux.

The original drive behind creating this application was/is to convince the Google Music application to re-upload music, though it can't be guaranteed that Google Musics approach will continue to use these flags. Also, the Google Music app seems to be extremely fickle, if not totally unpredictable.

Presumably there are many other use cases for updating these timestamps that I can't name right now.

![Screenshot of v0.2](https://raw.githubusercontent.com/dunctait/file-toucher/master/Screenshot.png)

# Code Structure

Version 0.2 involved a full rewrite to implement the MVVM approach with a few caveats in that the View Model also functions as the model and there are a few calls from the ViewModel to open dialogs/message boxes.

It is fair to say that MVVM is over-engineering for an application as small as this but the real function of this program is essentially to familiarise myself with larger scale design practices.

# To do

Implement dialogs using the MVVM approach, then implement more information dialogs (i.e "4 files were successfully touched" etc)

Style dialogs

Add an "Add Directory with File Mask" function, or at least by file extension.

Add option to save file list that can be imported later to save people finding and adding files again.

Save last folder that files were added from

Add language selection/datetime format selection

Add progress windows (with Cancel function) and some sort of threading in case people "Add Directory -> C:/*.*" etc.

# Acknowledgements

This application makes use of the WPF Extension toolkit, and the Ookii Dialogs collection; so thank you to the WPF Extension community and Sven Groot at ookii.org.
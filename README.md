# file-toucher

File toucher is an application to update the various timestamps on files in a Windows environment, similarly to the 'touch' function in Linux.

The original drive behind creating this application was/is to convince the Google Music application to re-upload music, though it can't be guaranteed that Google Musics approach will continue to use these flags. Also, the Google Music app seems to be extremely fickle, if not totally unpredictable.

Presumably there are many other use cases for updating these timestamps that I can't name right now.

![Screenshot of v0.3](https://raw.githubusercontent.com/dunctait/file-toucher/master/Screenshot.png)

# Code Structure

Version 0.3 removed any MVVM written by myself and implemented MVVM-Light as a replacement.

It is fair to say that MVVM is over-engineering for an application as small as this but the real function of this program is essentially to familiarise myself with larger scale design practices.

# To do

Add an "Add Directory with File Mask" function, or at least by file extension.

Add language selection/datetime format selection

Add progress windows (with Cancel function) and some sort of threading in case people "Add Directory -> C:/*.*" etc.

Add Unit Tests
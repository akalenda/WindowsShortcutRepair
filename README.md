# WindowsShortcutRepair

![](https://raw.githubusercontent.com/akalenda/WindowsShortcutRepair/master/screenshot.png)

## Wat?

It's a program I whipped together this afternoon. You select however many shortcuts, give it a regex for search and replace each, and then click apply. It will change the targets and working directories of all selected shortcuts with the regex you give it.

## Why?

I organize a lot of files into categories using shortcuts. When I relocated said files onto an SSD, I had well over 3000 broken links! Writing this program was a quick fix.

## How?

Windows Presentation Foundation, works in Windows 10, probably in other versions of Windows as well. You'll need Microsoft Visual Studio to tinker with it. (I used the 2015 Community Edition.)

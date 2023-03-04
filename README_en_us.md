# Interactive Music Player
Version No.: 1.0.0 | Last Update: 2022.6.5 | This version is not compatible with the new version configuration file and is set up differently, please check the project branch 1.0.0

Version number: 2.1.9 | Last update: 2023.3.5 | Current long-term support version

## Introduction
------
! [Enter image description](%E5%9B%BE%E7%89%87/1%E7%89%88%E6%9C%AC.png)
! [input image description](%E5%9B%BE%E7%89%87/%E5%B1%95%E7%A4%BA.PNG)
A music player that supports the current mainstream music playback modes (perpetual (seamless loop), seamless chapter switching, seamless step), the software uses Win11's acrylic transparent design style for a better user experience.

By adjusting the options, it can realize the simulation of music presentation mode in any game, movie or TV show. It can be used for game music testing, daily appreciation, etc.

Supported operating systems: Win8 - Win11 (Win7 users need to install the framework support package)

Currently there are two versions, one for 1.0.0 and the other for 2.1.6.

1.0.0 version please check the subbranch of the project, this version is easier to configure, no need to edit the play script file, directly put the file can be used, the introduction please check the branch of 1.0.0, this version features are available. If there is no major failure, no more maintenance.

Version 2.1.6 is the latest version currently available, this version allows you to configure your own playback scripts as you like to achieve more playback effects, in addition to simple configuration.

Recent Updates
Volume control bar, WAV, FLAC support, script reporting logic update

Here is the introduction of version 2.1.6



## Second development tips
------
Please remember to restore the NuGet package if you want to use the current project for development

## Software Architecture
------
The software is a WPF application using .

Net6. Internally, it uses a conventional three-layer architecture, where the playback logic/control/interface has been decoupled. The interface and playback logic can run without interfering with each other.
> Interface layer <> Music control middleware <> Playback logic

Note that the code structure is not completely separated as the current version is still in the process of improving its functionality.


## Software description
------
It is important to note that although most of the features work without problems, the software is not fully developed.

If you don't have special playback needs, it is still recommended that you use the version that only requires you to drop the file in and click two options to use 
Version 1.0.0.

! [Enter image description](%E5%9B%BE%E7%89%87/%E6%96%87%E4%BB%B6%E7%AE%A1%E7%90%86%E5%99%A8.PNG)

! [Enter image description](%E5%9B%BE%E7%89%87/%E8%84%9A%E6%9C%AC%E6%B5%8F%E8%A7%88%E5%99%A8.PNG)

! [Enter image description](%E5%9B%BE%E7%89%87/%E8%84%9A%E6%9C%AC%E6%B5%8F%E8%A7%88%E5%99%A82.PNG)

! [Enter image description](%E5%9B%BE%E7%89%87/%E8%84%9A%E6%9C%AC%E7%BC%96%E8%BE%91%E6%8F%90%E7%A4%BA.PNG)

! [Enter image description](%E5%9B%BE%E7%89%87/%E8%84%9A%E6%9C%AC%E7%BC%96%E8%BE%91%E6%8F%90%E7%A4%BA2.PNG)

! [Enter image description](%E5%9B%BE%E7%89%87/%E6%96%87%E4%BB%B6.png)

! [Enter image description](%E5%9B%BE%E7%89%87/%E6%AD%8C%E8%AF%8D.png)

For MP3 files, the playback processing operations that can be supported
1. regular playback
1. regular loop
1. Playlist loop (2.1.1) version is not yet implemented, try version 1.0.0

Playback processing operations that can be supported for OGG, WAV, and Flac files
1. Seamless loop from beginning to end (general Loop mode)
   >> Example
    >> 1. Play a piece of music and then start playing from the beginning immediately
    >> 2. A mode that plays background music all the time, which is used by almost all games

1. Seamless loop that plays to a certain millisecond or a certain beat (advanced Loop mode)
   >> Examples
    >> 1. Play a piece of music and then immediately jump to a position in this file to start playing or looping
    >> 1. Play a piece of music and then immediately start playing or looping from a position in another file

1. Seamlessly loop through the start and loop segments of multiple files
    > Selective looping of the group of audio files by identifying only audio files with different numbers (xx 1.ogg, xx 2.ogg) as a group and by modifying the configuration options
    >> Example
    >> 1. Play the first to the last segment, then loop the last segment (default operation)
    >> 1. Play all but the set looped audio files in order (just activate the loop button after the audio files in the configuration window)
    >> 1. In addition to the above playback methods, you can also dynamically switch paragraphs by editing the script to stop looping to the next paragraph when the button is clicked.

1. Seamless on-the-fly music switching for multiple files
    > By identifying audio files with different numbers only (xx 1.ogg, xx 2.ogg) as a group of audio files, selectively and seamlessly instantly switch music for that group of files by modifying the configuration options
    >> Example
    >> 1. Play the first paragraph to the set loop paragraph, and then automatically and seamlessly transition to the next paragraph by switching the chapter button (in the playback script settings, apply the preset "Interactive Forever" script, and simply modify your file chapter name and paragraph information.)
    >> 1. You can refer to the music switching mode of Eight Travelers in the character theme music and boss theme music, which can achieve almost the same effect after configuration. (Unlike version 1.0.0, now you don't need to do any processing of the audio in advance, you just need to configure the time as well as the file, and you can achieve such effect by simply modifying the script.)

1. Step music switching for multiple files
    > By identifying only audio files with different numbers (xx 1.ogg, xx 2.ogg) as a group of audio files, selectively and seamlessly switch music on-the-fly by modifying the configuration options for that group of files
    >> Example
    >> 1. Play this group of audio files and instantly step through the music to the next section by switching the chapter button (in the playback script settings, select the preset "Step Loop" and simply change the chapter name and paragraph information of your file.)
    >> 1. You can refer to the music switching mode of most of the current games in daily state and battle state (such as the music switching between land birds running and walking in Final Fantasy 15, and the music switching between spirit world and normal music in Oriental ProJect temple), which can achieve almost the same effect after configuration.



## Instructions for use
------

When the software is opened, it will navigate to your music folder by default
You can open a folder by browsing music in the program's own file browser, or by clicking on the folder button in the upper right corner.
You can right-click on the music in the file browser
1. Locate the location of the file
1. Locate the path to the configuration file

After selecting the music, you can edit it by clicking on the script edit icon on the left side
1. Modify the file's display alias, author, and display BPM information
1. Manually test the BPM of the music
1. simply configure paragraph and loop information for a file or group of files
1. use preset or self-written playback scripts for your music (Scripts are fully documented with tips at the time of writing)

When you use any mode of this player, you can adjust the basic content of playing music through the buttons on the interface
1. play/pause/stop
2. progress adjustment
3. volume level and mute
4. Play progress

When you play a song and want to display the desktop lyrics, you can open the desktop song through the Live Manager

Translated with www.DeepL.com/Translator (free version)
# Interactive Music Player
Version number: 1.0.0 | Last update: 2022.6.5


## Introduction
------
A music player that supports the current mainstream music playback modes (perpetual (seamless loop), seamless chapter switching, seamless step). The software uses Win11's acrylic transparent design style for a better user experience.

By adjusting the options, it can realize the simulation of music presentation mode in any game, movie or TV show. It can be used for game music testing, daily appreciation, etc.

Supported operating systems: Win8 - Win11 (Win7 users need to install the framework support package)

## Second development tips
------
If you want to use the current project for development, please remember to restore the NuGet package

## Software architecture
------
Net 4.7 framework for WPF applications

Net 4.7 framework. Internally, it uses a conventional three-layer architecture, where the playback logic/control/interface has been decoupled. The interface and playback logic can run without interfering with each other.
> Interface layer <> Music control middleware <> Playback logic

Modules can be easily decoupled and added at any time.
If you want to add modules to the project, just add them to the play logic and set up the control middleware.


## Software description
------
For MP3 files, the supported playback processing operations are
1. regular playback
1. regular loop
1. playlist loop

Playback processing operations that can be supported for OGG files
1. Seamless loop from beginning to end (general Loop mode)
   >> Example
    >> 1. Play a piece of music and then immediately start playing from the beginning
    >> 2. Playback mode used by almost all games

    > The next step will be to continue writing about implementing a single-file custom Loop loop through a single file, setting the playback byte position, and making a more user-friendly configuration interface.
   
1. Seamless looping of start and loop segments of multiple files
    > By identifying audio files with different numbers only (xx 1.ogg, xx 2.ogg) as a group of audio files and selectively looping the group by modifying the configuration options
    >> Example
    >> 1. play the first to the last paragraph, then loop the last paragraph (default operation)
    >> 2. Play all but the set looped audio files in order (select "Forever" in the configuration options)

1. Seamless on-the-fly music switching for multiple files
      > Selectively switch music seamlessly and instantly by identifying audio files with different numbers only (xx 1.ogg, xx 2.ogg) as a group of audio files and by modifying the configuration options to selectively switch music seamlessly for that group of files
    >> Example
    >> 1. Play the first section up to the set loop paragraph, and then automatically transition seamlessly to the next section by switching the chapter button (select "
    Seamless Chapter Loop", and set the BPM, beats per bar, and loop passage, in addition to the files you have to prepare beforehand that can do this)
    >> 1. you can refer to the music switching mode of Eight Travelers in the character theme music and boss theme music, through the configuration can achieve almost the same effect.
1. Step music switching of multiple files
    > By identifying audio files with different numbers only (xx 1.ogg, xx 2.ogg) as a group of audio files, selective and seamless instant music switching is performed selectively for this group of files by modifying the configuration options
    >> Example
    >> 1. Play this group of audio files and immediately step through the music to the next section by switching the chapter button (select "Step Loop" in the configuration options)
    >> 1. You can refer to the music switching mode of most current games in daily state and battle state (such as the music switching between land birds running and walking in Final Fantasy 15, and the music switching between spirit world and normal music in Oriental ProJect temple), which can be configured to achieve almost the same effect.



## Instructions for use
------

When the software is opened, it will navigate to your music folder by default
You can open a folder by browsing music in the program's own file browser, or by clicking the folder button in the upper right corner.
You can right-click on a music item in the file browser
1. locate the file location
2. create/delete/create a new profile

While you are using any mode of this player, you can adjust the basic content of the playing music by using the buttons on the interface
1. play/pause/stop
2. pace adjustment
3. volume level and mute
4. playback progress (moving the mouse over the progress bar will show the hidden controls)

When using some special modes you can adjust the playback information by using the buttons at the bottom of the software interface
1. Loop mode (next to volume)
2. paragraph and chapter switching and selection (below the time display)

Others
1. When using some modes, you can see the number of loops and the current beat information read by the software.

## Contribute
-------

Submit code

1. Fork this repository
2. Create a new branch
3. Submit the code
4. Create your new PullRequest

Submit a suggestion

1. Submit your ideas in Issue, I will probably improve it in the next version

## Open source project use
------
Thanks to the author of the project

Naudio

PanuonUI



Translated with www.DeepL.com/Translator (free version)

**Note**: I have discontinued use / development of this program in favor of [iTuner](http://ituner.codeplex.com) which has resumed development again (it took a year break during 2011) and now includes features I wanted. If necessary I will contribute to that project.

A program that has a highly customizable popup display of iTunes information using either a glass window or a basic window combined with a highly customizable set of events and actions including system-wide keyboard events.

This project is in a _beta_ state, indicating that it seems to be mostly working but hasn't been thoroughly tested.

When there is no settings.xml file (for example when you first install) the actions used are the default actions. Since the program has no interface you will need to use these to set any further options:

  * Play: ShowTrackInfo
  * TrackInfoChange: ShowTrackInfo
  * GotFocus: KeepTrackInfoOpen
  * LostFocus: AllowTrackInfoToClose
  * Enter: KeepTrackInfoOpen
  * Leave: AllowTrackInfoToClose
  * LeftClick: HideTrackInfoNow
  * **RightClick: ShowOptions**
  * LeftDoubleClick: NextTrack
  * Ctrl + Alt + Win + P: PlayPause
  * Ctrl + Alt + Win + Left: PreviousTrack
  * Ctrl + Alt + Win + Right: NextTrack
  * **Ctrl + Alt + Win + O: ShowOptions**
  * Ctrl + Alt + Win + Q: DisQuit

Green logo by TheArcSage from http://thearcsage.deviantart.com/art/Green-iTunes-Icon-192747015

If you are looking for information about the iTunes COM library, see http://developer.apple.com/sdk/itunescomsdk.html

iTunes and the iTunes Logo are trademarks of Apple Inc., registered in the U.S. and other countries.
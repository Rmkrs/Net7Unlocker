# Net7Unlocker
Unlocker and Loader for Net7 (The Emulator version of the game Earth &amp; Beyond)

Back in 2002 I spent most of my waking (non-working) hours playing my first MMO, which was Earth &amp; Beyond, by Westwood Studios.
Later EA bought Westwood and about 2 years later the game was shutdown. I made so many amazing online friends back in those days. I always miss that time and feel quite melancholic about it.

Snap back to a few years ago, when some amazing people spent time decoding the encryption used in the original game and making an actual emulator server so the game could be played again.

For those interested, here you can find the emulator website : https://www.net-7.org/

Somewhere in 2015 I actually spent quite some time playing it again. And I noticed there were some challenges with multi-boxing. During the live game in 2002 I also multi-boxed the game, but then with multiple PCs (using a tool that allowed me to control all PCs with one keyboard and mouse, just by moving the mouse over the border of the monitor).

With modern day hardware it's actually possible to run 6 instances of the game on 1 machine. But that came with some challenges.
For one, the game actually contained a mutex lock to prevent it from being opened more than once on 1 machine. The dreaded "Already running" popup is still fresh in my mind.

And also with positioning the game windows, it was not as easy as you think as there are some finicky things going on with the window border.

This all encouraged me to spent some coding time on a solution for it all.

This solution consists of 2 parts, one is the GUI you can launch and where you can set options and create loadouts for 1-many game clients. It saves the positioning and also changes the window border text to include the account name, so you can easily see which account to login on which window.

The other part is a little helper that gets spawned during game launch to remove the mutex lock. It mostly does a hostile ownership take over on the mutex, before dying itself, so the mutex lock dies with it ;)

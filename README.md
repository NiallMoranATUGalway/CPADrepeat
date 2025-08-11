# CPADrepeat
14/07/2025
This is my CPAD repeat project so far. I have chosen the 'Countdown' game with just the letters. I so far have a basic UI, and code behind the UI. I have implemented the sound, and I can read in 'dictionary.txt' locally but I cannot get the file download working.

23/07/2025
I have removed the 'start game' button and replaced it with 'vowel' and 'consonant' buttons, and added their respective distribution rates. There is lots of bugs in this version and no repeat method but this will be amended in the future

26/07/2025
I had to do a big amount of debugging this evening as for some reason the IsValidWord() method kept returning false, despite inputted words from the user being valid. There was a case miss-match that occured when I declared var available letters. The available letters were in lower case while the dictionary text file was all in upper case.

05/08/2025
I added player 1/2 functionality

09/08/2025 Using the skia.extended.tools.ui plugin in, I used a 'lottie' animation of a circular block that is 30 seconds long that ticks down. The lottie animation was on the website https://lottiefiles.com/ initially in purple, but I was able to use the built in editor on the website to change the purple to teal, which suits the theme of my project better. This video https://www.youtube.com/watch?v=o5X5yXdWpuc showed me how to implement the animation

11/08/2025 I added file handling to the project, the project will now save the history of the games to a file in the AppData trajectory 


```
Author:     Shu Chen
Partner:    Ping-Hsun Hsieh
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  Samuel-233
Repo:       https://github.com/uofu-cs3500-spring24/assignment-eight-agario-asianclassmate
Date:       18/4/2024
Solution:   Agario 
Copyright:  CS 3500 Shu Chen - This work may not be copied for use in Academic Coursework.
```

# Overview of the Agario Client functionality
This solution is Agario client, it works on multi platforms like phones or computers. 
User need to enter name, port and server address to join the game. 
On the platform that have mouse, user can just put the mouse on the canvas to make circle to move, and click to split the circle.
On the platform that don't have mouse, user can drag on the screen to make circle to move, double click to split the circle.
If the game is over, it will show game is over, and display the largest radius player got.
If server disconnected, the program will back to login page.


# Partnership Information:
For Shu Chen, I mainly did the client part, and Ping did the Agario models and networking part.

# Branching:
For this homework, this is way more easier than the Assign 7, which is my first time work on networking.
So for this homework, it doesn't have too much to do with branching. 
*Client Back End Branch*: Mainly work on how to revive message from the server and display it on GUI.
*Master branch*: After finished client back end branch, I merge it to master branch, and try to fix all the bugs
*Tap And Pan Branch*: Work on the movement on the phone. And found out drag is a good choice. User can split the circle by double click



# Testing:
For this solution, all testing are done by hand. I build the program and run it and try to find any thing that went wrong.
For the first stage - user cannot interact with the program, I mainly pause the program and use debugger to check what the message
program received and think about how should I use it.

# Time Expenditures:
	Predicted Hours:   25        Actual Hours:   30 
	Note:
	debugging: It is annoying that debugging with GUI - when some calculation is wrong, it is hard to found out which process is wrong
	it took me about 5 hours to solve all problems with how to display object on screen
	Reflecting: This homework is easier than I thought - I thought student need to do both client and server,
	but since student just need to deal with client, I just took a week to finished this homework.

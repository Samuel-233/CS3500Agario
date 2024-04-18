```
Author:     Shu Chen
Partner:    Ping-Hsun Hsieh
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  Samuel-233
Repo:       https://github.com/uofu-cs3500-spring24/assignment-eight-agario-asianclassmate
Date:       9/4/2024
Project:    AgarioModels
Copyright:  CS 3500 Shu Chen - This work may not be copied for use in Academic Coursework.
```


# Partnership Information:

# Testing:
Testing by execution.

# Comments to Evaluators:
I didn't add the split button, because to split the player, user can left click on the canvas. If user is using the phone, they can tap twice in a
short time to split.
I make the GUI to re paint when there is a new data arrived. Since if there is no data arrived, any re drawing will be waste of computation
I tried to do the prediction of player movement to achieve a high frame rate, but I cannot figure out how to do it correctly.

# Extra features:
As mentioned at above, player can split the ball by just clicking. In addition, There are a lerp animation when zoom the camera. 
On the top left, there is a mini map, to show which place is player at, and how big is the total map is. The mini map will be invisible
when the player get big enough.


# Project content
This Project is the MAUI that show all the info that received from the server. It also has a back end class, which store all the info that receive from
the server.

# References:

1. How to get mouse position from the canvas - Bing AI
2. How to use GraphicsView.GestureRecognizers - Microsoft doc

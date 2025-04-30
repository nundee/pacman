## Introduction
In the 37 years since it quietly slipped into Tokyo arcades, Pac-Man has become a global phenomenon. 
Developed by Namco, the pill-munching maze game remains a pop culture staple despite the fact that most of us rarely 
step foot inside an arcade anymore. And while the machines themselves have dwindled somewhat in our public spaces, 
Pac-Man still thrives on consoles and mobile phones.

Pac-Man’s genius lies in its simplicity. 
We doubt you need a refresher but just in case: you're the hungry yellow blob with the unenviable task of traversing a maze
and eating all the dots within it as a quartet of ghosts chase you, block your path, and generally make your life more 
complicated. 
You're not powerless against the googly-eyed hunters though—the power pellets located around the screen render the ghosts 
vulnerable and suddenly keen to avoid your attention, making you the pursuer. 
Eating the ghosts in these few precious seconds will net you extra points, but only buy you a short grace period before 
they re-emerge to chase you down once more.

Designer Toru Iwatani once said that Pac-Man would have been boring had it simply been about collecting pellets, 
so he added the ghosts as an additional layer of challenge. Even better, Iwatani programmed the ghosts so that each 
has its own unique set of movements and quirks. The start of the game even gives them names: Blinky, Pinky, Inky and Clyde. 
The programming behind these individual ghosts is governed by an ingenious-but-simple set of behaviors, and understanding them 
is key to mastering the game. Here’s a brief look at the rules behind Pac-Man’s ghosts.

## CHASING AND SCATTERING
When a new game of Pac-Man begins, only one ghost is present in the maze: the red one, Blinky. 
Gradually, more ghosts enter from the holding pen—widely dubbed the 'ghost house' in the center of the screen. 
A less ambitious designer might have programmed the ghosts so that they constantly follow the player around, ultimately 
boxing them into a corner and relieving them of one of their lives. Iwatani realized, however, that a constant sense of 
pressure would make for a stressful—and perhaps not particularly fun—game, so he made sure that the ghosts’ behavior 
changed every few seconds.

The ghosts, therefore, cycle between two states, which Jamey Pittman, writing on his website The Pac-Man Dossier, calls 
"scatter mode" and the "chase mode." In scatter mode, each ghost will head for a target in its own pre-defined corner of the 
screen. Pinky, the pink ghost, will head for the top left of the maze; Blinky the top right; blue ghost Inky the bottom right; 
Clyde the orange ghost, bottom left. (There's also a third mode, "frightened," after Pac-Man eats a power pellet, 
but we're ignoring that for now since it's when the player has the power.)

As the target square isn't actually reachable, once they’ve reached their target zone, the ghosts will follow a looping 
path around their individual corners for a few seconds before they switch over to chase mode. As the name implies, 
this means the ghosts will start to actively pursue the player until they switch back to scatter mode. The way each ghost 
pursues the player is defined by its unique, pre-programmed behaviors.

## BLINKY, THE RED GHOST
Of all the ghosts, this one’s the easiest to describe. In chase mode, Blinky’s target is Pac-Man, and the ghost will hunt 
the player constantly until its pattern switches to scatter mode, when it heads back to its corner of the maze. 
There is, however a bit of a twist: when there are 20 dots left in the maze (increasing at higher levels), Blinky’s speed 
will increase slightly. Its speed will increase yet again when there are 10 dots left. At these points, Blinky’s behavior in 
scatter mode will also change; instead of moving back to its corner at the top right of the maze, it will continue to chase 
Pac-Man. This behavior, dubbed "Cruise Elroy" by players, makes Blinky the most fearsome character in the game, since it’ll 
only switch back to its less speedy, persistent state once the player’s either eaten all the dots or lost a life.

## PINKY, THE PINK GHOST
Pinky's strategy for taking down Pac-Man is a bit more strategic. It's programmed to take a route to a position four spaces 
ahead of wherever Pac-Man’s moving, which can easily leave the player trapped. But there is a bug that can work in your favor: 
when Pac-Man’s moving up the screen, Pinky will target a space four squares in front of his position and four squares to 
the left. Skillful players can exploit this to their own advantage, and confuse Pinky’s AI by briefly moving towards the 
ghost as it closes in—this will often send it down a different branch of the maze as its target in front of Pac-Man suddenly 
shifts.

## CLYDE, THE ORANGE GHOST
The orange ghost’s behavior is more unpredictable at first glance, but it’s still governed by its own simple rules. In chase 
mode, Clyde’s movements will vary based on its proximity to Pac-Man; if it’s more than eight squares away, it’ll behave like 
Blinky and select Pac-Man as its target, but within eight squares, it’ll move back to the bottom left corner of the maze—a.k.a. 
its zone in scatter mode. This means that Clyde can look as threatening as Blinky one moment, and then cowardly and indecisive 
the next. Once its cycle of movement is understood, avoiding Clyde becomes a relatively simple task.

## INKY, THE BLUE GHOST
The wiliest of all the ghosts, Inky’s movements are perhaps the most difficult to track. This is because its target is based 
not just on the player’s direction and position, but also Blinky’s. In chase mode, Inky heads to a space just in front of 
Pac-Man (much like Pinky), but just to make things more confusing, it will then move that position to a spot that is twice 
the distance from Blinky’s current position.

In other words, Inky draws a line from Blinky, through a spot two spaces in front of Pac-Man, and will head to a position that 
is double the distance from Blinky. This gives the impression of almost random movement when Inky’s further away, 
but will mean that, as Blinky closes in on Pac-Man, Inky will start to get closer, too. Inky is, however, weighed down with 
the same bug as Pinky, and can also be fooled into moving in the wrong direction in a similar fashion.

## GAME ON
While all this might seem like serious, in-depth game knowledge, it's actually a pretty basic overview of the logic behind 
Pac-Man’s ghosts (cue Pac-Man death noise). The timing of their exit from the 'ghost house' at the center of the maze and 
their switch between chase and scatter modes, for example, is complex and changes from level to level. 
Don't let that intimidate you though: simply understanding the rules that govern the ghosts’ movements can help to improve 
your score, plus it offers a glimpse inside the ingenuity and sheer effort that went into designing one of the most 
popular games of all time.


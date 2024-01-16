CONST variable = "hello"

-> main

=== main ===
This is a sample dialogue
 + [What do you mean?]
    -> Explanation
 + [How does it work?]
    -> Explanation
 + [Oh... ok.]
    -> Acceptance
 + [I don't care]
    -> Acceptance
 + [Skip]
    -> END
    
=== Explanation ===
This dialogue is meant to test the dialogue system
It contains a few small branching paths, longer lines like this one, and even a few tags to test the speaker system, among other things like variables and such.
-> SpeakerTest

=== Acceptance ===
Well let's get it started then
-> SpeakerTest


=== SpeakerTest === // tests the speaker system, placement of sprites, etc.
Hello, I am a test speaker #speakertest
I'm here to make sure that that speakers show up properly#speakertest
Brb#speakertest
The test speaker left #RemoveSPRITEONLEFT
But then I came back!!!! #SpeaKerTEST #PLACESPRITEONLEFTtest#SpeakerIsOnLeft
You should see a sprite this time! #SpeaKerTEST
For a second the speaker did not speak, and you noticed because the nameplate was not displaying.
Oh wait, I gotta test that the nameplate comes back on the correct side! #speakertest
And it left again #Removesprites
And then I came back once again!#   Speaker teST#PlaceSpriteOnRighttest#SpeakerIsOnRight
This time I should be on the right side of the screen#Speaker teST
Now I'm on the left again #speakertest#PLACE SPRITE ON LEFT test#speakerisonleft    #removespriteOnRight
While I'm here, let me test a few other things for you#speakertest
First, make sure a warning was logged to Unity Console if you can see it #Invalid Tag #speakertest
Should say something along the lines of ("Invalid Tag" could not be read, please check its formatting/spelling it or have it removed. Refer to the docs for more information on proper tag writing)#speakertest
One Sec#speakertest
The test speaker once again left#       REMOVESPRITEONLEFT
Ok, this is the end of the speaker test. #speakertest#PLACESPRITEONRIGHTtest#speakerisOnRight
hopefully everything went well.#speakertest
good luck with the other testing!#speakertest
for the final time, the speaker left#REMOVESPRITEONRIGHT
-> VariableTest

=== VariableTest ===
VARIABLE TEST:
Does the next line say "hello"?
{variable}
If the last line displayed "hello", then variables are displaying correctly
-> STOP

=== STOP === // Runs any final dialogue before ending the story
-> END

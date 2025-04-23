# Change is inevitable: versioning event-driven systems

![chnage-is-inevitable](versioning-banner.jpg)

## Abstract

Building an event-driven system is anything but trivial. However, once you make it past the sea of pub-sub vs. command-response debates and the service boundaries conundrum, you'll soon face the inevitable: change. The conversations that follow sound all too familiar... "Who's subscribed to this message?" "Do other services depend on this field in the payload?" "Why on earth is that thing in the payload?" "That service should never rely on this data!" And, of course, the obvious "Can't we -just- remove this?"

But are those the right questions to ask? As software developers, we aim to be agents of change, not chaos. To achieve this, we need to understand the impact of tweaking a message contract without breaking half of the system or forcing other teams beyond their deadlines. Oh, and let’s not forget that we’re supposed to solve this problem with zero downtime, as our users are spread across every time zone. In this session, we will discuss techniques to prepare your event-driven systems for change, how to evolve contracts in ways that ensure compatibility, and finally, how CNCF's xRegistry project will impact the evolution of event-driven systems in the future. Join me to find out how you can prepare your event-driven system so that, next time a stakeholder approaches with a change request, your heart doesn't sink to the floor.

## Additional information

Make sure to check the additional [resources](resources) for this topic.

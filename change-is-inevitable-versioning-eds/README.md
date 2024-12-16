# Change is inevitable: versioning event-driven systems

## Abstract

Building an event-driven system is anything but trivial. However, once you make it past the sea of pub-sub vs. command-response debates, and the service boundaries conundrum, you’ll soon face the inevitable: change. The conversations that follow sound all too familiar: “Who’s subscribed to this message?” “Do other services depend on this field in the payload?” “Why on earth is that thing in the payload?” “That service should never rely on this data!” And, of course, the obvious “Can’t we -just- remove this?”

But are those the right questions to ask? As software developers, we aim to be agents of change, not chaos. To achieve this, we need to understand the impact of tweaking a message contract without breaking half of the system or forcing other teams beyond their deadlines. We should focus on techniques that provide compatibility and also ask ourselves how long compatibility should be maintained. Oh, and let’s not forget that we’re supposed to solve this problem with zero downtime, as our users are spread across every time zone. In this session, we’ll discuss practical techniques and tooling that can enable the evolution of your event-driven system so that, next time a stakeholder approaches with a change request, your heart doesn't sink to the floor.

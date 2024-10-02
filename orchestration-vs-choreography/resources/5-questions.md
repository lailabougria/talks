# The 5-question decision-framework

I developed this framework over the years based on my learnings from the field and the [books listed in the resources](README.md#books).
This framework consists of five pillar questions, but it's important to dig further into the details of each pillar.

The five main questions to keep in mind are:
1. What type of communication is suitable? Synchronous or asynchronous communication?
    - Are there business requirements for synchronous communication?
    - Is it acceptable for errors to occur and, in the worst case, for requests to be lost? Is the contrary true?
    - Are there scaling requirements that may affect this decision?
2. Which direction of coupling is preferred? Does it make sense to couple the sender to the receiver or the receiver to a publisher?
    - Does it make sense for the sender to be coupled to the receiver and introduce command coupling?
    - Is it more suitable to decouple the sender from the receiver and instead rely on the inverse coupling?
3. Are there complex compensating flows? Would they introduce significant bidirectional coupling?
    - How many compensation flows are there in this business process?
    - How extensive are they? Which services do they impact?
    - How many "hops back" are required to complete a compensating flow? How much bidirectional coupling does this introduce?
4. Is there a high probability of change?
    - Are you working on a relatively stable domain? Or is change on the horizon?
    - What are some of the most plausible changes that may need to be implemented here?
    - How would these affect your current design?
5. Is there someone responsible for the end-to-end flow?
    - If a workflow is stuck, who would be responsible for it?
    - Who would be responsible/accountable for the flow?
    - Are there parts of the workflow with high responsibility involved that could be isolated?

Finally, it's important to continuously reassess the scope of the workflow and question whether some parts can be isolated into dedicated workflows.
Remember to draw your workflow using both styles. A visual representation can be useful for surfacing hidden requirements and helping visualize the impact of change. It's also a helpful tool in discussions with business experts.

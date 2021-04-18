# KD6-3.7 Simplexity AI

**Author:** Pedro Dias Marques

**Note:** whenever I refer to a run I mean a batch simulation of 100, 200 or 300
games against a MinimaxD3 algorithm with a center heuristic. The percentage of
wins were noted.

## Algorithm

KD6-3.7 uses a Monte Carlo Tree Search (MCTS) algorithm. The current
implementation uses a purely random expansion and playout policies, meaning it
**does not** perform any sort of board or move evaluation.

### Current Results vs Minimax Depth 3 with Center Heuristic

The run of 200 games performed at the time of this writing registered a win of
187 games. That's a 93.5% win percentage. KD-6-3.7 always plays RED.

### An Opinion of the Suitability of MCTS for Simplexity Play

If MCTS has sufficient time to think it will consistently beat any heuristic
based search (HBS), however if this hypothetical HBS has a particularly good
static evaluation function (SEF) the win rate will go down significantly. This
problem scales with the number of possible plays. Simplexity has enough possible
board states that, if given very little time, a purely random MCTS will lose a
significant amount of games. This worsens the bigger the board.

### Attempted Variations/Improvements and Their Results

It's time that limits the potential of a purely random MCTS algorithm. More time
more wins. If time is tight MCTS can benefit from some guidance.

KD6-3.7 occasionally loses to even a HBS with an extremely simple heuristic.
This happens, more often, very early in games. This is the downside of MCTS
of course, sometimes it just misses obvious plays. My first attempt at plugging
this hole was to add a SEF and add it to the UCT function, multiplying it by a
factor constant (c). The SEF only used one heuristic that valued the center of
the board.

The results were very underwhelming. The results didn't see an improvement and,
more concerning, the number of simulations went down significantly. This was the
reason I decided to return to a purely random approach and focus instead on
increasing the number of simulations.

First I ran simulations with the SEF used only to break ties between nodes. The
results didn't see an improvement.

I then got curious about the impact of the random numbers generator (RNG). The
version of KD6-3.7 in this repository uses System.Random however I decided to
write a XorShift RNG. The impact on the number of simulations appeared to be
minor, sometimes doing better, sometimes worse. The win rate difference fell
inside the normal variance already observed.

It's important to note that some of these attempted improvements, that failed to
prove worth it, could be valuable under different circumstances.

### Other Possible Improvements

The SEF is only as good has its heuristics. Providing better ones could make the
loss in performance worth it. On top of the performance hit, it's important to
note that it's undeniable that a SEF makes an AI more narrow. The downsides
scale the more heuristics are used and the bigger the factor (c) is.

A very promising improvement is the concept of tree reuse. Since the board
states are deterministic we could gain some serious performance by caching nodes
for future use. In theory, it seems to me, that the factor determining the
bias towards unexplored nodes (k) could be set higher. This is just an opinion,
since this improvement was not attempted yet.

#### References

* MCTS TickTackToe made available by the professor

* AI for Games by Ian Millington (3rd Edition)

* "Fastest C# Random Number Generator: XorShift+" ->
http://codingha.us/2018/12/17/xorshift-fast-csharp-random-number-generator/

* "Monte Carlo Tree Search Experiments in Hearthstone" (download) ->
https://fenix.tecnico.ulisboa.pt/downloadFile/1970719973966524/paper.pdf

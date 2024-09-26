## More things to implement

* evaluation suite
* evaluation suite evaluator
* move ordering
* periodic info messages
* MCTS (or UCT, whatever the appropriate algo is)
* mvv-lva
* IID

## Evaluation Suite

Basically, a long list of positions, each notated by the stockfish evaluation per move.

1. Download a database from here https://database.nikonoel.fr/
2. Parse the PGNs into fens. Write utility function to step through PGN and append fen to the file, while making sure it hasn't seen the fen before (hashset of hashes?).
3. Run stockfish against each position with MultiPV set to max. Write wrapper utility which takes a fen, launches stockfish, passes commands and parses responses, and writes a line to a file. Stores each move, and the cp for each move. Stores best move (or multiple if there are multiple best moves). Stores the final depth, and the last depth at which the best moves were stable (it's still stable if there were additional best moves; it's not stable if there was a better move, or if any of the best moves were't considered best).

Tag the data in 3 ways:

1. Positions which have a tactic (maybe the first position is over 300 cp higher than the 4th [or last, if less than 4] position).
2. Positions which are clearly losing (say -500 cp or worse).
3. Positions with only a few moves (maybe 5 or fewer).

The quiet data set (non-tagged) will be the most useful for the evaluation suite.

## Evaluation Suite Evaluator

Runs the engine against the evaluation suite, to a given number of nodes. Writes some file of best moves. For each position, it makes an evaluation of how bad it was (how much loss). Need to craft some formula that maintains the following invariants:

1. If the position is not clearly winning, and the engine gives a move that is near optimal (within ~20cp), then the loss is very low.
2. If the position if clearly winning, and the engine gives another move that's also clearly winning (even if distant in cp), then the loss is very low. For example, if it goes from #10 to +15.
3. If the position goes from clearly winning to drawn, from drawn to losing, or from winning to losing, then the loss will be near maximum.

Something like the following table:

  from |     to | loss
-------+--------+------
 10000 | -10000 |   98
 10000 |      0 |   49
     0 | -10000 |   49
   300 |   -300 |   49
   100 |   -100 |   25
 10000 |    300 |   25
   700 |    200 |   25
     0 |   -200 |   25
  -200 |   -700 |   25
   100 |     50 |    5
    50 |      0 |    5
 10000 |   2000 |    5
 10000 |  10000 |    0
   100 |    100 |    0
   500 |    500 |    0
   200 |    200 |    0
     0 |      0 |    0
  -100 |   -100 |    0

Define a logistic function like:

f(x) = 1 / [ 1 + e^-(x/275) ]

Then you can get the loss by doing:

[ f(from) - f(to) ] * 100


Then, 2 runs can be evaluated against each other, both in terms of total loss, and individual positions can be evaluated against each other. In particular, regressions will be useful - positions where loss increased.
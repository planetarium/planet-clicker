Example
=======

In this document, we'll learn how to use Libplanet through a simple clicker game.


Prerequisites & Requirements
----------------------------

- Libplanet is compatible with Unity 2019.1.0f2 or above.
- Unity Player must be set to Scripting Runtime version 4.x equivalent, Mono as Scripting backend, and API compatibility level must be set to .NET 4.x.
- Support for Windows/macOS/Linux (including Headless).


Game Design
-----------

The clicker game this article refers to is very simple. There are buttons on the screen, press it to increase the score. In Libplanet's terms, a player's score is **State** and the button press is **Action**. This can be described briefly as follows:

```
+----------+            +----------+            +----------+
| Player 1 |  AddCount  | Player 1 |  AddCount  | Player 1 |
|          +----------->+          +----------->+          |
| count: 0 |            | count: 1 |            | count: 2 |
+----------|            +----------+            +----------+
```

When multiplayer is considered, the situation becomes a bit more complicated. The game must pay attention to the status of other players.

```
+------------+          +-------------+          +-------------+
|            |          |             |          |             |
| State #0   |          | State #1    |          | State #2    |
|            |          |             |          |             |
|+----------+|          | +----------+|          | +----------+|
|| Player 1 || AddCount | | Player 1 || AddCount | | Player 1 ||
||          ++----------+>+          ++----------+>+          ||
|| count: 0 ||          | | count: 1 ||          | | count: 2 ||
|+----------||          | +----------+|          | +----------+|
|            |          |             |          |             |
|+----------+|          | +----------+|          | +----------+|
|| Player 2 || AddCount | | Player 2 ||          | | Player 2 ||
||          ++----------+>+          ||          | |          ||
|| count: 0 ||          | | count: 1 ||          | | count: 1 ||
|+----------||          | +----------+|          | +----------+|
|            |          |             |          |             |
+------------+          +-------------+          +-------------+
```

- We need to save player's score separately for each player in State. Since we're using key-value interface, it isn't difficult if the key is selected properly.
  - In this case, we use the address derived from the user's private key as the State's key.
- `AddCount` only changes the state of one user at a time.

We want players to see and compete with others' scores. To do this, we need a list of other users. If we put score information together, we can also reduce the number of status checks. The `RankingState` containing this information is shown below.

```csharp
namespace _Script.State
{
    public class RankingInfo
    {
        public Address Address;
        public long Count;

        public RankingInfo(Address address, long count)
        {
            Address = address;
            Count = count;
        }
    }

    [Serializable]
    public class RankingState : State
    {
        public static readonly Address Address = new Address(new byte[]
            {
                0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1
            }
        );

        private readonly Dictionary<Address, long> _map;

        public RankingState() : base(Address)
        {
            _map = new Dictionary<Address, long>();
        }

        public void Update(Address address, long count)
        {
            _map[address] = count;
        }

        public IEnumerable<RankingInfo> GetRanking()
        {
            return _map
                .Select(pair => new RankingInfo(pair.Key, pair.Value))
                .OrderByDescending(info => info.Count);
        }
    }
}
```

The overall State, including ranking and player scores, is shown below.

```
+----------------------------------------------------------------------+
| State                                                                |
|                                                                      |
| +--------------------+ +--------------------+ +--------------------+ |
| | RankingState       | | Player 1           | | Player 2           | |
| |                    | |                    | |                    | |
| | Address: 0x000...  | | Address: 0x1234... | | Address: 0x2345... | |
| | _map:              | | Count: 1           | | Count: 2           | |
| |   0x1234...: 1     | +--------------------+ +--------------------+ |
| |   0x2345...: 2     |                                               |
| +--------------------+                                               |
+----------------------------------------------------------------------+
```


Lifetime of Action
-------------------

Below is a schematic of the flow until the action is reflected in the game.

```
+----------------+
|                |
| new AddCount() |
|                |
+--------+-------+
         |
         |
         v
+--------+----------------+     +----------------------+
|                         |     |                      |
| Agent.MakeTransaction() |---->| BlockChain<T>.Mine() |
|                         |     |                      |
+-------------------------+     +-----------+----------+
                                            |
                                            |
                                            v
+-------------------------+     +----------------------+     +----------------------+
|                         |     |                      |     |                      |
| Game.UpdateScore()      |<----+ AddCount.Render()    +---->| Game.UpdateRanking() |
|                         |     |                      |     |                      |
+-------------------------+     +----------------------+     +----------------------+
```


How to implement `AddCount`
---------------------------

The `AddCount` is defined in `Assets/_Script/Action/AddCount.cs`.

```csharp
using System.Collections.Generic;
using System.Collections.Immutable;
using _Script.State;
using Libplanet.Action;
using LibplanetUnity.Action;
using UnityEngine;

namespace _Script.Action
{
    [ActionType("store_count")]
    public class AddCount : ActionBase
    {
        private long _count;

        public AddCount()
        {
        }

        public AddCount(long count)
        {
            _count = count;
        }

        public override IImmutableDictionary<string, object> PlainValue =>
            new Dictionary<string, object>
            {
                ["count"] = _count.ToString(),
            }.ToImmutableDictionary();

        public override void LoadPlainValue(IImmutableDictionary<string, object> plainValue)
        {
            _count = long.Parse(plainValue["count"].ToString());
        }
}
```

- We use the `ActionBase` abstract class provided by Libplanet for Unity.
- Every action has an `ActionType` attribute to indicate the type to be serialized.
  - The value of the property must be unique for each action.
- Set `_count` as a member variable so that we can determine how much the value is incremented.
- Define default constructor (`AddCount()`), and `PlainValue`, ` LoadPlainValue()` so that they can be serialized / deserialized to file storage or network.


The next thing to look at is `Execute()`, where the actual game code is implemented.

```csharp
        public override IAccountStateDelta Execute(IActionContext ctx)
        {
            var states = ctx.PreviousStates;
            var rankingAddress = RankingState.Address;
            var currentCount = (long?)states.GetState(ctx.Signer)?? 0;
            var nextCount = currentCount + _count;

            var rankingState = (RankingState) states.GetState(rankingAddress) ?? new RankingState();
            rankingState.Update(ctx.Signer, nextCount);
            states = states.SetState(rankingAddress, rankingState);
            return states.SetState(ctx.Signer, nextCount);
        }
```

- `ctx` is the execution context that contains the block and transaction information which contains this action.
- Since we decided to use the address of the user who signed the transaction as the key in state, we set the next score as 1 plus the number obtained from the previous state by signer address(`ctx.signer`) and `_count`.
- Since the ranking(`RankingState`) is information shared by all users, the state is obtained and updated using a predetermined constant (`RankingState.Address`).
- Because this method runs on multiple nodes, it must not have any side effects and depend on any global variables or data other than `ctx` or member variable` _count` passed as an argument.

The last thing to look at is `Render()`. `AddCount` needs to update 'My Score' and 'Ranking' when executed, so it will invoke `Game.OnCountUpdated`, `Game.OnRunkUpdated` event for them.

```csharp
        public override void Render(IActionContext context, IAccountStateDelta nextStates)
        {
            var agent = Agent.instance;
            var count = (long?)nextStates.GetState(context.Signer) ?? 0;
            var rankingState = (RankingState)nextStates.GetState(RankingState.Address) ?? new RankingState();

            agent.RunOnMainThread(() =>
            {
                Game.OnCountUpdated.Invoke(count);
            });
            agent.RunOnMainThread(() =>
            {
                Game.OnRankUpdated.Invoke(rankingState);
            });
        }
```

Caution) The thread that `Render()` is invoked is prohibited from handling Unity UI object because it may not be the main Unity thread. To avoid this problem, we use the `Agent.RunOnMainthread()`.


`Agent`
-------

How can we use the `AddCount` defined above? One way is to use `BlockChain <T>` provided by Libplanet, but since it is a generic .netstandard library, it requires a lot of boiler plate code to use it inside Unity.

Instead, we can use `Agent`, a helper class provided by Libplanet for Unity for convenience. Let's take a look at `Game` class (defined in `Assets/_Script/Game.cs`) to learn how to use it.


```csharp
    public class Game : MonoBehaviour
    {
        // ...

        // Internal storage to storing clicking count
        public Click click;

        // Custom events for AddConut.Render()
        public class CountUpdated : UnityEvent<long>
        {
        }

        public class RankUpdated: UnityEvent<RankingState>
        {
        }

        public static CountUpdated OnCountUpdated = new CountUpdated();

        public static RankUpdated OnRankUpdated = new RankUpdated();

        private void Awake()
        {
            // ...

            // Initialize Agent object globally.
            Agent.Initialize();


            // Install event listener for `AddCount.Render()`
            OnCountUpdated.AddListener(UpdateTotalCount);
            OnRankUpdated.AddListener(rs =>
            {
                StartCoroutine(UpdateRankingBoard(rs));
            });

            // Load previous state from state
            OnCountUpdated.Invoke((long?) agent.GetState(Agent.instance.Address) ?? 0);
            OnRankUpdated.Invoke((RankingState) agent.GetState(RankingState.Address) ?? new RankingState());
        }

        // ...
    }
```

`Agent` has the following characteristics.

- `Agent` is [Unity MonoBehavior], so it follows the Unity Object Lifecycle and can also run coroutines.
- Since it was implemented with the singleton pattern, there is only one object (`Agent.instance`) throughout the game process.

So how do we add `AddCount` using this` Agent`?

```csharp
        // MonoBehaviour.FixedUpdate()
        private void FixedUpdate()
        {
            if (_time > 0)
            {
                _time -= Time.deltaTime;
                SetTimer(_time);
            }
            else
            {
                _time = TxProcessInterval;
                var actions = new List<ActionBase>();
                if (click._count > 0)
                {
                    var action = new AddCount(click._count);
                    actions.Add(action);
                }

                actions.AddRange(_attacks.Select(pair => new SubCount(pair.Key, pair.Value)));
                if (actions.Any())
                {
                    Agent.instance.MakeTransaction(actions);
                }
                _attacks = new Dictionary<Address, int>();

                ResetTimer();
            }
        }
```

We create `AddCount` in `Game.FixedUpdate()` which is called periodically by Unity, and register it via `Agent.MakeTransaction()`.


[Unity Monobehavior]: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html


How to create `Block<ActionBase>`
---------------------------------

In order for `AddCount` to be calculated and reflected in the state and game, it must be included in the block and confirmed. How can we include an action in a block?
The answer is 'We don't have to do anything.'. `Agent` executes a coroutine that creates a block, including the transaction added automatically when it is initialized.


Configure `Agent`
-----------------

`Agent` receives various parameters such as a private key, a listening port and a relay server URL for the setting of `BlockChain` and `Swarm <T>`. `Agent` provides a way to specify these parameters through the command-line arguments, or specific JSON file(`clo.json`) in `StreamingAssets` that can be replaced at runtime.

Command-line arguments or `clo.json` options are as follows:

- `--private-key` (clo.json:` privateKey`): Specifies the private key to use.
- `--host` (clo.json:` host`): Specifies the Host name to be used.
- `--port` (clo.json:` port`): Specifies the Port to use.
- `--no-miner` (clo.json:` noMiner`): Do not use mining.
- `--peer` (clo.json:` peers`): Adds a Peer. If you want to add Peer has several can be added as --peer peerA peerB ....
- `--storage-path` (clo.json:` storagePath`): Specifies the path to store the data.


How to connect other peers
--------------------------

Setting up a network using Libplanet is also simple. First, a user with a public IP and port that other players can access from anywhere will disclose their address information as below.

```
// Public Key,Host,Port
040fce03fd42dbd5feed2d3f0bc7103d59d30e6a70f8a5c420a8f8a06c358c0467a45455398360886fdaaee814e67d5d919794b7755f4efca6100eb0051c94fdec,1.2.3.4,5678
```

Other players choose one of the following ways to set up a peer.

1. Using `clo.json`

Create a `clo.json` file in the` StreamingAssets` directory and set it as shown below.

```json
{
    "peers": ["040fce03fd42dbd5feed2d3f0bc7103d59d30e6a70f8a5c420a8f8a06c358c0467a45455398360886fdaaee814e67d5d919794b7755f4efca6100eb0051c94fdec,1.2.3.4,5678"]
}
```

2. Using command line arguments

Run a game built with Unity with the following command line arguments:

```sh
> planet-clicker.exe --peer "040fce03fd42dbd5feed2d3f0bc7103d59d30e6a70f8a5c420a8f8a06c358c0467a45455398360886fdaaee814e67d5d919794b7755f4efca6100eb0051c94fdec,1.2.3.4,5678"
```

This peer information is read by `Agent` and then connected. This process is handled automatically within `Agent`, so there is nothing more that game developers need to implement.

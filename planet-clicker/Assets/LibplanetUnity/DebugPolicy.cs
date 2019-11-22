using System;
using System.Threading;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blockchain.Policies;
using Libplanet.Blocks;
using LibplanetUnity.Action;

namespace UnityEngine
{
     public class DebugPolicy : IBlockPolicy<PolymorphicAction<ActionBase>>
     {
         private static readonly TimeSpan SleepInterval = TimeSpan.FromSeconds(3);

         public DebugPolicy()
         {

         }

         public IAction BlockAction { get; }

         public InvalidBlockException ValidateNextBlock(
             BlockChain<PolymorphicAction<ActionBase>> blocks,
             Block<PolymorphicAction<ActionBase>> nextBlock
         )
         {
             return null;
         }

         public long GetNextBlockDifficulty(BlockChain<PolymorphicAction<ActionBase>> blocks)
         {
             Thread.Sleep(SleepInterval);
             return blocks.Tip is null ? 0 : 1;
         }
     }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NBitcoin.Tests
{
	public class brainaddress_tests
	{
		[Fact]
		public void CanCreateBrainAddress()
		{
			var repo = new NoSqlBlockRepository();
			var chain = new ConcurrentChain();

			Block b = new Block();
			b.Transactions.Add(new Transaction());
			b.Transactions.Add(new Transaction()
			{
				Outputs =
				{
					new TxOut(),
					new TxOut(Money.Zero,BitcoinAddress.Create("15sYbVpRh6dyWycZMwPdxJWD4xbfxReeHe"))
				}
			});

			repo.PutAsync(b).Wait();
			chain.SetTip(b.Header);


			BrainAddress address = BrainAddress.FetchAsync(chain, repo, 0, 1, 1).Result;
			BrainAddress address2 = BrainAddress.FetchAsync(chain, repo, Wordlist.English, address.ToString(Wordlist.English)).Result;
			Assert.Equal(address.ToString(), address2.ToString());
		}
		[Fact]
		public void CanCreateBrainAddress2()
		{
			Test(1782, 50, 1000, 1, 2);
		}

		class MockChain : ChainBase
		{
			public void Return(BlockHeader header)
			{
				_Return = header;
			}
			BlockHeader _Return;
			public override ChainedBlock GetBlock(uint256 id)
			{
				throw new NotImplementedException();
			}

			public override ChainedBlock GetBlock(int height)
			{
				return new ChainedBlock(_Return, height);
			}

			public override ChainedBlock Tip
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public override int Height
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			protected override IEnumerable<ChainedBlock> EnumerateFromStart()
			{
				throw new NotImplementedException();
			}

			public override ChainedBlock SetTip(ChainedBlock pindex)
			{
				throw new NotImplementedException();
			}
		}
		private void Test(int blockHeight, int txIndex, int txCount, int txOutIndex, int txOutCount)
		{
			var repo = new NoSqlBlockRepository();
			var chain = new MockChain();
			var block = new Block();
			for(int i = 0 ; i < txCount ; i++)
			{
				var tx = block.AddTransaction(new Transaction());
				if(i == txIndex)
				{
					for(int ii = 0 ; ii < txOutCount ; ii++)
					{
						var txout = tx.AddOutput(new TxOut());
						if(ii == txOutIndex)
							txout.Value = Money.Coins(1.0m);
					}
				}
			}
			block.ComputeMerkleRoot();
			chain.Return(block.Header);

			repo.PutAsync(block).Wait();


			var address = BrainAddress.FetchAsync(chain, repo, blockHeight, txIndex, txOutIndex).Result;
			var address2 = BrainAddress.FetchAsync(chain, repo, Wordlist.English, address.ToString()).Result;
			Assert.Equal(address.ToString(), address2.ToString());
			Assert.Equal(Money.Coins(1.0m), address.Output.Value);
			Assert.Equal(Money.Coins(1.0m), address2.Output.Value);
		}
	}
}
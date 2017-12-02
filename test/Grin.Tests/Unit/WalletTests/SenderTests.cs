using System;
using Grin.CoreImpl.Core.Build;
using Grin.KeychainImpl;
using Xunit;

namespace Grin.Tests.Unit.WalletTests
{
    public class SenderTests:IClassFixture<LoggingFixture>
    {

        // demonstrate that input.commitment == referenced output.commitment
        // based on the public key and amount begin spent
        [Fact]
        public void Output_commitment_equals_input_commitment_on_spend()
       {
           var keychain = Keychain.From_random_seed();
           var keyId1 = keychain.Derive_key_id(1);

           var (tx1,_) = Build.Transaction(new Func<Context, Append>[]
               {
                   context=>context.Output(105,keyId1.Clone())
               }
               , keychain);

            var (tx2, _) = Build.Transaction(new Func<Context, Append>[]
               {
                   context=>context.Input(105,keyId1.Clone())
               }
               , keychain);


            Assert.Equal(tx1.Outputs[0].Commit.Value, tx2.Inputs[0].Commitment.Value);
        }

    }
}

namespace Grin.Wallet
{
    /// Status of an output that's being tracked by the wallet. Can either be
    /// unconfirmed, spent, unspent, or locked (when it's been used to generate
    /// a transaction but we don't have confirmation that the transaction was
    /// broadcasted or mined).

//#[derive(Serialize, Deserialize, Debug, Clone, PartialEq, Eq)]
    public enum OutputStatus
    {
        Unconfirmed,
        Unspent,
        Locked,
        Spent
    }
}